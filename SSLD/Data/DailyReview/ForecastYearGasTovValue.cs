using Microsoft.EntityFrameworkCore;
using NPOI.SS.Formula.Functions;

namespace SSLD.Data.DailyReview;

[Index(nameof(ForecastYearId), nameof(Month), IsUnique = true)]
public class ForecastYearGasTovValue : ForecastYearValue
{
    public override async Task Update(ApplicationDbContext db)
    {
        var year = ForecastYear.Year;
        var source = await db.GisCountryValues
            .Where(x => x.ReportDate.Month == Month && x.ReportDate.Year == year)
            .ToListAsync();
        var addonSource = await db.GisCountryAddonValues
            .Where(x =>
                x.GisCountryAddon.Types.Any(a => a.StartDate.Month == Month && a.IsCommGas)
                && x.ReportDate.Month == Month && x.ReportDate.Year == year)
            .ToListAsync();
        var days = DateTime.DaysInMonth(year, Month);
        double value = 0;
        for (var day = days; day > 0; day--)
        {
            var date = new DateOnly(year, Month, day);
            var val = (double) source.Where(x => x.ReportDate == date).Sum(x => x.FactValue);
            var addonVal = (double) source.Where(x => x.ReportDate == date).Sum(x => x.FactValue);
            if (val > 0)
            {
                value += val + addonVal;
                continue;
            }

            val = (double) source.Where(x => x.ReportDate == date).Sum(x => x.EstimatedValue);
            addonVal = (double) source.Where(x => x.ReportDate == date).Sum(x => x.EstimatedValue);
            if (val > 0)
            {
                value += val + addonVal;
                continue;
            }
            val = (double) source.Where(x => x.ReportDate == date).Sum(x => x.AllocatedValue);
            addonVal = (double) source.Where(x => x.ReportDate == date).Sum(x => x.AllocatedValue);
            value += val + addonVal;
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