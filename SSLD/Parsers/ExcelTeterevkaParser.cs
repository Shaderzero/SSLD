using System.ComponentModel;
using Microsoft.AspNetCore.Components.Forms;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using Radzen;
using SSLD.Data.DailyReview;
using SSLD.Tools;

namespace SSLD.Parsers;

public class ExcelTeterevkaParser
{
    private NotificationService NotificationService { get; set; }
    private readonly IBrowserFile _file;
    private ISheet _sheet;
    private string _filename;
    private DateOnly _reportDate;
    private readonly IEnumerable<Gis> _gisList;
    private List<ReviewValueInput> _valueList = new();
    private readonly FileTypeSetting _settings;
    private int _requestedCol = 0;
    private int _allocatedCol = 0;
    private int _estimatedCol = 0;
    private int _factCol = 0;
    private int _countryCol = 0;
    private string _message;

    public ExcelTeterevkaParser(IBrowserFile file, IEnumerable<Gis> gisList, FileTypeSetting settings, NotificationService notificationService)
    {
        _file = file;
        _gisList = gisList;
        _settings = settings;
        NotificationService = notificationService;
    }

    public async Task<List<ReviewValueInput>> GetResult()
    {
        await Parse();
        _valueList = SplitExcelValues(_valueList);
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
        var gis = _gisList.FirstOrDefault(x => x.Names.Any(n => n.ToLower() == "тетеревка"));
        if (gis == null)
        {
            _message = "В БД не найден ГИС Тетеревка";
            return;
        }
            
        var ms = new MemoryStream();
        var stream = _file.OpenReadStream(_file.Size);
        await stream.CopyToAsync(ms);
        stream.Close();
        ms.Position = 0;
        var xssWorkbook = new HSSFWorkbook(ms, true);
        _sheet = xssWorkbook.GetSheetAt(0);
            
        //var revisionTime = ReportDate.AddHours(12);
        if (_filename.Contains("perv"))
        {
            _requestedCol = FindColumnEntry(_settings.RequestedValueEntry);
            _allocatedCol = FindColumnEntry(_settings.AllocatedValueEntry);
        }
        else if (_filename.Contains("utoch"))
        {
            _estimatedCol = FindColumnEntry(_settings.EstimatedValueEntry);
        }
        else if (_filename.Contains("fakt"))
        {
            _factCol = FindColumnEntry(_settings.FactValueEntry);
        }
        _countryCol = FindColumnEntry(_settings.CountryEntry);
        for (var i = 1; i <= _sheet.LastRowNum; i++)
        {
            var row = _sheet.GetRow(i);
            var cell = row?.GetCell(_countryCol);
            if (cell == null) break;
            if (!string.IsNullOrEmpty(cell.ToString()))
            {
                GetCountryValue(gis, i);
            }
        }
        xssWorkbook.Close();
        await ms.DisposeAsync();
    }

    private bool GetCountryValue(Gis gis, int r)
    {
        var row = _sheet.GetRow(r);
        var cellText = row.GetCell(_countryCol).ToString();
        if (string.IsNullOrWhiteSpace(cellText)) return false;
        var gc = gis.Countries.FirstOrDefault(x => x.Country.Names.Any(n => StringParser.StrictLike(n, cellText)));
        if (gc == null) return false;
        try
        {
            if (_requestedCol > 0)
            {
                var cell = row.GetCell(_requestedCol);
                var val = cell.NumericCellValue;
                _valueList.Add(new ReviewValueInput()
                {
                    GisId = gis.Id,
                    ValueId = gc.Id,
                    ReportDate = _reportDate,
                    InType = ReviewValueInput.InputType.Country,
                    ValType = ReviewValueInput.ValueType.Requsted,
                    Value = val
                });
            }
            if (_allocatedCol > 0)
            {
                var cell = row.GetCell(_allocatedCol);
                var val = cell.NumericCellValue;
                _valueList.Add(new ReviewValueInput()
                {
                    GisId = gis.Id,
                    ValueId = gc.Id,
                    ReportDate = _reportDate,
                    InType = ReviewValueInput.InputType.Country,
                    ValType = ReviewValueInput.ValueType.Allocated,
                    Value = val
                });
            }
            if (_estimatedCol > 0)
            {
                var cell = row.GetCell(_estimatedCol);
                var val = cell.NumericCellValue;
                _valueList.Add(new ReviewValueInput()
                {
                    GisId = gis.Id,
                    ValueId = gc.Id,
                    ReportDate = _reportDate,
                    InType = ReviewValueInput.InputType.Country,
                    ValType = ReviewValueInput.ValueType.Estimated,
                    Value = val
                });
            }
            if (_factCol > 0)
            {
                var cell = row.GetCell(_factCol);
                var val = cell.NumericCellValue;
                _valueList.Add(new ReviewValueInput()
                {
                    GisId = gis.Id,
                    ValueId = gc.Id,
                    ReportDate = _reportDate,
                    InType = ReviewValueInput.InputType.Country,
                    ValType = ReviewValueInput.ValueType.Fact,
                    Value = val
                });
            }
        }
        catch (Exception ex)
        {
            NotificationService.Notify(new NotificationMessage
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

    private static List<ReviewValueInput> SplitExcelValues(List<ReviewValueInput> list)
    {
        var result = new List<ReviewValueInput>();
        foreach (var item in list)
        {
            var value = result.FirstOrDefault(x => x.LikeValue(item));
            if (value != null)
            {
                value.Value += item.Value;
            }
            else
            {
                result.Add(item);
            }
        }
        return result;
    }
}