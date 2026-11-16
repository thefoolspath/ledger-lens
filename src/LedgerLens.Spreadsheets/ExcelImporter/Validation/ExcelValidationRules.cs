
// -----------------------------------------------------------------------------
// 7) UtilityLibrary.Excel/Validation/ExcelValidationRules.cs
//    - Custom rules extension point (business-level validation)
// -----------------------------------------------------------------------------
namespace UtilityLibrary.Excel
{
    public static class ExcelValidationRules
    {
        private static readonly HashSet<string> BitColumns = new(StringComparer.OrdinalIgnoreCase) { "is_active", "is_delete", "is_system" };

        public static void ValidateBitColumns<T>(T instance, ExcelErrorManager errors, IReadOnlyList<ExcelHeaderDTO> headers, int rowIndex)
        {
            foreach (var h in headers)
            {
                if (!BitColumns.Contains(h.PropertyName) || h.ColumnIndex == null) continue;
                var val = h.Property.GetValue(instance)?.ToString();
                if (string.IsNullOrWhiteSpace(val)) continue;
                if (!(val == "0" || val == "1" || val.Equals("true", StringComparison.OrdinalIgnoreCase) || val.Equals("false", StringComparison.OrdinalIgnoreCase)))
                {
                    errors.AddError(h.PropertyName, rowIndex, h.ColumnIndex, $"Field '{h.PropertyName}' must be 0/1 or true/false", eExcelErrorSeverity.ParseErr);
                }
            }
        }

    }
}

