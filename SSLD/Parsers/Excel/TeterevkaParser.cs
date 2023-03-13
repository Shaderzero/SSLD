using Microsoft.AspNetCore.Components.Forms;
using SSLD.Data.DailyReview;
using SSLD.Tools;

namespace SSLD.Parsers.Excel;

public class TeterevkaParser : IFileParser
{
    private IBrowserFile _file;
    private IExcelParser _parser;
    private FileTypeSetting _fileTypeSetting;
    private readonly ParserResultImpl<ReviewValueInput> _parserResult = new();
    private readonly IParserHelper _helper;
    private Gis[] _gisArray;
    private Gis _teterevkaGis;
    private int _requestedCol = 0;
    private int _allocatedCol = 0;
    private int _estimatedCol = 0;
    private int _factCol = 0;
    private int _countryCol = 0;
    private readonly string _sheetName = null;
    private readonly string _teterevkaName = "тетеревка";

    public TeterevkaParser(IParserHelper helper)
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
        if (!await GetReferences()) return false;
        var setSheetResult = await _parser.SetSheet(_sheetName);
        return setSheetResult && ParseFileName();
    }

    private bool ParseFileName()
    {
        var revisionTime = _file.LastModified.DateTime;
        var reportDate = _parserResult.Filename.GetFirstDate();
        if (reportDate is null)
        {
            _parserResult.Messages.Add("Не удалось определить время отчета");
            return false;
        }

        _parserResult.ReportDate = reportDate.Value;
        _parserResult.FileTimeStamp = revisionTime;
        
        if (_parserResult.Filename.Contains("perv"))
        {
            _parser.GetStringEntry(_fileTypeSetting.RequestedValueEntry, out _, out _allocatedCol);
            _parser.GetStringEntry(_fileTypeSetting.AllocatedValueEntry, out _, out _allocatedCol);
        }
        else if (_parserResult.Filename.Contains("utoch"))
        {
            _parser.GetStringEntry(_fileTypeSetting.EstimatedValueEntry, out _, out _estimatedCol);
        }
        else if (_parserResult.Filename.Contains("fakt"))
        {
            _parser.GetStringEntry(_fileTypeSetting.FactValueEntry, out _, out _factCol);
        }

        _parser.GetStringEntry(_fileTypeSetting.CountryEntry, out _, out _countryCol);
        return true;
    }

    private async Task<bool> GetReferences()
    {
        _gisArray = await _helper.GetGisDetailArrayAsync();
        _teterevkaGis = _gisArray.FirstOrDefault(x => StringParser.NameContainAnyList(x.Names, _teterevkaName));
        _fileTypeSetting = await _helper.GetFileSettings(_parserResult.Filename);
        return true;
    }

    public Task ParseAsync()
    {
        _parser.GetLastCell(out var lastRow, out _);
        for (var i = 1; i <= lastRow; i++)
        {
            var cellText = _parser.GetCellString(i, _countryCol);
            if (!string.IsNullOrEmpty(cellText))
            {
                GetCountryValue(i);
            }
        }

        SplitResultValues();
        return Task.CompletedTask;
    }

    private void GetCountryValue(int r)
    {
        var cellText = _parser.GetCellString(r, _countryCol);
        if (string.IsNullOrWhiteSpace(cellText)) return;
        var gc = _teterevkaGis.Countries.FirstOrDefault(x =>
            StringParser.NameContainAnyList(x.Country.Names, cellText));
        if (gc == null) return;
        if (_requestedCol > 0)
        {
            var val = _parser.GetCellDouble(r, _requestedCol);
            _parserResult.Result.Add(new ReviewValueInput()
            {
                GisId = _teterevkaGis.Id,
                ValueId = gc.Id,
                ReportDate = _parserResult.ReportDate,
                InType = ReviewValueInput.InputType.Country,
                ValType = ReviewValueInput.ValueType.Requsted,
                Value = val
            });
        }

        if (_allocatedCol > 0)
        {
            var val = _parser.GetCellDouble(r, _allocatedCol);
            _parserResult.Result.Add(new ReviewValueInput()
            {
                GisId = _teterevkaGis.Id,
                ValueId = gc.Id,
                ReportDate = _parserResult.ReportDate,
                InType = ReviewValueInput.InputType.Country,
                ValType = ReviewValueInput.ValueType.Allocated,
                Value = val
            });
        }

        if (_estimatedCol > 0)
        {
            var val = _parser.GetCellDouble(r, _estimatedCol);
            _parserResult.Result.Add(new ReviewValueInput()
            {
                GisId = _teterevkaGis.Id,
                ValueId = gc.Id,
                ReportDate = _parserResult.ReportDate,
                InType = ReviewValueInput.InputType.Country,
                ValType = ReviewValueInput.ValueType.Estimated,
                Value = val
            });
        }

        if (_factCol > 0)
        {
            var val = _parser.GetCellDouble(r, _factCol);
            _parserResult.Result.Add(new ReviewValueInput()
            {
                GisId = _teterevkaGis.Id,
                ValueId = gc.Id,
                ReportDate = _parserResult.ReportDate,
                InType = ReviewValueInput.InputType.Country,
                ValType = ReviewValueInput.ValueType.Fact,
                Value = val
            });
        }
    }
    
    private void SplitResultValues()
    {
        var result = new List<ReviewValueInput>();
        foreach (var item in _parserResult.Result)
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
        _parserResult.Result = result;
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