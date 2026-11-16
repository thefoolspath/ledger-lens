using System.Runtime.Serialization;

namespace UtilityLibrary.Report
{
    public class BaseReportResponse
    {
        public byte[]? file_bytes { get; set; }
        public string? file_name { get; set; }
        public string? file_type { get; set; }

    }

    public enum eFileMimeType
    {
        [EnumMember(Value = "EXCELOPENXML")] EXCEL,
        [EnumMember(Value = "PDF")] PDF,
        [EnumMember(Value = "CSV")] CSV,
        [EnumMember(Value = "EXCEL97")] EXCEL97
    }

    public enum eReportParameterName
    {
        [EnumMember(Value = "TypeDateTime")] TypeDateTime
    }

    public static class FileMimeTypeExtensions
    {
        public static string GetMimeType(this eFileMimeType fileType)
        {
            return fileType switch
            {
                eFileMimeType.EXCEL => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                eFileMimeType.EXCEL97 => "application/vnd.ms-excel",
                eFileMimeType.PDF => "application/pdf",
                eFileMimeType.CSV => "text/csv",
                _ => "application/octet-stream"
            };
        }
    }

}
