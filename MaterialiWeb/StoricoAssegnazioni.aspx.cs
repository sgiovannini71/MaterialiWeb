using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.UI.WebControls;
using MaterialiGestioneWeb.Infrastructure;
using MaterialiGestioneWeb.Models;
using MaterialiGestioneWeb.Services;

namespace MaterialiGestioneWeb
{
    public partial class StoricoAssegnazioniPage : System.Web.UI.Page
    {
        private readonly InventarioRepository _repository = new InventarioRepository();
        private readonly PersonaleRepository _personaleRepository = new PersonaleRepository();

        protected void Page_Init(object sender, EventArgs e)
        {
            if (IsPostBack)
            {
                BindPersonale(GetPostedIsPersonaleEsternoSelected(), GetPostedIncludeNonAttivi());
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                BindProdottiByCategorico(null, null);
                BindPersonale();
                SetExportAvailability(false);
            }
        }

        protected void CategoricoText_TextChanged(object sender, EventArgs e)
        {
            FilterProducts();
        }

        protected void FiltraProdottoButton_Click(object sender, EventArgs e)
        {
            FilterProducts();
        }

        protected void TipoPersonaleRadio_SelectedIndexChanged(object sender, EventArgs e)
        {
            ResultPanel.Visible = false;
            SetExportAvailability(false);
            BindPersonale();
        }

        protected void MostraNonAttiviCheck_CheckedChanged(object sender, EventArgs e)
        {
            ResultPanel.Visible = false;
            SetExportAvailability(false);
            BindPersonale();
        }

        protected void SearchButton_Click(object sender, EventArgs e)
        {
            SearchStorico();
        }

        protected void ResetButton_Click(object sender, EventArgs e)
        {
            ErrorPanel.Visible = false;
            ResultPanel.Visible = false;
            SetExportAvailability(false);
            CategoricoText.Text = string.Empty;
            TipoPersonaleRadio.SelectedValue = "I";
            MostraNonAttiviCheck.Checked = false;
            BindProdottiByCategorico(null, null);
            BindPersonale();
        }

        protected void ExportPdfButton_Click(object sender, EventArgs e)
        {
            try
            {
                ErrorPanel.Visible = false;
                if (!ExportPdfButton.Enabled)
                {
                    throw new InvalidOperationException("Caricare prima dei dati da esportare.");
                }

                var idProdotto = ParseOptionalInt(ProdottoDropDown.SelectedValue);
                var idPersonale = ParseOptionalInt(PersonaleDropDown.SelectedValue);
                ExportPdf(_repository.GetStoricoAssegnazioni(CategoricoText.Text, idProdotto, idPersonale));
            }
            catch (Exception ex)
            {
                AppLogger.Error("StoricoAssegnazioniPage.ExportPdfButton_Click", "Errore durante export PDF storico assegnazioni.", ex);
                ErrorPanel.Visible = true;
                ErrorMessage.Text = Server.HtmlEncode(ex.Message);
            }
        }

        private void SearchStorico()
        {
            try
            {
                ErrorPanel.Visible = false;
                var idProdotto = ParseOptionalInt(ProdottoDropDown.SelectedValue);
                var idPersonale = ParseOptionalInt(PersonaleDropDown.SelectedValue);
                var storico = _repository.GetStoricoAssegnazioni(CategoricoText.Text, idProdotto, idPersonale);
                StoricoGrid.DataSource = storico;
                StoricoGrid.DataBind();
                ResultTitle.Text = Server.HtmlEncode(BuildResultTitle(storico == null ? 0 : storico.Count));
                ResultPanel.Visible = true;
                SetExportAvailability(storico != null && storico.Count > 0);
            }
            catch (Exception ex)
            {
                AppLogger.Error("StoricoAssegnazioniPage.SearchStorico", "Errore durante la ricerca storico assegnazioni.", ex);
                ResultPanel.Visible = false;
                SetExportAvailability(false);
                ErrorPanel.Visible = true;
                ErrorMessage.Text = Server.HtmlEncode(ex.Message);
            }
        }

        private void FilterProducts()
        {
            ErrorPanel.Visible = false;
            ResultPanel.Visible = false;
            SetExportAvailability(false);
            BindProdottiByCategorico(CategoricoText.Text, null);
            SearchStorico();
        }

