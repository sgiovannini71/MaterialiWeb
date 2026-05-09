using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
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
    public partial class ProdottiAssegnatiPage : System.Web.UI.Page
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
                BindPersonale();
                SetOutputAvailability(false);
            }
        }

        protected void TipoPersonaleRadio_SelectedIndexChanged(object sender, EventArgs e)
        {
            ResultPanel.Visible = false;
            PreviewPanel.Visible = false;
            SetOutputAvailability(false);
            BindPersonale();
        }

        protected void MostraNonAttiviCheck_CheckedChanged(object sender, EventArgs e)
        {
            ResultPanel.Visible = false;
            PreviewPanel.Visible = false;
            SetOutputAvailability(false);
            BindPersonale();
        }

        protected void SearchButton_Click(object sender, EventArgs e)
        {
            try
            {
                ErrorPanel.Visible = false;
                var idPersonale = ParseRequiredInt(PersonaleDropDown.SelectedValue, "Personale");
                var prodotti = _repository.GetProdottiAssegnati(idPersonale);
                ProdottiGrid.DataSource = prodotti;
                ProdottiGrid.DataBind();
                ResultTitle.Text = Server.HtmlEncode("Materiale assegnato a " + PersonaleDropDown.SelectedItem.Text);
                ResultPanel.Visible = true;
                PreviewPanel.Visible = false;
                SetOutputAvailability(prodotti != null && prodotti.Count > 0);
            }
            catch (Exception ex)
            {
                AppLogger.Error("ProdottiAssegnatiPage.SearchButton_Click", "Errore durante il caricamento prodotti assegnati.", ex);
                ResultPanel.Visible = false;
                SetOutputAvailability(false);
                ErrorPanel.Visible = true;
                ErrorMessage.Text = Server.HtmlEncode(ex.Message);
            }
        }

        protected void ResetButton_Click(object sender, EventArgs e)
        {
            ErrorPanel.Visible = false;
            ResultPanel.Visible = false;
            PreviewPanel.Visible = false;
            SetOutputAvailability(false);
            TipoPersonaleRadio.SelectedValue = "I";
            MostraNonAttiviCheck.Checked = false;
            BindPersonale();
        }

        protected void ExportCsvButton_Click(object sender, EventArgs e)
        {
            try
            {
                ErrorPanel.Visible = false;
                if (!ExportCsvButton.Enabled)
                {
                    throw new InvalidOperationException("Caricare prima dei dati da esportare.");
                }

                var idPersonale = ParseRequiredInt(PersonaleDropDown.SelectedValue, "Personale");
                ExportCsv(_repository.GetProdottiAssegnati(idPersonale), PersonaleDropDown.SelectedItem.Text);
            }
            catch (Exception ex)
            {
                AppLogger.Error("ProdottiAssegnatiPage.ExportCsvButton_Click", "Errore durante export prodotti assegnati.", ex);
                ErrorPanel.Visible = true;
                ErrorMessage.Text = Server.HtmlEncode(ex.Message);
            }
        }

        protected void PreviewSheetButton_Click(object sender, EventArgs e)
        {
            try
            {
                ErrorPanel.Visible = false;
                if (!PreviewSheetButton.Enabled)
                {
                    throw new InvalidOperationException("Caricare prima dei dati da visualizzare.");
                }

                var idPersonale = ParseRequiredInt(PersonaleDropDown.SelectedValue, "Personale");
                var prodotti = _repository.GetProdottiAssegnati(idPersonale);
                var personale = GetSelectedPersonale(idPersonale);
                var dataScheda = DateTime.Today;
                var model = BuildAssignmentSheetModel(prodotti, personale, dataScheda);

                PreviewHtmlLiteral.Text = RenderAssignmentSheetTemplate(model);
                PreviewPanel.Visible = true;
            }
            catch (Exception ex)
            {
                AppLogger.Error("ProdottiAssegnatiPage.PreviewSheetButton_Click", "Errore durante rendering anteprima scheda assegnazione.", ex);
                PreviewPanel.Visible = false;
                ErrorPanel.Visible = true;
                ErrorMessage.Text = Server.HtmlEncode(ex.Message);
            }
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

                var idPersonale = ParseRequiredInt(PersonaleDropDown.SelectedValue, "Personale");
                var prodotti = _repository.GetProdottiAssegnati(idPersonale);
                var personale = GetSelectedPersonale(idPersonale);
                var dataScheda = DateTime.Today;
                ExportPdf(prodotti, personale, dataScheda);
            }
            catch (Exception ex)
            {
                AppLogger.Error("ProdottiAssegnatiPage.ExportPdfButton_Click", "Errore durante export PDF prodotti assegnati.", ex);
                ErrorPanel.Visible = true;
                ErrorMessage.Text = Server.HtmlEncode(ex.Message);
            }
        }

        private void BindPersonale()
        {
            BindPersonale(IsPersonaleEsternoSelected(), MostraNonAttiviCheck.Checked);
        }

        private void BindPersonale(bool isEsterno, bool includeNonAttivi)
        {
            PersonaleDropDown.Items.Clear();
            PersonaleDropDown.Items.Add(new ListItem("-- seleziona personale --", string.Empty));
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

        private PersonaleLookupItem GetSelectedPersonale(int idPersonale)
        {
            var personale = _personaleRepository.GetPersonale(IsPersonaleEsternoSelected(), MostraNonAttiviCheck.Checked)
                .FirstOrDefault(item => item.Id == idPersonale);

            if (personale != null)
            {
                return personale;
            }

            var rawDisplayName = NormalizeSelectedPersonaleText(PersonaleDropDown.SelectedItem == null ? null : PersonaleDropDown.SelectedItem.Text);
            var parts = SplitDisplayName(rawDisplayName);
            return new PersonaleLookupItem
            {
                Id = idPersonale,
                Cognome = parts.cognome,
                Nome = parts.nome
            };
        }

        private void ExportCsv(IList<ProdottoCorrente> prodotti, string personale)
        {
            CsvExport.Write(this, "ProdottiAssegnati", new[]
            {
                "Personale",
                "Categorico",
                "Descrizione",
                "Matricola",
                "Categoria",
                "Modello",
                "Stato",
                "Stanza",
                "Nome macchina",
                "MAC"
            }, BuildCsvRows(prodotti, personale));
        }

        private static IEnumerable<IEnumerable<string>> BuildCsvRows(IEnumerable<ProdottoCorrente> prodotti, string personale)
        {
            foreach (var prodotto in prodotti)
            {
                yield return new[]
                {
                    personale,
                    prodotto.Categorico.HasValue ? prodotto.Categorico.Value.ToString(CultureInfo.InvariantCulture) : string.Empty,
                    prodotto.DescrizioneProdotto,
                    prodotto.Matricola,
                    prodotto.Categoria,
                    prodotto.Modello,
                    prodotto.LivelloEfficienza,
                    prodotto.NumeroStanza,
                    prodotto.NomeMacchina,
                    prodotto.MacAddress
                };
            }
        }

        private void ExportPdf(IList<ProdottoCorrente> prodotti, PersonaleLookupItem personale, DateTime dataScheda)
        {
            var model = BuildAssignmentSheetModel(prodotti, personale, dataScheda);
            var bytes = AssignmentSheetPdfExporter.Create(model);
            var fileName = string.Format(
                CultureInfo.InvariantCulture,
                "SchedaAssegnazione_{0}_{1}_{2:yyyyMMdd}.pdf",
                SanitizeFileName(model.Cognome),
                SanitizeFileName(model.Nome),
                model.Data);

            Response.Clear();
            Response.ContentType = "application/pdf";
            Response.AddHeader("content-disposition", "attachment; filename=\"" + fileName + "\"");
            Response.BinaryWrite(bytes);
            Response.Flush();
            Context.ApplicationInstance.CompleteRequest();
        }

        private AssignmentSheetModel BuildAssignmentSheetModel(IList<ProdottoCorrente> prodotti, PersonaleLookupItem personale, DateTime dataScheda)
        {
            return new AssignmentSheetModel
            {
                Ente = GetConfiguredHeaderText(),
                Nome = personale.Nome,
                Cognome = personale.Cognome,
                Data = dataScheda,
                Prodotti = prodotti ?? new List<ProdottoCorrente>(),
                Logo = LoadLogo(),
                LogoVirtualPath = GetConfiguredLogoVirtualPath()
            };
        }

        private void SetOutputAvailability(bool enabled)
        {
            PreviewSheetButton.Enabled = enabled;
            ExportCsvButton.Enabled = enabled;
            ExportPdfButton.Enabled = enabled;
        }

        private static int ParseRequiredInt(string value, string fieldName)
        {
            int parsed;
            if (!int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out parsed))
            {
                throw new InvalidOperationException(fieldName + " non valido.");
            }

            return parsed;
        }

        private static string NormalizeSelectedPersonaleText(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            return value.Replace(" [non attivo]", string.Empty).Trim();
        }

        private static (string cognome, string nome) SplitDisplayName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return (string.Empty, string.Empty);
            }

            var parts = value.Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 1)
            {
                return (parts[0], string.Empty);
            }

            return (parts[0], parts[1]);
        }

        private static string SanitizeFileName(string value)
        {
            var fallback = string.IsNullOrWhiteSpace(value) ? "Utente" : value.Trim();
            foreach (var invalid in Path.GetInvalidFileNameChars())
            {
                fallback = fallback.Replace(invalid, '_');
            }

            return fallback.Replace(' ', '_');
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

        private string RenderAssignmentSheetTemplate(AssignmentSheetModel model)
        {
            var relativePath = ConfigurationManager.AppSettings["AssignmentSheetTemplatePath"];
            var templatePath = string.IsNullOrWhiteSpace(relativePath)
                ? Server.MapPath("~/html/AssignmentSheetTemplate.html")
                : Server.MapPath("~/" + relativePath.Replace('\\', '/').TrimStart('/'));

            if (!File.Exists(templatePath))
            {
                throw new InvalidOperationException("Template scheda assegnazione non trovato.");
            }

            var template = File.ReadAllText(templatePath);
            return template
                .Replace("{{LOGO_HTML}}", BuildLogoHtml(model.LogoVirtualPath))
                .Replace("{{HEADER_TEXT}}", Server.HtmlEncode(model.Ente))
                .Replace("{{COGNOME}}", Server.HtmlEncode(model.Cognome))
                .Replace("{{NOME}}", Server.HtmlEncode(model.Nome))
                .Replace("{{DATA}}", Server.HtmlEncode(model.Data.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)))
                .Replace("{{PRODUCT_ROWS}}", BuildProductRowsHtml(model.Prodotti));
        }

        private string BuildProductRowsHtml(IEnumerable<ProdottoCorrente> prodotti)
        {
            var builder = new StringBuilder();
            var hasRows = false;

            foreach (var prodotto in prodotti ?? Enumerable.Empty<ProdottoCorrente>())
            {
                hasRows = true;
                builder.Append("<tr>")
                    .Append("<td>").Append(Server.HtmlEncode(ToText(prodotto.Categorico))).Append("</td>")
                    .Append("<td>").Append(Server.HtmlEncode(prodotto.DescrizioneProdotto)).Append("</td>")
                    .Append("<td>").Append(Server.HtmlEncode(prodotto.Matricola)).Append("</td>")
                    .Append("<td>").Append(Server.HtmlEncode(prodotto.Categoria)).Append("</td>")
                    .Append("<td>").Append(Server.HtmlEncode(prodotto.Modello)).Append("</td>")
                    .Append("</tr>");
            }

            if (!hasRows)
            {
                builder.Append("<tr><td colspan=\"5\" class=\"assignment-sheet-empty\">Nessun prodotto assegnato.</td></tr>");
            }

            return builder.ToString();
        }

        private string BuildLogoHtml(string virtualPath)
        {
            if (string.IsNullOrWhiteSpace(virtualPath))
            {
                return string.Empty;
            }

            return "<img class=\"assignment-sheet-logo\" src=\"" + ResolveUrl(virtualPath) + "\" alt=\"Logo ente\" />";
        }

        private static string GetConfiguredHeaderText()
        {
            var configured = ConfigurationManager.AppSettings["AssignmentSheetHeaderText"];
            return string.IsNullOrWhiteSpace(configured) ? "DNA - DAAAA" : configured.Trim();
        }

        private static string GetConfiguredLogoVirtualPath()
        {
            var configured = ConfigurationManager.AppSettings["AssignmentSheetLogoPath"];
            if (string.IsNullOrWhiteSpace(configured))
            {
                return null;
            }

            return "~/" + configured.Replace('\\', '/').TrimStart('/');
        }

        private static string ToText(int? value)
        {
            return value.HasValue ? value.Value.ToString(CultureInfo.InvariantCulture) : string.Empty;
        }
    }

    internal sealed class AssignmentSheetModel
    {
        public string Ente { get; set; }
        public string Nome { get; set; }
        public string Cognome { get; set; }
        public DateTime Data { get; set; }
        public IList<ProdottoCorrente> Prodotti { get; set; }
        public AssignmentSheetLogo Logo { get; set; }
        public string LogoVirtualPath { get; set; }
    }

    internal sealed class AssignmentSheetLogo
    {
        public byte[] JpegBytes { get; set; }
        public int PixelWidth { get; set; }
        public int PixelHeight { get; set; }
    }

    internal static class AssignmentSheetPdfExporter
    {
        private const float PageWidth = 595f;
        private const float PageHeight = 842f;
        private const float MarginLeft = 40f;
        private const float MarginRight = 40f;
        private const float MarginTop = 48f;
        private const float MarginBottom = 48f;
        private const float TableHeaderHeight = 22f;
        private static readonly Encoding PdfEncoding = Encoding.GetEncoding(1252);

        private static List<string> BuildPages(AssignmentSheetModel model)
        {
            var pages = new List<string>();
            var current = new StringBuilder();
            var y = PageHeight - MarginTop;

            AppendHeader(current, model, ref y, includeLogo: true);
            y -= 8f;
            AppendTableHeader(current, ref y);

            foreach (var prodotto in model.Prodotti)
            {
                var row = BuildRow(prodotto);
                var rowHeight = MeasureRowHeight(row);
                if (y - rowHeight < MarginBottom + 90f)
                {
                    pages.Add(current.ToString());
                    current.Clear();
                    y = PageHeight - MarginTop;
                    AppendContinuationHeader(current, model, ref y);
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
                AppendContinuationHeader(current, model, ref y);
            }

            AppendSignature(current, model, ref y);
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
            var maxLines = row.Max(cell => Math.Max(cell.Length, 1));
            return 8f + (maxLines * 11f);
        }

        private static void AppendHeader(StringBuilder sb, AssignmentSheetModel model, ref float y, bool includeLogo)
        {
            if (includeLogo && model.Logo != null && model.Logo.JpegBytes != null && model.Logo.JpegBytes.Length > 0)
            {
                DrawLogo(sb, model.Logo, MarginLeft, y - 36f);
            }

            AppendCenteredText(sb, Safe(model.Ente).ToUpperInvariant(), 14f, true, y);
            y -= 26f;
            AppendCenteredText(sb, "Scheda prodotti assegnati", 13f, true, y);
            y -= 28f;

            AppendText(sb, "Cognome: " + Safe(model.Cognome), MarginLeft, y, 11f, true);
            AppendText(sb, "Nome: " + Safe(model.Nome), 290f, y, 11f, true);
            y -= 20f;
            AppendText(sb, "Data: " + model.Data.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture), MarginLeft, y, 11f, false);
            y -= 24f;
        }

        private static void AppendContinuationHeader(StringBuilder sb, AssignmentSheetModel model, ref float y)
        {
            AppendHeader(sb, model, ref y, includeLogo: false);
            AppendCenteredText(sb, "Continua", 11f, false, y + 10f);
            y -= 22f;
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
                foreach (var line in row[index])
                {
                    AppendText(sb, line, x + 4f, lineY, 9f, false);
                    lineY -= 11f;
                }

                x += widths[index];
            }

            y -= rowHeight;
        }

        private static void AppendSignature(StringBuilder sb, AssignmentSheetModel model, ref float y)
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

        private static byte[] BuildPdf(IList<string> pagesContent)
        {
            return BuildPdf(pagesContent, null);
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

        public static byte[] Create(AssignmentSheetModel model)
        {
            var pages = BuildPages(model);
            return BuildPdf(pages, model.Logo);
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
            var width = EstimateTextWidth(text, fontSize, bold);
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

        private static float EstimateTextWidth(string text, float fontSize, bool bold)
        {
            var factor = bold ? 0.56f : 0.52f;
            return Safe(text).Length * fontSize * factor;
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
