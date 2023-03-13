using Microsoft.AspNetCore.Components.Forms;
using SSLD.Data.DailyReview;
using SSLD.Tools;

namespace SSLD.Parsers.Excel;

public class GasDayParser : IFileParser
{
    private IBrowserFile _file;
    protected IExcelParser _parser;
    private FileTypeSetting _fileTypeSetting;
    private Gis[] _gisArray;
    protected Gis _velke;
    private GisAddon _inputGeoplin;
    private GisAddon _outputGeoplin;
    protected readonly ParserResultImpl<ReviewValueInput> _parserResult = new();
    private readonly IParserHelper _helper;
    protected int _requestedCol = 0;
    protected int _allocatedCol = 0;
    protected int _estimatedCol = 0;
    protected int _factCol = 0;
    private const int GisCol = 1;
    private readonly string _sheetName = null;
    private const string VelkeName = "велке капушаны";
    private const string InputGeoplinName = "закачка геоплин";
    private const string OutputGeoplinName = "отбор геоплин";

    public GasDayParser(IParserHelper helper)
    {
        _helper = helper;
    }

    public async Task Dispose()
    {
        await _parser.Dispose();
    }

    public Task ParseAsync()
    {
        var wasInput = false;
        var wasOutput = false;
        _parser.GetLastCell(out var lastRow, out _);
        for (var row = 1; row <= lastRow; row++)
        {
            var cellText = _parser.GetCellString(row, GisCol);
            if (string.IsNullOrEmpty(cellText)) continue;
            // проверяем не наш ли это Геоплин
            var isInput = StringParser.NameContainAnyList(_inputGeoplin.Names, cellText);
            if (isInput)
            {
                wasInput = true;
                if (GetAddonValue(_inputGeoplin, row)) continue;
            }

            var isOutput = StringParser.NameContainAnyList(_outputGeoplin.Names, cellText);
            if (isOutput)
            {
                wasOutput = true;
                if (GetAddonValue(_outputGeoplin, row)) continue;
            }

            if (wasInput && wasOutput) break;
        }
        return Task.CompletedTask;
    }

    protected virtual bool GetAddonValue(GisAddon addon, int r)
    {
        if (_requestedCol > 0)
        {
            var newValue = new ReviewValueInput()
            {
                GisId = _velke.Id,
                ValueId = addon.Id,
                ReportDate = _parserResult.ReportDate,
                InType = ReviewValueInput.InputType.Addon,
                ValType = ReviewValueInput.ValueType.Requsted,
                Value = _parser.GetCellDouble(r, _requestedCol) / 10.45d / 1000000d
            };
            _parserResult.Result.Add(newValue);
        }

        if (_allocatedCol > 0)
        {
            var newValue = new ReviewValueInput()
            {
                GisId = _velke.Id,
                ValueId = addon.Id,
                ReportDate = _parserResult.ReportDate,
                InType = ReviewValueInput.InputType.Addon,
                ValType = ReviewValueInput.ValueType.Allocated,
                Value = _parser.GetCellDouble(r, _allocatedCol) / 10.45d / 1000000d
            };
            _parserResult.Result.Add(newValue);
        }

        if (_estimatedCol > 0)
        {
            var newValue = new ReviewValueInput()
            {
                GisId = _velke.Id,
                ValueId = addon.Id,
                ReportDate = _parserResult.ReportDate,
                InType = ReviewValueInput.InputType.Addon,
                ValType = ReviewValueInput.ValueType.Estimated,
                Value = _parser.GetCellDouble(r, _estimatedCol) / 10.45d / 1000000d
            };
            _parserResult.Result.Add(newValue);
        }
        
        if (_factCol > 0)
        {
            var newValue = new ReviewValueInput()
            {
                GisId = _velke.Id,
                ValueId = addon.Id,
                ReportDate = _parserResult.ReportDate,
                InType = ReviewValueInput.InputType.Addon,
                ValType = ReviewValueInput.ValueType.Fact,
                Value = _parser.GetCellDouble(r, _factCol) / 10.45d / 1000000d
            };
            _parserResult.Result.Add(newValue);
        }

        return true;
    }

    public async Task<ParserResult> SaveResultAsync()
    {
        return await _helper.SaveResultAsync(_parserResult);
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

    private async Task<bool> GetReferences()
    {
        _gisArray = await _helper.GetGisDetailArrayAsync();
        _fileTypeSetting = await _helper.GetFileSettings(_parserResult.Filename);
        
        _velke = _gisArray.FirstOrDefault(x => StringParser.NameEqualsAnyList(x.Names, VelkeName));
        if (_velke == null)
        {
            _parserResult.Messages.Add("В БД не найден ГИС Велке Капушаны");
            return false;
        }

        _inputGeoplin =
            _velke.Addons.FirstOrDefault(x => StringParser.NameEqualsAnyList(x.Names, InputGeoplinName));
        if (_inputGeoplin == null)
        {
            _parserResult.Messages.Add("В БД не найдена Закачка Геоплин");
            return false;
        }

        _outputGeoplin =
            _velke.Addons.FirstOrDefault(x => StringParser.NameEqualsAnyList(x.Names, OutputGeoplinName));
        if (_outputGeoplin == null)
        {
            _parserResult.Messages.Add("В БД не найден Отбор Геоплин");
            return false;
        }
 
        return true;
    }

    private bool ParseFileName()
    {
        var revisionTime = _parserResult.Filename.GetDateTime();
        var hour = _fileTypeSetting.LastHour;
        var timeSpan = new TimeOnly(hour, 0);
        var reportDate = _parserResult.Filename.GetFirstDate();
        if (reportDate is null || revisionTime is null)
        {
            _parserResult.Messages.Add("Не удалось определить время отчета или ревизии");
            return false;
        }

        _parserResult.ReportDate = reportDate.Value;
        _parserResult.FileTimeStamp = revisionTime.Value;
        
        if (_helper.IsForced())
        {
            _parser.GetStringEntry(_fileTypeSetting.RequestedValueEntry, out _, out _requestedCol);
            _parser.GetStringEntry(_fileTypeSetting.AllocatedValueEntry, out _, out _allocatedCol);
            _parser.GetStringEntry(_fileTypeSetting.EstimatedValueEntry, out _, out _estimatedCol);
            _parser.GetStringEntry(_fileTypeSetting.FactValueEntry, out _, out _factCol);
            return true;
        }

        var minTime = _parserResult.ReportDate.AddDays(-1).ToDateTime(new TimeOnly(0, 0));
        var maxTime = _parserResult.ReportDate.ToDateTime(timeSpan);
        if (minTime < _parserResult.FileTimeStamp && _parserResult.FileTimeStamp < maxTime)
        {
            _parser.GetStringEntry(_fileTypeSetting.RequestedValueEntry, out _, out _requestedCol);
            _parser.GetStringEntry(_fileTypeSetting.AllocatedValueEntry, out _, out _allocatedCol);
        }
        else
        {
            _parser.GetStringEntry(_fileTypeSetting.EstimatedValueEntry, out _, out _estimatedCol);
            _parser.GetStringEntry(_fileTypeSetting.FactValueEntry, out _, out _factCol);
        }

        return true;
    }
}