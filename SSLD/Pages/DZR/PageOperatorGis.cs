using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Radzen;
using Radzen.Blazor;
using SSLD.Data;
using SSLD.Data.DZZR;
using SSLD.Tools;

namespace SSLD.Pages.DZR;

[Authorize(Roles = SD.Role_User)]
public partial class PageOperatorGis
{
    //[Parameter] public int? Id { get; set; }
    [Inject] public NavigationManager NavigationManager { get; set; }
    [Inject] public ApplicationDbContext Db { get; set; }
    [Inject] public NotificationService NotificationService { get; set; }
    [Inject] public DialogService DialogService { get; set; }
    private IList<OperatorGis> _selectedGis;
    private IQueryable<OperatorGis> _gises;
    private RadzenDataGrid<OperatorGis> _gisGrid;
    private bool _watchMode = true;

    protected override Task OnInitializedAsync()
    {
        _gises = Db.OperatorGises.OrderBy(x => x.Name).AsQueryable();
        return Task.CompletedTask;
    }

    private Task RowExpand(OperatorGis gis)
    {
        _selectedGis = new List<OperatorGis>() { gis };
        _watchMode = true;
        return Task.CompletedTask;
    }

    private async Task OnSelect(OperatorGis gis)
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
        var newGis = new OperatorGis();
        await _gisGrid.InsertRow(newGis);
    }

    private async Task OnCreateGis(OperatorGis gis)
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

    private async Task DeleteGis(OperatorGis gis)
    {
        var valToDelete = _gises
            .Include(x => x.Resources)
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

    private void EditGis(OperatorGis gis)
    {
        _watchMode = false;
        _gisGrid.EditRow(gis);
    }

    private async Task OnUpdateGis(OperatorGis gis)
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

    private async Task SaveGis(OperatorGis gis)
    {
        await _gisGrid.UpdateRow(gis);
    }

    private void CancelEditGis(OperatorGis gis)
    {
        _gisGrid.CancelEditRow(gis);
        _watchMode = true;
    }
}