        private void BindProdottiByCategorico(string categoricoFilter, int? selectedId)
        {
            var items = string.IsNullOrWhiteSpace(categoricoFilter)
                ? new LookupItem[0]
                : _repository.GetProdottiLookupByCategorico(categoricoFilter);
            BindDropDown(ProdottoDropDown, items, "-- tutti i prodotti filtrati --", "DisplayName");
            SelectDropDownValue(ProdottoDropDown, selectedId);
        }

        private void BindPersonale()
        {
            BindPersonale(IsPersonaleEsternoSelected(), MostraNonAttiviCheck.Checked);
        }

        private void BindPersonale(bool isEsterno, bool includeNonAttivi)
        {
            PersonaleDropDown.Items.Clear();
            PersonaleDropDown.Items.Add(new ListItem("-- tutti gli assegnatari --", string.Empty));
            PersonaleDropDown.AppendDataBoundItems = true;
            PersonaleDropDown.DataSource = _personaleRepository.GetPersonale(isEsterno, includeNonAttivi);
            PersonaleDropDown.DataTextField = "DisplayName";
            PersonaleDropDown.DataValueField = "Id";
            PersonaleDropDown.DataBind();
            PersonaleDropDown.AppendDataBoundItems = false;
        }

        private bool IsPersonaleEsternoSelected()
        {
            return TipoPersonaleRadio.SelectedValue == "E";
        }

        private bool GetPostedIsPersonaleEsternoSelected()
        {
            return string.Equals(Request.Form[TipoPersonaleRadio.UniqueID], "E", StringComparison.OrdinalIgnoreCase);
        }

        private bool GetPostedIncludeNonAttivi()
        {
            return !string.IsNullOrEmpty(Request.Form[MostraNonAttiviCheck.UniqueID]);
        }

        private static void BindDropDown(ListControl control, object dataSource, string emptyText, string textField)
        {
            control.Items.Clear();
            control.Items.Add(new ListItem(emptyText, string.Empty));
            control.AppendDataBoundItems = true;
            control.DataSource = dataSource;
            control.DataTextField = textField;
            control.DataValueField = "Id";
            control.DataBind();
            control.AppendDataBoundItems = false;
        }

        private static void SelectDropDownValue(ListControl control, int? selectedId)
        {
            if (!selectedId.HasValue)
            {
                return;
            }

            var selectedValue = selectedId.Value.ToString(CultureInfo.InvariantCulture);
            if (control.Items.FindByValue(selectedValue) != null)
            {
                control.SelectedValue = selectedValue;
            }
        }

        private static int? ParseOptionalInt(string value)
        {
            int parsed;
            return int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out parsed) ? (int?)parsed : null;
        }

        private void SetExportAvailability(bool enabled)
        {
            ExportPdfButton.Enabled = enabled;
        }

        private string BuildResultTitle(int count)
        {
            var filter = NormalizeFilterValue(CategoricoText.Text);
            var suffix = string.IsNullOrWhiteSpace(filter)
                ? string.Empty
                : " per categorico " + filter;
            return count == 1
                ? "1 risultato storico" + suffix
                : count.ToString("N0", CultureInfo.InvariantCulture) + " risultati storico" + suffix;
        }

        private void ExportPdf(IList<StoricoAssegnazioneConsultazioneItem> storico)
        {
            EnsureHistoryTemplateExists();

            var model = new HistoryReportModel
            {
                Ente = GetConfiguredHeaderText(),
                Data = DateTime.Today,
                CategoricoFiltro = NormalizeFilterValue(CategoricoText.Text),
                ProdottoFiltro = GetSelectedDropDownText(ProdottoDropDown, "-- tutti i prodotti filtrati --"),
                AssegnatarioFiltro = GetSelectedDropDownText(PersonaleDropDown, "-- tutti gli assegnatari --"),
                TipoPersonale = TipoPersonaleRadio.SelectedValue == "E" ? "Esterno" : "Interno",
                IncludeNonAttivi = MostraNonAttiviCheck.Checked,
                Items = storico ?? new List<StoricoAssegnazioneConsultazioneItem>(),
                Logo = LoadLogo()
            };

            var bytes = HistoryReportPdfExporter.Create(model);
            Response.Clear();
            Response.ContentType = "application/pdf";
            Response.AddHeader("content-disposition", "attachment; filename=\"StoricoAssegnazioni.pdf\"");
            Response.BinaryWrite(bytes);
            Response.Flush();
            Context.ApplicationInstance.CompleteRequest();
        }

