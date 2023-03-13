using System.ComponentModel;
using System.Linq;
using Microsoft.AspNetCore.Components.Forms;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using Radzen;
using SSLD.Data.DailyReview;
using SSLD.Tools;

namespace SSLD.Parsers;

public class ExcelGeMailParser
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
    private int _countryCol = 0;
    private int _gisCol = 0;
    private string _message;

    public ExcelGeMailParser(IBrowserFile file, IEnumerable<Gis> gisList, FileTypeSetting settings, NotificationService notificationService)
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
        _reportDate = reportDate.Value;
            
        var ms = new MemoryStream();
        var stream = _file.OpenReadStream(_file.Size);
        await stream.CopyToAsync(ms);
        stream.Close();
        ms.Position = 0;
        var xssWorkbook = new XSSFWorkbook(ms);
        _sheet = xssWorkbook.GetSheetAt(0);
        var revisionTime = StringParser.GetDateWithTimeFromString(_filename); //европейское время
        var hour = _settings.LastHour;
        var timeSpan = new TimeOnly(hour, 0);
        var minTime = _reportDate.AddDays(-1).ToDateTime(new TimeOnly(0, 0));
        var maxTime = _reportDate.ToDateTime(timeSpan);
        if (minTime < revisionTime && revisionTime < maxTime)
        {
            _requestedCol = FindColumnEntry(_settings.RequestedValueEntry);
            _allocatedCol = FindColumnEntry(_settings.AllocatedValueEntry);
            //надо проверить, обычно нет файла для оценки
            _estimatedCol = FindColumnEntry(_settings.EstimatedValueEntry);
        }
        else
        {
            _estimatedCol = FindColumnEntry(_settings.EstimatedValueEntry);
        }
        _countryCol = FindColumnEntry(_settings.CountryEntry);
        _gisCol = FindColumnEntry(_settings.GisEntry);
        if (_countryCol == 0) _countryCol = _gisCol + 1;
        Gis gis = null;
        for (var row = 1; row <= _sheet.LastRowNum; row++)
        {
            var cellGis = _sheet.GetRow(row).GetCell(_gisCol);
            var cellCountry = _sheet.GetRow(row).GetCell(_countryCol);
            if (cellGis == null || cellCountry == null) continue;
            var cellGisText = cellGis.StringCellValue;
            var cellCountryText = cellCountry.StringCellValue;
            if (!string.IsNullOrEmpty(cellGisText))
            {
                // проверяем не гис ли нам попался в ячейке
                var isGis = _gisList.FirstOrDefault(x => x.Names.Any(n => StringParser.StrictLike(n, cellGisText)));
                if (isGis != null)
                {
                    //попался гис, делаем его текущим, выходим из цикла, чтобы начать обрабатывать следующую строку
                    gis = isGis;
                    continue;
                }
            }
            if (!string.IsNullOrEmpty(cellCountryText))
            {                    
                if (gis != null)
                {
                    //проверяем не страна ли это и подшиваем по ней значения
                    if (GetCountryValue(gis, row)) continue;
                    //проверяем не дополнение к стране ли это и подшиваем по нему значения
                    if (GetCountryAddonValue(gis, row)) continue;
                    //проверяем не аддон ли это и подшиваем по нему значения
                    if (GetAddonValue(gis, row)) continue;
                    //проверяем не закачка ли это и подшиваем по ней значения
                    if (GetInputValue(gis, row)) continue;
                    //проверяем не отбор ли это и подшиваем по нему значения
                    if (GetOutputValue(gis, row)) continue;
                }
            }
            //обнуляем ГИС на пустой ячейке
            if (string.IsNullOrEmpty(cellGisText) && string.IsNullOrEmpty(cellCountryText))
            {
                gis = null;
            }
        }
        xssWorkbook.Close();
        await ms.DisposeAsync();
    }

    private bool GetCountryValue(Gis gis, int r)
    {
        var row = _sheet.GetRow(r);
        var cell = row.GetCell(_countryCol);
        var cellText = cell.StringCellValue;
        var gc = gis.Countries.FirstOrDefault(x => x.Country.Names.Any(n => StringParser.StrictLike(n, cellText)));
        if (gc == null) return false;
        try
        {
            if (_requestedCol > 0)
            {
                _valueList.Add(new ReviewValueInput()
                {
                    GisId = gis.Id,
                    ValueId = gc.Id,
                    ReportDate = _reportDate,
                    InType = ReviewValueInput.InputType.Country,
                    ValType = ReviewValueInput.ValueType.Requsted,
                    Value = row.GetCell(_requestedCol).NumericCellValue
                });
            }
            if (_allocatedCol > 0)
            {
                _valueList.Add(new ReviewValueInput()
                {
                    GisId = gis.Id,
                    ValueId = gc.Id,
                    ReportDate = _reportDate,
                    InType = ReviewValueInput.InputType.Country,
                    ValType = ReviewValueInput.ValueType.Allocated,
                    Value = row.GetCell(_allocatedCol).NumericCellValue
                });
            }
            if (_estimatedCol > 0)
            {
                _valueList.Add(new ReviewValueInput()
                {
                    GisId = gis.Id,
                    ValueId = gc.Id,
                    ReportDate = _reportDate,
                    InType = ReviewValueInput.InputType.Country,
                    ValType = ReviewValueInput.ValueType.Estimated,
                    Value = row.GetCell(_estimatedCol).NumericCellValue
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

    private bool GetCountryAddonValue(Gis gis, int r)
    {
        if (r < 2) return false;
        var row = _sheet.GetRow(r);
        var cell = row.GetCell(_countryCol);
        var cellText = cell.StringCellValue;
        var prevCellText = _sheet.GetRow(r - 1).GetCell(_countryCol).StringCellValue;
        var gc = gis.Countries.FirstOrDefault(x => x.Country.Names.Any(n => StringParser.StrictLike(n, prevCellText)));
        var addon = gc?.Addons.FirstOrDefault(x => x.Names.Any(n => StringParser.StrictLike(n, cellText)));
        if (addon == null) return false;
        {
            try
            {
                if (_requestedCol > 0)
                {
                    var newVal = new ReviewValueInput()
                    {
                        GisId = gis.Id,
                        ValueId = addon.Id,
                        ReportDate = _reportDate,
                        InType = ReviewValueInput.InputType.CountryAddon,
                        ValType = ReviewValueInput.ValueType.Requsted,
                        Value = row.GetCell(_requestedCol).NumericCellValue
                    };
                    _valueList.Add(newVal);
                    var countryVal = _valueList.FirstOrDefault(x => x.GisId == gis.Id &&
                                                                    x.ValueId == gc.Id &&
                                                                    x.InType == ReviewValueInput.InputType.Country &&
                                                                    x.ValType == ReviewValueInput.ValueType.Requsted);
                    if (countryVal != null) countryVal.Value -= newVal.Value;
                }
                if (_allocatedCol > 0)
                {
                    var newVal = new ReviewValueInput()
                    {
                        GisId = gis.Id,
                        ValueId = addon.Id,
                        ReportDate = _reportDate,
                        InType = ReviewValueInput.InputType.CountryAddon,
                        ValType = ReviewValueInput.ValueType.Allocated,
                        Value = row.GetCell(_allocatedCol).NumericCellValue
                    };
                    _valueList.Add(newVal);
                    var countryVal = _valueList.FirstOrDefault(x => x.GisId == gis.Id &&
                                                                    x.ValueId == gc.Id &&
                                                                    x.InType == ReviewValueInput.InputType.Country &&
                                                                    x.ValType == ReviewValueInput.ValueType.Allocated);
                    if (countryVal != null) countryVal.Value -= newVal.Value;
                }
                if (_estimatedCol > 0)
                {
                    var newVal = new ReviewValueInput()
                    {
                        GisId = gis.Id,
                        ValueId = addon.Id,
                        ReportDate = _reportDate,
                        InType = ReviewValueInput.InputType.CountryAddon,
                        ValType = ReviewValueInput.ValueType.Estimated,
                        Value = row.GetCell(_estimatedCol).NumericCellValue
                    };
                    _valueList.Add(newVal);
                    var countryVal = _valueList.FirstOrDefault(x => x.GisId == gis.Id &&
                                                                    x.ValueId == gc.Id &&
                                                                    x.InType == ReviewValueInput.InputType.Country &&
                                                                    x.ValType == ReviewValueInput.ValueType.Estimated);
                    if (countryVal != null) countryVal.Value -= newVal.Value;
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
    }

    private bool GetAddonValue(Gis gis, int r)
    {
        var row = _sheet.GetRow(r);
        var cell = row.GetCell(_countryCol);
        var cellText = cell.StringCellValue;
        var addon = gis.Addons.FirstOrDefault(x => x.Names.Any(n => StringParser.StrictLike(n, cellText)));
        if (addon == null) return false;
        try
        {
            if (_requestedCol > 0)
            {
                _valueList.Add(new ReviewValueInput()
                {
                    GisId = gis.Id,
                    ValueId = addon.Id,
                    ReportDate = _reportDate,
                    InType = ReviewValueInput.InputType.Addon,
                    ValType = ReviewValueInput.ValueType.Requsted,
                    Value = row.GetCell(_requestedCol).NumericCellValue
                });
            }
            if (_allocatedCol > 0)
            {
                _valueList.Add(new ReviewValueInput()
                {
                    GisId = gis.Id,
                    ValueId = addon.Id,
                    ReportDate = _reportDate,
                    InType = ReviewValueInput.InputType.Addon,
                    ValType = ReviewValueInput.ValueType.Allocated,
                    Value = row.GetCell(_allocatedCol).NumericCellValue
                });
            }
            if (_estimatedCol > 0)
            {
                _valueList.Add(new ReviewValueInput()
                {
                    GisId = gis.Id,
                    ValueId = addon.Id,
                    ReportDate = _reportDate,
                    InType = ReviewValueInput.InputType.Addon,
                    ValType = ReviewValueInput.ValueType.Estimated,
                    Value = row.GetCell(_estimatedCol).NumericCellValue
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

    private bool GetInputValue(Gis gis, int r)
    {
        var row = _sheet.GetRow(r);
        var cell = row.GetCell(_countryCol);
        var cellText = cell.StringCellValue;
        if (!gis.GisInputNames.Any(x => StringParser.StrictLike(x, cellText))) return false;
        try
        {
            if (_requestedCol > 0)
            {
                _valueList.Add(new ReviewValueInput()
                {
                    GisId = gis.Id,
                    ReportDate = _reportDate,
                    InType = ReviewValueInput.InputType.Input,
                    ValType = ReviewValueInput.ValueType.Requsted,
                    Value = row.GetCell(_requestedCol).NumericCellValue
                });
            }
            if (_allocatedCol > 0)
            {
                _valueList.Add(new ReviewValueInput()
                {
                    GisId = gis.Id,
                    ReportDate = _reportDate,
                    InType = ReviewValueInput.InputType.Input,
                    ValType = ReviewValueInput.ValueType.Allocated,
                    Value = row.GetCell(_allocatedCol).NumericCellValue
                });
            }
            if (_estimatedCol > 0)
            {
                _valueList.Add(new ReviewValueInput()
                {
                    GisId = gis.Id,
                    ReportDate = _reportDate,
                    InType = ReviewValueInput.InputType.Input,
                    ValType = ReviewValueInput.ValueType.Estimated,
                    Value = row.GetCell(_estimatedCol).NumericCellValue
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

    private bool GetOutputValue(Gis gis, int r)
    {
        var row = _sheet.GetRow(r);
        var cell = row.GetCell(_countryCol);
        var cellText = cell.StringCellValue;
        if (!gis.GisOutputNames.Any(x => StringParser.StrictLike(x, cellText))) return false;
        try
        {
            if (_requestedCol > 0)
            {
                _valueList.Add(new ReviewValueInput()
                {
                    GisId = gis.Id,
                    ReportDate = _reportDate,
                    InType = ReviewValueInput.InputType.Output,
                    ValType = ReviewValueInput.ValueType.Requsted,
                    Value = row.GetCell(_requestedCol).NumericCellValue
                });
            }
            if (_allocatedCol > 0)
            {
                _valueList.Add(new ReviewValueInput()
                {
                    GisId = gis.Id,
                    ReportDate = _reportDate,
                    InType = ReviewValueInput.InputType.Output,
                    ValType = ReviewValueInput.ValueType.Allocated,
                    Value = row.GetCell(_allocatedCol).NumericCellValue
                });
            }
            if (_estimatedCol > 0)
            {
                _valueList.Add(new ReviewValueInput()
                {
                    GisId = gis.Id,
                    ReportDate = _reportDate,
                    InType = ReviewValueInput.InputType.Output,
                    ValType = ReviewValueInput.ValueType.Estimated,
                    Value = row.GetCell(_estimatedCol).NumericCellValue
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