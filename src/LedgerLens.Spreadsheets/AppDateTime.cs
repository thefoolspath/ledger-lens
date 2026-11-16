namespace UtilityLibrary;

public static class AppDateTime
{
        private static TimeZoneInfo _timeZone = TimeZoneInfo.Local;
        public static void SetTimeZone(string timeZoneId)
        {
            _timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        }

        public static DateTime Now => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _timeZone);
}
