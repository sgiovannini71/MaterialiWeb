using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Text;
using MaterialiGestioneWeb.Models;

namespace MaterialiGestioneWeb
{
    internal sealed class MovementSheetModel
    {
        public string Ente { get; set; }
        public string Titolo { get; set; }
        public string PersonaLabel { get; set; }
        public string PersonaValore { get; set; }
        public DateTime Data { get; set; }
        public IList<ProdottoCorrente> Prodotti { get; set; }
        public AssignmentSheetLogo Logo { get; set; }
    }

    internal static class MovementSheetSupport
    {
        public static string GetConfiguredHeaderText()
        {
            var configured = ConfigurationManager.AppSettings["AssignmentSheetHeaderText"];
            return string.IsNullOrWhiteSpace(configured) ? "DNA - DAAAA" : configured.Trim();
        }

        public static string GetMovementTemplatePath(System.Web.UI.Page page)
        {
            var relativePath = ConfigurationManager.AppSettings["MovementSheetTemplatePath"];
            return string.IsNullOrWhiteSpace(relativePath)
                ? page.Server.MapPath("~/html/SchedaMovimentazioneTemplate.html")
                : page.Server.MapPath("~/" + relativePath.Replace('\\', '/').TrimStart('/'));
        }

        public static AssignmentSheetLogo LoadLogo(System.Web.UI.Page page)
        {
            var configuredPath = ConfigurationManager.AppSettings["AssignmentSheetLogoPath"];
            if (string.IsNullOrWhiteSpace(configuredPath))
            {
                return null;
            }

            var absolutePath = page.Server.MapPath("~/" + configuredPath.Replace('\\', '/').TrimStart('/'));
            if (!File.Exists(absolutePath))
            {
                return null;
            }

            using (var image = System.Drawing.Image.FromFile(absolutePath))
            using (var stream = new MemoryStream())
            {
                image.Save(stream, System.Drawing.Imaging.ImageFormat.Jpeg);
                return new AssignmentSheetLogo
                {
                    JpegBytes = stream.ToArray(),
                    PixelWidth = image.Width,
                    PixelHeight = image.Height
                };
            }
        }
    }

    internal static class MovementSheetPdfExporter
    {
        private const float PageWidth = 595f;
        private const float PageHeight = 842f;
        private const float MarginLeft = 40f;
        private const float MarginRight = 40f;
        private const float MarginTop = 48f;
        private const float MarginBottom = 48f;
        private const float TableHeaderHeight = 22f;
        private static readonly Encoding PdfEncoding = Encoding.GetEncoding(1252);

        public static byte[] Create(MovementSheetModel model)
        {
            var pages = BuildPages(model);
            return BuildPdf(pages, model.Logo);
        }

        private static List<string> BuildPages(MovementSheetModel model)
        {
            var pages = new List<string>();
            var current = new StringBuilder();
            var y = PageHeight - MarginTop;

            AppendHeader(current, model, ref y, includeLogo: true);
            y -= 8f;
            AppendTableHeader(current, ref y);

            foreach (var prodotto in model.Prodotti ?? new List<ProdottoCorrente>())
            {
                var row = BuildRow(prodotto);
                var rowHeight = MeasureRowHeight(row);
                if (y - rowHeight < MarginBottom + 90f)
                {
                    pages.Add(current.ToString());
                    current.Clear();
                    y = PageHeight - MarginTop;
                    AppendHeader(current, model, ref y, includeLogo: false);
                    y -= 8f;
                    AppendTableHeader(current, ref y);
                }

                AppendTableRow(current, row, ref y, rowHeight);
            }

            if (y < MarginBottom + 70f)
            {
                pages.Add(current.ToString());
                current.Clear();
                y = PageHeight - MarginTop;
                AppendHeader(current, model, ref y, includeLogo: false);
            }

            AppendSignature(current, ref y);
            pages.Add(current.ToString());
            return pages;
        }

        private static string[][] BuildRow(ProdottoCorrente prodotto)
        {
            return new[]
            {
                WrapText(ToText(prodotto.Categorico), 52f, 9f),
                WrapText(prodotto.DescrizioneProdotto, 158f, 9f),
                WrapText(prodotto.Matricola, 78f, 9f),
                WrapText(prodotto.Categoria, 88f, 9f),
                WrapText(prodotto.Modello, 110f, 9f)
            };
        }

