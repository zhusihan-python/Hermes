using System;
using System.Timers;

namespace Hermes.Common.Extensions;

public static class DateTimeExtensions
{
    public static DateTime ToStartOfDay(this DateTime datetime)
    {
        return new DateTime(datetime.Year, datetime.Month, datetime.Day, 0, 0, 0);
    }

    public static DateTime ToEndOfDay(this DateTime datetime)
    {
        return new DateTime(datetime.Year, datetime.Month, datetime.Day, 23, 59, 59);
    }
}