using SSLD.Data.DailyReview;
using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SSLD.Tools;

public static class StringParser
{
    private static readonly Regex SWhitespace = new Regex(@"\s+");
  
    public static string ReplaceWhitespace(string input, string replacement) 
    {
        return SWhitespace.Replace(input, replacement);
    }

    public static Gis FindGisByName(IEnumerable<Gis> list, string text)
    {
        var gis = list.Where(x => NameContainAnyList(x.Names, text)).ToList();
        if (gis.Count <= 1) return gis.FirstOrDefault();
        {
            var maxName =  gis.Max(x => x.Name);
            return gis.FirstOrDefault(x => x.Name == maxName);
        }

    }
    
    public static string ToCyr(this string name)
    {
        return LatinToCyr(name);
    }

    public static string LatinToCyr(string name)
    {
        name = name.Replace("e", "е");
        name = name.Replace("E", "Е");
        name = name.Replace("t", "т");
        name = name.Replace("T", "Т");
        name = name.Replace("y", "у");
        name = name.Replace("Y", "У");
        name = name.Replace("u", "и");
        name = name.Replace("U", "И");
        name = name.Replace("o", "о");
        name = name.Replace("O", "О");
        name = name.Replace("p", "р");
        name = name.Replace("P", "Р");
        name = name.Replace("x", "х");
        name = name.Replace("X", "Х");
        name = name.Replace("a", "а");
        name = name.Replace("A", "А");
        name = name.Replace("h", "н");
        name = name.Replace("H", "Н");
        name = name.Replace("k", "к");
        name = name.Replace("K", "К");
        name = name.Replace("c", "с");
        name = name.Replace("C", "С");
        name = name.Replace("b", "в");
        name = name.Replace("B", "В");
        name = name.Replace("m", "м");
        name = name.Replace("M", "М");

        return name;
    }

    public static int CevTimeToInt(string time)
    {
        var result = time switch
        {
            "08:00-09:00" => 1,
            "09:00-10:00" => 2,
            "10:00-11:00" => 3,
            "11:00-12:00" => 4,
            "12:00-13:00" => 5,
            "13:00-14:00" => 6,
            "14:00-15:00" => 7,
            "15:00-16:00" => 8,
            "16:00-17:00" => 9,
            "17:00-18:00" => 10,
            "18:00-19:00" => 11,
            "19:00-20:00" => 12,
            "20:00-21:00" => 13,
            "21:00-22:00" => 14,
            "22:00-23:00" => 15,
            "23:00-00:00" => 16,
            "00:00-01:00" => 17,
            "01:00-02:00" => 18,
            "02:00-03:00" => 19,
            "03:00-04:00" => 20,
            "04:00-05:00" => 21,
            "05:00-06:00" => 22,
            "06:00-07:00" => 23,
            "07:00-08:00" => 24,
            "01:00-01:00" => 18, // переход времени
            "02:00-02:00" => 19,
            "03:00-03:00" => 20,
            "04:00-04:00" => 21,
            "05:00-05:00" => 22,
            "06:00-06:00" => 23,
            "07:00-07:00" => 24,
            _ => 0
        };
        return result;
    }

