namespace erp_backend.Helpers
{
    /// <summary>
    /// Helper class ?? x? lý DateTime v?i PostgreSQL
    /// PostgreSQL yêu c?u DateTime ph?i có Kind = Utc
    /// </summary>
    public static class DateTimeHelper
    {
        /// <summary>
        /// Chuy?n DateTime sang UTC n?u ch?a ph?i UTC
        /// </summary>
        public static DateTime EnsureUtc(DateTime dateTime)
        {
            if (dateTime.Kind == DateTimeKind.Utc)
            {
                return dateTime;
            }
            else if (dateTime.Kind == DateTimeKind.Local)
            {
                return dateTime.ToUniversalTime();
            }
            else // DateTimeKind.Unspecified
            {
                return DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
            }
        }

        /// <summary>
        /// Chuy?n DateTime? sang UTC n?u không null
        /// </summary>
        public static DateTime? EnsureUtc(DateTime? dateTime)
        {
            if (!dateTime.HasValue)
            {
                return null;
            }

            return EnsureUtc(dateTime.Value);
        }

        /// <summary>
        /// L?y DateTime.UtcNow
        /// </summary>
        public static DateTime UtcNow => DateTime.UtcNow;

        /// <summary>
        /// Parse string thành DateTime UTC
        /// </summary>
        public static DateTime ParseUtc(string dateTimeString)
        {
            var dateTime = DateTime.Parse(dateTimeString);
            return EnsureUtc(dateTime);
        }

        /// <summary>
        /// L?y ??u tháng (UTC)
        /// </summary>
        public static DateTime GetStartOfMonth(int year, int month)
        {
            return new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
        }

        /// <summary>
        /// L?y cu?i tháng (UTC)
        /// </summary>
        public static DateTime GetEndOfMonth(int year, int month)
        {
            var startOfMonth = GetStartOfMonth(year, month);
            return startOfMonth.AddMonths(1).AddSeconds(-1);
        }

        /// <summary>
        /// Parse period "2025-01" thành startDate và endDate
        /// </summary>
        public static (DateTime startDate, DateTime endDate) ParsePeriod(string period)
        {
            var parts = period.Split('-');
            var year = int.Parse(parts[0]);
            var month = int.Parse(parts[1]);
            
            var startDate = GetStartOfMonth(year, month);
            var endDate = GetEndOfMonth(year, month);
            
            return (startDate, endDate);
        }
    }
}
