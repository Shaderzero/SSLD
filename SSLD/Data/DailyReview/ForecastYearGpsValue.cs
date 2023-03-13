using Microsoft.EntityFrameworkCore;

namespace SSLD.Data.DailyReview;

[Index(nameof(ForecastYearId), nameof(Month), IsUnique = true)]
public class ForecastYearGpsValue : ForecastYearValue
{
    public override async Task Update(ApplicationDbContext db)
    {
        var year = ForecastYear.Year;

        var source = db.GisCountryAddonValues
            .Where(x =>
                !x.GisCountryAddon.Types.Any(a => a.StartDate.Month == Month && a.IsCommGas)
                && x.ReportDate.Month == Month && x.ReportDate.Year == year)
            .AsQueryable();
        var days = DateTime.DaysInMonth(year, Month);
        double value = 0;
        for (var day = days; day > 0; day--)
        {
            var date = new DateOnly(year, Month, day);
            var val = (double)await source.Where(x => x.ReportDate == date).SumAsync(x => x.FactValue);
            if (val > 0)
            {
                value += val;
                continue;
            }

            val = (double)await source.Where(x => x.ReportDate == date).SumAsync(x => x.EstimatedValue);
            if (val > 0)
            {
                value += val;
                continue;
            }

            val = (double)await source.Where(x => x.ReportDate == date).SumAsync(x => x.AllocatedValue);
            value += val;
        }

        Value = value;
        Info = "Обновлено " + DateTime.Now.ToString("dd.MM.yyyy");
        if (Id > 0)
        {
            db.Update(this);
        }
        else
        {
            db.Add(this);
        }

        await db.SaveChangesAsync();
    }
}