using IssueTracker.Domain.Entities;

namespace IssueTracker.Application.Abstractions;

public interface ILabelRepository
{
    Task AddAsync(Label label, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Label>> ListByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default);
}
