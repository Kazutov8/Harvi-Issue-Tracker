using IssueTracker.Domain.Entities;
using Xunit;

namespace IssueTracker.Domain.Tests.Labels;

public sealed class LabelTests
{
    [Fact]
    public void Create_NormalizesNameToUpperInvariant()
    {
        var label = Label.Create(Guid.NewGuid(), "  Bug  ");

        Assert.Equal("Bug", label.Name);
        Assert.Equal("BUG", label.NormalizedName);
    }
}
