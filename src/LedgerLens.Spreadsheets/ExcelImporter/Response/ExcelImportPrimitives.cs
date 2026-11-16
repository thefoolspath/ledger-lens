

// 1) UtilityLibrary.Excel/Response/ExcelImportPrimitives.cs
// - ExcelErrorSeverity, ExcelErrorDTO
// - ExcelHeaderDTO (metadata for columns)
// -----------------------------------------------------------------------------
using System.Reflection;
using System.Linq.Expressions;


namespace UtilityLibrary.Excel
{
    // ===== Errors (Primitives) =====
    public enum eExcelErrorSeverity
    {
        ParseErr,
        Critical,
        ValidateEntityAutoMapper,
        ExternalErr
    }


    public class ExcelErrorDTO
    {
        public string ColumnName { get; set; } = string.Empty;
        public int? RowIndex { get; set; }
        public int? ColumnIndex { get; set; }
        public string? CellAddress { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public eExcelErrorSeverity Severity { get; set; } = eExcelErrorSeverity.ParseErr;
        public bool IsCritical => Severity == eExcelErrorSeverity.Critical;
    }


    // ===== Headers (Metadata) =====
    public class ExcelHeaderDTO
    {
        public required PropertyInfo Property { get; set; }
        public required Type DataType { get; set; }
        public int? ColumnIndex { get; set; }
        public bool IsRequired { get; set; }
        public string PropertyName => Property.Name;


        private Action<object, object?>? _setter;
        public void SafeSetValue(object target, object? value)
        {
            if (value == null || Property.PropertyType.IsAssignableFrom(value.GetType()))
            {
                _setter ??= BuildSetter();
                _setter(target, value);
            }
        }
        private Action<object, object?> BuildSetter()
        {
            var target = Expression.Parameter(typeof(object), "target");
            var value = Expression.Parameter(typeof(object), "value");
            var castTarget = Expression.Convert(target, Property.DeclaringType!);
            var castValue = Expression.Convert(value, Property.PropertyType);
            var body = Expression.Assign(Expression.Property(castTarget, Property), castValue);
            return Expression.Lambda<Action<object, object?>>(body, target, value).Compile();
        }
    }
}