using System.Globalization;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppIt.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/exports")]
    public class ExportsController : ControllerBase
    {
        [HttpPost("operational-row/pdf")]
        public IActionResult ExportOperationalRowPdf([FromBody] OperationalRowPdfExportRequest request)
        {
            if (request.Row.Count == 0 && request.Rows.Count == 0)
            {
                return BadRequest("No row data was supplied for export.");
            }

            var sourceRow = request.Row.Count > 0 ? request.Row : request.Rows.First();
            var columns = request.Columns.Count > 0
                ? request.Columns
                : sourceRow.Keys.Take(10).ToList();

            var title = string.IsNullOrWhiteSpace(request.Title) ? "Operational Row Export" : request.Title.Trim();
            var pdf = request.Rows.Count > 0
                ? SimplePdfWriter.WriteTable(title, columns, request.Rows)
                : SimplePdfWriter.WriteDetails(title, columns
                    .Where(column => request.Row.ContainsKey(column))
                    .Select(column => (Label: ToLabel(column), Value: Display(request.Row[column])))
                    .ToList());
            var fileName = $"{Slugify(title)}-{DateTime.UtcNow:yyyyMMddHHmmss}.pdf";
            return File(pdf, "application/pdf", fileName);
        }

        private static string Display(JsonElement value)
        {
            return value.ValueKind switch
            {
                JsonValueKind.Null or JsonValueKind.Undefined => "-",
                JsonValueKind.True => "Yes",
                JsonValueKind.False => "No",
                JsonValueKind.Number when value.TryGetDecimal(out var number) => number.ToString("0.##", CultureInfo.InvariantCulture),
                JsonValueKind.String => string.IsNullOrWhiteSpace(value.GetString()) ? "-" : value.GetString()!,
                _ => value.ToString()
            };
        }

        private static string ToLabel(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "Value";
            }

            var builder = new StringBuilder();
            for (var i = 0; i < value.Length; i++)
            {
                var current = value[i];
                if (i > 0 && char.IsUpper(current) && char.IsLower(value[i - 1]))
                {
                    builder.Append(' ');
                }

                builder.Append(i == 0 ? char.ToUpperInvariant(current) : current);
            }

            return builder.ToString();
        }

        private static string Slugify(string value)
        {
            var chars = value
                .ToLowerInvariant()
                .Select(ch => char.IsLetterOrDigit(ch) ? ch : '-')
                .ToArray();

            return string.Join('-', new string(chars).Split('-', StringSplitOptions.RemoveEmptyEntries)).Trim('-');
        }
    }

    public class OperationalRowPdfExportRequest
    {
        public string Title { get; set; } = "Operational Row Export";
        public List<string> Columns { get; set; } = new();
        public Dictionary<string, JsonElement> Row { get; set; } = new(StringComparer.OrdinalIgnoreCase);
        public List<Dictionary<string, JsonElement>> Rows { get; set; } = new();
    }

    internal static class SimplePdfWriter
    {
        public static byte[] WriteDetails(string title, IReadOnlyList<(string Label, string Value)> rows)
        {
            var content = BuildContent(title, rows);
            return WritePdf(content);
        }

        public static byte[] WriteTable(string title, IReadOnlyList<string> columns, IReadOnlyList<Dictionary<string, JsonElement>> rows)
        {
            var labels = columns.Select(ToLabel).ToList();
            var values = rows.Take(10)
                .Select(row => columns.Select(column => row.TryGetValue(column, out var value) ? Display(value) : "-").ToList())
                .ToList();

            var content = BuildTableContent(title, labels, values);
            return WritePdf(content);
        }

        private static byte[] WritePdf(string content)
        {
            var objects = new List<string>
            {
                "<< /Type /Catalog /Pages 2 0 R >>",
                "<< /Type /Pages /Kids [3 0 R] /Count 1 >>",
                "<< /Type /Page /Parent 2 0 R /MediaBox [0 0 612 792] /Resources << /Font << /F1 4 0 R /F2 5 0 R >> >> /Contents 6 0 R >>",
                "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>",
                "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica-Bold >>",
                $"<< /Length {Encoding.ASCII.GetByteCount(content)} >>\nstream\n{content}\nendstream"
            };

            using var stream = new MemoryStream();
            using var writer = new StreamWriter(stream, Encoding.ASCII, leaveOpen: true);
            writer.Write("%PDF-1.4\n");

            var offsets = new List<long> { 0 };
            for (var i = 0; i < objects.Count; i++)
            {
                writer.Flush();
                offsets.Add(stream.Position);
                writer.Write($"{i + 1} 0 obj\n{objects[i]}\nendobj\n");
            }

            writer.Flush();
            var xrefOffset = stream.Position;
            writer.Write($"xref\n0 {objects.Count + 1}\n");
            writer.Write("0000000000 65535 f \n");
            foreach (var offset in offsets.Skip(1))
            {
                writer.Write($"{offset:0000000000} 00000 n \n");
            }

            writer.Write($"trailer\n<< /Size {objects.Count + 1} /Root 1 0 R >>\nstartxref\n{xrefOffset}\n%%EOF");
            writer.Flush();
            return stream.ToArray();
        }

        private static string BuildTableContent(string title, IReadOnlyList<string> columns, IReadOnlyList<IReadOnlyList<string>> rows)
        {
            var builder = BuildHeader(title);
            var columnWidth = Math.Max(70, 540 / Math.Max(1, columns.Count));
            var x = 36;
            var y = 700;

            builder.AppendLine("0.98 0.72 0.18 rg");
            builder.AppendLine($"36 {y - 8} 540 22 re f");
            builder.AppendLine("0.07 0.09 0.15 rg");
            for (var i = 0; i < columns.Count; i++)
            {
                builder.AppendLine($"BT /F2 8 Tf {x + (i * columnWidth)} {y} Td ({Escape(Truncate(columns[i], 14))}) Tj ET");
            }

            y -= 28;
            foreach (var row in rows)
            {
                builder.AppendLine("0.96 0.96 0.95 rg");
                builder.AppendLine($"36 {y - 8} 540 22 re f");
                builder.AppendLine("0.22 0.25 0.31 rg");
                for (var i = 0; i < row.Count; i++)
                {
                    builder.AppendLine($"BT /F1 7 Tf {x + (i * columnWidth)} {y} Td ({Escape(Truncate(row[i], 16))}) Tj ET");
                }

                y -= 26;
            }

            return builder.ToString();
        }

        private static string Display(JsonElement value)
        {
            return value.ValueKind switch
            {
                JsonValueKind.Null or JsonValueKind.Undefined => "-",
                JsonValueKind.True => "Yes",
                JsonValueKind.False => "No",
                JsonValueKind.Number when value.TryGetDecimal(out var number) => number.ToString("0.##", CultureInfo.InvariantCulture),
                JsonValueKind.String => string.IsNullOrWhiteSpace(value.GetString()) ? "-" : value.GetString()!,
                _ => value.ToString()
            };
        }

        private static string ToLabel(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "Value";
            }

            var builder = new StringBuilder();
            for (var i = 0; i < value.Length; i++)
            {
                var current = value[i];
                if (i > 0 && char.IsUpper(current) && char.IsLower(value[i - 1]))
                {
                    builder.Append(' ');
                }

                builder.Append(i == 0 ? char.ToUpperInvariant(current) : current);
            }

            return builder.ToString();
        }

        private static string BuildContent(string title, IReadOnlyList<(string Label, string Value)> rows)
        {
            var builder = BuildHeader(title);
            var y = 698;
            foreach (var (label, value) in rows.Take(18))
            {
                builder.AppendLine("0.95 0.95 0.94 rg");
                builder.AppendLine($"36 {y - 8} 540 24 re f");
                builder.AppendLine("0.07 0.09 0.15 rg");
                builder.AppendLine($"BT /F2 10 Tf 48 {y} Td ({Escape(label)}) Tj ET");
                builder.AppendLine("0.22 0.25 0.31 rg");
                builder.AppendLine($"BT /F1 10 Tf 220 {y} Td ({Escape(Truncate(value, 72))}) Tj ET");
                y -= 30;
            }

            return builder.ToString();
        }

        private static StringBuilder BuildHeader(string title)
        {
            var builder = new StringBuilder();
            builder.AppendLine("0.98 0.72 0.18 rg");
            builder.AppendLine("0 742 612 50 re f");
            builder.AppendLine("0.07 0.09 0.15 rg");
            builder.AppendLine("BT /F2 18 Tf 36 762 Td");
            builder.AppendLine($"({Escape(title)}) Tj ET");
            builder.AppendLine("0.22 0.25 0.31 rg");
            builder.AppendLine("BT /F1 9 Tf 36 728 Td");
            builder.AppendLine($"(Generated {Escape(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm 'UTC'", CultureInfo.InvariantCulture))}) Tj ET");
            return builder;
        }

        private static string Escape(string value)
        {
            return value
                .Replace("\\", "\\\\", StringComparison.Ordinal)
                .Replace("(", "\\(", StringComparison.Ordinal)
                .Replace(")", "\\)", StringComparison.Ordinal)
                .Replace("\r", " ", StringComparison.Ordinal)
                .Replace("\n", " ", StringComparison.Ordinal);
        }

        private static string Truncate(string value, int maxLength)
        {
            return value.Length <= maxLength ? value : $"{value[..Math.Max(0, maxLength - 3)]}...";
        }
    }
}
