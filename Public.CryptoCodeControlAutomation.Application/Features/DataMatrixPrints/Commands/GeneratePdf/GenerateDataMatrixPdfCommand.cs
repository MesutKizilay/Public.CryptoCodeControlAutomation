using System.Globalization;
using System.Text;
using MediatR;
using Microsoft.AspNetCore.Http;
using ZXing;
using ZXing.Common;
using ZXing.Datamatrix;
using ZXing.Datamatrix.Encoder;

namespace CryptoCodeControlAutomation.Application.Features.DataMatrixPrints.Commands.GeneratePdf
{
    public class GenerateDataMatrixPdfCommand : IRequest<GenerateDataMatrixPdfResponse>
    {
        public IFormFile? File { get; set; }

        public class GenerateDataMatrixPdfCommandHandler : IRequestHandler<GenerateDataMatrixPdfCommand, GenerateDataMatrixPdfResponse>
        {
            public async Task<GenerateDataMatrixPdfResponse> Handle(GenerateDataMatrixPdfCommand request, CancellationToken cancellationToken)
            {
                if (request.File == null || request.File.Length == 0)
                {
                    throw new InvalidOperationException("CSV dosyası yüklenmelidir.");
                }

                var codes = await ReadCodes(request.File, cancellationToken);
                if (codes.Count == 0)
                {
                    throw new InvalidOperationException("PDF oluşturulacak kod bulunamadı.");
                }

                var content = DataMatrixPdfBuilder.Build(codes);

                return new GenerateDataMatrixPdfResponse
                {
                    Content = content,
                    FileName = $"datamatrix_{DateTime.Now:yyyyMMdd_HHmmss}.pdf",
                    CodeCount = codes.Count
                };
            }

            private static async Task<List<string>> ReadCodes(IFormFile file, CancellationToken cancellationToken)
            {
                using var stream = file.OpenReadStream();
                using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
                var text = await reader.ReadToEndAsync(cancellationToken);

                return text
                    .Split(["\r\n", "\n", "\r"], StringSplitOptions.None)
                    .Select(NormalizeCsvLine)
                    .Where(line => !string.IsNullOrWhiteSpace(line))
                    .ToList();
            }

            private static string NormalizeCsvLine(string rawLine)
            {
                var line = rawLine.Trim();

                if (line.Length > 0 && line[0] == '\uFEFF')
                {
                    line = line[1..];
                }

                if (line.StartsWith('"') && line.EndsWith('"') && line.Length >= 2)
                {
                    line = line[1..^1].Replace("\"\"", "\"");
                }

                return line;
            }
        }

        private static class DataMatrixPdfBuilder
        {
            private const int RowsPerPage = 10;
            private const int ItemsPerPage = RowsPerPage;
            private const double PageWidth = 595.28;
            private const double PageHeight = 841.89;
            private const double Margin = 24;
            private const double TableWidth = PageWidth - (Margin * 2);
            private const double TableHeight = PageHeight - (Margin * 2);
            private const double SerialNumberColumnWidth = 48;
            private const double MatrixColumnWidth = 98;
            private const double MatrixSize = 58;
            private const double TextSize = 6;
            private static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;

            public static byte[] Build(IReadOnlyList<string> codes)
            {
                var rows = codes
                    .Select((code, index) => new DataMatrixPdfRow(index + 1, code))
                    .ToList();

                var pages = rows.Chunk(ItemsPerPage).Select(BuildPageContent).ToList();
                return WritePdf(pages);
            }

            private static string BuildPageContent(DataMatrixPdfRow[] rows)
            {
                var rowHeight = TableHeight / RowsPerPage;
                var tableLeft = Margin;
                var tableRight = Margin + TableWidth;
                var tableTop = PageHeight - Margin;
                var tableBottom = Margin;
                var numberColumnRight = tableLeft + SerialNumberColumnWidth;
                var matrixColumnRight = numberColumnRight + MatrixColumnWidth;

                var content = new StringBuilder();
                content.AppendLine("0 g");
                content.AppendLine("0.5 w");

                AppendTableGrid(
                    content,
                    tableLeft,
                    tableRight,
                    tableTop,
                    tableBottom,
                    numberColumnRight,
                    matrixColumnRight,
                    rowHeight);

                for (var index = 0; index < rows.Length; index++)
                {
                    var row = rows[index];
                    var matrix = CreateMatrix(row.Code);
                    var rowTop = tableTop - (index * rowHeight);
                    var rowBottom = rowTop - rowHeight;
                    var rowMiddle = rowBottom + (rowHeight / 2);

                    var numberText = row.SerialNumber.ToString(CultureInfo.InvariantCulture);
                    var numberX = tableLeft + ((SerialNumberColumnWidth - EstimateTextWidth(numberText, TextSize)) / 2);
                    var textBaselineY = rowMiddle - (TextSize / 2);

                    var matrixX = numberColumnRight + ((MatrixColumnWidth - MatrixSize) / 2);
                    var matrixY = rowMiddle - (MatrixSize / 2);

                    var readableText = GetReadableText(row.Code);
                    var readableTextX = matrixColumnRight + 8;

                    AppendText(content, numberText, numberX, textBaselineY);
                    AppendMatrix(content, matrix, matrixX, matrixY);
                    AppendText(content, readableText, readableTextX, textBaselineY);
                }

                return content.ToString();
            }

