using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Web;
using System.Web.UI;

namespace MaterialiGestioneWeb.Infrastructure
{
    public static class CsvExport
    {
        public static void Write(Page page, string baseFileName, IEnumerable<string> headers, IEnumerable<IEnumerable<string>> rows)
        {
            var builder = new StringBuilder();
            AppendRow(builder, headers);

            foreach (var row in rows)
            {
                AppendRow(builder, row);
            }

            var fileName = baseFileName + "_" + DateTime.Now.ToString("yyyyMMdd_HHmm", CultureInfo.InvariantCulture) + ".csv";
            page.Response.Clear();
            page.Response.Buffer = true;
            page.Response.ContentEncoding = Encoding.UTF8;
            page.Response.ContentType = "text/csv";
            page.Response.AddHeader("Content-Disposition", "attachment; filename=" + fileName);
            page.Response.BinaryWrite(Encoding.UTF8.GetPreamble());
            page.Response.Write(builder.ToString());
            page.Response.Flush();
            page.Response.SuppressContent = true;
            HttpContext.Current.ApplicationInstance.CompleteRequest();
        }

        private static void AppendRow(StringBuilder builder, IEnumerable<string> values)
        {
            var first = true;
            foreach (var value in values)
            {
                if (!first)
                {
                    builder.Append(';');
                }

                builder.Append(Escape(value));
                first = false;
            }

            builder.AppendLine();
        }

        private static string Escape(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            var normalized = value.Replace("\r\n", " ").Replace("\r", " ").Replace("\n", " ");
            if (normalized.IndexOfAny(new[] { ';', '"', '\r', '\n' }) < 0)
            {
                return normalized;
            }

            return "\"" + normalized.Replace("\"", "\"\"") + "\"";
        }
    }
}
