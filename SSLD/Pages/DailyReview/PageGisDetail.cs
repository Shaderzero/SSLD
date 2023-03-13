using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;
using NPOI.OpenXmlFormats.Spreadsheet;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using Radzen;
using Radzen.Blazor;
using SSLD.Data;
using SSLD.Data.DailyReview;
using SSLD.Shared;
using SSLD.Tools;

namespace SSLD.Pages.DailyReview;

[Authorize(Roles = SD.Role_User)]
public partial class PageGisDetail
{
    [Inject] public ApplicationDbContext Db { get; set; }
    [Inject] public NotificationService NotificationService { get; set; }
    [Inject] public DialogService DialogService { get; set; }
    [Inject] public IJSRuntime Js { get; set; }
    [Parameter] public DateTime StartDate { get; set; }
    [Parameter] public DateTime FinishDate { get; set; }
    [Parameter] public Gis Gis { get; set; }
    [Parameter] public string Type { get; set; }
    private IList<DayValue> _values;
    private bool _isLoading = false;
    private Consolidation _consolidation;

    protected override async Task OnParametersSetAsync()
    {
        _consolidation = new Consolidation(Db);
        await LoadData();
    }

    private async Task LoadData()
    {
        _isLoading = true;
        var startDate = DateOnly.FromDateTime(StartDate);
        var finishDate = DateOnly.FromDateTime(FinishDate);
        _values = Type switch
        {
            "gis" => await _consolidation.GisSumOnDateRangeAsync(Gis, startDate, finishDate),
            "comgas" => await _consolidation.ComGisOnDateRangeAsync(Gis, startDate, finishDate),
            _ => _values
        };
        _isLoading = false;
    }
        
    private async Task ExportToExcel()
    {
        var startDate = DateOnly.FromDateTime(StartDate);
        var finishDate = DateOnly.FromDateTime(FinishDate);
        var values = _values.Where(x => x.ReportDate >= startDate && x.ReportDate <= finishDate)
            .ToList();
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

        var dayValues = values.Cast<DayValue>().ToList();
        var name = $"Данные по направлению {Gis.Name}";
        name += $" за период с {startDate:dd.MM.yy} по {finishDate:dd.MM.yy}";
        var excel = new DayValueExcel(dayValues, name, startDate, finishDate);
        var excelBytes = await excel.GenerateExcelReport();
        await Js.InvokeVoidAsync("saveAsFile",
            $"{Gis.Name} - {DateTime.Now:yyyyMMdd-HHmmss}.xlsx",
            Convert.ToBase64String(excelBytes));
    }
}