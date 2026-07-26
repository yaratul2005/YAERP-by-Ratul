using System.Collections.Generic;
using System.Linq;
using Xunit;
using YAERP.Infrastructure.Persistence;

namespace YAERP.UI.Tests.Persistence;

public class TestEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class QueryableExtensionsTests
{
    [Fact]
    public void ApplySorting_ShouldSortByPropertyName()
    {
        var data = new List<TestEntity>
        {
            new TestEntity { Id = 1, Name = "Zebra" },
            new TestEntity { Id = 2, Name = "Apple" }
        }.AsQueryable();

        var sorted = data.ApplySorting("Name", false).ToList();

        Assert.Equal("Apple", sorted[0].Name);
        Assert.Equal("Zebra", sorted[1].Name);
    }

    [Fact]
    public void ApplyPaging_ShouldReturnPagedResults()
    {
        var data = Enumerable.Range(1, 10).Select(i => new TestEntity { Id = i }).AsQueryable();

        var paged = data.ApplyPaging(2, 3).ToList();

        Assert.Equal(3, paged.Count);
        Assert.Equal(4, paged[0].Id);
        Assert.Equal(5, paged[1].Id);
        Assert.Equal(6, paged[2].Id);
    }
}
