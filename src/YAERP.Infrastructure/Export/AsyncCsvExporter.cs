using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace YAERP.Infrastructure.Export;

public class AsyncCsvExporter
{
    public async Task ExportToStreamAsync<T>(IAsyncEnumerable<T> data, Stream stream, CancellationToken cancellationToken = default)
    {
        using var writer = new StreamWriter(stream, Encoding.UTF8, bufferSize: 65536, leaveOpen: true);

        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        if (properties.Length == 0) return;

        // Write header
        for (int i = 0; i < properties.Length; i++)
        {
            await writer.WriteAsync(EscapeCsv(properties[i].Name));
            if (i < properties.Length - 1)
                await writer.WriteAsync(",");
        }
        await writer.WriteLineAsync();

        // Write data streaming
        await foreach (var item in data.WithCancellation(cancellationToken))
        {
            if (item == null) continue;

            for (int i = 0; i < properties.Length; i++)
            {
                var val = properties[i].GetValue(item)?.ToString() ?? string.Empty;
                await writer.WriteAsync(EscapeCsv(val));
                if (i < properties.Length - 1)
                    await writer.WriteAsync(",");
            }
            await writer.WriteLineAsync();
        }

        await writer.FlushAsync();
    }

    private string EscapeCsv(string field)
    {
        if (string.IsNullOrEmpty(field)) return "";
        if (field.Contains(',') || field.Contains('"') || field.Contains('\n') || field.Contains('\r'))
        {
            return $"\"{field.Replace("\"", "\"\"")}\"";
        }
        return field;
    }
}
