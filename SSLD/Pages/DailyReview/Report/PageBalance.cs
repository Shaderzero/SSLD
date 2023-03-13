using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Radzen;
using SSLD.Data;
using SSLD.Data.DailyReview;

namespace SSLD.Pages.DailyReview.Report;

public partial class PageBalance
{
    [Inject] public ApplicationDbContext Db { get; set; }
    [Inject] public NotificationService NotificationService { get; set; }
    [Inject] public DialogService DialogService { get; set; }
    // private List<ForecastYear> _forecastYears;
    private int _year = 2022;
    private ForecastYear _firstYear;
    private ForecastYear _secondYear;
    private ForecastYear _thirdYear;

    protected override async Task OnInitializedAsync()
    {
        _firstYear = await Db.ForecastYears
            .Where(x => x.Forecasts.Any(f => f.InDayReview) && x.Year == _year)
            .Include(x => x.Forecasts).ThenInclude(f => f.Countries)
            .FirstOrDefaultAsync();
        _secondYear = await Db.ForecastYears
            .Where(x => x.Forecasts.Any(f => f.InDayReview) && x.Year == (_year - 1))
            .Include(x => x.Forecasts).ThenInclude(f => f.Countries)
            .FirstOrDefaultAsync();
        _thirdYear = await Db.ForecastYears
            .Where(x => x.Forecasts.Any(f => f.InDayReview) && x.Year == (_year - 2))
            .Include(x => x.Forecasts).ThenInclude(f => f.Countries)
            .FirstOrDefaultAsync();
    }

    private async Task UpdateGasTovValue(ForecastYear fy, int index)
    {
        var result = await DialogService.OpenAsync<ModalConfirmOption>(null,
            new Dictionary<string, object>() { { "Title", "Вы хотите изменить значение вручную или обновить из базы данных?" } } ,
            new DialogOptions() { ShowTitle = false, Width = "400px", Height = "150px", Resizable = false, Draggable = true });
        if (result == null)
        {
            DialogService.Close();
            return;
        }

        var value = 0d;
        
        if (result == "update")
        {
            var year = fy.Year;
            var month = index + 1;
            var source = await Db.GisCountryValues
                .Where(x => x.ReportDate.Month == month && x.ReportDate.Year == year)
                .ToListAsync();
            var addonSource = await Db.GisCountryAddonValues
                .Where(x =>
                    x.GisCountryAddon.Types.Any(a => a.StartDate.Month == month && a.IsCommGas)
                    && x.ReportDate.Month == month && x.ReportDate.Year == year)
                .ToListAsync();
            var days = DateTime.DaysInMonth(year, month);
            for (var day = days; day > 0; day--)
            {
                var date = new DateOnly(year, month, day);
                var val = (double) source.Where(x => x.ReportDate == date).Sum(x => x.FactValue);
                var addonVal = (double) addonSource.Where(x => x.ReportDate == date).Sum(x => x.FactValue);
                if (val > 0)
                {
                    value += val + addonVal;
                    continue;
                }

                val = (double) source.Where(x => x.ReportDate == date).Sum(x => x.EstimatedValue);
                addonVal = (double) addonSource.Where(x => x.ReportDate == date).Sum(x => x.EstimatedValue);
                if (val > 0)
                {
                    value += val + addonVal;
                    continue;
                }
                val = (double) source.Where(x => x.ReportDate == date).Sum(x => x.AllocatedValue);
                addonVal = (double) addonSource.Where(x => x.ReportDate == date).Sum(x => x.AllocatedValue);
                value += val + addonVal;
            }
        }
        
        var confirmResult = await DialogService.OpenAsync<ModalBalanceValueChange>(null,
            new Dictionary<string, object>()
            {
                { "Title", "Изменяем значение" },
                { "Value", fy.GasTovValues[index] },
                { "NewValue", value }
            }  ,
            new DialogOptions() { ShowTitle = false, Width = "400px", Height = "200px", Resizable = false, Draggable = true });

        if (confirmResult is double newValue)
        {
            fy.GasTovValues[index] = newValue;
            fy.GasTovTxts[index] = "Обновлено " + DateTime.Now.ToString("dd.MM.yyyy");
            Db.Update(fy);
            await Db.SaveChangesAsync();
        }
    }
    
    private async Task UpdateGpsValue(ForecastYear fy, int index)
    {
        var result = await DialogService.OpenAsync<ModalConfirmOption>(null,
            new Dictionary<string, object>() { { "Title", "Вы хотите изменить значение вручную или обновить из базы данных?" } } ,
            new DialogOptions() { ShowTitle = false, Width = "400px", Height = "150px", Resizable = false, Draggable = true });
        if (result == null)
        {
            DialogService.Close();
            return;
        }

        var value = 0d;
        
        if (result == "update")
        {
            var year = fy.Year;
            var month = index + 1;
            //узнать весь ли ГПШ показываем или только НЕ коммерческий
            var source = await Db.GisCountryAddonValues
                .Where(x => x.ReportDate.Month == month && x.ReportDate.Year == year)
                .ToListAsync();
            var addonSource = await Db.GisCountryAddonValues
                .Where(x =>
                    x.GisCountryAddon.Types.Any(a => a.StartDate.Month == month && a.IsCommGas)
                    && x.ReportDate.Month == month && x.ReportDate.Year == year)
                .ToListAsync();
            var days = DateTime.DaysInMonth(year, month);
            for (var day = days; day > 0; day--)
            {
                var date = new DateOnly(year, month, day);
                var val = (double) source.Where(x => x.ReportDate == date).Sum(x => x.FactValue);
                if (val > 0)
                {
                    value += val;
                    continue;
                }

                val = (double) source.Where(x => x.ReportDate == date).Sum(x => x.EstimatedValue);
                if (val > 0)
                {
                    value += val;
                    continue;
                }
                val = (double) source.Where(x => x.ReportDate == date).Sum(x => x.AllocatedValue);
                value += val;
            }
        }
        
        var confirmResult = await DialogService.OpenAsync<ModalBalanceValueChange>(null,
            new Dictionary<string, object>()
            {
                { "Title", "Изменяем значение" },
                { "Value", fy.GpsValues[index] },
                { "NewValue", value }
            }  ,
            new DialogOptions() { ShowTitle = false, Width = "400px", Height = "200px", Resizable = false, Draggable = true });

        if (confirmResult is double newValue)
        {
            fy.GpsValues[index] = newValue;
            fy.GpsTxts[index] = "Обновлено " + DateTime.Now.ToString("dd.MM.yyyy");
            Db.Update(fy);
            await Db.SaveChangesAsync();
        }
    }
    
