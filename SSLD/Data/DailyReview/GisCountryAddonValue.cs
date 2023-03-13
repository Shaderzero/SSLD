using Microsoft.EntityFrameworkCore;

namespace SSLD.Data.DailyReview;

[Index(nameof(ReportDate))]
public class GisCountryAddonValue : DayValue
{
    public int GisCountryAddonId { get; set; }
    public GisCountryAddon GisCountryAddon { get; set; }
}