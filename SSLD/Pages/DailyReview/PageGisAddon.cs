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
public partial class PageGisAddon
{
    [Inject] public ApplicationDbContext Db { get; set; }
    [Inject] public NotificationService NotificationService { get; set; }
    [Inject] public DialogService DialogService { get; set; }
    private IList<GisAddon> _selectedAddon;
    private IQueryable<GisAddon> _addons;
    private IQueryable<Gis> _gises;
    private List<string> _names;
    private RadzenDataGrid<GisAddon> _addonsGrid;
    private bool _watchMode = true;

    protected override Task OnInitializedAsync()
    {
        _addons = Db.GisAddons
            .Include(x => x.Gis)
            .OrderBy(x => x.Name).AsQueryable();
        _gises = Db.Gises.OrderBy(x => x.Name).AsQueryable();
        return Task.CompletedTask;
    }

    private void RowExpand(GisAddon addon)
    {
        _selectedAddon = new List<GisAddon>() { addon };
        _names = addon.Names;
        _watchMode = true;
    }

    private void OnSelect(GisAddon addon)
    {
        if (!_watchMode) return;
        RowExpand(addon);
        _addonsGrid.ExpandRow(addon);
    }

    private async Task InsertAddon()
    {
        _watchMode = false;
        await _addonsGrid.InsertRow(new GisAddon());
    }

    private async Task OnCreateAddon(GisAddon addon)
    {
        await Db.AddAsync(addon);
        var result = await Db.SaveChangesAsync();
        if (result > 0)
        {
            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Success,
                Summary = "Дополнение к ГИС создано",
                Detail = $"Дополнение {addon.Name} успешно создано",
                Duration = 3000
            });
        }
        else
        {
            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Error,
                Summary = "Ошибка создания дополнения к ГИС",
                Detail = $"Дополнение к ГИС {addon.Name} не удалось сохранить в базе данных",
                Duration = 3000
            });
        }
        _watchMode = true;
    }

    private async Task DeleteAddon(GisAddon addon)
    {
        var valToDelete = _addons
            .Include(x => x.Values)
            .FirstOrDefault(x => x.Id == addon.Id);
        var confirmation = await DialogService.Confirm($"Вы уверены, что хотите удалить дополнения к ГИС {addon.Name}?", "Удаление",
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
                    Summary = "Дополнения к ГИС удалено",
                    Detail = "Дополнение к ГИС " + addon.Name + " удалено",
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

    private void EditAddon(GisAddon addon)
    {
        _watchMode = false;
        _addonsGrid.EditRow(addon);
    }

    private async Task OnUpdateAddon(GisAddon addon)
    {
        Db.Update(addon);
        var result = await Db.SaveChangesAsync();
        if (result > 0)
        {
            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Success,
                Summary = "Дополнение к ГИС обновлено",
                Detail = "Дополнение к ГИС " + addon.Name + " успешно обновлено",
                Duration = 3000
            });
        }
        else
        {
            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Error,
                Summary = "Ошибка обновления",
                Detail = "Не удалось обновить",
                Duration = 3000
            });
        }
        _watchMode = true;
    }

    private async Task SaveAddon(GisAddon addon)
    {
        await _addonsGrid.UpdateRow(addon);
    }

    private async Task SaveAddonName(List<string> names)
    {
        var entity = _selectedAddon.FirstOrDefault();
        if (entity != null)
        {
            entity.Names = names;
            await OnUpdateAddon(entity);
        }
        _watchMode = true;
    }

    private void CancelEditAddon(GisAddon addon)
    {
        _addonsGrid.CancelEditRow(addon);
        _watchMode = true;
    }
}