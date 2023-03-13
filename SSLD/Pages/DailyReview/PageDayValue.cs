using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Radzen.Blazor;
using SSLD.Data.DailyReview;
using SSLD.Tools;

namespace SSLD.Pages.DailyReview;

public partial class PageDayValue
{
    [Parameter] public IQueryable<DayValue> Values { get; set; }
    [Parameter] public EventCallback<long?> ShowInfo { get; set; }
    [Parameter] public EventCallback<long> DeleteValue { get; set; }
    [Parameter] public EventCallback<DayValue> SaveValue { get; set; }
    [Parameter] public EventCallback ExportExcel { get; set; }
    [Parameter] public string Name { get; set; }
    [Parameter] public bool IsEditable { get; set; }
    private RadzenDataGrid<DayValue> _valuesGrid;
    

    private static void OnCreateValue(DayValue val)
    {
        if (val.EstimatedValue == 0) { val.EstimatedValue = val.FactValue; }
        if (val.AllocatedValue == 0) { val.AllocatedValue = val.EstimatedValue; }
        if (val.RequestedValue == 0) { val.RequestedValue = val.AllocatedValue; }
    }

    private async Task ShowValueInfo(long? id)
    {
        await ShowInfo.InvokeAsync(id);
    }
    
    private static void OnChange(DayValue val, int type, string input)
    {
        var dec = StringParser.TryGetDecimal(input);
        switch (type)
        {
            case 1:
                val.RequestedValue = dec;
                break;
            case 2:
                val.AllocatedValue = dec;
                break;
            case 3:
                val.EstimatedValue = dec;
                break;
            case 4:
                val.FactValue = dec;
                break;
        }
    }

    private async Task DeleteVal(long id)
    {
        await DeleteValue.InvokeAsync(id);
        await _valuesGrid.Reload();
    }

    private async Task SaveVal(DayValue val)
    {
        await SaveValue.InvokeAsync(val);
        _valuesGrid.CancelEditRow(val);
        await _valuesGrid.Reload();
    }
    
    private async Task InsertValue()
    {
        var val = new DayValue();
        var valuesCount = await Values.CountAsync();
        if (valuesCount > 0)
        {
            var lastDate = await Values
                .Select(x => x.ReportDate).MaxAsync();
            val.ReportDate = lastDate.AddDays(1);
        }
        else
        {
            val.ReportDate = DateOnly.FromDateTime(DateTime.Today);
        }
            
        await _valuesGrid.InsertRow(val);
    }

    private async Task ExportToExcel()
    {
        await ExportExcel.InvokeAsync();
    }
}