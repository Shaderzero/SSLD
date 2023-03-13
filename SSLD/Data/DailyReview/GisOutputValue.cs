using Microsoft.EntityFrameworkCore;

namespace SSLD.Data.DailyReview;

[Index(nameof(ReportDate))]
public class GisOutputValue : DayValue 
{
    public int GisId { get; set; }
    public Gis Gis { get; set; }
}