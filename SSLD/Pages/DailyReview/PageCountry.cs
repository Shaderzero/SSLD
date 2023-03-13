using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Radzen;
using Radzen.Blazor;
using SSLD.Data;
using SSLD.Data.DailyReview;
using SSLD.Tools;

namespace SSLD.Pages.DailyReview;

[Authorize(Roles = SD.Role_User)]
public partial class PageCountry
{
    [Inject] public ApplicationDbContext Db { get; set; }
    [Inject] public NotificationService NotificationService { get; set; }
    [Inject] public DialogService DialogService { get; set; }
    private List<Country> _selectedCountry = new();
    private IQueryable<Country> _countries;
    private List<string> _names;
    private RadzenDataGrid<Country> _countriesGrid;
    private bool _watchMode = true;

    protected override Task OnInitializedAsync()
    {
        _countries = Db.Countries
            .Include(x => x.GisCountries).ThenInclude(x => x.Gis)
            .OrderBy(x => x.Name).AsQueryable();
        return Task.CompletedTask;
    }

    private void RowExpand(Country country)
    {
        _names = country.Names;
        _watchMode = true;
    }

    private void OnSelect(Country country)
    {
        _selectedCountry = new List<Country>() { country };
        if (country.Id <= 0 || !_watchMode) return;
        RowExpand(country);
        _countriesGrid.ExpandRow(country);
    }

    private async Task InsertRow()
    {
        _watchMode = false;
        await _countriesGrid.InsertRow(new Country());
    }

    private async Task SaveNames(List<string> names)
    {
        var country = _selectedCountry.FirstOrDefault();
        if (country != null)
        {
            country.Names = names;
            await OnUpdateRow(country);
        }
        _watchMode = true;
    }

    private async Task OnCreateRow(Country country)
    {
        await Db.AddAsync(country);
        var result = await Db.SaveChangesAsync();
        if (result > 0)
        {
            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Success,
                Summary = "Страна создана",
                Detail = "Страна " + country.Name + " успешно создана",
                Duration = 3000
            });
        }
        else
        {
            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Error,
                Summary = "Ошибка создания страны",
                Detail = "Страну " + country.Name + " не удалось сохранить в базе данных",
                Duration = 3000
            });
        }
        _watchMode = true;
    }

    private async Task DeleteRow(Country country)
    {
        var valToDelete = _countries
            .FirstOrDefault(x => x.Id == country.Id);
        var confirmation = await DialogService.Confirm($"Вы уверены, что хотите удалить страну {country.Name}?", "Удаление",
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
                    Summary = "Страна удалена",
                    Detail = "Страна " + country.Name + " удалена",
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
            await _countriesGrid.Reload();
        }
        else
        {
            _countriesGrid.CancelEditRow(country);
        }
    }

    private void EditRow(Country country)
    {
        _watchMode = false;
        _countriesGrid.EditRow(country);
    }

    private async Task OnUpdateRow(Country country)
    {
        Db.Update(country);
        var result = await Db.SaveChangesAsync();
        if (result > 0)
        {
            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Success,
                Summary = "Страна обновлена",
                Detail = "Страна " + country.Name + " успешно обновлена",
                Duration = 3000
            });
        }
        else
        {
            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Error,
                Summary = "Ошибка обновления страны",
                Detail = "Страну " + country.Name + " не удалось обновить",
                Duration = 3000
            });
        }
        _watchMode = true;
    }

    private async Task SaveRow(Country country)
    {
        await _countriesGrid.UpdateRow(country);
    }

    private void CancelEdit(Country country)
    {
        _countriesGrid.CancelEditRow(country);
        _watchMode = true;
    }

}