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
public partial class PageCountryAddon
{
    [Inject] public ApplicationDbContext Db { get; set; }
    [Inject] public NotificationService NotificationService { get; set; }
    [Inject] public DialogService DialogService { get; set; }
    private IList<GisCountryAddon> _selectedAddon;
    private Gis _selectedGis;
    private IQueryable<Gis> _gises;
    private IQueryable<GisCountry> _countries;
    private IQueryable<GisCountryAddon> _addons;
    private IQueryable<GisCountryAddonType> _addonTypes;
    private List<string> _names;
    private RadzenDataGrid<GisCountryAddon> _addonsGrid;
    private RadzenDataGrid<GisCountryAddonType> _addonTypesGrid;
    private bool _watchMode = true;

    protected override Task OnInitializedAsync()
    {
        _addons = Db.GisCountryAddons
            .Include(x => x.GisCountry).ThenInclude(x => x.Gis)
            .Include(x => x.GisCountry).ThenInclude(x => x.Country)
            .OrderBy(x => x.Name).AsQueryable();
        return Task.CompletedTask;
    }

    private void RowExpand(GisCountryAddon addon)
    {
        _selectedAddon = new List<GisCountryAddon>() { addon };
        _addonTypes = Db.GisCountryAddonTypes.Where(x => x.GisCountryAddonId == addon.Id).AsQueryable();
        _names = addon.Names;
        _watchMode = true;
    }

    private void OnSelectRow(GisCountryAddon addon)
    {
        if (addon.Id <= 0 || !_watchMode) return;
        RowExpand(addon);
        _addonsGrid.ExpandRow(addon);
    }

    private Task OnChangeGis(Gis gis)
    {
        _countries = Db.GisCountries
            .Include(x => x.Country)
            .Where(c => c.GisId == gis.Id)
            .OrderBy(x => x.Country.Name)
            .AsQueryable();
        return Task.CompletedTask;
    }

    private async Task SaveNames(List<string> names)
    {
        var entity = _selectedAddon.FirstOrDefault();
        if (entity != null)
        {
            entity.Names = names;
            await OnUpdateRow(entity);
        }
        _watchMode = true;
    }

    private async Task InsertAddon()
    {
        _watchMode = false;
        _gises = Db.Gises
            .OrderBy(x => x.Name).AsQueryable();
        await _addonsGrid.InsertRow(new GisCountryAddon());
    }

    private async Task InsertAddonType()
    {
        var addon = _selectedAddon.FirstOrDefault();
        if (addon == null) return;
        _watchMode = false;
        var newValue = new GisCountryAddonType()
        {
            GisCountryAddonId = addon.Id,
            StartDate = new DateOnly(DateTime.Now.Year, DateTime.Now.Month, 1)
        };
        await _addonTypesGrid.InsertRow(newValue);
    }

    private async Task OnCreateRow(GisCountryAddon addon)
    {
        await Db.AddAsync(addon);
        var result = await Db.SaveChangesAsync();
        if (result > 0)
        {
            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Success,
                Summary = "Дополнение создано",
                Detail = "Дополнение " + addon.Name + " успешно создана",
                Duration = 3000
            });
        }
        else
        {
            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Error,
                Summary = "Ошибка создания дополнения",
                Detail = "Дополнение " + addon.Name + " не удалось сохранить в базе данных",
                Duration = 3000
            });
        }
        _watchMode = true;
    }

    private async Task OnCreateRow(GisCountryAddonType type)
    {
        Db.Add(type);
        var result = await Db.SaveChangesAsync();
        if (result > 0)
        {
            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Success,
                Summary = "Учет для дополнения добавлен",
                Detail = $"Учет на дату {type.StartDate} успешно создан",
                Duration = 3000
            });
        }
        else
        {
            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Error,
                Summary = "Ошибка создания учета для дополнения",
                Detail = $"Учет на дату {type.StartDate} не удалось сохранить в базе данных",
                Duration = 3000
            });
        }
        _watchMode = true;
    }

    private async Task DeleteRow(GisCountryAddon addon)
    {
        var valToDelete = _addons
            .FirstOrDefault(x => x.Id == addon.Id);
        var confirmation = await DialogService.Confirm($"Вы уверены, что хотите удалить дополнение {addon.Name}?", "Удаление",
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
                    Summary = "Дополнение удалено",
                    Detail = "Дополнение " + addon.Name + " удалено",
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
            await _addonsGrid.Reload();
        }
        else
        {
            _addonsGrid.CancelEditRow(addon);
        }
    }

    private async Task DeleteRow(GisCountryAddonType type)
    {
        var valToDelete = await Db.GisCountryAddonTypes.FirstOrDefaultAsync(x => x.Id == type.Id);
        if (valToDelete == null)
        {
            _addonTypesGrid.CancelEditRow(type);
            return;
        }
        var confirmation = await DialogService.Confirm($"Вы уверены, что хотите удалить учет на дату {valToDelete.StartDate}?", "Удаление",
            new ConfirmOptions() { OkButtonText = "Да", CancelButtonText = "Отмена" });
        if (confirmation == true)
        {
            Db.Remove(valToDelete);
            var result = await Db.SaveChangesAsync();
            if (result > 0)
            {
                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Success,
                    Summary = "Учет для дополнения удален",
                    Detail = $"Учет на дату {type.StartDate} удален",
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
            await _addonTypesGrid.Reload();
        }
        else
        {
            _addonTypesGrid.CancelEditRow(type);
        }
    }

    private void EditRow(GisCountryAddon addon)
    {
        _watchMode = false;
        _addonsGrid.EditRow(addon);
    }

    private void EditRow(GisCountryAddonType type)
    {
        _watchMode = false;
        _addonTypesGrid.EditRow(type);
    }

    private async Task OnUpdateRow(GisCountryAddon addon)
    {
        Db.Update(addon);
        var result = await Db.SaveChangesAsync();
        if (result > 0)
        {
            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Success,
                Summary = "Дополнение обновлено",
                Detail = "Дополнение " + addon.Name + " успешно обновлено",
                Duration = 3000
            });
        }
        else
        {
            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Error,
                Summary = "Ошибка обновления дополнения",
                Detail = "Дополнение " + addon.Name + " не удалось обновить",
                Duration = 3000
            });
        }
        _watchMode = true;
    }

    private async Task OnUpdateRow(GisCountryAddonType type)
    {
        Db.Update(type);
        var result = await Db.SaveChangesAsync();
        if (result > 0)
        {
            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Success,
                Summary = "Учет обновлен",
                Detail = $"Учет на дату {type.StartDate} успешно обновлен",
                Duration = 3000
            });
        }
        else
        {
            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Error,
                Summary = "Ошибка обновления учета",
                Detail = $"Учет на дату {type.StartDate} не удалось обновить",
                Duration = 3000
            });
        }
        _watchMode = true;
    }

    private async Task SaveRow(GisCountryAddon addon)
    {
        await _addonsGrid.UpdateRow(addon);
    }

    private async Task SaveRow(GisCountryAddonType type)
    {
        await _addonTypesGrid.UpdateRow(type);
    }

    private void CancelEdit(GisCountryAddon addon)
    {
        _addonsGrid.CancelEditRow(addon);
        _watchMode = true;
    }

    private void CancelEdit(GisCountryAddonType type)
    {
        _addonTypesGrid.CancelEditRow(type);
        _watchMode = true;
    }

}