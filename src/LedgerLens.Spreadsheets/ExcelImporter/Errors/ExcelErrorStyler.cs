
// -----------------------------------------------------------------------------
// 4) UtilityLibrary.Excel/Errors/ExcelErrorStyler.cs
//    - Thin wrapper kept for backward compatibility, delegates to ErrorManager
// -----------------------------------------------------------------------------
namespace UtilityLibrary.Excel
{
    public static class ExcelErrorStyler
    {
        public static void ApplyErrors<TEntity>(ExcelImportResult<TEntity> result, ExcelErrorManager manager)
        {
            if (result.FileBytes == null || manager.Errors.Count == 0) return;
            result.FileBytes = manager.ApplyErrorsToWorkbook(result.FileBytes);
        }
    }
}