using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SSLD.Data.DailyReview;

[Index(nameof(ForecastId), nameof(GisCountryId), nameof(Month), IsUnique = true)]
public class ForecastGisCountry
{
    public int Id { get; set; }
    public int ForecastId { get; set; }
    public Forecast Forecast { get; set; }
    public int GisCountryId { get; set; }
    public GisCountry GisCountry { get; set; }
    public int Month { get; set; }
    [Column(TypeName = "decimal(16, 8)")]
    public decimal Value { get; set; }

}