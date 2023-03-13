using Microsoft.EntityFrameworkCore;

namespace SSLD.Data.DailyReview;

[Index(nameof(ForecastYearId), nameof(Month), IsUnique = true)]
public class ForecastYearChinaValue : ForecastYearValue
{
    public override async Task Update(ApplicationDbContext db)
    {
        var china = await db.GisCountries.FirstOrDefaultAsync(x => x.Country.Names.Any(n => n.Equals("Китай")));
        var source = db.GisCountryValues
            .Where(x => x.GisCountryId == china.Id && x.ReportDate.Month == Month &&
                        x.ReportDate.Year == ForecastYear.Year)
            .AsQueryable();
        var valFact = await source
            .SumAsync(x => x.FactValue);
        Value = (double) valFact;
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