using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using Radzen;
using Radzen.Blazor;
using Microsoft.AspNetCore.Authorization;
using Microsoft.JSInterop;
using SSLD.Tools;
using SSLD.Data;
using SSLD.Data.DailyReview;

namespace SSLD.Pages.DailyReview;

[Authorize(Roles = SD.Role_User)]
public partial class PageGisCountryDetail
{
    [Inject] public ApplicationDbContext Db { get; set; }
    [Inject] public NotificationService NotificationService { get; set; }
    [Inject] public DialogService DialogService { get; set; }
    [Inject] public AuthenticationStateProvider AuthProvider { get; set; }
    [Inject] public IJSRuntime Js { get; set; }
    [Parameter] public GisCountry Country { get; set; }
    [Parameter] public DateTime StartDate { get; set; }
    [Parameter] public DateTime FinishDate { get; set; }
    private IQueryable<GisCountryValue> _values;
    private string _log;

    protected override Task OnParametersSetAsync()
    {
        _values = Db.GisCountryValues
            .OrderByDescending(v => v.ReportDate)
            .Where(x => x.GisCountryId == Country.Id)
            .AsQueryable();
        return Task.CompletedTask;
    }

    private async Task DeleteRow(long id)
    {
        var valToDelete = _values
            .FirstOrDefault(x => x.Id == id);
        if (valToDelete == null) return;
        var confirmation = await DialogService.Confirm(
            $"Вы уверены, что хотите удалить запись за {valToDelete.ReportDate}?", "Удаление",
            new ConfirmOptions() {OkButtonText = "Да", CancelButtonText = "Отмена"});
        if (confirmation == true)
        {
            Db.Remove(valToDelete);
            var result = await Db.SaveChangesAsync();
            if (result > 0)
            {
                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Success,
                    Summary = "Значения удалены",
                    Detail = "Значения на дату " + valToDelete.ReportDate + " удалены",
                    Duration = 3000
                });
            }
            else
            {
                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Error,
                    Summary = "Ошибка удаления значений",
                    Detail = "",
                    Duration = 3000
                });
            }
        }
    }

    private async Task SaveRow(DayValue value)
    {
        var dateNotUnique = await _values.AnyAsync(x => x.ReportDate == value.ReportDate && x.Id != value.Id);
        if (dateNotUnique)
        {
            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Error,
                Summary = "Ошибка создания записи",
                Detail = value.ReportDate + " уже существует",
                Duration = 3000
            });
            return;
        }

        var authState = await AuthProvider.GetAuthenticationStateAsync();
        var user = authState.User;
        var userId = user.FindFirst(c => c.Type.Contains("nameidentifier"))?.Value;
        var today = DateOnly.FromDateTime(DateTime.Today);
        var timeNow = DateTime.Now;
        var inputLog = new InputFileLog()
        {
            FileDate = today,
            FileTime = timeNow,
            InputTime = timeNow,
            UserId = userId,
            Filename = "user_input"
        };

        var result = 0;
        if (value.Id > 0)
        {
            value.RequestedValueTime = inputLog;
            value.AllocatedValueTime = inputLog;
            value.EstimatedValueTime = inputLog;
            value.FactValueTime = inputLog;
            Db.Update(value);
            result = await Db.SaveChangesAsync();
        }
        else
        {
            var val = new GisCountryValue()
            {
                GisCountryId = Country.Id,
                ReportDate = value.ReportDate,
                RequestedValue = value.RequestedValue,
                AllocatedValue = value.AllocatedValue,
                EstimatedValue = value.EstimatedValue,
                FactValue = value.FactValue,
                RequestedValueTime = inputLog,
                AllocatedValueTime = inputLog,
                EstimatedValueTime = inputLog,
                FactValueTime = inputLog
            };
            Db.Add(val);
            result = await Db.SaveChangesAsync();
        }

        if (result > 0)
        {
            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Success,
                Summary = "Данные обновлены",
                Detail = "Данные на дату " + value.ReportDate + " успешно обновлены",
                Duration = 3000
            });
        }
        else
        {
            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Error,
                Summary = "Ошибка обновления даты",
                Detail = "Страну " + value.ReportDate + " не удалось обновить",
                Duration = 3000
            });
        }
    }

    private void ShowValueInfo(long? logId)
    {
        if (logId == null)
        {
            _log = "Нет данных";
        }
        else
        {
            var log = Db.InputFilesLogs
                .Include(x => x.User)
                .FirstOrDefault(x => x.Id == logId);
            _log = StringParser.ShowLogInfo(log);
        }
            
    }

    private async Task ExportToExcel()
    {
        var startDate = DateOnly.FromDateTime(StartDate);
        var finishDate = DateOnly.FromDateTime(FinishDate);
        var values = await _values.Where(x => x.ReportDate >= startDate && x.ReportDate <= finishDate)
            .ToListAsync();
        var dayValues = values.Cast<DayValue>().ToList();
        if (values.Count == 0)
        {
            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Error,
                Summary = "Нет данных",
                Detail = "В выбранном диапазоне нет данных",
                Duration = 3000
            });
            return;
        }
        var name = $"Данные по направлению {Country.Gis.Name} -> {Country.Country.Name}";
        name += $" за период с {startDate:dd.MM.yy} по {finishDate:dd.MM.yy}";
        var excel = new DayValueExcel(dayValues, name, startDate, finishDate);
        var excelBytes = await excel.GenerateExcelReport();
        await Js.InvokeVoidAsync("saveAsFile",
            $"{Country.Gis.Name} - {Country.Country.Name} - {DateTime.Now:yyyyMMdd-HHmmss}.xlsx",
            Convert.ToBase64String(excelBytes));
    }
}