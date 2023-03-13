using Microsoft.EntityFrameworkCore;

namespace SSLD.Data.DailyReview;

[Index(nameof(ReportDate))]
public class GisInputValue : DayValue
{
    public int GisId { get; set; }
    public Gis Gis { get; set; }

}