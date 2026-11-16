using ClosedXML.Excel;
using System.Collections.Concurrent;
using System.Reflection;
using UtilityLibrary.Report;

namespace UtilityLibrary;

public static class CreateReport
{
        [AttributeUsage(AttributeTargets.Property)]
        public sealed class ExcelHeaderAttribute : Attribute
        {
            public string HeaderName { get; }

            public ExcelHeaderAttribute(string headerName)
            {
                HeaderName = headerName;
            }
        }

        private const string DefaultNoDataMessage = "No data available";
        private const double ExtraColumnWidth = 2.0;
        private const double MaxFileSizeMb = 30.0;

        // Cache reflection metadata ต่อ TRow (ลด overhead ตอนเรียกซ้ำ ๆ)
        private static readonly ConcurrentDictionary<Type, RowMeta> _metaCache = new();

        public static (TResponse? Response, string? StatusCode)
            CreateExcel<TResponse, TRow>(
                IReadOnlyList<TRow> data,
                string sheetName)
            where TResponse : BaseReportResponse, new()
        {
            var meta = GetMeta<TRow>();
            if (meta.ColumnCount == 0)
                return (null, StatusCodeResponse.C4369); 

            if (IsExcelTooLargeEstimate(data, meta.ColumnCount))
                return (null, StatusCodeResponse.C4369);

            using var workbook = new XLWorkbook();
            var worksheet = workbook.AddWorksheet(sheetName);

            InsertTableFast(worksheet, data, meta);

            StyleHeaderRow(worksheet, meta.ColumnCount);

            if (data.Count == 0)
                InsertNoDataMessage(worksheet, meta.ColumnCount);

            OptimizeColumnAlignmentAndFormatUltraFast(worksheet, data.Count, meta);

            AutoResizeColumnsHeaderOnly(worksheet, meta.ColumnCount);

            ApplyTableTheme(worksheet);

            return (BuildResponse<TResponse>(workbook, sheetName), null);
        }

        // ==================================================
        // Insert Table (Fast path)
        // ==================================================
        private static void InsertTableFast<TRow>(IXLWorksheet sheet, IReadOnlyList<TRow> data, RowMeta meta)
        {
            // 1) เขียน header จาก meta เสมอ (ไม่ให้ ClosedXML auto-generate)
            for (int c = 0; c < meta.ColumnCount; c++)
                sheet.Cell(1, c + 1).Value = meta.Headers[c];

            if (data.Count == 0)
            {
                // 2) no data: สร้าง table ครอบ header + แถว 2
                var range = sheet.Range(1, 1, 2, meta.ColumnCount);
                range.CreateTable("DataTable");
                return;
            }

            // 3) มี data: insert แบบ bulk
            var props = typeof(TRow)
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(p => p.CanRead && p.GetIndexParameters().Length == 0)
                .OrderBy(p => p.MetadataToken)
                .ToArray();

            IEnumerable<object?[]> Rows()
            {
                foreach (var row in data)
                {
                    var arr = new object?[props.Length];
                    for (int i = 0; i < props.Length; i++)
                        arr[i] = props[i].GetValue(row);
                    yield return arr;
                }
            }

            sheet.Cell(2, 1).InsertData(Rows());

            // 4) Create table ครอบ header + data
            var lastRow = 1 + data.Count;
            sheet.Range(1, 1, lastRow, meta.ColumnCount).CreateTable("DataTable");
        }

        // ==================================================
        // Header
        // ==================================================
        private static void StyleHeaderRow(IXLWorksheet sheet, int columnCount)
        {
            if (columnCount <= 0) return;

            var headerRange = sheet.Range(1, 1, 1, columnCount);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
            headerRange.Style.Font.FontColor = XLColor.SmokyBlack;
        }

        // ==================================================
        // No Data
        // ==================================================
        private static void InsertNoDataMessage(IXLWorksheet sheet, int columnCount)
        {
            if (columnCount <= 0) return;

            var cell = sheet.Cell(2, 1);
            cell.Value = DefaultNoDataMessage;

            sheet.Range(2, 1, 2, columnCount).Merge();

            var style = sheet.Row(2).Style;
            style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            style.Font.Italic = true;
            style.Font.FontColor = XLColor.Gray;
        }

