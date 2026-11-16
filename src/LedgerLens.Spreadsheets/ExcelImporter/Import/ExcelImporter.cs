

// -----------------------------------------------------------------------------
// 8) UtilityLibrary.Excel/Import/ExcelImporter.cs
//    - Parse workbook → DTO → Entity; push all errors into ExcelErrorManager
// -----------------------------------------------------------------------------
using ClosedXML.Excel;
using Microsoft.AspNetCore.Http;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace UtilityLibrary.Excel
{
    public static class ExcelImporter
    {
        public static ExcelImportResult<TEntity> Import<TDto, TEntity>(IFormFile file, bool returnErrorFile = true, IEnumerable<string>? ignoreColumns = null)
            where TDto : new()
            where TEntity : class, new()
        {
            var result = new ExcelImportResult<TEntity> { FileName = file.FileName };
            var errors = new ExcelErrorManager();

            using var stream = new MemoryStream();
            file.CopyTo(stream);
            stream.Position = 0;

            using var workbook = new XLWorkbook(stream);
            var ws = workbook.Worksheets.FirstOrDefault();
            if (ws == null || ws.LastRowUsed() == null)
            {
                errors.AddError(string.Empty, null, null, "Worksheet is empty or invalid", eExcelErrorSeverity.Critical);
                result.FileBytes = stream.ToArray();
                result.AddErrors(errors.Errors);
                return result;
            }

            // headers
            var headers = ReflectHeaders<TDto>(ignoreColumns);
            ValidateAndBindColumnIndex(ws, headers, errors);
            result.Data.SetHeaders(headers);

            if (errors.AnyCritical())
            {
                result.FileBytes = stream.ToArray();
                result.AddErrors(errors.Errors);
                return result;
            }

            // Parse rows → DTO → Entity
            int lastRow = ws.LastRowUsed()?.RowNumber() ?? 1;
            for (int r = 2; r <= lastRow; r++)
            {
                var dto = new TDto();
                foreach (var h in headers)
                {
                    if (h.ColumnIndex == null) continue;
                    var cell = ws.Row(r).Cell(h.ColumnIndex.Value);
                    var text = cell.Value.ToString();

                    // Required at DTO-level based on reflection (IsRequired if value type or [Required])
                    if (string.IsNullOrWhiteSpace(text))
                    {
                        if (h.IsRequired)
                            errors.AddError(h.PropertyName, r, h.ColumnIndex, $"Required field '{h.PropertyName}' cannot be null", eExcelErrorSeverity.ParseErr, cell.Address.ToString());
                        continue;
                    }

                    // Convert with registry
                    object? converted = null;
                    var targetType = h.DataType;
                    var type = Nullable.GetUnderlyingType(targetType) ?? targetType;
                    bool ok;
                    if (type.IsEnum)
                        ok = Enum.TryParse(type, text, true, out converted);
                    else if (ExcelValueConverters.Converters.TryGetValue(type, out var conv))
                    {
                        var (success, value) = conv(text);
                        ok = success; converted = value;
                    }
                    else
                    {
                        try { converted = Convert.ChangeType(text, type); ok = true; }
                        catch { ok = false; }
                    }

                    if (!ok)
                    {
                        errors.AddError(h.PropertyName, r, h.ColumnIndex, $"Cannot convert '{text}' to {type.Name}", eExcelErrorSeverity.ParseErr, cell.Address.ToString());
                        continue;
                    }

                    h.SafeSetValue(dto!, converted);
                }
                ExcelValidationRules.ValidateBitColumns(dto, errors, headers, r);

                // map DTO → Entity
                var entity = ExcelMapper.MapDtoToEntity<TDto, TEntity>(dto);

                // entity validation (DataAnnotations)
                ExcelEntityValidator.ValidateEntity(entity, errors, headers, r);

                // custom rules (bit-columns etc.)
                //ExcelValidationRules.ValidateBitColumns(entity, errors, headers, r);


                result.Data.Add(entity);
            }

            // Attach workbook (optionally styled) to bytes
            if (returnErrorFile)
            {
                // Save original first
                using var outStream = new MemoryStream();
                workbook.SaveAs(outStream);
                result.FileBytes = outStream.ToArray();

                if (errors.Errors.Count > 0)
                    result.FileBytes = errors.ApplyErrorsToWorkbook(result.FileBytes);
            }

            result.AddErrors(errors.Errors);
            return result;
        }

        // ----- Helpers -----
        private static List<ExcelHeaderDTO> ReflectHeaders<T>(IEnumerable<string>? ignoreColumns)
        {
            var ignore = new HashSet<string>(ignoreColumns ?? Enumerable.Empty<string>(), StringComparer.OrdinalIgnoreCase);

            return typeof(T)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => !ignore.Contains(p.Name))
                .Select(p => new ExcelHeaderDTO
                {
                    Property = p,
                    DataType = p.PropertyType,
                    IsRequired =
                        Attribute.IsDefined(p, typeof(RequiredAttribute))
                        || (p.PropertyType.IsValueType && Nullable.GetUnderlyingType(p.PropertyType) == null)
                        || (p.PropertyType == typeof(string) && !IsNullableString(p))
                })
                .ToList();
        }

        private static bool IsNullableString(PropertyInfo p)
        {
            if (p.PropertyType != typeof(string))
                return false;

            var ctx = new NullabilityInfoContext();
            var info = ctx.Create(p);

            return info.ReadState == NullabilityState.Nullable;
        }


        private static void ValidateAndBindColumnIndex(IXLWorksheet ws, List<ExcelHeaderDTO> headers, ExcelErrorManager errors)
        {
            var headerRow = ws.Row(1);
            var map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (var cell in headerRow.CellsUsed())
            {
                var name = cell.GetString().Trim();
                if (map.ContainsKey(name))
                {
                    errors.AddError(name, 1, cell.Address.ColumnNumber, $"Duplicate column '{name}'", eExcelErrorSeverity.Critical, cell.Address.ToString());
                }
                else map[name] = cell.Address.ColumnNumber;
            }

            foreach (var h in headers)
            {
                if (map.TryGetValue(h.PropertyName, out var c)) h.ColumnIndex = c;
                else errors.AddError(h.PropertyName, 1, null, $"Missing column '{h.PropertyName}'", eExcelErrorSeverity.Critical);
            }
        }
    }
}
