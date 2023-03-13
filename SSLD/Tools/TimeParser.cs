using System.Globalization;

namespace SSLD.Tools;

public static class TimeParser
{
    private const string Cet = "Central European Standard Time";
    private const string Msk = "Russian Standard Time";
    private static TimeZoneInfo _cetZone = TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time");
    private static TimeZoneInfo _mskZone = TimeZoneInfo.FindSystemTimeZoneById("Russian Standard Time");
    
    public static DateTime CetToMsk(DateTime dt)
    {
        return TimeZoneInfo.ConvertTimeBySystemTimeZoneId(dt, Cet, Msk);
    }
    
    public static DateTime MskToCet(DateTime dt)
    {
        return TimeZoneInfo.ConvertTimeBySystemTimeZoneId(dt, Msk, Cet);
    }
    
    public static int MskToCet(int hour)
    {
        var tmpTime = DateTime.Now;
        tmpTime = new DateTime(tmpTime.Year, tmpTime.Month, tmpTime.Day, hour, 0, 0);
        tmpTime = MskToCet(tmpTime);
        return tmpTime.Hour;
    }
    
    public static int CetToMsk(int hour)
    {
        var tmpTime = DateTime.Now;
        tmpTime = new DateTime(tmpTime.Year, tmpTime.Month, tmpTime.Day, hour, 0, 0);
        tmpTime = CetToMsk(tmpTime);
        return tmpTime.Hour;
    }

    public static int TimeDiff(DateTime dt)
    {
        var tmpTime = MskToCet(dt);
        var result = dt - tmpTime;
        return result.Hours;
    }

    public static string NumToMonth(int num)
    {
        if (num is < 1 or > 12) return null;
        return num switch
        {
            1 => "январь",
            2 => "февраль",
            3 => "март",
            4 => "апрель",
            5 => "май",
            6 => "июнь",
            7 => "июль",
            8 => "август",
            9 => "сентябрь",
            10 => "октябрь",
            11 => "ноябрь",
            12 => "декабрь"
        };
    }
}