        // ==================================================
        // Column Alignment & Format (Ultra Fast)
        // - ไม่อ่าน DataType จาก cell (ช้า) แต่ดูจาก typeof(TRow)
        // - set style แบบ "เป็นบล็อก" ก่อน แล้วค่อยปรับเฉพาะคอลัมน์ที่จำเป็น
        // ==================================================
        private static void OptimizeColumnAlignmentAndFormatUltraFast(
            IXLWorksheet sheet,
            int dataCount,
            RowMeta meta)
        {
            if (meta.ColumnCount <= 0) return;
            if (dataCount <= 0) return; // ไม่มี data จริง ก็ไม่ต้องจัด format data block

            var headerRow = 1;
            var firstDataRow = headerRow + 1;
            var lastRowUsed = firstDataRow + dataCount - 1;

            // 1) Header: center (เผื่อ InsertTable ตอนมี data)
            sheet.Range(headerRow, 1, headerRow, meta.ColumnCount)
                 .Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            // 2) Default: data block เป็น Left ก่อน (ครั้งเดียว)
            var dataBlock = sheet.Range(firstDataRow, 1, lastRowUsed, meta.ColumnCount);
            dataBlock.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

            // 3) Numeric columns -> Right
            foreach (var col in meta.NumericColumns1Based)
            {
                sheet.Range(firstDataRow, col, lastRowUsed, col)
                     .Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            }

            // 4) DateTime columns -> Center + format
            foreach (var col in meta.DateTimeColumns1Based)
            {
                var r = sheet.Range(firstDataRow, col, lastRowUsed, col);
                r.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                r.Style.DateFormat.Format = "dd/MM/yyyy HH:mm";
            }
        }

        // ==================================================
        // Auto Resize (Fast)
        // - ปรับกว้างจาก header เท่านั้น เร็วกว่ามากเมื่อ data เยอะ
        // ==================================================
        private static void AutoResizeColumnsHeaderOnly(IXLWorksheet sheet, int columnCount)
        {
            if (columnCount <= 0) return;

            for (int col = 1; col <= columnCount; col++)
            {
                var column = sheet.Column(col);
                column.AdjustToContents(1, 1);  
                column.Width += ExtraColumnWidth;
            }
        }

        // ==================================================
        // Table Theme
        // ==================================================
        private static void ApplyTableTheme(IXLWorksheet sheet)
        {
            var table = sheet.Tables.FirstOrDefault();
            if (table == null) return;

            table.Theme = XLTableTheme.TableStyleLight9;
            sheet.SheetView.FreezeRows(1);
        }

        // ==================================================
        // Response
        // ==================================================
        private static TResponse BuildResponse<TResponse>(XLWorkbook workbook, string sheetName)
            where TResponse : BaseReportResponse, new()
        {
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);

            return new TResponse
            {
                file_bytes = stream.ToArray(),
                file_name = $"{sheetName}_{AppDateTime.Now:yyyyMMdd_HHmmss}.xlsx",
                file_type = eFileMimeType.EXCEL.GetMimeType()
            };
        }

        // ==================================================
        // File Size Estimate
        // ==================================================
        private static bool IsExcelTooLargeEstimate<TRow>(IReadOnlyList<TRow> data, int columnCount)
        {
            if (data.Count == 0) return false;

            long totalCells = (long)data.Count * columnCount;
            var estimatedMb = (totalCells * 25d) / (1024d * 1024d);
            return estimatedMb > MaxFileSizeMb;
        }

        // ==================================================
        // Reflection meta (cached)
        // ==================================================
        private static RowMeta GetMeta<TRow>()
        {
            var t = typeof(TRow);
            return _metaCache.GetOrAdd(t, _ => BuildMeta(t));
        }

        private static RowMeta BuildMeta(Type rowType)
        {
            var props = rowType
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(p => p.CanRead && p.GetIndexParameters().Length == 0)
                .OrderBy(p => p.MetadataToken) // ทำให้ลำดับ stable (ภายใน runtime เดียวกัน)
                .ToArray();

            // ✅ header: ExcelHeaderAttribute > PropertyName
            var headers = new string[props.Length];
            for (int i = 0; i < props.Length; i++)
            {
                var attr = props[i].GetCustomAttribute<ExcelHeaderAttribute>(inherit: true);
                headers[i] = !string.IsNullOrWhiteSpace(attr?.HeaderName) ? attr!.HeaderName : props[i].Name;
            }

            var numericCols = new List<int>();
            var dateCols = new List<int>();

            for (int i = 0; i < props.Length; i++)
            {
                var col1 = i + 1;
                var t = Nullable.GetUnderlyingType(props[i].PropertyType) ?? props[i].PropertyType;

                if (IsNumericType(t))
                    numericCols.Add(col1);
                else if (t == typeof(DateTime))
                    dateCols.Add(col1);
            }

            return new RowMeta(
                ColumnCount: props.Length,
                Headers: headers,
                NumericColumns1Based: numericCols.ToArray(),
                DateTimeColumns1Based: dateCols.ToArray()
            );
        }

        private static bool IsNumericType(Type t) =>
            t == typeof(byte) || t == typeof(sbyte) ||
            t == typeof(short) || t == typeof(ushort) ||
            t == typeof(int) || t == typeof(uint) ||
            t == typeof(long) || t == typeof(ulong) ||
            t == typeof(float) || t == typeof(double) ||
            t == typeof(decimal);

        private sealed record RowMeta(
            int ColumnCount,
            string[] Headers,
            int[] NumericColumns1Based,
            int[] DateTimeColumns1Based
        );

        // ==================================================
        // (คุณมีของเดิมอยู่แล้ว) ถ้าอยากให้ครบไฟล์คอมไพล์
        // ==================================================
        private static class StatusCodeResponse
        {
            public const string C4369 = "C4369";
        }
}
