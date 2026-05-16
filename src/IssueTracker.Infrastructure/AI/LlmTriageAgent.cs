using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using IssueTracker.Application.Abstractions;
using Microsoft.Extensions.Options;

namespace IssueTracker.Infrastructure.AI;

public sealed class LlmTriageAgent(HttpClient httpClient, IOptions<TriageOptions> options) : ITriageAgent
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly TriageOptions _options = options.Value;

    public async Task<TriageAgentResponse> SuggestAsync(TriageAgentRequest request, CancellationToken cancellationToken = default)
    {
        var prompt = BuildPrompt(request);
        var chatRequest = new ChatCompletionsRequest(
            _options.Model,
            [
                new ChatMessage("system", "You generate issue triage suggestions. Return JSON only with fields: priority, labels, acceptanceCriteria. Use only labels explicitly provided for the project. Never invent labels. If no labels are available, return an empty labels array. Placeholder values like none, null, and n/a are not valid labels unless explicitly provided. The acceptanceCriteria text must be written in Russian."),
                new ChatMessage("user", prompt),
            ]);

        using var response = await httpClient.PostAsJsonAsync("/v1/chat/completions", chatRequest, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException($"AI provider request failed with status {(int)response.StatusCode}: {body}");
        }

        var chatResponse = await response.Content.ReadFromJsonAsync<ChatCompletionsResponse>(JsonOptions, cancellationToken);
        var content = chatResponse?.Choices?.FirstOrDefault()?.Message?.Content;

        if (string.IsNullOrWhiteSpace(content))
        {
            throw new InvalidOperationException("AI provider returned an empty response.");
        }

        try
        {
            var parsed = JsonSerializer.Deserialize<TriagePayload>(ExtractJson(content), JsonOptions);

            return new TriageAgentResponse(
                parsed?.Priority,
                parsed?.Labels ?? [],
                parsed?.AcceptanceCriteria);
        }
        catch (JsonException exception)
        {
            throw new InvalidOperationException($"AI provider returned invalid JSON: {exception.Message}");
        }
    }

    private static string BuildPrompt(TriageAgentRequest request)
    {
        var builder = new StringBuilder();
        builder.AppendLine("Suggest triage for the issue below.");
        builder.AppendLine($"Title: {request.Title}");
        builder.AppendLine($"Description: {(string.IsNullOrWhiteSpace(request.Description) ? "(none)" : request.Description)}");
        if (request.AvailableLabels.Count == 0)
        {
            builder.AppendLine("Available labels: no project labels are available.");
            builder.AppendLine("Return \"labels\": [] and do not invent placeholder labels like \"none\", \"null\", or \"n/a\".");
            builder.AppendLine("A non-empty labels array is invalid for this request.");
        }
        else
        {
            builder.AppendLine($"Available labels: {string.Join(", ", request.AvailableLabels)}");
            builder.AppendLine("Use only labels from the available labels list.");
            builder.AppendLine("Do not output placeholders, synonyms, or inferred labels outside that list.");
        }

        builder.AppendLine("Write the acceptanceCriteria text in Russian.");
        builder.AppendLine("Return strict JSON with this shape:");
        builder.AppendLine("{\"priority\":\"medium\",\"labels\":[],\"acceptanceCriteria\":\"...\"}");
        return builder.ToString();
    }

    private static string ExtractJson(string content)
    {
        var trimmed = content.Trim();

        if (trimmed.StartsWith("```") && trimmed.EndsWith("```"))
        {
            var firstBrace = trimmed.IndexOf('{');
            var lastBrace = trimmed.LastIndexOf('}');

            if (firstBrace >= 0 && lastBrace > firstBrace)
            {
                return trimmed[firstBrace..(lastBrace + 1)];
            }
        }

        return trimmed;
    }

    private sealed record ChatCompletionsRequest(string Model, IReadOnlyList<ChatMessage> Messages);

    private sealed record ChatMessage(string Role, string Content);

    private sealed record ChatCompletionsResponse(IReadOnlyList<ChatChoice>? Choices);

    private sealed record ChatChoice(ChatMessage? Message);

    private sealed record TriagePayload(string? Priority, IReadOnlyList<string>? Labels, string? AcceptanceCriteria);
}