        private static float MeasureRowHeight(IReadOnlyList<string[]> row)
        {
            var maxLines = 1;
            for (var i = 0; i < row.Count; i++)
            {
                maxLines = Math.Max(maxLines, Math.Max(row[i].Length, 1));
            }

            return 8f + (maxLines * 11f);
        }

        private static void AppendHeader(StringBuilder sb, MovementSheetModel model, ref float y, bool includeLogo)
        {
            if (includeLogo && model.Logo != null && model.Logo.JpegBytes != null && model.Logo.JpegBytes.Length > 0)
            {
                DrawLogo(sb, model.Logo, MarginLeft, y - 36f);
            }

            AppendCenteredText(sb, Safe(model.Ente).ToUpperInvariant(), 14f, true, y);
            y -= 26f;
            AppendCenteredText(sb, Safe(model.Titolo), 13f, true, y);
            y -= 28f;

            AppendText(sb, "Data: " + model.Data.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture), MarginLeft, y, 11f, false);
            y -= 20f;
            AppendText(sb, Safe(model.PersonaLabel) + ": " + Safe(model.PersonaValore), MarginLeft, y, 11f, true);
            y -= 24f;
        }

        private static void AppendTableHeader(StringBuilder sb, ref float y)
        {
            var widths = GetColumnWidths();
            var headers = new[] { "Categorico", "Descrizione", "Matricola", "Categoria", "Modello" };
            var x = MarginLeft;
            DrawRectangle(sb, MarginLeft, y - TableHeaderHeight, TotalTableWidth(), TableHeaderHeight);

            for (var i = 0; i < widths.Length - 1; i++)
            {
                x += widths[i];
                DrawLine(sb, x, y, x, y - TableHeaderHeight);
            }

            x = MarginLeft;
            for (var index = 0; index < headers.Length; index++)
            {
                AppendText(sb, headers[index], x + 4f, y - 15f, 9f, true);
                x += widths[index];
            }

            y -= TableHeaderHeight;
        }

        private static void AppendTableRow(StringBuilder sb, IReadOnlyList<string[]> row, ref float y, float rowHeight)
        {
            var widths = GetColumnWidths();
            DrawRectangle(sb, MarginLeft, y - rowHeight, TotalTableWidth(), rowHeight);

            var x = MarginLeft;
            for (var i = 0; i < widths.Length - 1; i++)
            {
                x += widths[i];
                DrawLine(sb, x, y, x, y - rowHeight);
            }

            x = MarginLeft;
            for (var index = 0; index < row.Count; index++)
            {
                var lineY = y - 13f;
                for (var j = 0; j < row[index].Length; j++)
                {
                    AppendText(sb, row[index][j], x + 4f, lineY, 9f, false);
                    lineY -= 11f;
                }

                x += widths[index];
            }

            y -= rowHeight;
        }

        private static void AppendSignature(StringBuilder sb, ref float y)
        {
            y -= 30f;
            AppendText(sb, "Firma:", 360f, y, 11f, false);
            DrawLine(sb, 405f, y - 2f, 540f, y - 2f);
        }

        private static string[] WrapText(string value, float width, float fontSize)
        {
            var normalized = Safe(value);
            if (string.IsNullOrWhiteSpace(normalized))
            {
                return new[] { string.Empty };
            }

            var maxChars = Math.Max(1, (int)Math.Floor(width / (fontSize * 0.52f)));
            var words = normalized.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var lines = new List<string>();
            var current = string.Empty;

            foreach (var word in words)
            {
                var candidate = string.IsNullOrEmpty(current) ? word : current + " " + word;
                if (candidate.Length <= maxChars)
                {
                    current = candidate;
                    continue;
                }

                if (!string.IsNullOrEmpty(current))
                {
                    lines.Add(current);
                    current = string.Empty;
                }

                if (word.Length <= maxChars)
                {
                    current = word;
                    continue;
                }

                var remaining = word;
                while (remaining.Length > maxChars)
                {
                    lines.Add(remaining.Substring(0, maxChars));
                    remaining = remaining.Substring(maxChars);
                }

                current = remaining;
            }

            if (!string.IsNullOrEmpty(current))
            {
                lines.Add(current);
            }

            return lines.Count == 0 ? new[] { string.Empty } : lines.ToArray();
        }

