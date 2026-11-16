

// -----------------------------------------------------------------------------
// 3) UtilityLibrary.Excel/Errors/ExcelErrorManager.cs
//    - Centralized error collection & workbook styling integration
// -----------------------------------------------------------------------------
using ClosedXML.Excel;

namespace UtilityLibrary.Excel
{
    public class ExcelErrorManager
    {
        private readonly List<ExcelErrorDTO> _errors = new();
        public IReadOnlyList<ExcelErrorDTO> Errors => _errors;
        public void AddError(string property, int? row, int? col, string message, eExcelErrorSeverity sev, string? cellAddr = null)
        {
            _errors.Add(new ExcelErrorDTO
            {
                ColumnName = property,
                RowIndex = row,
                ColumnIndex = col,
                ErrorMessage = message,
                Severity = sev,
                CellAddress = cellAddr
            });
        }
        public void Add(ExcelErrorDTO e) => _errors.Add(e);
        public void AddRange(IEnumerable<ExcelErrorDTO> es) { _errors.AddRange(es); }

        public bool AnyCritical() => _errors.Any(e => e.IsCritical);

        // Apply errors into a workbook (coloring + IsErr/ErrDesc columns)
        public byte[] ApplyErrorsToWorkbook(byte[] originalBytes)
        {
            if (originalBytes == null || _errors.Count == 0) return originalBytes!;

            using var stream = new MemoryStream(originalBytes);
            using var wb = new XLWorkbook(stream);
            var ws = wb.Worksheets.FirstOrDefault();
            if (ws == null) return originalBytes;

            // Ensure IsErr/ErrDesc columns
            int lastCol = ws.LastColumnUsed()?.ColumnNumber() ?? 0;
            int isErrCol = lastCol + 1;
            ws.Cell(1, isErrCol).Value = "IsErr";
            ws.Cell(1, isErrCol + 1).Value = "ErrDesc";

            var errDescByRow = new Dictionary<int, List<string>>();

            foreach (var err in _errors)
            {
                if (err.RowIndex is int r && err.ColumnIndex is int c)
                {
                    var cell = ws.Cell(r, c);
                    if (cell.Style.Fill.BackgroundColor.Color == default)
                    {
                        switch (err.Severity)
                        {
                            case eExcelErrorSeverity.ParseErr:
                            case eExcelErrorSeverity.ValidateEntityAutoMapper:
                                cell.Style.Fill.BackgroundColor = XLColor.Red; break;
                            case eExcelErrorSeverity.ExternalErr:
                                cell.Style.Fill.BackgroundColor = XLColor.Orange; break;
                            case eExcelErrorSeverity.Critical:
                                cell.Style.Fill.BackgroundColor = XLColor.DarkRed; break;
                        }
                    }

                    if (!errDescByRow.TryGetValue(r, out var list))
                    {
                        list = new List<string>();
                        errDescByRow[r] = list;
                    }
                    var colName = string.IsNullOrWhiteSpace(err.ColumnName) ? $"C{c}" : err.ColumnName;
                    list.Add($"[{colName}] {err.ErrorMessage}");
                }
            }

            foreach (var kv in errDescByRow)
            {
                ws.Cell(kv.Key, isErrCol).Value = 1;
                ws.Cell(kv.Key, isErrCol + 1).Value = string.Join(" | ", kv.Value);
            }

            ws.Column(isErrCol).AdjustToContents();
            ws.Column(isErrCol + 1).AdjustToContents();
            var range = ws.Range(1, 1, ws.LastRowUsed()?.RowNumber() ?? 1, ws.LastColumnUsed()?.ColumnNumber() ?? isErrCol + 1);
            range.SetAutoFilter().Column(isErrCol).AddFilter("1");

            using var outStream = new MemoryStream();
            wb.SaveAs(outStream);
            return outStream.ToArray();
        }
    }
}