    public static string IntToCevTime(int value)
    {
        var result = value switch
        {
            1 => "08-09",
            2 => "09-10",
            3 => "10-11",
            4 => "11-12",
            5 => "12-13",
            6 => "13-14",
            7 => "14-15",
            8 => "15-16",
            9 => "16-17",
            10 => "17-18",
            11 => "18-19",
            12 => "19-20",
            13 => "20-21",
            14 => "21-22",
            15 => "22-23",
            16 => "23-00",
            17 => "00-01",
            18 => "01-02",
            19 => "02-03",
            20 => "03-04",
            21 => "04-05",
            22 => "05-06",
            23 => "06-07",
            24 => "07-08",
            _ => "error"
        };
        return result;
    }
    public static string IntToMonthRu(int value)
    {
        var result = value switch
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
            12 => "декабрь",
            _ => "error"
        };
        return result;
    }

    public static bool ContainLike(string source, string name)
    {
        if (source == null || name == null) return false;
        source = LatinToCyr(source).ToLower().Trim();
        name = LatinToCyr(name).ToLower().Trim();
        return source.Contains(name);
    }

    public static bool StrictLike(string source, string name)
    {
        source = LatinToCyr(source).ToLower().Trim();
        name = LatinToCyr(name).ToLower().Trim();
        return source.Equals(name);
    }

    public static bool NameContainAnyList(List<string> source, string name)
    {
        if (name == null) return false;
        name = LatinToCyr(name);
        return source.Select(item => LatinToCyr(item)).Any(val => name.ToLower().Contains(val.ToLower()));
    }

    public static bool NameEqualsAnyList(List<string> source, string name)
    {
        name = LatinToCyr(name);
        foreach (var item in source)
        {
            var val = LatinToCyr(item);
            if (name.ToLower().Trim().Equals(val.ToLower().Trim()))
            {
                return true;
            }
        }
        return false;
    }

    public static bool NameContainAllList(List<string> source, string name)
    {
        name = LatinToCyr(name);
        foreach (var item in source)
        {
            var val = LatinToCyr(item);
            if (!name.ToLower().Contains(val.ToLower()))
            {
                return false;
            }
        }
        return true;
    }

    public static DateTime? GetFirstDateFromString(string name)
    {
        var array = name.Split(new char[] { ',', ' ', '.', '_', '-' });

        for (var i = 0; i < array.Length - 1; i++)
        {
            var day = 0;
            var month = 0;
            var year = 0;
            if (array[i].Length == 2 && int.TryParse(array[i], out day))
            {
                if (int.TryParse(array[i + 1], out month) && int.TryParse(array[i + 2], out year))
                {
                    return TryGetDate(year, month, day);
                }
            }

            if (array[i].Length != 4 || !int.TryParse(array[i], out year)) continue;
            if (int.TryParse(array[i], out year) && int.TryParse(array[i + 1], out month) && int.TryParse(array[i + 2], out day))
            {
                return TryGetDate(year, month, day);
            }
        }
        return null;
    }

    public static DateOnly? GetFirstDateOnlyFromString(string name)
    {
        var array = name.Split(new char[] { ',', ' ', '.', '_', '-' });

        for (var i = 0; i < array.Length - 1; i++)
        {
            var day = 0;
            var month = 0;
            var year = 0;
            if (array[i].Length == 2 && int.TryParse(array[i], out day))
            {
                if (int.TryParse(array[i + 1], out month) && int.TryParse(array[i + 2], out year))
                {
                    return TryGetDateOnly(year, month, day);
                }
            }

            if (array[i].Length != 4 || !int.TryParse(array[i], out year)) continue;
            if (int.TryParse(array[i], out year) && int.TryParse(array[i + 1], out month) && int.TryParse(array[i + 2], out day))
            {
                return TryGetDateOnly(year, month, day);
            }
        }
        return null;
    }

    public static DateOnly? GetFirstDate(this string name)
    {
        return GetFirstDateOnlyFromString(name);
    }

    public static DateTime? GetSecondDateFromString(string name)
    {
        var array = name.Split(new char[] { ',', ' ', '.', '_', '-' });

        var count = 0;
        for (var i = 0; i < array.Length - 1; i++)
        {
            var month = 0;
            var year = 0;
            if (array[i].Length == 2 && int.TryParse(array[i], out var day))
            {
                if (int.TryParse(array[i + 1], out month) && int.TryParse(array[i + 2], out year))
                {
                    count++;
                    if (count > 1) return TryGetDate(year, month, day);
                }
            }

            if (array[i].Length != 4 || !int.TryParse(array[i], out year)) continue;
            if (!int.TryParse(array[i], out year) || !int.TryParse(array[i + 1], out month) ||
                !int.TryParse(array[i + 2], out day)) continue;
            count++;
            if (count > 1) return TryGetDate(year, month, day);
        }
        return null;
    }

    public static DateTime? GetDateWithTimeFromString(string name)
    {
        var array = name.Split(new char[] { ',', ' ', '.', '_', '-' });

        for (var i = 0; i < array.Length - 3; i++)
        {
            var month = 0;
            var year = 0;
            var hour = 0;
            var minute = 0;
            if (array[i].Length == 2 && int.TryParse(array[i], out var day)
                                     && (array[i + 1].Length == 2)
                                     && (array[i + 2].Length == 4)
                                     && array[i + 3].Length is 2 or 1
                                     && array[i + 4].Length is 2 or 1)
            {
                if (int.TryParse(array[i + 1], out month) && int.TryParse(array[i + 2], out year) && int.TryParse(array[i + 3], out hour) && int.TryParse(array[i + 4], out minute))
                {
                    return TryGetDate(year, month, day, hour, minute);
                }
            }
            if (array[i].Length != 4 || !int.TryParse(array[i], out year) || (array[i + 1].Length != 2) ||
                (array[i + 2].Length != 2) || (array[i + 3].Length != 2 && array[i + 3].Length != 1) ||
                (array[i + 4].Length != 2 && array[i + 4].Length != 1)) continue;
            if (int.TryParse(array[i + 1], out month) && int.TryParse(array[i + 2], out day) && int.TryParse(array[i + 3], out hour) && int.TryParse(array[i + 4], out minute))
            {
                return TryGetDate(year, month, day, hour, minute);
            }
        }
        return null;
    }
    
    public static DateTime? GetDateTime(this string name)
    {
        return GetDateWithTimeFromString(name);
    }

    public static DateTime? GetFileTime(string name)
    {
        var array = name.Split(new char[] { ',', ' ', '.', '_', '-' });

        for (var i = 0; i < array.Length - 3; i++)
        {
            var month = 0;
            var year = 0;
            var hour = 0;
            var minute = 0;
            if (array[i].Length == 2 && int.TryParse(array[i], out var day)
                                     && (array[i + 1].Length == 2)
                                     && (array[i + 2].Length == 4)
                                     && array[i + 3].Length is 2 or 1
                                     && array[i + 4].Length is 2 or 1)
            {
                if (int.TryParse(array[i + 1], out month) && int.TryParse(array[i + 2], out year) && int.TryParse(array[i + 3], out hour) && int.TryParse(array[i + 4], out minute))
                {
                    return TryGetDate(year, month, day, hour, minute);
                }
            }

            if (array[i].Length != 4 || !int.TryParse(array[i], out year) || (array[i + 1].Length != 2) ||
                (array[i + 2].Length != 2) || (array[i + 3].Length != 2 && array[i + 3].Length != 1) ||
                (array[i + 4].Length != 2 && array[i + 4].Length != 1)) continue;
            if (int.TryParse(array[i + 1], out month) && int.TryParse(array[i + 2], out day) && int.TryParse(array[i + 3], out hour) && int.TryParse(array[i + 4], out minute))
            {
                return TryGetDate(year, month, day, hour, minute);
            }
        }
        return null;
    }

    private static DateTime? TryGetDate(int one, int two, int three)
    {
        try
        {
            return one >= 1000 ? new DateTime(one, two, three) : new DateTime(three, two, one);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return null;
        }
    }

    private static DateOnly? TryGetDateOnly(int one, int two, int three)
    {
        try
        {
            return one >= 1000 ? new DateOnly(one, two, three) : new DateOnly(three, two, one);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return null;
        }
    }

    private static DateTime? TryGetDate(int one, int two, int three, int four, int five)
    {
        try
        {
            return one >= 1000 ? new DateTime(one, two, three, four, five, 0) : new DateTime(three, two, one, four, five, 0);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return null;
        }
    }

    public static double PrepareDoubleForDb(double val)
    {
        return val < 0 ? 0d : Math.Round(val, 8);
    }

    public static double TryGetDouble(string value)
    {
        try
        {
            var provider = new NumberFormatInfo();
            provider.NumberDecimalSeparator = value.Contains(',') ? "," : ".";
            return Convert.ToDouble(value, provider);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return 0d;
        }
    }

    public static decimal TryGetDecimal(string value)
    {
        value = ReplaceWhitespace(value, "");
        try
        {
            var provider = new NumberFormatInfo
            {
                NumberDecimalSeparator = value.Contains(',') ? "," : "."
            };
            return Convert.ToDecimal(value, provider);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return 0;
        }
    }

    public static string GetString(decimal value)
    {
        //return value.ToString();
        return Math.Round(value, 4).ToString(CultureInfo.CurrentCulture);
    }

    public static string ShowLogInfo(InputFileLog log)
    {
        var result = "Значение подшито из файла '"
                     + @log.Filename + "' " + @log.InputTime
                     + " " + @log.User.Name;
        return result;
    }

    public static string ShowLogInfo(InputFileLog log, ReviewValueInput.ValueType type)
    {
        var result = " из файла '"
                     + @log.Filename + "' " + @log.InputTime
                     + " " + @log.User.Name;
        return result;
    }
}