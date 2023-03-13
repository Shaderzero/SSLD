using Microsoft.AspNetCore.Components.Forms;
using SSLD.Data.DailyReview;
using SSLD.Tools;

namespace SSLD.Parsers.Excel;

public class GeMailParser : IFileParser
{
    private IBrowserFile _file;
    private IExcelParser _parser;
    private FileTypeSetting _fileTypeSetting;
    private Gis[] _gisArray;
    private readonly ParserResultImpl<ReviewValueInput> _parserResult = new();
    private readonly IParserHelper _helper;
    private int _requestedCol = 0;
    private int _allocatedCol = 0;
    private int _estimatedCol = 0;
    private int _factCol = 0;
    private int _gisCol = 0;
    private int _countryCol = 0;
    private readonly string _sheetName = null;

    public GeMailParser(IParserHelper helper)
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

        _fileTypeSetting = await _helper.GetFileSettings(_parserResult.Filename);
        _gisArray = await _helper.GetGisDetailArrayAsync();

        var setSheetResult = await _parser.SetSheet(_sheetName);
        return setSheetResult && ParseFileName();
    }

    public Task ParseAsync()
    {
        ParseRows();
        return Task.CompletedTask;
    }

    private void ParseRows()
    {
        Gis gis = null;
        _parser.GetLastCell(out var lastRow, out _);
        for (var i = 1; i <= lastRow; i++)
        {
            var gisName = _parser.GetCellString(i, _gisCol);
            var countryName = _parser.GetCellString(i, _countryCol);
            if (string.IsNullOrEmpty(gisName) && string.IsNullOrEmpty(countryName)) continue;
            // проверяем не гис ли нам попался в ячейке
            var isGis = _gisArray.FirstOrDefault(x =>
                StringParser.NameContainAnyList(x.Names, gisName));
            if (isGis != null)
            {
                //попался гис, делаем его текущим, выходим из цикла, чтобы начать обрабатывать следующую строку
                gis = isGis;
                continue;
            }
            if (gis == null) continue;
            ParseRowValue(gis, i);
        }
    }

    private static void GetCellType(Gis gis, string name, out int valueId, out ReviewValueInput.InputType? inType)
    {
        var gisCountry = gis.Countries.FirstOrDefault(x =>
            StringParser.NameContainAnyList(x.Country.Names, name));
        if (gisCountry is not null)
        {
            valueId = gisCountry.Id;
            inType = ReviewValueInput.InputType.Country;
            return;
        }

        var gisAddon = gis.Addons.FirstOrDefault(x =>
            StringParser.NameContainAnyList(x.Names, name));
        if (gisAddon is not null)
        {
            valueId = gisAddon.Id;
            inType = ReviewValueInput.InputType.Addon;
            return;
        }

        var gisInput = StringParser.NameContainAnyList(gis.GisInputNames, name);
        if (gisInput)
        {
            valueId = 0;
            inType = ReviewValueInput.InputType.Input;
            return;
        }

        var gisOutput = StringParser.NameContainAnyList(gis.GisOutputNames, name);
        if (gisOutput)
        {
            valueId = 0;
            inType = ReviewValueInput.InputType.Output;
            return;
        }

        valueId = 0;
        inType = null;
    }

    private void AddInputValues(int gisId, int valueId, ReviewValueInput.InputType inType,
        int row)
    {
        try
        {
            if (_requestedCol > 0)
            {
                var newValue = new ReviewValueInput()
                {
                    GisId = gisId,
                    ValueId = valueId,
                    ReportDate = _parserResult.ReportDate,
                    InType = inType,
                    ValType = ReviewValueInput.ValueType.Requsted,
                    Value = _parser.GetCellDouble(row, _requestedCol)
                };
                _parserResult.Result.Add(newValue);
            }

            if (_allocatedCol > 0)
            {
                var newValue = new ReviewValueInput()
                {
                    GisId = gisId,
                    ValueId = valueId,
                    ReportDate = _parserResult.ReportDate,
                    InType = inType,
                    ValType = ReviewValueInput.ValueType.Allocated,
                    Value = _parser.GetCellDouble(row, _allocatedCol)
                };
                _parserResult.Result.Add(newValue);
            }

            if (_estimatedCol > 0)
            {
                var newValue = new ReviewValueInput()
                {
                    GisId = gisId,
                    ValueId = valueId,
                    ReportDate = _parserResult.ReportDate,
                    InType = inType,
                    ValType = ReviewValueInput.ValueType.Estimated,
                    Value = _parser.GetCellDouble(row, _estimatedCol)
                };
                _parserResult.Result.Add(newValue);
            }

            if (_factCol > 0)
            {
                var newValue = new ReviewValueInput()
                {
                    GisId = gisId,
                    ValueId = valueId,
                    ReportDate = _parserResult.ReportDate,
                    InType = inType,
                    ValType = ReviewValueInput.ValueType.Fact,
                    Value = _parser.GetCellDouble(row, _factCol)
                };
                _parserResult.Result.Add(newValue);
            }
        }
        catch (Exception ex)
        {
            _parserResult.Messages.Add(ex.Message);
        }
    }

    private void ParseRowValue(Gis gis, int row)
    {
        var cellText = _parser.GetCellString(row, _countryCol);
        if (string.IsNullOrEmpty(cellText)) return;
        GetCellType(gis, cellText, out var valueId, out var inType);
        if (inType is null) return;
        AddInputValues(gis.Id, valueId, inType.Value, row);
    }

    public async Task<ParserResult> SaveResultAsync()
    {
        return await _helper.SaveResultAsync(_parserResult);
    }

    public async Task Dispose()
    {
        await _parser.Dispose();
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

        _parser.GetStringEntry(_fileTypeSetting.CountryEntry, out _, out _countryCol);
        _parser.GetStringEntry(_fileTypeSetting.GisEntry, out _, out _gisCol);
        if (_countryCol == 0) _countryCol = _gisCol + 1;

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
            _parser.GetStringEntry(_fileTypeSetting.EstimatedValueEntry, out _, out _estimatedCol);
        }
        else
        {
            _parser.GetStringEntry(_fileTypeSetting.EstimatedValueEntry, out _, out _estimatedCol);
        }

        return true;
    }
}