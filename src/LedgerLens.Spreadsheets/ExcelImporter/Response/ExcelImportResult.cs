

// -----------------------------------------------------------------------------
// 2) UtilityLibrary.Excel/Response/ExcelImportResult.cs
// - ExcelImportData<T>: headers + parsed data (no errors)
// - ExcelImportResult<T>: data + errors + file bytes
// -----------------------------------------------------------------------------
using System.Collections.ObjectModel;


namespace UtilityLibrary.Excel
{
    public class ExcelImportData<T>
    {
        internal readonly List<T> _items = new();
        internal readonly List<ExcelHeaderDTO> _headers = new();
        public ReadOnlyCollection<T> Items => _items.AsReadOnly();
        public ReadOnlyCollection<ExcelHeaderDTO> Headers => _headers.AsReadOnly();
        internal void Add(T item) => _items.Add(item);
        internal void SetHeaders(IEnumerable<ExcelHeaderDTO> headers)
        {
            _headers.Clear();
            _headers.AddRange(headers);
        }
    }


    public class ExcelImportResult<T>
    {
        public string? FileName { get; set; }
        public byte[]? FileBytes { get; set; }
        public ExcelImportData<T> Data { get; } = new();
        private readonly List<ExcelErrorDTO> _errors = new();
        public IReadOnlyList<ExcelErrorDTO> Errors => _errors;
        public bool HasCriticalError => _errors.Any(e => e.IsCritical);
        public void AddError(ExcelErrorDTO error) => _errors.Add(error);
        public void AddErrors(IEnumerable<ExcelErrorDTO> errors) => _errors.AddRange(errors);
    }
}