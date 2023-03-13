using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;
using SSLD.Data;
using SSLD.Data.DZZR;
using SSLD.DTO;
using SSLD.Tools;

namespace SSLD.Pages.DZR;

[Authorize(Roles = SD.Role_User)]
public partial class PageDzrReview
{
    [Inject] public ApplicationDbContext Db { get; set; }
    [Inject] public IJSRuntime Js { get; set; }
    private OperatorGis _selectedGis;
    private int _selectedGisId = 0;
    private List<OperatorGis> _gises = new();
    private List<OperatorResource> _resources;
    private List<OperatorResourceOnDate> _sResources;
    private IList<DataItem> _dzItems;
    private IList<DataItem> _zrItems;
    private DateTime _startDate;
    private DateTime _finishDate;
    private const int Limiter = 3;
    private const string HourWidth = "20px";
    private bool _chartFs;
    private bool _loading = true;
    private bool _showOverall;
    private IEnumerable<int> _types = new[] { 1, 2 };
    private readonly OperatorGis _simpleOperator = new() { Id = 0, Name = "ВСЕ направления" };

    protected override async Task OnInitializedAsync()
    {
        _gises.Add(_simpleOperator);
        _gises.AddRange(await Db.OperatorGises
            .OrderBy(x => x.Name)
            .ToListAsync());
        if (await Db.OperatorResources.AnyAsync())
        {
            var finishDateOnly = await Db.OperatorResources.MaxAsync(x => x.SupplyDate);
            _finishDate = finishDateOnly.ToDateTime(new TimeOnly(12));
        }
        else
        {
            _finishDate = DateTime.Now;
        }

        _startDate = _finishDate.AddDays(-7);
        await OnSelect(_gises.FirstOrDefault());
    }

    private async Task OnSelect(OperatorGis gis)
    {
        if (gis.Id == 0)
        {
            // _finishDate = _startDate;
            _selectedGis = _simpleOperator;
        }
        else
        {
            _showOverall = false;
            _selectedGis = await Db.OperatorGises
                .Where(x => x.Id == gis.Id)
                .FirstOrDefaultAsync();
        }

        await Run();
    }

    private async Task Run()
    {
        _loading = true;
        StateHasChanged();
        var source = await Db.OperatorResources
            .Include(x => x.Hours)
            .Where(x => x.SupplyDate >= DateOnly.FromDateTime(_startDate)
                        && x.SupplyDate <= DateOnly.FromDateTime(_finishDate))
            .OrderByDescending(x => x.SupplyDate)
            .ThenByDescending(x => x.ReportDate)
            .ToListAsync();
        if (_selectedGisId == 0)
        {
            // _resources = SplitResources(source);
            await ShowOverall();
            PrepareOverallDataItem();
        }
        else
        {
            _selectedGis = await Db.OperatorGises.FirstOrDefaultAsync(x => x.Id == _selectedGisId);
            _showOverall = false;
            _resources = source.Where(x => x.OperatorGisId == _selectedGisId).ToList();
            PrepareDataItem();
        }

        _loading = false;
    }

