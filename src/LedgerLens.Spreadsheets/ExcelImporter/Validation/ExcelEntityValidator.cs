
// -----------------------------------------------------------------------------
// 6) UtilityLibrary.Excel/Validation/ExcelEntityValidator.cs
//    - Uses DataAnnotations; logs to ExcelErrorManager
// -----------------------------------------------------------------------------
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace UtilityLibrary.Excel
{
    public static class ExcelEntityValidator
    {
        public static void ValidateEntity<TEntity>(TEntity entity, ExcelErrorManager errors, IReadOnlyList<ExcelHeaderDTO> headers, int rowIndex)
        {
            foreach (var prop in typeof(TEntity).GetProperties())
            {
                var value = prop.GetValue(entity);
                var header = headers.FirstOrDefault(h => h.PropertyName == prop.Name);
                int? colIndex = header?.ColumnIndex;

                // Required
                if (Attribute.IsDefined(prop, typeof(RequiredAttribute)))
                {
                    bool missing = value == null || (value is string s && string.IsNullOrWhiteSpace(s));
                    if (missing)
                        errors.AddError(prop.Name, rowIndex, colIndex, $"Entity field '{prop.Name}' is required but missing", eExcelErrorSeverity.ValidateEntityAutoMapper);
                }

                // StringLength
                var strLen = prop.GetCustomAttribute<StringLengthAttribute>();
                if (strLen != null && value is string str)
                {
                    if (str.Length < strLen.MinimumLength)
                        errors.AddError(prop.Name, rowIndex, colIndex, $"Entity field '{prop.Name}' must be at least {strLen.MinimumLength} characters", eExcelErrorSeverity.ValidateEntityAutoMapper);
                    if (str.Length > strLen.MaximumLength)
                        errors.AddError(prop.Name, rowIndex, colIndex, $"Entity field '{prop.Name}' exceeds max length {strLen.MaximumLength} (actual {str.Length})", eExcelErrorSeverity.ValidateEntityAutoMapper);
                }

                // MaxLength
                var maxLen = prop.GetCustomAttribute<MaxLengthAttribute>();
                if (maxLen != null && value is string sv)
                {
                    if (sv.Length > maxLen.Length)
                        errors.AddError(prop.Name, rowIndex, colIndex, $"Entity field '{prop.Name}' exceeds max length {maxLen.Length} (actual {sv.Length})", eExcelErrorSeverity.ValidateEntityAutoMapper);
                }
            }
        }


        public static bool IsNullable(PropertyInfo property)
        {
            if (!property.PropertyType.IsValueType)
            {
                // ดู context-level nullable metadata
                var nullable = property.CustomAttributes
                    .FirstOrDefault(a => a.AttributeType.FullName == "System.Runtime.CompilerServices.NullableAttribute");
                if (nullable != null && nullable.ConstructorArguments.Count == 1)
                {
                    var arg = nullable.ConstructorArguments[0];
                    if (arg.ArgumentType == typeof(byte[]))
                    {
                        var flags = (ReadOnlyCollection<CustomAttributeTypedArgument>)arg.Value!;
                        if (flags.Count > 0 && flags[0].Value is byte b && b == 2)
                            return true;
                    }
                    else if (arg.Value is byte b && b == 2)
                        return true;
                }

                var context = property.DeclaringType?.CustomAttributes
                    .FirstOrDefault(a => a.AttributeType.FullName == "System.Runtime.CompilerServices.NullableContextAttribute");
                if (context != null && context.ConstructorArguments.Count == 1 &&
                    context.ConstructorArguments[0].Value is byte b2 && b2 == 2)
                    return true;
            }

            return Nullable.GetUnderlyingType(property.PropertyType) != null;
        }

    }
}