        private void EnsureHistoryTemplateExists()
        {
            var relativePath = ConfigurationManager.AppSettings["HistoryReportTemplatePath"];
            var templatePath = string.IsNullOrWhiteSpace(relativePath)
                ? Server.MapPath("~/html/StoricoAssegnazioniTemplate.html")
                : Server.MapPath("~/" + relativePath.Replace('\\', '/').TrimStart('/'));

            if (!File.Exists(templatePath))
            {
                throw new InvalidOperationException("Template report storico non trovato.");
            }
        }

        private AssignmentSheetLogo LoadLogo()
        {
            var configuredPath = ConfigurationManager.AppSettings["AssignmentSheetLogoPath"];
            if (string.IsNullOrWhiteSpace(configuredPath))
            {
                return null;
            }

            var absolutePath = Server.MapPath("~/" + configuredPath.Replace('\\', '/').TrimStart('/'));
            if (!File.Exists(absolutePath))
            {
                return null;
            }

            using (var image = System.Drawing.Image.FromFile(absolutePath))
            using (var stream = new MemoryStream())
            {
                image.Save(stream, ImageFormat.Jpeg);
                return new AssignmentSheetLogo
                {
                    JpegBytes = stream.ToArray(),
                    PixelWidth = image.Width,
                    PixelHeight = image.Height
                };
            }
        }

        private static string GetConfiguredHeaderText()
        {
            var configured = ConfigurationManager.AppSettings["AssignmentSheetHeaderText"];
            return string.IsNullOrWhiteSpace(configured) ? "DNA - DAAAA" : configured.Trim();
        }

        private static string NormalizeFilterValue(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? "Tutti" : value.Trim();
        }

