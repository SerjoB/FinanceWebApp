using System.Data;
using System.Globalization;
using System.Text.RegularExpressions;
using ExcelDataReader;
using FinanceWebApp.Models.DTOs;
using FinanceWebApp.Models.Enums;

namespace FinanceWebApp.Utilities;

public static class ParserUtility
{
    public static List<TransactionImportModel> ParseExcel(Stream fileStream)// Do not look at this abomination.
                                                                            // It's just for testing and so horrible because it does not matter right now
    {
        // ExcelDataReader requires this for some encodings
        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

        DataTable table;
        // Read the workbook synchronously (ExcelDataReader is sync)
        using (var reader = ExcelReaderFactory.CreateReader(fileStream))
        {
            var conf = new ExcelDataSetConfiguration
            {
                ConfigureDataTable = _ => new ExcelDataTableConfiguration
                {
                    UseHeaderRow = false // we'll detect header manually
                }
            };

            var ds = reader.AsDataSet(conf);
            table = ds.Tables.Count > 0 ? ds.Tables[0] : null;
        }

        var result = new List<TransactionImportModel>();
        if (table == null) return result;

        // header keywords (add variants your bank uses)
        string[] dateHeaders   = { "date", "transaction date", "дата", "дата операции", "operation date" };
        string[] amountHeaders = { "amount", "sum", "сумма", "сумма в валюте", "сумма в валюте счета" };
        string[] descHeaders   = { "description", "details", "memo", "описание", "назначение", "назначение платежа" };
        string[] catHeaders    = { "category", "категория", "категория расходов" };

        // helper to normalize header text
        static string NormalizeHeader(string s)
        {
            if (s == null) return string.Empty;
            // replace non-breaking spaces and collapse whitespace, lower-case
            var cleaned = s.Replace("\u00A0", " ")
                .Replace('\u2007', ' ')
                .Replace('\u202F', ' ')
                .Trim();
            cleaned = Regex.Replace(cleaned, @"\s+", " ");
            return cleaned.ToLowerInvariant();
        }

        // search for a header row among the first N rows
        int headerRowIndex = -1;
        int dateCol = -1, amountCol = -1, descCol = -1, catCol = -1;
        int maxRowsToSearch = Math.Min(20, table.Rows.Count);

        for (int r = 0; r < maxRowsToSearch; r++)
        {
            var row = table.Rows[r];
            int hits = 0;

            for (int c = 0; c < table.Columns.Count; c++)
            {
                var raw = row[c]?.ToString() ?? string.Empty;
                var norm = NormalizeHeader(raw);
                if (string.IsNullOrWhiteSpace(norm)) continue;

                if (dateCol == -1 && dateHeaders.Any(h => norm.Contains(h))) { dateCol = c; hits++; }
                if (amountCol == -1 && amountHeaders.Any(h => norm.Contains(h))) { amountCol = c; hits++; }
                if (descCol == -1 && descHeaders.Any(h => norm.Contains(h))) { descCol = c; hits++; }
                if (catCol == -1 && catHeaders.Any(h => norm.Contains(h))) { catCol = c; hits++; }
            }

            // if we found at least two header matches in this row, treat it as header
            if (hits >= 2)
            {
                headerRowIndex = r;
                break;
            }
        }

        // fallback: if not found, assume first used row is header (row 0)
        if (headerRowIndex == -1)
        {
            headerRowIndex = 0;

            // try to detect columns by inspecting header row content even if heuristics failed
            var headerRow = table.Rows[0];
            for (int c = 0; c < table.Columns.Count; c++)
            {
                var norm = NormalizeHeader(headerRow[c]?.ToString() ?? string.Empty);
                if (dateCol == -1 && dateHeaders.Any(h => norm.Contains(h))) dateCol = c;
                if (amountCol == -1 && amountHeaders.Any(h => norm.Contains(h))) amountCol = c;
                if (descCol == -1 && descHeaders.Any(h => norm.Contains(h))) descCol = c;
                if (catCol == -1 && catHeaders.Any(h => norm.Contains(h))) catCol = c;
            }
        }

        // If still missing essential columns (date or amount) try heuristics:
        if (dateCol == -1 || amountCol == -1)
        {
            // heuristic: search the whole table for cells that look like dates/amounts and pick columns
            for (int c = 0; c < table.Columns.Count; c++)
            {
                int dateMatches = 0, amountMatches = 0;
                int rowsToCheck = Math.Min(50, table.Rows.Count);
                for (int r = headerRowIndex + 1; r < headerRowIndex + 1 + rowsToCheck && r < table.Rows.Count; r++)
                {
                    var cell = table.Rows[r][c]?.ToString() ?? "";
                    if (DateTime.TryParse(cell, out _)) dateMatches++;
                    if (decimal.TryParse(cell.Replace(" ", "").Replace("\u00A0", ""), NumberStyles.Any, CultureInfo.InvariantCulture, out _)) amountMatches++;
                }

                if (dateCol == -1 && dateMatches > rowsToCheck / 4) dateCol = c;
                if (amountCol == -1 && amountMatches > rowsToCheck / 4) amountCol = c;
            }
        }

        // parse rows after header
        for (int r = headerRowIndex + 1; r < table.Rows.Count; r++)
        {
            var row = table.Rows[r];

            // helper to get cell text
            static string GetCellText(object? cell) => cell?.ToString()?.Trim() ?? string.Empty;

            // parse date
            DateTime date = DateTime.MinValue;
            if (dateCol != -1)
            {
                var dateRaw = GetCellText(row[dateCol]);
                if (!DateTime.TryParse(dateRaw, out date))
                {
                    // try parsing with invariant / specific formats
                    DateTime.TryParseExact(dateRaw, new[] { "dd.MM.yyyy", "MM/dd/yyyy", "yyyy-MM-dd" }, CultureInfo.InvariantCulture, DateTimeStyles.None, out date);
                }
            }

            // parse amount
            decimal amount = 0;
            if (amountCol != -1)
            {
                var amountRaw = GetCellText(row[amountCol])
                    .Replace("\u00A0", "") // remove NBSP
                    .Replace(" ", "")
                    .Replace(",", ".");    // try to normalize
                decimal.TryParse(amountRaw, NumberStyles.Any, CultureInfo.InvariantCulture, out amount);

                // as fallback try current culture
                if (amount == 0 && decimal.TryParse(GetCellText(row[amountCol]), NumberStyles.Any, CultureInfo.CurrentCulture, out var alt)) amount = alt;
            }

            var desc = descCol != -1 ? GetCellText(row[descCol]) : string.Empty;
            var cat  = catCol  != -1 ? GetCellText(row[catCol])  : null;

            // Skip empty / totals / junk rows
            if (date == DateTime.MinValue && amount == 0 && string.IsNullOrWhiteSpace(desc)) continue;

            result.Add(new TransactionImportModel
            {
                Date = date == DateTime.MinValue ? DateTime.MinValue : date,
                Amount = amount,
                Description = desc,
                ParsedCategoryName = string.IsNullOrWhiteSpace(cat) ? null : cat,
                TransactionType = amount < 0 ? TransactionType.Expense : TransactionType.Income
            });
        }
        return result;

    }
    
    public static List<TransactionImportModel> ParseCsv(Stream fileStream) 
    {
        throw new NotImplementedException();
    }
}