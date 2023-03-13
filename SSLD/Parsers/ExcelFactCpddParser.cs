using Microsoft.AspNetCore.Components.Forms;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using Radzen;
using SSLD.Data.DailyReview;
using SSLD.Tools;

namespace SSLD.Parsers;

public class ExcelFactCpddParser
{
    private readonly NotificationService _notificationService;
    private readonly IBrowserFile _file;
    private ISheet _sheet;
    private string _filename;
    private DateOnly _reportDate;
    private readonly IEnumerable<Gis> _gisList;
    private readonly List<ReviewValueInput> _valueList = new();
    private readonly FileTypeSetting _settings;
    private int _factCol = 0;
    private int _countryCol = 0;
    private int _gisCol = 0;
    private string _message;

    public ExcelFactCpddParser(IBrowserFile file, IEnumerable<Gis> gisList, FileTypeSetting settings, NotificationService notificationService)
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
        if (_sheet == null)
        {
            xssWorkbook.Close();
            await ms.DisposeAsync();
            return;
        }
        _factCol = FindColumnEntry(_settings.FactValueEntry);
        _countryCol = FindColumnEntry(_settings.CountryEntry);
        _gisCol = FindColumnEntry(_settings.GisEntry);
        Gis gis = null;
        for (var i = 1; i <= _sheet.LastRowNum; i++)
        {
            var row = _sheet.GetRow(i);
            var cell = row?.GetCell(_gisCol);
            if (cell is not { CellType: CellType.String }) continue;
            var cellText = cell.StringCellValue;
            if (!string.IsNullOrEmpty(cellText))
            {
                // проверяем не гис ли нам попался в ячейке
                var isGis = _gisList.FirstOrDefault(x => x.Names.Any(n => StringParser.StrictLike(n, cellText)));
                if (isGis != null)
                {
                    //попался гис, делаем его текущим, выходим из цикла, чтобы начать обрабатывать следующую строку
                    gis = isGis;
                    continue;
                }

                if (gis == null) continue;
                //проверяем не страна ли это и подщиваем по ней значения
                if (GetCountryValue(gis, row)) continue;
                //проверяем не аддон ли это и подшиваем по нему значения
                if (GetAddonValue(gis, row)) continue;
                //проверяем не закачка ли это и подшиваем по ней значения
                if (GetInputValue(gis, row)) continue;
                //проверяем не отбор ли это и подшиваем по нему значения
                if (GetOutputValue(gis, row)) continue;
            }
            //обнуляем ГИС на пустой ячейке
            else
            {
                gis = null;
            }
        }
        xssWorkbook.Close();
        await ms.DisposeAsync();
    }

    private bool GetCountryValue(Gis gis, IRow row)
    {
        var cellText = row.GetCell(_countryCol).StringCellValue;
        var gc = gis.Countries.FirstOrDefault(x => x.Country.Names.Any(n => StringParser.StrictLike(n, cellText)));
        if (gc == null) return false;
        try
        {
            if (_factCol > 0)
            {
                _valueList.Add(new ReviewValueInput()
                {
                    GisId = gis.Id,
                    ValueId = gc.Id,
                    ReportDate = _reportDate,
                    InType = ReviewValueInput.InputType.Country,
                    ValType = ReviewValueInput.ValueType.Fact,
                    Value = row.GetCell(_factCol).NumericCellValue
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

    private bool GetAddonValue(Gis gis, IRow row)
    {
        var cellText = row.GetCell(_countryCol).StringCellValue;
        var addon = gis.Addons.FirstOrDefault(x => x.Names.Any(n => StringParser.StrictLike(n, cellText)));
        if (addon == null)
        {
            return false;
        }
        try
        {
            if (_factCol > 0)
            {
                _valueList.Add(new ReviewValueInput()
                {
                    GisId = gis.Id,
                    ValueId = addon.Id,
                    ReportDate = _reportDate,
                    InType = ReviewValueInput.InputType.Addon,
                    ValType = ReviewValueInput.ValueType.Fact,
                    Value = row.GetCell(_factCol).NumericCellValue
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

    private bool GetInputValue(Gis gis, IRow row)
    {
        var cellText = row.GetCell(_countryCol).StringCellValue;
        if (!gis.GisInputNames.Any(x => StringParser.StrictLike(x, cellText)))
            return false;

        try
        {
            if (_factCol > 0)
            {
                _valueList.Add(new ReviewValueInput()
                {
                    GisId = gis.Id,
                    ReportDate = _reportDate,
                    InType = ReviewValueInput.InputType.Input,
                    ValType = ReviewValueInput.ValueType.Fact,
                    Value = row.GetCell(_factCol).NumericCellValue
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

    private bool GetOutputValue(Gis gis, IRow row)
    {
        var cellText = row.GetCell(_countryCol).StringCellValue;
        if (!gis.GisOutputNames.Any(x => StringParser.StrictLike(x, cellText))) return false;
        try
        {
            if (_factCol > 0)
            {
                _valueList.Add(new ReviewValueInput()
                {
                    GisId = gis.Id,
                    ReportDate = _reportDate,
                    InType = ReviewValueInput.InputType.Output,
                    ValType = ReviewValueInput.ValueType.Fact,
                    Value = row.GetCell(_factCol).NumericCellValue
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
        for (var i = 0; i <= _sheet.LastRowNum; i++)
        {
            var row = _sheet.GetRow(i);
            if (row == null) continue;
            for (var col = 0; col <= row.LastCellNum; col++)
            {
                var cell = row.GetCell(col);
                if (cell is not { CellType: CellType.String }) continue;
                if (string.IsNullOrEmpty(cell.ToString())) continue;
                if (StringParser.NameEqualsAnyList(names, cell.ToString()))
                {
                    return col;
                }
            }
        }
        return 0;
    }
}