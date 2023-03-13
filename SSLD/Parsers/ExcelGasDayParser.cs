using System.ComponentModel;
using Microsoft.AspNetCore.Components.Forms;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using Radzen;
using SSLD.Data.DailyReview;
using SSLD.Tools;

namespace SSLD.Parsers;

public class ExcelGasDayParser
{
    private readonly NotificationService _notificationService;
    private readonly IBrowserFile _file;
    private ISheet _sheet;
    private string _filename;
    private DateOnly _reportDate;
    private readonly IEnumerable<Gis> _gisList;
    private readonly List<ReviewValueInput> _valueList = new();
    private readonly FileTypeSetting _settings;
    private int _requestedCol = 0;
    private int _allocatedCol = 0;
    private int _estimatedCol = 0;
    private string _message;

    public ExcelGasDayParser(IBrowserFile file, IEnumerable<Gis> gisList, FileTypeSetting settings, NotificationService notificationService)
    {
        _file = file;
        _gisList = gisList;
        _settings = settings;
        _notificationService = notificationService;
    }

    public async Task<List<ReviewValueInput>> GetResult()
    {
        await Parse();
        return _valueList;
    }

    public string GetMessage()
    {
        return _message;
    }

    private async Task Parse()
    {
        _filename = _file.Name;
        var reportDate = StringParser.GetFirstDateOnlyFromString(_filename);
        if (reportDate == null)
        {
            _message = "В результате парсинга файла " + _filename + " не удалось установить дату";
            return;
        }
        var gisVelke = _gisList.FirstOrDefault(x => x.Names.Any(n => n.ToLower() == "велке капушаны"));
        if (gisVelke == null)
        {
            _message = "В БД не найден ГИС Велке Капушаны";
            return;
        }
        //находим закачку
        var geoplinInput = gisVelke.Addons.FirstOrDefault(x => x.Names.Any(n => n.ToLower() == "закачка геоплин"));
        if (geoplinInput == null)
        {
            _message = "В БД не найдена Закачка Геоплин";
            return;
        }
        var geoplinOutput = gisVelke.Addons.FirstOrDefault(x => x.Names.Any(n => n.ToLower() == "отбор геоплин"));
        if (geoplinOutput == null)
        {
            _message = "В БД не найден Отбор Геоплин";
            return;
        }
            
        var ms = new MemoryStream();
        var stream = _file.OpenReadStream(_file.Size);
        await stream.CopyToAsync(ms);
        stream.Close();
        ms.Position = 0;
        var xssWorkbook = new XSSFWorkbook(ms);
        _sheet = xssWorkbook.GetSheetAt(0);
        _reportDate = reportDate.Value;
        var revisionTime = StringParser.GetDateWithTimeFromString(_filename); //европейское время
        var hour = _settings.LastHour;
        var timeSpan = new TimeOnly(hour, 0);
        var minTime = _reportDate.AddDays(-1).ToDateTime(new TimeOnly(0, 0));
        var maxTime = _reportDate.ToDateTime(timeSpan);
        if (minTime < revisionTime && revisionTime < maxTime)
        {
            _requestedCol = FindColumnEntry(_settings.RequestedValueEntry);
            _allocatedCol = FindColumnEntry(_settings.AllocatedValueEntry);
        }
        else
        {
            _estimatedCol = FindColumnEntry(_settings.EstimatedValueEntry);
        }
        var wasInput = false;
        var wasOutput = false;
        for (var row = 1; row <= _sheet.LastRowNum; row++)
        {
            var cell = _sheet.GetRow(row).GetCell(0);
            if (cell is not { CellType: CellType.String }) continue;
            var cellText = cell.StringCellValue;
            if (!string.IsNullOrEmpty(cellText))
            {
                // проверяем не наш ли это Геоплин
                var isInput = geoplinInput.Names.Any(x => StringParser.StrictLike(x, cellText));
                if (isInput)
                {
                    wasInput = true;
                    if (GetAddonValue(geoplinInput, row)) continue;
                }
                var isOutput = geoplinOutput.Names.Any(x => StringParser.StrictLike(x, cellText));
                if (isOutput)
                {
                    wasOutput = true;
                    if (GetAddonValue(geoplinOutput, row)) continue;
                }
            }
            if (wasInput && wasOutput) break;
        }
        xssWorkbook.Close();
        await ms.DisposeAsync();
    }

    private bool GetAddonValue(GisAddon addon, int r)
    {
        if (addon == null) return false;
        try
        {
            if (_requestedCol > 0)
            {
                var row = _sheet.GetRow(r).GetCell(_requestedCol);
                if (row is not {CellType: CellType.Numeric}) return false;
                var val = row.NumericCellValue;
                val = val / 10.45 / 1000000;
                _valueList.Add(new ReviewValueInput()
                {
                    GisId = addon.GisId,
                    ValueId = addon.Id,
                    ReportDate = _reportDate,
                    InType = ReviewValueInput.InputType.Addon,
                    ValType = ReviewValueInput.ValueType.Requsted,
                    Value = val
                });
            }
            if (_allocatedCol > 0)
            {
                var row = _sheet.GetRow(r).GetCell(_allocatedCol);
                if (row is not {CellType: CellType.Numeric}) return false;
                var val = row.NumericCellValue;
                val = val / 10.45 / 1000000;
                _valueList.Add(new ReviewValueInput()
                {
                    GisId = addon.GisId,
                    ValueId = addon.Id,
                    ReportDate = _reportDate,
                    InType = ReviewValueInput.InputType.Addon,
                    ValType = ReviewValueInput.ValueType.Allocated,
                    Value = val
                });
            }
            if (_estimatedCol > 0)
            {
                var row = _sheet.GetRow(r).GetCell(_estimatedCol);
                if (row is not {CellType: CellType.Numeric}) return false;
                var val = row.NumericCellValue;
                val = val / 10.45 / 1000000;
                _valueList.Add(new ReviewValueInput()
                {
                    GisId = addon.GisId,
                    ValueId = addon.Id,
                    ReportDate = _reportDate,
                    InType = ReviewValueInput.InputType.Addon,
                    ValType = ReviewValueInput.ValueType.Estimated,
                    Value = val
                });
            }
        }
        catch (Exception ex)
        {
            _notificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Error,
                Summary = ex.Message,
                Duration = 3000
            });
        }
        return true;
    }

    private int FindColumnEntry(List<string> names)
    {
        if (names == null) return 0;
            
        for (var r = 1; r <= _sheet.LastRowNum; r++)
        {
            var row = _sheet.GetRow(r);
            if (row is null) continue;
            for (var col = 1; col <= row.LastCellNum; col++)
            {
                var cell = row.GetCell(col);
                if (cell is not { CellType: CellType.String }) continue;
                var cellText = cell.StringCellValue;
                if (string.IsNullOrEmpty(cellText)) continue;
                if (StringParser.NameEqualsAnyList(names, cellText))
                {
                    return col;
                }
            }
        }
        return 0;
    }
}