    private async Task UpdateChinaValue(ForecastYear fy, int index)
    {
        var result = await DialogService.OpenAsync<ModalConfirmOption>(null,
            new Dictionary<string, object>() { { "Title", "Вы хотите изменить значение вручную или обновить из базы данных?" } } ,
            new DialogOptions() { ShowTitle = false, Width = "400px", Height = "150px", Resizable = false, Draggable = true });
        if (result == null)
        {
            DialogService.Close();
            return;
        }

        var value = 0d;
        
        if (result == "update")
        {
            var year = fy.Year;
            var month = index + 1;
            var source = await Db.GisCountryValues
                .Where(x => x.ReportDate.Month == month && 
                            x.ReportDate.Year == year &&
                            x.GisCountry.Country.Names.Any(n => n == "Китай"))
                .ToListAsync();
            var days = DateTime.DaysInMonth(year, month);
            for (var day = days; day > 0; day--)
            {
                var date = new DateOnly(year, month, day);
                var val = (double) source.Where(x => x.ReportDate == date).Sum(x => x.FactValue);
                if (val > 0)
                {
                    value += val;
                    continue;
                }

                val = (double) source.Where(x => x.ReportDate == date).Sum(x => x.EstimatedValue);
                if (val > 0)
                {
                    value += val;
                    continue;
                }
                val = (double) source.Where(x => x.ReportDate == date).Sum(x => x.AllocatedValue);
                value += val;
            }
        }
        
        var confirmResult = await DialogService.OpenAsync<ModalBalanceValueChange>(null,
            new Dictionary<string, object>()
            {
                { "Title", "Изменяем значение" },
                { "Value", fy.ChinaValues[index] },
                { "NewValue", value }
            }  ,
            new DialogOptions() { ShowTitle = false, Width = "400px", Height = "200px", Resizable = false, Draggable = true });

        if (confirmResult is double newValue)
        {
            fy.ChinaValues[index] = newValue;
            fy.ChinaTxts[index] = "Обновлено " + DateTime.Now.ToString("dd.MM.yyyy");
            Db.Update(fy);
            await Db.SaveChangesAsync();
        }
    }
    
    private async Task UpdateGasPhgTovValue(ForecastYear fy, int index)
    {
        var value = 0d;
        var confirmResult = await DialogService.OpenAsync<ModalBalanceValueChange>(null,
            new Dictionary<string, object>()
            {
                { "Title", "Изменяем значение" },
                { "Value", fy.GasPhgValues[index] },
                { "NewValue", value }
            }  ,
            new DialogOptions() { ShowTitle = false, Width = "400px", Height = "200px", Resizable = false, Draggable = true });

        if (confirmResult is double newValue)
        {
            fy.GasPhgValues[index] = newValue;
            fy.GasPhgTxts[index] = "Обновлено " + DateTime.Now.ToString("dd.MM.yyyy");
            Db.Update(fy);
            await Db.SaveChangesAsync();
        }
    }
    
    private async Task UpdateRepoValue(ForecastYear fy, int index)
    {
        var value = 0d;
        var confirmResult = await DialogService.OpenAsync<ModalBalanceValueChange>(null,
            new Dictionary<string, object>()
            {
                { "Title", "Изменяем значение" },
                { "Value", fy.GasPhgValues[index] },
                { "NewValue", value }
            }  ,
            new DialogOptions() { ShowTitle = false, Width = "400px", Height = "200px", Resizable = false, Draggable = true });

        if (confirmResult is double newValue)
        {
            fy.RepoValues[index] = newValue;
            fy.RepoTxts[index] = "Обновлено " + DateTime.Now.ToString("dd.MM.yyyy");
            Db.Update(fy);
            await Db.SaveChangesAsync();
        }
    }

    private async Task UpdateValue(Forecast value)
    {
        for (var i = 0; i < 12; i++)
        {
            value.Values[i] = (double) value.Countries.Where(x => x.Month == i + 1).Sum(x => x.Value) / 1000;
        }

        Db.Update(value);
        await Db.SaveChangesAsync();
    }
    
    private async Task SetValue(ForecastYearValue value, double val)
    {
        value.Value = val;
        Db.Update(value);
        await Db.SaveChangesAsync();
    }

    private async Task CreateForecastYear()
    {
        await DialogService.OpenAsync<ModalBalanceYear>(null,
            null ,
            new DialogOptions() { ShowTitle = false, Width = "300px", Height = "100px", Resizable = false, Draggable = true });
    }
}