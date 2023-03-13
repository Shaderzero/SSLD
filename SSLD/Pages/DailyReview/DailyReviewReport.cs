using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using Radzen;
using SSLD.Data;
using SSLD.Tools;
using SSLD.Tools.DailyReviewGenerator;

namespace SSLD.Pages.DailyReview;

[Authorize(Roles = SD.Role_User)]
public partial class DailyReviewReport
{
    [Inject] public ApplicationDbContext Db { get; set; }
    [Inject] public NotificationService NotificationService { get; set; }
    [Inject] public IJSRuntime Js { get; set; }
    private DateTime _startDate = DateTime.Today;
    private DateTime _finishDate = DateTime.Today;

    private async Task Run()
    {
        var startDate = DateOnly.FromDateTime(_startDate);
        var finishDate = DateOnly.FromDateTime(_finishDate);
        var report = new DailyReviewExcel(Db, NotificationService, startDate, finishDate);
        var excelBytes = await report.GenerateExcelReport();
        await Js.InvokeVoidAsync("saveAsFile", $"test_{DateTime.Now:yyyyMMdd_HHmmss}.xlsm",
            Convert.ToBase64String(excelBytes));
    }

    private async Task LoadExcelReport(InputFileChangeEventArgs e)
    {
        var file = e?.GetMultipleFiles()[0];
        if (file == null) return;
        const string fileName = "operativka.xlsm";
        var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "files", fileName);
        try
        {
            await using FileStream fs = new(fullPath, FileMode.Create);
            await file.OpenReadStream(file.Size).CopyToAsync(fs);
        }
        catch (Exception ex)
        {
            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Error,
                Summary = "Ошибка загрузки файла",
                Detail = $"Файл {file.Name} не удалось сохранить \n" + ex.Message,
                Duration = 5000
            });
        }
    }
}