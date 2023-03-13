using System.ComponentModel.DataAnnotations.Schema;

namespace SSLD.Data.DailyReview;

public abstract class ForecastYearValue
{
    public int Id { get; set; }
    public int ForecastYearId { get; set; }
    public ForecastYear ForecastYear { get; set; }
    public int Month { get; set; }
    public double Value { get; set; }
    public string Info { get; set; }
    
    [NotMapped]
    public double DayValue
    {
        get
        {
            var days = DateTime.DaysInMonth(ForecastYear.Year, Month);
            return Value / days;
        }
    }

    public abstract Task Update(ApplicationDbContext db);

    public List<DateOnly> DateList()
    {
        var year = ForecastYear.Year;
        var days = DateTime.DaysInMonth(year, Month);
        List<DateOnly> dateList = new();
        for (var i = 1; i <= days; i++)
        {
            dateList.Add(new DateOnly(year, Month, i));
        }

        return dateList;
    }
}