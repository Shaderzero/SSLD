using Microsoft.EntityFrameworkCore;

namespace SSLD.Data.DailyReview;

[Index(nameof(ForecastYearId), nameof(Month), IsUnique = true)]
public class ForecastYearPhgValue : ForecastYearValue
{
    public override Task Update(ApplicationDbContext db)
    {
        return Task.CompletedTask;
    }
}