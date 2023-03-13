using SSLD.Data.DailyReview;

namespace SSLD.DTO;

public class ForecastTableItem
{
    public string Name { get; set; }
    public ForecastType Type { get; set; }
    public decimal January { get; set; }
    public decimal February { get; set; }
    public decimal March { get; set; }
    public decimal April { get; set; }
    public decimal May { get; set; }
    public decimal June { get; set; }
    public decimal July { get; set; }
    public decimal August { get; set; }
    public decimal September { get; set; }
    public decimal October { get; set; }
    public decimal November { get; set; }
    public decimal December { get; set; }
    public decimal Year => QuarterOne + QuarterTwo + QuarterThree + QuarterFour;
    public decimal QuarterOne => January + February + March;
    public decimal QuarterTwo => April + May + June;  
    public decimal QuarterThree => July + August + September;  
    public decimal QuarterFour => October + November + December;

    public ForecastTableItem(string name, ForecastType type)
    {
        Name = name;
        Type = type;
    }

    public ForecastTableItem(string name, IList<ForecastGisCountry> list)
    {
        Name = name;
        Type = ForecastType.Country;
        January = list.FirstOrDefault(x => x.Month == 1)!.Value;
        February = list.FirstOrDefault(x => x.Month == 2)!.Value;
        March = list.FirstOrDefault(x => x.Month == 3)!.Value;
        April = list.FirstOrDefault(x => x.Month == 4)!.Value;
        May = list.FirstOrDefault(x => x.Month == 5)!.Value;
        June = list.FirstOrDefault(x => x.Month == 6)!.Value;
        July = list.FirstOrDefault(x => x.Month == 7)!.Value;
        August = list.FirstOrDefault(x => x.Month == 8)!.Value;
        September = list.FirstOrDefault(x => x.Month == 9)!.Value;
        October = list.FirstOrDefault(x => x.Month == 10)!.Value;
        November = list.FirstOrDefault(x => x.Month == 11)!.Value;
        December = list.FirstOrDefault(x => x.Month == 12)!.Value;
    }

    internal void AddMonthValues(IList<ForecastGisCountry> list)
    {
        January += list.FirstOrDefault(x => x.Month == 1)!.Value;
        February += list.FirstOrDefault(x => x.Month == 2)!.Value;
        March += list.FirstOrDefault(x => x.Month == 3)!.Value;
        April += list.FirstOrDefault(x => x.Month == 4)!.Value;
        May += list.FirstOrDefault(x => x.Month == 5)!.Value;
        June += list.FirstOrDefault(x => x.Month == 6)!.Value;
        July += list.FirstOrDefault(x => x.Month == 7)!.Value;
        August += list.FirstOrDefault(x => x.Month == 8)!.Value;
        September += list.FirstOrDefault(x => x.Month == 9)!.Value;
        October += list.FirstOrDefault(x => x.Month == 10)!.Value;
        November += list.FirstOrDefault(x => x.Month == 11)!.Value;
        December += list.FirstOrDefault(x => x.Month == 12)!.Value;
    }

    public bool IsDir()
    {
        return Type == ForecastType.Dir;
    }

    public enum ForecastType
    {
        Dir,
        Country
    }
}