using Microsoft.AspNetCore.Components.Forms;
using Radzen;
using SSLD.Data.DailyReview;
using SSLD.Data.DZZR;
using SSLD.Tools;

namespace SSLD.Parsers.Excel;

public class DzZrParser: IFileParser
{
    private IBrowserFile _file;
    private IExcelParser _parser;
    private FileTypeSetting _fileTypeSetting;
    private OperatorGis[] _gisArray;
    private OperatorResourceType _fileType;
    private readonly ParserResultImpl<OperatorResource> _parserResult = new();
    private readonly IParserHelper _helper;
    private readonly string _reportDateName = "Дата поставки:";
    private readonly string _gisName = "ГИС";
    private readonly string _sumName = "Итого";
    private readonly string _gName = "млн";
    private readonly string _sheetName = null;
    
    public DzZrParser(IParserHelper helper)
    {
        _helper = helper;
    }
    
    public async Task<bool> SetFileAsync(IBrowserFile file)
    {
        _file = file;
        _parserResult.Filename = _file.Name;
        var extension = _parserResult.Filename.Split(new char[] { '.' })[^1];
        _parser = extension switch
        {
            "xlsx" => new XlsxParser(_file, _parserResult),
            "xlsm" => new XlsxParser(_file, _parserResult),
            "xls" => new XlsParser(_file, _parserResult),
            _ => _parser
        };
        
        if (_parserResult.Filename.Contains("ДЗ"))
        {
            _fileType = OperatorResourceType.Dz;
        }
        else if (_parserResult.Filename.Contains("ЗР"))
        {
            _fileType = OperatorResourceType.Zr;
        }
        else
        {
            _parserResult.Messages.Add("В файле " + _parserResult.Filename + " нет ДЗ или ЗР");
            return false;
        }

        _fileTypeSetting = await _helper.GetFileSettings(_parserResult.Filename);
        _gisArray = await _helper.GetOperatorGisArrayAsync();

        var setSheetResult = await _parser.SetSheet(_sheetName);
        return setSheetResult && ParseFileName();
    }

    private bool ParseFileName()
    {
        var reportDate = StringParser.GetFirstDateOnlyFromString(_parserResult.Filename);
        var revisionTime = _parserResult.Filename.GetDateTime();
        if (reportDate is null || revisionTime is null)
        {
            _parserResult.Messages.Add("Не удалось определить время отчета или ревизии");
            return false;
        }
        _parserResult.ReportDate = reportDate.Value;
        _parserResult.FileTimeStamp = revisionTime.Value;

        return true;
    }

    public Task ParseAsync()
    {
        _parser.GetStringEntry(_reportDateName, out var row, out var col);
        var supplyDateTime = _parser.GetDateTime(row, col + 1);
        var reportDate = _parser.GetDateTime(row + 1, col + 1);
        var reportTime = _parser.GetDateTime(row + 2, col + 1);
        if (supplyDateTime is null || reportDate is null || reportTime is null) return Task.CompletedTask;
        var supplyDate = DateOnly.FromDateTime(supplyDateTime.Value);
        if (reportDate.Value.Hour == 0 && reportDate.Value.Minute == 0 && reportDate.Value.Second == 0)
        {
            reportDate = reportDate.Value.AddHours(reportTime.Value.Hour).AddMinutes(reportTime.Value.Minute).AddSeconds(reportTime.Value.Second);
        }
        _parser.GetStringEntry(_gisName, out var startRow, out var startCol);
        _parser.GetStringEntry(_sumName, out var endRow, out var _);
        startRow += 3;
        endRow -= 1;
        var endCol = col + 1;
        _parser.GetStringEntry(_gName, out var gRow, out var gCol);
        var divider = 1d;
        if (gRow == 0 && gCol == 0) divider = 1000d;
        
        for (col = startCol; col <= endCol; col += 3)
        {
            var gisName = _parser.GetCellString(startRow, col);
            var operatorGis = _gisArray.FirstOrDefault(x => StringParser.StrictLike(x.Name, gisName));
            if (operatorGis == null) continue;
            var diff = 0;
            if (!IsColumnCet(col + 1, startRow))
            {
                diff = TimeParser.TimeDiff(supplyDate.ToDateTime(new TimeOnly(12, 0)));
            }
            var operatorResource = new OperatorResource
            {
                OperatorGis = operatorGis,
                OperatorGisId = operatorGis.Id,
                SupplyDate = supplyDate,
                ReportDate = reportDate.Value,
                Type = _fileType
            };
            for (row = startRow; row <= endRow; row++)
            {
                var hour = _parser.GetCellString(row, col + 1);
                var value = _parser.GetCellDouble(row, col + 2);
                var operatorHour = new OperatorResourceHour()
                {
                    OperatorResource = operatorResource,
                    Hour = CalculateHour(hour, diff),
                    Volume = (decimal)(value / divider)
                };
                // проверка перехода времени
                var check = operatorResource.Hours.FirstOrDefault(x => x.Hour == operatorHour.Hour);
                if (check != null)
                {
                    check.Volume += operatorHour.Volume;
                }
                else
                {
                    operatorResource.Hours.Add(operatorHour);
                }
            }
            _parserResult.Result.Add(operatorResource);
        }
        return Task.CompletedTask;
    }
    
    private static int CalculateHour(string hour, int diff)
    {
        var result = StringParser.CevTimeToInt(hour);
        result -= diff;
        if (result <= 0) result = 24 + result;
        return result;
    }
    
    private bool IsColumnCet(int col, int startRow)
    {
        for (var row = 1; row <= startRow; row++)
        {
            var cellText = _parser.GetCellString(row, col);
            if (string.IsNullOrEmpty(cellText)) continue;
            if (StringParser.ContainLike(cellText, "мск"))
            {
                return false;
            }
            else if (StringParser.ContainLike(cellText, "цев"))
            {
                return true;
            }
        }
        return true;
    }

    public async Task<ParserResult> SaveResultAsync()
    {
        return await _helper.SaveResultAsync(_parserResult);
    }

    public async Task Dispose()
    {
        await _parser.Dispose();
    }
}