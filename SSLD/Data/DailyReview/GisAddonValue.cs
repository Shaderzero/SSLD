using Microsoft.EntityFrameworkCore;

namespace SSLD.Data.DailyReview;

[Index(nameof(ReportDate))]
public class GisAddonValue : DayValue
{
    public int GisAddonId { get; set; }
    public GisAddon GisAddon { get; set; }
}