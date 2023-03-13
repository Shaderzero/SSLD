using Microsoft.AspNetCore.Components.Forms;
using SSLD.Data.DailyReview;
using SSLD.Data.DZZR;
using SSLD.Tools;

namespace SSLD.Parsers.Excel;

public class ForecastParser : IFileParser
{
    private IBrowserFile _file;
    private IExcelParser _parser;
    private FileTypeSetting _fileTypeSetting;
    private Gis[] _gisArray;
    private readonly ParserResultImpl<ForecastGisCountry> _parserResult = new();
    private readonly IParserHelper _helper;
    private readonly string _sheetName = null;

    public ForecastParser(IParserHelper helper)
    {
        _helper = helper;
    }
    
    public async Task<bool> SetFileAsync(IBrowserFile file)
    {
        _file = file;
        _parserResult.Filename = _file.Name;
        _parserResult.ReportDate = DateOnly.FromDateTime(DateTime.Today);
        _parserResult.FileTimeStamp = _file.LastModified.DateTime;
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
        return setSheetResult;
    }
    
    private async Task<bool> GetReferences()
    {   
        _fileTypeSetting = await _helper.GetFileSettings(_parserResult.Filename);
        _gisArray = await _helper.GetGisDetailArrayAsync();
        return true;
    }

    private ExcelArea GetExcelArea()
    {
        _parser.GetStringEntry(_fileTypeSetting.CountryEntry, out var startRow, out var startCol );
        _parser.GetStringEntry(_fileTypeSetting.FactValueEntry, out _, out var endCol );
        _parser.GetStringEntry(_fileTypeSetting.GisEntry, out var endRow, out _);
        return new ExcelArea(startRow, startCol, endRow, endCol);
    }

    public Task ParseAsync()
    {
        var area = GetExcelArea();
        for (var row = area.EndRow; row >= area.StartRow; row--)
        {
            var txt = _parser.GetCellString(row, area.StartCol);
            if (txt == null) continue;
            var gis = StringParser.FindGisByName(_gisArray, txt);
            if (gis == null) continue;
            for (row = row - 1; row >= area.StartRow; row--)
            {
                var countryName = _parser.GetCellString(row, area.StartCol);
                if (_gisArray.FirstOrDefault(x =>
                        StringParser.NameContainAnyList(x.Names, countryName)) != null)
                {
                    row++;
                    break;
                }
                var gisCountry = gis.Countries.FirstOrDefault(x => StringParser.NameContainAnyList(x.Country.Names, countryName));
                if (gisCountry == null) continue;
                for (var num = 1; num <= 12; num++)
                {
                    var monthName = TimeParser.NumToMonth(num);
                    _parser.GetStringEntry(monthName, out _, out var monthCol);
                    _parserResult.Result.Add(new ForecastGisCountry(){GisCountryId = gisCountry.Id, Month = num, Value = (decimal) _parser.GetCellDouble(row, monthCol)});
                }
            }
        }
        return Task.CompletedTask;
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