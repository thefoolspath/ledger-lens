// -----------------------------------------------------------------------------
// 5) UtilityLibrary.Excel/Import/ExcelValueConverters.cs
// -----------------------------------------------------------------------------
namespace UtilityLibrary.Excel
{
    public static class ExcelValueConverters
    {
        public static readonly Dictionary<Type, Func<string, (bool success, object? value)>> Converters = new()
        {
            [typeof(Guid)] = s => (Guid.TryParse(s, out var v), v),
            [typeof(int)] = s => (int.TryParse(s, out var v), v),
            [typeof(long)] = s => (long.TryParse(s, out var v), v),
            [typeof(decimal)] = s => (decimal.TryParse(s, out var v), v),
            [typeof(double)] = s => (double.TryParse(s, out var v), v),
            [typeof(DateTime)] = s => (DateTime.TryParse(s, out var v), v),
            [typeof(bool)] = s => ConvertToBool(s),
        };
        private static (bool success, object? value) ConvertToBool(string s)
        {
            if (s == "1" || s.Equals("true", StringComparison.OrdinalIgnoreCase)) return (true, true);
            if (s == "0" || s.Equals("false", StringComparison.OrdinalIgnoreCase)) return (true, false);
            return (false, null);
        }
    }
}
