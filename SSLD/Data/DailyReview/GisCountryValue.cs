using Microsoft.EntityFrameworkCore;

namespace SSLD.Data.DailyReview;

[Index(nameof(ReportDate))]
public class GisCountryValue : DayValue
{
    public int GisCountryId { get; set; }
    public GisCountry GisCountry { get; set; }
}