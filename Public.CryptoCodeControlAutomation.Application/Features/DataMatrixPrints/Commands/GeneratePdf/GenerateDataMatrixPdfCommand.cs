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
            private const int Columns = 4;
            private const int Rows = 8;
            //private const int Columns = 1;
            //private const int Rows = 1;
            private const int ItemsPerPage = Columns * Rows;
            private const double PageWidth = 595.28;
            private const double PageHeight = 841.89;
            //private const double PageWidth = 74;
            //private const double PageHeight = 74;
            private const double Margin = 24;
            private const double MatrixSize = 54;
            private const double TextSize = 5;
            private static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;

            public static byte[] Build(IReadOnlyList<string> codes)
            {
                var pages = codes.Chunk(ItemsPerPage).Select(BuildPageContent).ToList();
                return WritePdf(pages);
            }

            private static string BuildPageContent(string[] codes)
            {
                var cellWidth = (PageWidth - (Margin * 2)) / Columns;
                var cellHeight = (PageHeight - (Margin * 2)) / Rows;
                var content = new StringBuilder();
                content.AppendLine("0 g");

                for (var index = 0; index < codes.Length; index++)
                {
                    var code = codes[index];
                    var matrix = CreateMatrix(code);
                    var column = index % Columns;
                    var row = index / Columns;
                    var cellX = Margin + (column * cellWidth);
                    var cellTop = PageHeight - Margin - (row * cellHeight);
                    var matrixX = cellX + ((cellWidth - MatrixSize) / 2);
                    var matrixY = cellTop - MatrixSize - 8;

                    AppendMatrix(content, matrix, matrixX, matrixY);
                    AppendText(content, GetReadableText(code), cellX + 4, matrixY - 9);
                }

                return content.ToString();
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
                    //.Append(EscapePdfText(text))
                    .AppendLine(") Tj ET");
            }

            private static string GetReadableText(string code)
            {
                return code.Replace("\u001D", string.Empty);
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