        private static string GetSelectedDropDownText(ListControl control, string fallback)
        {
            if (control == null || control.SelectedItem == null || string.IsNullOrWhiteSpace(control.SelectedValue))
            {
                return fallback;
            }

            return control.SelectedItem.Text;
        }
    }

    internal sealed class HistoryReportModel
    {
        public string Ente { get; set; }
        public DateTime Data { get; set; }
        public string CategoricoFiltro { get; set; }
        public string ProdottoFiltro { get; set; }
        public string AssegnatarioFiltro { get; set; }
        public string TipoPersonale { get; set; }
        public bool IncludeNonAttivi { get; set; }
        public IList<StoricoAssegnazioneConsultazioneItem> Items { get; set; }
        public AssignmentSheetLogo Logo { get; set; }
    }

    internal static class HistoryReportPdfExporter
    {
        private const float PageWidth = 595f;
        private const float PageHeight = 842f;
        private const float MarginLeft = 28f;
        private const float MarginRight = 28f;
        private const float MarginTop = 42f;
        private const float MarginBottom = 42f;
        private const float HeaderHeight = 24f;
        private static readonly Encoding PdfEncoding = Encoding.GetEncoding(1252);

        public static byte[] Create(HistoryReportModel model)
        {
            var pages = BuildPages(model);
            return BuildPdf(pages, model.Logo);
        }

        private static List<string> BuildPages(HistoryReportModel model)
        {
            var pages = new List<string>();
            var current = new StringBuilder();
            var y = PageHeight - MarginTop;

            AppendHeader(current, model, ref y, true);
            y -= 6f;
            AppendTableHeader(current, ref y);

            foreach (var item in model.Items ?? new List<StoricoAssegnazioneConsultazioneItem>())
            {
                var row = BuildRow(item);
                var rowHeight = MeasureRowHeight(row);
                if (y - rowHeight < MarginBottom + 24f)
                {
                    pages.Add(current.ToString());
                    current.Clear();
                    y = PageHeight - MarginTop;
                    AppendHeader(current, model, ref y, false);
                    y -= 6f;
                    AppendTableHeader(current, ref y);
                }

                AppendTableRow(current, row, ref y, rowHeight);
            }

            if ((model.Items == null || model.Items.Count == 0) && y - 26f >= MarginBottom)
            {
                AppendText(current, "Nessun movimento storico trovato.", MarginLeft + 4f, y - 18f, 10f, false);
                y -= 28f;
            }

            pages.Add(current.ToString());
            return pages;
        }

        private static string[][] BuildRow(StoricoAssegnazioneConsultazioneItem item)
        {
            return new[]
            {
                WrapText(ToText(item.Categorico), 34f, 8f),
                WrapText(item.DescrizioneProdotto, 82f, 8f),
                WrapText(item.Matricola, 52f, 8f),
                WrapText(item.AssegnatarioDisplay, 62f, 8f),
                WrapText(ToDate(item.DataAssegnazione), 42f, 8f),
                WrapText(ToDate(item.DataRestituzione), 42f, 8f),
                WrapText(item.NumeroStanza, 32f, 8f),
                WrapText(item.LivelloEfficienza, 32f, 8f),
                WrapText(item.NomeMacchina, 40f, 8f),
                WrapText(item.SerialNumber, 41f, 8f)
            };
        }

        private static float MeasureRowHeight(IReadOnlyList<string[]> row)
        {
            var maxLines = row.Max(cell => Math.Max(cell.Length, 1));
            return 7f + (maxLines * 10f);
        }

        private static void AppendHeader(StringBuilder sb, HistoryReportModel model, ref float y, bool includeLogo)
        {
            if (includeLogo && model.Logo != null && model.Logo.JpegBytes != null && model.Logo.JpegBytes.Length > 0)
            {
                DrawLogo(sb, model.Logo, MarginLeft, y - 34f);
            }

            AppendCenteredText(sb, Safe(model.Ente).ToUpperInvariant(), 14f, true, y);
            y -= 24f;
            AppendCenteredText(sb, "Report storico assegnazioni", 12f, true, y);
            y -= 24f;

            AppendText(sb, "Data: " + model.Data.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture), MarginLeft, y, 10f, false);
            AppendText(sb, "Categorico: " + Safe(model.CategoricoFiltro), 180f, y, 10f, false);
            AppendText(sb, "Prodotto: " + Safe(model.ProdottoFiltro), 360f, y, 10f, false);
            y -= 16f;
            AppendText(sb, "Assegnatario: " + Safe(model.AssegnatarioFiltro), MarginLeft, y, 10f, false);
            AppendText(sb, "Tipo personale: " + Safe(model.TipoPersonale), 280f, y, 10f, false);
            AppendText(sb, "Non attivi: " + (model.IncludeNonAttivi ? "Si" : "No"), 440f, y, 10f, false);
            y -= 20f;
        }

        private static void AppendTableHeader(StringBuilder sb, ref float y)
        {
            var widths = GetColumnWidths();
            var headers = new[] { "Cat.", "Descrizione", "Matricola", "Assegnatario", "Data ass.", "Data rest.", "Stanza", "Stato", "Macchina", "Seriale" };
            var x = MarginLeft;
            DrawRectangle(sb, MarginLeft, y - HeaderHeight, TotalTableWidth(), HeaderHeight);

            for (var i = 0; i < widths.Length - 1; i++)
            {
                x += widths[i];
                DrawLine(sb, x, y, x, y - HeaderHeight);
            }

            x = MarginLeft;
            for (var index = 0; index < headers.Length; index++)
            {
                AppendText(sb, headers[index], x + 3f, y - 14f, 8f, true);
                x += widths[index];
            }

            y -= HeaderHeight;
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
                var lineY = y - 12f;
                foreach (var line in row[index])
                {
                    AppendText(sb, line, x + 3f, lineY, 8f, false);
                    lineY -= 10f;
                }

                x += widths[index];
            }

            y -= rowHeight;
        }

        private static string[] WrapText(string value, float width, float fontSize)
        {
            var normalized = Safe(value);
            if (string.IsNullOrWhiteSpace(normalized))
            {
                return new[] { string.Empty };
            }

            var maxChars = Math.Max(1, (int)Math.Floor(width / (fontSize * 0.55f)));
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

            var kids = string.Join(" ", pageObjectNumbers.Select(number => number.ToString(CultureInfo.InvariantCulture) + " 0 R").ToArray());
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
            var maxHeight = 34f;
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
            return new[] { 36f, 84f, 54f, 64f, 44f, 44f, 34f, 34f, 42f, 43f };
        }

        private static float TotalTableWidth()
        {
            return PageWidth - MarginLeft - MarginRight;
        }

        private static string ToText(int? value)
        {
            return value.HasValue ? value.Value.ToString(CultureInfo.InvariantCulture) : string.Empty;
        }

        private static string ToDate(DateTime? value)
        {
            return value.HasValue ? value.Value.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture) : string.Empty;
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