    private async Task ShowOverall()
    {
        _showOverall = true;
        _loading = true;
        var source = await Db.OperatorResources
            .Include(x => x.Hours)
            .Where(x => x.SupplyDate >= DateOnly.FromDateTime(_startDate)
                        && x.SupplyDate <= DateOnly.FromDateTime(_finishDate))
            .OrderByDescending(x => x.SupplyDate)
            .ThenByDescending(x => x.ReportDate)
            .ToListAsync();
        _sResources = new List<OperatorResourceOnDate>();
        for (var date = DateOnly.FromDateTime(_startDate);
             date <= DateOnly.FromDateTime(_finishDate);
             date = date.AddDays(1))
        {
            var zrOnDate = new OperatorResourceOnDate()
            {
                SupplyDate = date,
                Type = OperatorResourceType.Zr
            };
            var dzOnDate = new OperatorResourceOnDate()
            {
                SupplyDate = date,
                Type = OperatorResourceType.Dz
            };
            zrOnDate.Operators.Add(new OperatorVolume()
            {
                OperatorGis = _simpleOperator,
                Volume = 0
            });
            dzOnDate.Operators.Add(new OperatorVolume()
            {
                OperatorGis = _simpleOperator,
                Volume = 0
            });
            var sourceOnDate = source.Where(x => x.SupplyDate == date).ToList();
            var zrSourceOnDate = sourceOnDate.Where(x => x.Type == OperatorResourceType.Zr).ToList();
            var dzSourceOnDate = sourceOnDate.Where(x => x.Type == OperatorResourceType.Dz).ToList();
            foreach (var gis in _gises)
            {
                var zrGisValues = zrSourceOnDate.Where(x => x.OperatorGisId == gis.Id).ToList();
                if (zrGisValues.Any())
                {
                    var zrLastDate = zrGisValues.Max(x => x.ReportDate);
                    var zrLastValue = zrGisValues.FirstOrDefault(x => x.ReportDate == zrLastDate);
                    if (zrLastValue != null)
                    {
                        var volume = new OperatorVolume(zrLastValue);
                        zrOnDate.Operators.Add(volume);
                        var simple = zrOnDate.Operators.FirstOrDefault(x => x.OperatorGis.Id == 0);
                        if (simple != null) simple.Volume += volume.Volume;
                    }
                }

                var dzGisValues = dzSourceOnDate.Where(x => x.OperatorGisId == gis.Id).ToList();
                if (dzGisValues.Any())
                {
                    var dzLastDate = dzGisValues.Max(x => x.ReportDate);
                    var dzLastValue = dzGisValues.FirstOrDefault(x => x.ReportDate == dzLastDate);
                    if (dzLastValue != null)
                    {
                        var volume = new OperatorVolume(dzLastValue);
                        dzOnDate.Operators.Add(volume);
                        var simple = dzOnDate.Operators.FirstOrDefault(x => x.OperatorGis.Id == 0);
                        if (simple != null) simple.Volume += volume.Volume;
                    }
                }
            }

            _sResources.Add(zrOnDate);
            _sResources.Add(dzOnDate);
        }

        _loading = false;
    }

    private void PrepareDataItem()
    {
        _dzItems = new List<DataItem>();
        _zrItems = new List<DataItem>();
        foreach (var item in _resources)
        {
            var dItem = new DataItem()
            {
                Date = item.ReportDate,
                Value = Math.Round(item.Hours.Sum(x => x.Volume), 1)
            };
            switch (item.Type)
            {
                case OperatorResourceType.Dz:
                    _dzItems.Add(dItem);
                    break;
                case OperatorResourceType.Zr:
                    _zrItems.Add(dItem);
                    break;
                default:
                    continue;
            }
        }
    }

    private void PrepareOverallDataItem()
    {
        _dzItems = new List<DataItem>();
        _zrItems = new List<DataItem>();
        foreach (var item in _sResources)
        {
            var volume = item.Operators.FirstOrDefault(x => x.OperatorGis.Id == 0)!.Volume;
            var dItem = new DataItem()
            {
                Date = item.SupplyDate.ToDateTime(new TimeOnly(12)),
                Value = Math.Round(volume, 1)
            };
            switch (item.Type)
            {
                case OperatorResourceType.Dz:
                    _dzItems.Add(dItem);
                    break;
                case OperatorResourceType.Zr:
                    _zrItems.Add(dItem);
                    break;
                default:
                    continue;
            }
        }
    }

    private static decimal GetValueOnHour(OperatorResource resource, int hour)
    {
        var rh = resource.Hours.FirstOrDefault(x => x.Hour == hour);
        return rh == null ? 0 : @Math.Round(rh.Volume, Limiter);
    }

    private static string FormatValue(object value)
    {
        var result = @Math.Round((double)value, Limiter);
        return result.ToString(CultureInfo.CurrentCulture);
    }

    private async Task ExportToExcel()
    {
        if (_selectedGis.Id > 0)
        {
            var excel = new DzZrExcel(_resources);
            var excelBytes = await excel.GenerateExcelReport();
            await Js.InvokeVoidAsync("saveAsFile", $"DZ-ZR_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx",
                Convert.ToBase64String(excelBytes));
        }
        else
        {
            var excel = new DzZrExcel(_sResources);
            var excelBytes = await excel.GenerateExcelOverallReport();
            await Js.InvokeVoidAsync("saveAsFile", $"DZ-ZR-SUM_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx",
                Convert.ToBase64String(excelBytes));
        }
    }

    private string ChartSize()
    {
        const string fs = "position: fixed;" +
                          "width: 100%;" +
                          "height: 100%;" +
                          "top: 0;" +
                          "left: 0;" +
                          "background: rgba(51,51,51,0.7);" +
                          "z-index: 99 !important;";
        return !_chartFs ? "height: calc(50vh - 4rem);" : fs;
    }

    private async Task SelectGis()
    {
        await Run();
    }
}