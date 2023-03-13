using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using NPOI.SS.Formula.Functions;
using Radzen;
using Radzen.Blazor;
using SSLD.Data;
using SSLD.Data.DailyReview;
using SSLD.Tools;

namespace SSLD.Pages.DailyReview;

[Authorize(Roles = SD.Role_User)]
public partial class PageCountryForecast
{
    [Inject] public ApplicationDbContext Db { get; set; }
    [Inject] public NotificationService NotificationService { get; set; }
    [Inject] public DialogService DialogService { get; set; }
    private IQueryable<Forecast> _entities;
    private IQueryable<ForecastGisCountry> _values;
    private RadzenDataGrid<ForecastGisCountry> _valuesGrid;
    private IQueryable<Gis> _gises;
    private IList<Forecast> _selectedForecast;
    private IList<ForecastGisCountry> _selectedValue;
    private bool _editMode = false;

    protected override void OnInitialized()
    {
        _entities = Db.Forecasts
            .Include(x => x.Countries)
            .OrderByDescending(x => x.ReportDate)
            .AsQueryable();
        _gises = Db.Gises
            .Include(x => x.Countries).ThenInclude(gc => gc.Country)
            .OrderBy(x => x.Name)
            .AsQueryable();
        _selectedForecast = new List<Forecast>() { _entities.FirstOrDefault() };
    }

    private void OnSelect(Forecast forecast)
    {
        _selectedForecast = new List<Forecast>() { _entities.FirstOrDefault(x => x.Id == forecast.Id) };
        _selectedValue = new List<ForecastGisCountry>();
        _values = null;
    }
    
    private void OnSelect(GisCountry gc)
    {
        var forecast = _selectedForecast.FirstOrDefault();
        _values = Db.ForecastGisCountries
            .Where(x => x.GisCountryId == gc.Id && x.ForecastId == forecast.Id)
            .OrderBy(x => x.Month)
            .AsQueryable();
        _selectedValue = new List<ForecastGisCountry>() { _values.FirstOrDefault() };
    }

    private void EditRow(ForecastGisCountry value)
    {
        _valuesGrid.EditRow(value);
        _editMode = true;
    }

    private async Task InsertRow()
    {
        _editMode = true;
        var gc = _selectedValue.FirstOrDefault();
        if (gc == null) return;
        var val = new ForecastGisCountry{GisCountryId = gc.GisCountryId};
        await _valuesGrid.InsertRow(val);
    }

    private async Task DeleteRow(ForecastGisCountry value)
    {
        var valToDelete = _values
            .FirstOrDefault(x => x.Id == value.Id);
        var confirmation = await DialogService.Confirm($"Вы уверены, что хотите удалить данные на {value.Month}?", "Удаление",
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
                    Summary = "Данные удалены",
                    Detail = "Данные на " + value.Month + " удалены",
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
            await _valuesGrid.Reload();
        }
        else
        {
            _valuesGrid.CancelEditRow(value);
        }
    }

    private async Task SaveRow(ForecastGisCountry value)
    {
        if (value.Id > 0)
        {
            var checkDb = await _values
                .Where(x => x.GisCountryId == value.GisCountryId && x.Month == value.Month && x.Id != value.Id)
                .FirstOrDefaultAsync();
            if (checkDb == null)
            {
                await _valuesGrid.UpdateRow(value);
                Db.Update(value);
                var result = await Db.SaveChangesAsync();
                if (result < 1)
                {
                    NotificationService.Notify(
                        NotificationSeverity.Error,
                        "Ошибка сохранения",
                        "В базе данных уже есть значение на данную дату",
                        5000);
                }
            }
            else
            {
                NotificationService.Notify(
                    NotificationSeverity.Error,
                    "Ошибка сохранения",
                    "В базе данных уже есть значение на данную дату",
                    5000);
            }
        }
        else
        {
            // value.GisCountryId = _selectedGisCountry.Id;
            var checkDb = _values
                .FirstOrDefault(x => x.GisCountryId == value.GisCountryId && x.Month == value.Month);
            if (checkDb == null)
            {
                await _valuesGrid.UpdateRow(value);
                Db.Add(value);
                var result = await Db.SaveChangesAsync();
                if (result < 1)
                {
                    NotificationService.Notify(
                        NotificationSeverity.Error,
                        "Ошибка сохранения",
                        "В базе данных уже есть значение на данную дату",
                        5000);
                }
            }
            else
            {
                NotificationService.Notify(
                    NotificationSeverity.Error,
                    "Ошибка сохранения",
                    "В базе данных уже есть значение на данную дату",
                    5000);
            }
        }
        _editMode = false;
    }

    private void CancelEdit(ForecastGisCountry value)
    {
        _valuesGrid.CancelEditRow(value);
        _editMode = false;
    }

    private static void OnChangeVolume(ForecastGisCountry val, string input)
    {
        var dec = StringParser.TryGetDecimal(input);
        val.Value = dec;
    }
}