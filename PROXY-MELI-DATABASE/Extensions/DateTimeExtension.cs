using System;
namespace System.Linq
{
    public static class DateTimeExtension
    {
        public static DateTime TimeMin(this DateTime dateTime)
        {
            return dateTime.Date;
        }

        public static DateTime TimeMax(this DateTime dateTime)
        {
            return TimeMin(dateTime).AddDays(1).AddTicks(-1);
        }
    }
}