        private static byte[] BuildPdf(IList<string> pagesContent, AssignmentSheetLogo logo)
        {
            var objects = new List<string>();
            objects.Add("<< /Type /Catalog /Pages 2 0 R >>");

            var pageObjectNumbers = new List<int>();
            var contentObjectNumbers = new List<int>();
            var nextObjectNumber = 5;
            var logoObjectNumber = 0;
            if (logo != null && logo.JpegBytes != null && logo.JpegBytes.Length > 0)
            {
                logoObjectNumber = nextObjectNumber++;
            }

            for (var i = 0; i < pagesContent.Count; i++)
            {
                pageObjectNumbers.Add(nextObjectNumber++);
                contentObjectNumbers.Add(nextObjectNumber++);
            }

            var kids = string.Join(" ", pageObjectNumbers.ConvertAll(number => number.ToString(CultureInfo.InvariantCulture) + " 0 R").ToArray());
            objects.Add("<< /Type /Pages /Count " + pagesContent.Count.ToString(CultureInfo.InvariantCulture) + " /Kids [ " + kids + " ] >>");
            objects.Add("<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>");
            objects.Add("<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica-Bold >>");

            if (logoObjectNumber != 0)
            {
                var hexImage = ToHexString(logo.JpegBytes) + ">";
                objects.Add("<< /Type /XObject /Subtype /Image /Width " + logo.PixelWidth.ToString(CultureInfo.InvariantCulture) +
                    " /Height " + logo.PixelHeight.ToString(CultureInfo.InvariantCulture) +
                    " /ColorSpace /DeviceRGB /BitsPerComponent 8 /Filter [/ASCIIHexDecode /DCTDecode] /Length " + hexImage.Length.ToString(CultureInfo.InvariantCulture) +
                    " >>\nstream\n" + hexImage + "\nendstream");
            }

            for (var i = 0; i < pagesContent.Count; i++)
            {
                var imageResources = logoObjectNumber == 0 ? string.Empty : " /XObject << /Im1 " + logoObjectNumber.ToString(CultureInfo.InvariantCulture) + " 0 R >>";
                objects.Add("<< /Type /Page /Parent 2 0 R /MediaBox [0 0 595 842] /Resources << /Font << /F1 3 0 R /F2 4 0 R >>" + imageResources + " >> /Contents " + contentObjectNumbers[i].ToString(CultureInfo.InvariantCulture) + " 0 R >>");
                var streamBytes = PdfEncoding.GetBytes(pagesContent[i]);
                var stream = PdfEncoding.GetString(streamBytes);
                objects.Add("<< /Length " + streamBytes.Length.ToString(CultureInfo.InvariantCulture) + " >>\nstream\n" + stream + "\nendstream");
            }

            using (var memory = new MemoryStream())
            {
                WriteString(memory, "%PDF-1.4\n");
                var offsets = new List<long> { 0 };
                for (var index = 0; index < objects.Count; index++)
                {
                    offsets.Add(memory.Position);
                    WriteString(memory, (index + 1).ToString(CultureInfo.InvariantCulture) + " 0 obj\n");
                    WriteString(memory, objects[index]);
                    WriteString(memory, "\nendobj\n");
                }

                var xrefPosition = memory.Position;
                WriteString(memory, "xref\n0 " + (objects.Count + 1).ToString(CultureInfo.InvariantCulture) + "\n");
                WriteString(memory, "0000000000 65535 f \n");
                for (var i = 1; i < offsets.Count; i++)
                {
                    WriteString(memory, offsets[i].ToString("0000000000", CultureInfo.InvariantCulture) + " 00000 n \n");
                }

                WriteString(memory, "trailer\n<< /Size " + (objects.Count + 1).ToString(CultureInfo.InvariantCulture) + " /Root 1 0 R >>\n");
                WriteString(memory, "startxref\n" + xrefPosition.ToString(CultureInfo.InvariantCulture) + "\n%%EOF");
                return memory.ToArray();
            }
        }