            private static void AppendTableGrid(
                StringBuilder content,
                double tableLeft,
                double tableRight,
                double tableTop,
                double tableBottom,
                double numberColumnRight,
                double matrixColumnRight,
                double rowHeight)
            {
                AppendLine(content, tableLeft, tableBottom, tableLeft, tableTop);
                AppendLine(content, numberColumnRight, tableBottom, numberColumnRight, tableTop);
                AppendLine(content, matrixColumnRight, tableBottom, matrixColumnRight, tableTop);
                AppendLine(content, tableRight, tableBottom, tableRight, tableTop);

                for (var row = 0; row <= RowsPerPage; row++)
                {
                    var y = tableTop - (row * rowHeight);
                    AppendLine(content, tableLeft, y, tableRight, y);
                }
            }

            private static void AppendLine(StringBuilder content, double x1, double y1, double x2, double y2)
            {
                content
                    .Append(Format(x1)).Append(' ')
                    .Append(Format(y1)).Append(" m ")
                    .Append(Format(x2)).Append(' ')
                    .Append(Format(y2)).AppendLine(" l S");
            }

            private static BitMatrix CreateMatrix(string code)
            {
                var writer = new DataMatrixWriter();
                var hints = new Dictionary<EncodeHintType, object>
                {
                    [EncodeHintType.DATA_MATRIX_SHAPE] = SymbolShapeHint.FORCE_SQUARE,
                    [EncodeHintType.DATA_MATRIX_COMPACT] = true,
                    [EncodeHintType.GS1_FORMAT] = true,
                    [EncodeHintType.MARGIN] = 0
                };

                return writer.encode(code, BarcodeFormat.DATA_MATRIX, 0, 0, hints);
            }

            private static void AppendMatrix(StringBuilder content, BitMatrix matrix, double x, double y)
            {
                var moduleSize = MatrixSize / Math.Max(matrix.Width, matrix.Height);
                var offsetX = x + ((MatrixSize - (matrix.Width * moduleSize)) / 2);
                var offsetY = y + ((MatrixSize - (matrix.Height * moduleSize)) / 2);

                for (var row = 0; row < matrix.Height; row++)
                {
                    for (var column = 0; column < matrix.Width; column++)
                    {
                        if (!matrix[column, row])
                        {
                            continue;
                        }

                        var moduleX = offsetX + (column * moduleSize);
                        var moduleY = offsetY + ((matrix.Height - row - 1) * moduleSize);
                        content
                            .Append(Format(moduleX)).Append(' ')
                            .Append(Format(moduleY)).Append(' ')
                            .Append(Format(moduleSize + 0.01)).Append(' ')
                            .Append(Format(moduleSize + 0.01)).AppendLine(" re f");
                    }
                }
            }

            private static void AppendText(StringBuilder content, string text, double x, double y)
            {
                content
                    .Append("BT /F1 ")
                    .Append(Format(TextSize))
                    .Append(" Tf ")
                    .Append(Format(x))
                    .Append(' ')
                    .Append(Format(y))
                    .Append(" Td (")
                    .Append(EscapePdfText(text))
                    .AppendLine(") Tj ET");
            }

