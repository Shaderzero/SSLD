using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Radzen;
using Radzen.Blazor;
using SSLD.Data;
using SSLD.Data.DailyReview;
using SSLD.DTO;
using SSLD.Tools;

namespace SSLD.Pages.DailyReview;

[Authorize(Roles = SD.Role_User)]
public partial class PageForecast
{
    [Inject] public ApplicationDbContext Db { get; set; }
    [Inject] public NotificationService NotificationService { get; set; }
    [Inject] public DialogService DialogService { get; set; }
    private IList<Forecast> _selectedForecast;
    private IList<ForecastTableItem> _tableItems;
    private IQueryable<Forecast> _entities;
    private IQueryable<Gis> _gises;
    private RadzenDataGrid<Forecast> _entitiesGrid;

    protected override Task OnInitializedAsync()
    {
        _gises = Db.Gises
            .Include(x => x.Countries).ThenInclude(c => c.Country)
            .OrderBy(x => x.DailyReviewOrder).AsQueryable();
        _entities = Db.Forecasts
            .Include(x => x.ForecastYear)
            .Include(x => x.Countries)
            .AsQueryable();
        return Task.CompletedTask;
    }

    private Task RowExpand(Forecast forecast)
    {
        _selectedForecast = new List<Forecast>() { forecast };
        GenerateTableItems();
        return Task.CompletedTask;
    }

    private void GenerateTableItems()
    {
        _tableItems = new List<ForecastTableItem>();
        var forecast = _selectedForecast.FirstOrDefault();
        if (forecast == null) return;
        foreach (var gis in _gises)
        {
            var gisItem = new ForecastTableItem(gis.Name, ForecastTableItem.ForecastType.Dir);
            foreach (var country in gis.Countries)
            {
                var monthValues = forecast.Countries.Where(x => x.GisCountryId == country.Id).ToList();
                if (monthValues.Count == 0 || monthValues.Sum(x => x.Value) == 0) continue;
                var countryItem = new ForecastTableItem(country.Country.Name, monthValues);
                _tableItems.Add(countryItem);
                gisItem.AddMonthValues(monthValues);
            }
            if (gisItem.Year > 0) _tableItems.Add(gisItem);
        }
    }

    private async Task OnSelect(Forecast forecast)
    {
        if (forecast.Id > 0)
        {
            await RowExpand(forecast);
            await _entitiesGrid.ExpandRow(forecast);
        }
    }

    private async Task DeleteEntity(Forecast forecast)
    {
        var valToDelete = Db.Forecasts
            .Include(x => x.Countries)
            .Include(x => x.InputFileLog)
            .FirstOrDefault(x => x.Id == forecast.Id);
        var confirmation = await DialogService.Confirm($"Вы уверены, что хотите удалить Прогноз баланса {forecast.Name}?", "Удаление",
            new ConfirmOptions() { OkButtonText = "Да", CancelButtonText = "Отмена" });
        if (valToDelete != null && confirmation == true)
        {
            Db.Remove(valToDelete);
            var result = await Db.SaveChangesAsync();
            if (result > 0)
            {
                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Success,
                    Summary = "Объект удален",
                    Detail = "",
                    Duration = 3000
                });
            }
            else
            {
                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Error,
                    Summary = "Ошибка удаления",
                    Detail = "",
                    Duration = 3000
                });
            }
            await _entitiesGrid.Reload();
        }
        else
        {
            _entitiesGrid.CancelEditRow(forecast);
        }
    }

    private void EditEntity(Forecast forecast)
    {
        _entitiesGrid.EditRow(forecast);
    }

    private async Task OnUpdateRow(Forecast forecast)
    {
        if (forecast.InMain)
        {
            var forecasts = await Db.Forecasts.Where(x =>
                x.ForecastYear.Year == forecast.ForecastYear.Year &&
                x.InMain == true &&
                x.Id != forecast.Id).ToListAsync();
            forecasts.ForEach(x => x.InMain = false);
            Db.UpdateRange(forecasts);
        }
        Db.Update(forecast);
        var result = await Db.SaveChangesAsync();
        if (result > 0)
        {
            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Success,
                Summary = "ГИС обновлен",
                Detail = "",
                Duration = 3000
            });
        }
        else
        {
            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Error,
                Summary = "Ошибка обновления ГИС",
                Detail = "",
                Duration = 3000
            });
        }
    }

    private async Task Save(Forecast forecast)
    {
        await _entitiesGrid.UpdateRow(forecast);
    }

    private void CancelEdit(Forecast forecast)
    {
        _entitiesGrid.CancelEditRow(forecast);
    }
}