        private static void AppendText(StringBuilder sb, string text, float x, float y, float fontSize, bool bold)
        {
            sb.Append("BT ");
            sb.Append(bold ? "/F2 " : "/F1 ");
            sb.Append(fontSize.ToString("0.##", CultureInfo.InvariantCulture));
            sb.Append(" Tf 1 0 0 1 ");
            sb.Append(x.ToString("0.##", CultureInfo.InvariantCulture));
            sb.Append(" ");
            sb.Append(y.ToString("0.##", CultureInfo.InvariantCulture));
            sb.Append(" Tm (");
            sb.Append(EscapePdfString(Safe(text)));
            sb.Append(") Tj ET\n");
        }

        private static void AppendCenteredText(StringBuilder sb, string text, float fontSize, bool bold, float y)
        {
            var width = Safe(text).Length * fontSize * (bold ? 0.56f : 0.52f);
            var x = (PageWidth - width) / 2f;
            AppendText(sb, text, x, y, fontSize, bold);
        }

        private static void DrawLine(StringBuilder sb, float x1, float y1, float x2, float y2)
        {
            sb.Append(x1.ToString("0.##", CultureInfo.InvariantCulture)).Append(" ")
              .Append(y1.ToString("0.##", CultureInfo.InvariantCulture)).Append(" m ")
              .Append(x2.ToString("0.##", CultureInfo.InvariantCulture)).Append(" ")
              .Append(y2.ToString("0.##", CultureInfo.InvariantCulture)).Append(" l S\n");
        }

        private static void DrawRectangle(StringBuilder sb, float x, float y, float width, float height)
        {
            sb.Append(x.ToString("0.##", CultureInfo.InvariantCulture)).Append(" ")
              .Append(y.ToString("0.##", CultureInfo.InvariantCulture)).Append(" ")
              .Append(width.ToString("0.##", CultureInfo.InvariantCulture)).Append(" ")
              .Append(height.ToString("0.##", CultureInfo.InvariantCulture)).Append(" re S\n");
        }

        private static void DrawLogo(StringBuilder sb, AssignmentSheetLogo logo, float x, float y)
        {
            var maxWidth = 110f;
            var maxHeight = 36f;
            var scale = Math.Min(maxWidth / logo.PixelWidth, maxHeight / logo.PixelHeight);
            var width = logo.PixelWidth * scale;
            var height = logo.PixelHeight * scale;

            sb.Append("q ")
              .Append(width.ToString("0.##", CultureInfo.InvariantCulture)).Append(" 0 0 ")
              .Append(height.ToString("0.##", CultureInfo.InvariantCulture)).Append(" ")
              .Append(x.ToString("0.##", CultureInfo.InvariantCulture)).Append(" ")
              .Append(y.ToString("0.##", CultureInfo.InvariantCulture)).Append(" cm /Im1 Do Q\n");
        }

        private static float[] GetColumnWidths()
        {
            return new[] { 60f, 165f, 85f, 95f, 110f };
        }

        private static float TotalTableWidth()
        {
            return PageWidth - MarginLeft - MarginRight;
        }

        private static string ToText(int? value)
        {
            return value.HasValue ? value.Value.ToString(CultureInfo.InvariantCulture) : string.Empty;
        }

        private static string Safe(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }

        private static string EscapePdfString(string value)
        {
            return value
                .Replace("\\", "\\\\")
                .Replace("(", "\\(")
                .Replace(")", "\\)")
                .Replace("\r", " ")
                .Replace("\n", " ");
        }

        private static void WriteString(Stream stream, string value)
        {
            var bytes = PdfEncoding.GetBytes(value);
            stream.Write(bytes, 0, bytes.Length);
        }

        private static string ToHexString(byte[] bytes)
        {
            var builder = new StringBuilder(bytes.Length * 2);
            for (var index = 0; index < bytes.Length; index++)
            {
                builder.Append(bytes[index].ToString("X2", CultureInfo.InvariantCulture));
            }

            return builder.ToString();
        }
    }
}