            private static string GetReadableText(string code)
            {
                const char groupSeparator = '\u001D';

                if (string.IsNullOrWhiteSpace(code))
                {
                    return string.Empty;
                }

                var value = code.Trim();
                var parts = new List<string>();

                if (value.Length >= 18 && value.StartsWith("01", StringComparison.Ordinal))
                {
                    var gtin = value.Substring(2, 14);
                    parts.Add($"(01) {gtin}");

                    var index = 16;
                    if (value.Length >= index + 2 && value.Substring(index, 2) == "21")
                    {
                        index += 2;
                        var serialEnd = value.IndexOf(groupSeparator, index);
                        var serial = serialEnd >= 0
                            ? value[index..serialEnd]
                            : value[index..];

                        parts.Add($"(21) {serial}");
                        index = serialEnd >= 0 ? serialEnd + 1 : value.Length;

                        while (index < value.Length)
                        {
                            if (value[index] == groupSeparator)
                            {
                                index++;
                                continue;
                            }

                            if (index + 2 > value.Length)
                            {
                                break;
                            }

                            var applicationIdentifier = value.Substring(index, 2);
                            index += 2;

                            var nextSeparator = value.IndexOf(groupSeparator, index);
                            var applicationIdentifierValue = nextSeparator >= 0
                                ? value[index..nextSeparator]
                                : value[index..];

                            parts.Add($"({applicationIdentifier}) {applicationIdentifierValue}");
                            index = nextSeparator >= 0 ? nextSeparator + 1 : value.Length;
                        }

                        return string.Join(" ", parts);
                    }
                }

                return value.Replace(groupSeparator, ' ');
            }

            private static string EscapePdfText(string value)
            {
                return value
                    .Replace("\\", "\\\\")
                    .Replace("(", "\\(")
                    .Replace(")", "\\)");
            }

            private static byte[] WritePdf(IReadOnlyList<string> pageContents)
            {
                var objects = new List<PdfObject>
                {
                    new(1, "<< /Type /Catalog /Pages 2 0 R >>"),
                    new(2, string.Empty),
                    new(3, "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>")
                };

                var pageIds = new List<int>();
                foreach (var pageContent in pageContents)
                {
                    var pageId = objects.Count + 1;
                    var contentId = pageId + 1;
                    pageIds.Add(pageId);

                    objects.Add(new PdfObject(pageId, $"<< /Type /Page /Parent 2 0 R /MediaBox [0 0 {Format(PageWidth)} {Format(PageHeight)}] /Resources << /Font << /F1 3 0 R >> >> /Contents {contentId} 0 R >>"));
                    objects.Add(PdfObject.Stream(contentId, Encoding.ASCII.GetBytes(pageContent)));
                }

                objects[1] = new PdfObject(2, $"<< /Type /Pages /Kids [{string.Join(" ", pageIds.Select(id => $"{id} 0 R"))}] /Count {pageIds.Count} >>");

                using var stream = new MemoryStream();
                WriteAscii(stream, "%PDF-1.4\n%\u00E2\u00E3\u00CF\u00D3\n");

                var offsets = new long[objects.Count + 1];
                foreach (var pdfObject in objects.OrderBy(o => o.Id))
                {
                    offsets[pdfObject.Id] = stream.Position;
                    WriteAscii(stream, $"{pdfObject.Id} 0 obj\n");

                    if (pdfObject.StreamContent != null)
                    {
                        WriteAscii(stream, $"<< /Length {pdfObject.StreamContent.Length} >>\nstream\n");
                        stream.Write(pdfObject.StreamContent, 0, pdfObject.StreamContent.Length);
                        WriteAscii(stream, "\nendstream\n");
                    }
                    else
                    {
                        WriteAscii(stream, pdfObject.Content);
                        WriteAscii(stream, "\n");
                    }

                    WriteAscii(stream, "endobj\n");
                }

                var xrefPosition = stream.Position;
                WriteAscii(stream, $"xref\n0 {objects.Count + 1}\n");
                WriteAscii(stream, "0000000000 65535 f \n");

                for (var id = 1; id <= objects.Count; id++)
                {
                    WriteAscii(stream, $"{offsets[id]:0000000000} 00000 n \n");
                }

                WriteAscii(stream, $"trailer\n<< /Size {objects.Count + 1} /Root 1 0 R >>\nstartxref\n{xrefPosition}\n%%EOF");
                return stream.ToArray();
            }

            private static void WriteAscii(Stream stream, string value)
            {
                var bytes = Encoding.ASCII.GetBytes(value);
                stream.Write(bytes, 0, bytes.Length);
            }

            private static string Format(double value)
            {
                return value.ToString("0.###", Invariant);
            }

            private static double EstimateTextWidth(string value, double fontSize)
            {
                return value.Length * fontSize * 0.55;
            }

            private sealed record DataMatrixPdfRow(int SerialNumber, string Code);

            private sealed record PdfObject(int Id, string Content)
            {
                public byte[]? StreamContent { get; init; }

                public static PdfObject Stream(int id, byte[] content)
                {
                    return new PdfObject(id, string.Empty) { StreamContent = content };
                }
            }
        }
    }
}
