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
        private const string BrandName = "AppIt";
        private const string BrandTagline = "Adventure and Hospitality Management Suite";
        private const string BrandEmail = "admin@appit.com";
        private const string BrandPhone = "+263 77 000 0000";
        private const string BrandAddress = "Harare, Zimbabwe";
        private const string BrandPoweredBy = "Powered By Tedwell (YourItGuy - 2026)";

        public static byte[] WriteDetails(string title, IReadOnlyList<(string Label, string Value)> rows)
        {
            var layout = PdfLayout.Portrait();
            var content = BuildContent(title, rows, layout);
            return WritePdf(content, layout);
        }

        public static byte[] WriteTable(string title, IReadOnlyList<string> columns, IReadOnlyList<Dictionary<string, JsonElement>> rows)
        {
            var layout = columns.Count > 5 ? PdfLayout.Landscape() : PdfLayout.Portrait();
            var labels = columns.Select(ToLabel).ToList();
            var values = rows.Take(10)
                .Select(row => columns.Select(column => row.TryGetValue(column, out var value) ? Display(value) : "-").ToList())
                .ToList();

            var content = BuildTableContent(title, labels, values, layout);
            return WritePdf(content, layout);
        }

        private static byte[] WritePdf(string content, PdfLayout layout)
        {
            var objects = new List<string>
            {
                "<< /Type /Catalog /Pages 2 0 R >>",
                "<< /Type /Pages /Kids [3 0 R] /Count 1 >>",
                $"<< /Type /Page /Parent 2 0 R /MediaBox [0 0 {layout.Width:0} {layout.Height:0}] /Resources << /Font << /F1 4 0 R /F2 5 0 R >> >> /Contents 6 0 R >>",
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

        private static string BuildTableContent(string title, IReadOnlyList<string> columns, IReadOnlyList<IReadOnlyList<string>> rows, PdfLayout layout)
        {
            var builder = BuildHeader(title, layout);
            var usableWidth = layout.Width - (layout.Margin * 2);
            var columnWidth = Math.Max(38, usableWidth / Math.Max(1, columns.Count));
            var fontSize = columns.Count > 9 ? 5 : columns.Count > 7 ? 6 : 7;
            var headerFontSize = Math.Min(8, fontSize + 1);
            var maxHeaderChars = Math.Max(6, (int)(columnWidth / Math.Max(3, headerFontSize * 0.48)));
            var maxValueChars = Math.Max(8, (int)(columnWidth / Math.Max(3, fontSize * 0.48)));
            var x = layout.Margin;
            var y = layout.ContentStartY;

            builder.AppendLine("0.98 0.72 0.18 rg");
            builder.AppendLine($"{layout.Margin:0.##} {y - 8:0.##} {usableWidth:0.##} 22 re f");
            builder.AppendLine("0.07 0.09 0.15 rg");
            for (var i = 0; i < columns.Count; i++)
            {
                builder.AppendLine($"BT /F2 {headerFontSize:0.##} Tf {x + (i * columnWidth):0.##} {y:0.##} Td ({Escape(Truncate(columns[i], maxHeaderChars))}) Tj ET");
            }

            y -= 28;
            foreach (var row in rows)
            {
                builder.AppendLine("0.96 0.96 0.95 rg");
                builder.AppendLine($"{layout.Margin:0.##} {y - 8:0.##} {usableWidth:0.##} 22 re f");
                builder.AppendLine("0.22 0.25 0.31 rg");
                for (var i = 0; i < row.Count; i++)
                {
                    builder.AppendLine($"BT /F1 {fontSize:0.##} Tf {x + (i * columnWidth):0.##} {y:0.##} Td ({Escape(Truncate(row[i], maxValueChars))}) Tj ET");
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

        private static string BuildContent(string title, IReadOnlyList<(string Label, string Value)> rows, PdfLayout layout)
        {
            var builder = BuildHeader(title, layout);
            var y = layout.ContentStartY - 2;
            foreach (var (label, value) in rows.Take(18))
            {
                builder.AppendLine("0.95 0.95 0.94 rg");
                builder.AppendLine($"{layout.Margin:0.##} {y - 8:0.##} {layout.Width - (layout.Margin * 2):0.##} 24 re f");
                builder.AppendLine("0.07 0.09 0.15 rg");
                builder.AppendLine($"BT /F2 10 Tf {layout.Margin + 12:0.##} {y:0.##} Td ({Escape(label)}) Tj ET");
                builder.AppendLine("0.22 0.25 0.31 rg");
                builder.AppendLine($"BT /F1 10 Tf {layout.Margin + 184:0.##} {y:0.##} Td ({Escape(Truncate(value, layout.IsLandscape ? 104 : 72))}) Tj ET");
                y -= 30;
            }

            return builder.ToString();
        }

        private static StringBuilder BuildHeader(string title, PdfLayout layout)
        {
            var builder = new StringBuilder();
            var headerY = layout.Height - 50;
            var titleY = layout.Height - 72;
            var metaY = layout.Height - 86;
            var contactX = layout.Width - 282;
            builder.AppendLine("0.98 0.72 0.18 rg");
            builder.AppendLine($"0 {headerY:0.##} {layout.Width:0.##} 50 re f");
            builder.AppendLine("0.07 0.09 0.15 rg");
            builder.AppendLine($"{layout.Margin:0.##} {headerY + 13:0.##} 28 24 re f");
            builder.AppendLine("0.98 0.72 0.18 rg");
            builder.AppendLine($"BT /F2 16 Tf {layout.Margin + 9:0.##} {headerY + 20:0.##} Td");
            builder.AppendLine("(A) Tj ET");
            builder.AppendLine("0.07 0.09 0.15 rg");
            builder.AppendLine($"BT /F2 18 Tf {layout.Margin + 38:0.##} {headerY + 22:0.##} Td ({Escape(BrandName)}) Tj ET");
            builder.AppendLine("0.22 0.25 0.31 rg");
            builder.AppendLine($"BT /F1 8 Tf {layout.Margin + 38:0.##} {headerY + 10:0.##} Td ({Escape(BrandTagline)}) Tj ET");
            builder.AppendLine("0.07 0.09 0.15 rg");
            builder.AppendLine($"BT /F2 14 Tf {layout.Margin:0.##} {titleY:0.##} Td");
            builder.AppendLine($"({Escape(title)}) Tj ET");
            builder.AppendLine("0.22 0.25 0.31 rg");
            builder.AppendLine($"BT /F1 8 Tf {layout.Margin:0.##} {metaY:0.##} Td");
            builder.AppendLine($"(Generated {Escape(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm 'UTC'", CultureInfo.InvariantCulture))}) Tj ET");
            builder.AppendLine($"BT /F1 8 Tf {contactX:0.##} {titleY:0.##} Td");
            builder.AppendLine($"({Escape($"{BrandEmail} | {BrandPhone}")}) Tj ET");
            builder.AppendLine($"BT /F1 8 Tf {contactX:0.##} {titleY - 12:0.##} Td");
            builder.AppendLine($"({Escape(BrandAddress)}) Tj ET");
            builder.AppendLine($"BT /F1 8 Tf {contactX:0.##} {titleY - 24:0.##} Td");
            builder.AppendLine($"({Escape(BrandPoweredBy)}) Tj ET");
            return builder;
        }

        private readonly record struct PdfLayout(double Width, double Height, double Margin, double ContentStartY, bool IsLandscape)
        {
            public static PdfLayout Portrait() => new(612, 792, 30, 670, false);
            public static PdfLayout Landscape() => new(792, 612, 30, 490, true);
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
