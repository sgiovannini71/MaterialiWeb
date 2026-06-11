using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MaterialiGestioneWeb.Infrastructure;
using MaterialiGestioneWeb.Services;

namespace MaterialiGestioneWeb
{
    public partial class ImportaRetePage : Auth.BaseAuthenticatedPage
    {
        private readonly InventarioRepository _repository = new InventarioRepository();

        protected override int[] LivelliConsentiti
        {
            get { return new[] { (int)Auth.LivelliUtente.Operatore, (int)Auth.LivelliUtente.Amministratore }; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            Page.Form.Enctype = "multipart/form-data";
        }

        protected void ImportButton_Click(object sender, EventArgs e)
        {
            try
            {
                ErrorPanel.Visible = false;
                SuccessPanel.Visible = false;
                ResultPanel.Visible = false;

                if (!ReteFileUpload.HasFile)
                {
                    throw new InvalidOperationException("Selezionare un file TXT.");
                }

                var extension = Path.GetExtension(ReteFileUpload.FileName);
                if (!string.Equals(extension, ".txt", StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException("Il file deve avere estensione .txt.");
                }

                var items = ParseFile();
                if (items.Count == 0)
                {
                    throw new InvalidOperationException("Il file non contiene righe valide da elaborare.");
                }

                var results = _repository.ImportaReteDaNomiMacchina(items);
                ResultGrid.DataSource = results;
                ResultGrid.DataBind();
                ResultPanel.Visible = true;
                SuccessPanel.Visible = true;
                SuccessMessage.Text = Server.HtmlEncode(BuildSummary(results));
            }
            catch (Exception ex)
            {
                AppLogger.Error("ImportaRetePage.ImportButton_Click", "Errore durante import rete da TXT.", ex);
                ErrorPanel.Visible = true;
                ErrorMessage.Text = Server.HtmlEncode(ex.Message);
            }
        }

        private IList<InventarioRepository.ImportReteInputItem> ParseFile()
        {
            var items = new List<InventarioRepository.ImportReteInputItem>();
            using (var reader = new StreamReader(ReteFileUpload.FileContent))
            {
                var rowNumber = 0;
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    rowNumber++;
                    if (string.IsNullOrWhiteSpace(line))
                    {
                        continue;
                    }

                    var parts = SplitLine(line);
                    if (parts.Length < 2)
                    {
                        items.Add(new InventarioRepository.ImportReteInputItem
                        {
                            Riga = rowNumber,
                            NomeMacchina = line.Trim(),
                            MacAddress = string.Empty
                        });
                        continue;
                    }

                    if (IsHeader(parts))
                    {
                        continue;
                    }

                    items.Add(new InventarioRepository.ImportReteInputItem
                    {
                        Riga = rowNumber,
                        NomeMacchina = parts[0],
                        MacAddress = parts[1]
                    });
                }
            }

            return items;
        }

        private static string[] SplitLine(string line)
        {
            return line
                .Trim()
                .Split(new[] { '\t', ';', ',', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(part => part.Trim())
                .Where(part => part.Length > 0)
                .ToArray();
        }

        private static bool IsHeader(string[] parts)
        {
            return parts.Length >= 2
                && parts[0].IndexOf("nome", StringComparison.OrdinalIgnoreCase) >= 0
                && parts[1].IndexOf("mac", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static string BuildSummary(IList<InventarioRepository.ImportReteResultItem> results)
        {
            var updated = results.Count(item => string.Equals(item.Esito, "Aggiornata", StringComparison.OrdinalIgnoreCase));
            var skipped = results.Count - updated;
            return "Import completato. Aggiornate: " + updated + ". Scartate: " + skipped + ".";
        }
    }
}
