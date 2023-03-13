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
public partial class PageGis
{
    [Inject] public ApplicationDbContext Db { get; set; }
    [Inject] public NotificationService NotificationService { get; set; }
    [Inject] public DialogService DialogService { get; set; }
    private IList<Gis> _selectedGis;
    private IQueryable<Country> _countries;
    private IQueryable<Gis> _gises;
    private IQueryable<GisCountry> _gisCountries;
    private List<string> _gisNames;
    private List<string> _gisInputNames;
    private List<string> _gisOutputNames;
    private RadzenDataGrid<Gis> _gisGrid;
    private RadzenDataGrid<GisCountry> _gisCountryGrid;
    private bool _watchMode = true;
    private IList<int> _gisOrder;

    protected override Task OnInitializedAsync()
    {
        _gises = Db.Gises.OrderBy(x => x.Name).AsQueryable();
        _gisOrder = new List<int>();
        for (var i = 1; i < _gises.Count(); i++)
        {
            _gisOrder.Add(i);
        }
        return Task.CompletedTask;
    }

    private Task RowExpand(Gis gis)
    {
        _selectedGis = new List<Gis>() { gis };
        _gisCountries = Db.GisCountries
            .Include(x => x.Country)
            .Where(x => x.GisId == gis.Id)
            .AsQueryable();
        _gisNames = gis.Names;
        _gisInputNames = gis.GisInputNames;
        _gisOutputNames = gis.GisOutputNames;
        _watchMode = true;
        return Task.CompletedTask;
    }

    private async Task OnSelect(Gis gis)
    {
        if (!_watchMode) return;
        if (gis.Id > 0)
        {
            await RowExpand(gis);
            await _gisGrid.ExpandRow(gis);
        }
    }

    private async Task InsertGis()
    {
        _watchMode = false;
        var newGis = new Gis();
        await _gisGrid.InsertRow(newGis);
    }

    private async Task InsertGisCountry()
    {
        _watchMode = false;
        var gisId = _selectedGis.FirstOrDefault()!.Id;
        _countries = Db.Countries
            .Where(x => x.GisCountries.All(gisCountry => gisCountry.GisId != gisId))
            .OrderBy(x => x.Name).AsQueryable();
        await _gisCountryGrid.InsertRow(new GisCountry()
        {
            GisId = gisId
        });
    }

