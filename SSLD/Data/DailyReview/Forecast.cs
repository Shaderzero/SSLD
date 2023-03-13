using System.ComponentModel.DataAnnotations.Schema;

namespace SSLD.Data.DailyReview;

public class Forecast
{
    public int Id { get; set; }
    public int ForecastYearId { get; set; }
    public ForecastYear ForecastYear { get; set; }
    public string Name { get; set; }
    public string Fullname { get; set; }
    public DateOnly ReportDate { get; set; }
    public long InputFileLogId { get; set; }
    public InputFileLog InputFileLog { get; set; }
    public double[] Values { get; set; } = new double[12];
    public bool InDayReview { get; set; }
    public bool InMain { get; set; }
    public List<ForecastGisCountry> Countries { get; set; } = new();

    [NotMapped]
    public DateTime ReportDateTime
    {
        get => ReportDate.ToDateTime(new TimeOnly(0));
        set => ReportDate = DateOnly.FromDateTime(value);
    }

    [NotMapped]
    public double[] DailyValues
    {
        get
        {
            var array = new double[12];
            for (var i = 0; i < 12; i++)
            {
                var dayInMonth = (double)DateTime.DaysInMonth(ForecastYear.Year, i + 1);
                array[i] = Values[i] * 1000d / dayInMonth;
            }

            return array;
        }
    }

}