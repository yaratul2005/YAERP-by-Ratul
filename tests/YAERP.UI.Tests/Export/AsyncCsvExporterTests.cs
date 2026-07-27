using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using YAERP.Infrastructure.Export;

namespace YAERP.UI.Tests.Export;

public class TestData
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class AsyncCsvExporterTests
{
    [Fact]
    public async Task ExportToStreamAsync_ShouldWriteCsvFormat()
    {
        var data = new List<TestData>
        {
            new TestData { Id = 1, Name = "Apple" },
            new TestData { Id = 2, Name = "Banana, \"Yellow\"" }
        };

        var exporter = new AsyncCsvExporter();
        using var ms = new MemoryStream();

        await exporter.ExportToStreamAsync(ToAsyncEnumerable(data), ms);

        ms.Position = 0;
        using var reader = new StreamReader(ms, Encoding.UTF8);
        var content = await reader.ReadToEndAsync();

        var lines = content.Split(System.Environment.NewLine, System.StringSplitOptions.RemoveEmptyEntries);
        Assert.Equal(3, lines.Length);
        Assert.Contains("Id,Name", lines[0]);
        Assert.Contains("1,Apple", lines[1]);
        Assert.Contains("2,\"Banana, \"\"Yellow\"\"\"", lines[2]);
    }

    private async IAsyncEnumerable<T> ToAsyncEnumerable<T>(IEnumerable<T> items)
    {
        foreach (var item in items)
        {
            await Task.Yield();
            yield return item;
        }
    }
}