    private async Task OnCreateGis(Gis gis)
    {
        await Db.AddAsync(gis);
        var result = await Db.SaveChangesAsync();
        if (result > 0)
        {
            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Success,
                Summary = "ГИС создан",
                Detail = "ГИС " + gis.Name + " успешно создан",
                Duration = 3000
            });
        }
        else
        {
            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Error,
                Summary = "Ошибка создания ГИС",
                Detail = "ГИС " + gis.Name + " не удалось сохранить в базе данных",
                Duration = 3000
            });
        }
        _watchMode = true;
    }

    private async Task OnCreateGisCountry(GisCountry gc)
    {
        Db.Add(gc);
        var result = await Db.SaveChangesAsync();
        if (result > 0)
        {
            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Success,
                Summary = "В ПХГ добавлена страна",
                Detail = "Страна " + gc.Country.Name + " успешно добавлена",
                Duration = 3000
            });
        }
        else
        {
            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Error,
                Summary = "Ошибка добавления страны в ПХГ",
                Detail = "Страну " + gc.Country.Name + " не удалось сохранить в базе данных",
                Duration = 3000
            });
        }
        _watchMode = true;
    }

    private async Task DeleteGis(Gis gis)
    {
        var valToDelete = _gises
            .Include(x => x.Countries).ThenInclude(x => x.Values)
            .Include(x => x.Countries).ThenInclude(x => x.Addons).ThenInclude(x => x.Types)
            .Include(x => x.Countries).ThenInclude(x => x.Addons).ThenInclude(x => x.Values)
            .Include(x => x.Addons).ThenInclude(x => x.Values)
            .Include(x => x.GisInputValues)
            .Include(x => x.GisOutputValues)
            .FirstOrDefault(x => x.Id == gis.Id);
        var confirmation = await DialogService.Confirm($"Вы уверены, что хотите удалить ГИС {gis.Name}?", "Удаление",
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
                    Summary = "ГИС удален",
                    Detail = "ГИС " + gis.Name + " удален",
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
            await _gisGrid.Reload();
        }
        else
        {
            _gisGrid.CancelEditRow(gis);
        }
    }

    private async Task DeleteGisCountry(GisCountry gc)
    {
        var gis = _selectedGis.FirstOrDefault();
        if (gis == null)
        {
            _gisCountryGrid.CancelEditRow(gc);
            return;
        }
        var valToDelete = gis.Countries
            .FirstOrDefault(x => x.Id == gc.Id);
        var confirmation = await DialogService.Confirm($"Вы уверены, что хотите удалить страну {gc.Country.Name} в ГИС {gis.Name}?", "Удаление",
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
                    Summary = "Удалено",
                    Detail = $"Страна {gc.Country.Name} в ГИС {gis.Name} удалена",
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
            await _gisCountryGrid.Reload();
        }
        else
        {
            _gisCountryGrid.CancelEditRow(gc);
        }
    }

    private void EditGis(Gis gis)
    {
        _watchMode = false;
        _gisGrid.EditRow(gis);
    }

    private void EditGisCountry(GisCountry gc)
    {
        _watchMode = false;
        _gisCountryGrid.EditRow(gc);
    }

    private async Task OnUpdateRow(Gis gis)
    {
        Db.Update(gis);
        var result = await Db.SaveChangesAsync();
        if (result > 0)
        {
            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Success,
                Summary = "ГИС обновлен",
                Detail = "ГИС " + gis.Name + " успешно обновлена",
                Duration = 3000
            });
        }
        else
        {
            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Error,
                Summary = "Ошибка обновления ГИС",
                Detail = "ГИС " + gis.Name + " не удалось обновить",
                Duration = 3000
            });
        }
        _watchMode = true;
    }

    private async Task OnUpdateGisCountry(GisCountry gc)
    {
        Db.Update(gc);
        var result = await Db.SaveChangesAsync();
        if (result > 0)
        {
            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Success,
                Summary = "Страна обновлена",
                Detail = $"Страна {gc.Country.Name} для ГИС {_selectedGis.FirstOrDefault().Name} успешно обновлена",
                Duration = 3000
            });
        }
        else
        {
            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Error,
                Summary = "Ошибка обновления",
                Detail = "",
                Duration = 3000
            });
        }
        _watchMode = true;
    }

    private async Task SaveGis(Gis gis)
    {
        await _gisGrid.UpdateRow(gis);
    }

    private async Task SaveGisName(List<string> names)
    {
        var entity = _selectedGis.FirstOrDefault();
        if (entity != null)
        {
            entity.Names = names;
            await OnUpdateRow(entity);
        }
        _watchMode = true;
    }

    private async Task SaveGisInputName(List<string> names)
    {
        var entity = _selectedGis.FirstOrDefault();
        if (entity != null)
        {
            entity.GisInputNames = names;
            await OnUpdateRow(entity);
        }
        _watchMode = true;
    }

    private async Task SaveGisOutputName(List<string> names)
    {
        var entity = _selectedGis.FirstOrDefault();
        if (entity != null)
        {
            entity.GisOutputNames = names;
            await OnUpdateRow(entity);
        }
        _watchMode = true;
    }

    private async Task SaveGisCountry(GisCountry gc)
    {
        await _gisCountryGrid.UpdateRow(gc);
    }

    private void CancelEditGis(Gis gis)
    {
        _gisGrid.CancelEditRow(gis);
        _watchMode = true;
    }

    private void CancelEditGisCountry(GisCountry gc)
    {
        _gisCountryGrid.CancelEditRow(gc);
        _watchMode = true;
    }
}