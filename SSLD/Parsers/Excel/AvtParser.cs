using Microsoft.AspNetCore.Components.Forms;
using SSLD.Data.DailyReview;
using SSLD.Tools;

namespace SSLD.Parsers.Excel;

public class AvtParser : IFileParser
{
    private IBrowserFile _file;
    private IExcelParser _parser;
    private FileTypeSetting _fileTypeSetting;
    private Gis[] _gisArray;
    private Gis _gisTurkey;
    private Gis _gisBlue;
    private readonly ParserResultImpl<ReviewValueInput> _parserResult = new();
    private readonly IParserHelper _helper;
    private int _dateCol;
    private int _dateRow;
    private readonly string _sheetName = null;
    private readonly string _gpe = "экспорт";
    private readonly string _gas = "Топл. газ";
    private readonly string _summ = "Итого";

    public AvtParser(IParserHelper helper)
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
        return await ParseFileName();
    }

    private async Task<bool> ParseFileName()
    {
        _fileTypeSetting = await _helper.GetFileSettings(_parserResult.Filename);
        _gisArray = await _helper.GetGisDetailArrayAsync();
        _gisTurkey = _gisArray.FirstOrDefault(x => x.Names.Any(n => n.ToLower().Equals("турецкий поток")));
        if (_gisTurkey == null)
        {
            _parserResult.Messages.Add("В БД не обнаружен Турецкий поток");
            return false;
        }

        _gisBlue = _gisArray.FirstOrDefault(x => x.Names.Any(n => n.ToLower().Equals("голубой поток")));
        if (_gisBlue == null)
        {
            _parserResult.Messages.Add("В БД не обнаружен Голубой поток");
            return false;
        }

        var revisionTime = _parserResult.Filename.GetDateTime();
        var reportDate = _parserResult.Filename.GetFirstDate();
        if (reportDate is null || revisionTime is null)
        {
            _parserResult.Messages.Add("Не удалось определить время отчета или ревизии");
            return false;
        }

        _parserResult.ReportDate = reportDate.Value;
        _parserResult.FileTimeStamp = revisionTime.Value;
        var setSheetResult = await _parser.SetSheet(_sheetName);
        return true;
    }

    public Task ParseAsync()
    {
        _parser.GetStringEntry(_fileTypeSetting.DataEntry, out _dateRow, out _dateCol);
        _parser.GetLastCell(out _, out var lastCol);
        for (var col = _dateCol + 1; col <= lastCol; col++)
        {
            var area = _parser.GetExcelArea(_dateRow, col);
            ParseFirstArea(area);
            col = area.EndCol;
        }
        return Task.CompletedTask;
    }

    private void ParseFirstArea(ExcelArea area)
    {
        var countryName = _parser.GetCellString(area.StartRow, area.StartCol);
        if (string.IsNullOrEmpty(countryName)) return;

        for (var col = area.EndCol; col >= area.StartCol; col--)
        {
            var secondArea = _parser.GetExcelArea(area.EndRow + 1, col);
            var secondAreaText = _parser.GetCellString(secondArea.StartRow, secondArea.StartCol);
            if (string.IsNullOrEmpty(secondAreaText)) continue;
            GisCountry country;
            if (StringParser.StrictLike(secondAreaText, _summ))
            {
                //оказалось значение "Итого", парсим весь столбец и выходим из диапазона
                country = _gisTurkey.Countries.FirstOrDefault(x =>
                    StringParser.NameContainAnyList(x.Country.Names, countryName));
                if (country != null)
                {
                    ParseColumn(country, col);
                    break;
                }
            }

            if (StringParser.StrictLike(secondAreaText, _gas))
            {
                //оказалось значение "Итого", парсим весь столбец и выходим из диапазона
                country = _gisTurkey.Countries.FirstOrDefault(x =>
                    StringParser.NameContainAnyList(x.Country.Names, countryName));
                if (country != null)
                {
                    ParseColumn(country, col);
                    break;
                }
            }

            var gis = _gisArray.FirstOrDefault(x =>
                StringParser.NameContainAnyList(x.Names, secondAreaText));
            country = gis?.Countries.FirstOrDefault(x => StringParser.NameContainAnyList(x.Country.Names, countryName));
            if (country != null)
            {
                ParseSecondArea(country, secondArea);
            }
            col = secondArea.StartCol;
        }
    }

    private void ParseSecondArea(GisCountry country, ExcelArea nextArea)
    {
        for (var col = nextArea.EndCol; col >= nextArea.StartCol; col--)
        {
            var thirdArea = _parser.GetExcelArea(nextArea.EndRow + 1, col);
            col = thirdArea.StartCol;
            var thirdAreaText = _parser.GetCellString(thirdArea.StartRow, thirdArea.StartCol);
            if (string.IsNullOrEmpty(thirdAreaText)) continue;

            if (StringParser.NameContainAnyList(_fileTypeSetting.FactValueEntry, thirdAreaText))
            {
                //в строке оказались Qсутки, значит парсим весь столбец
                ParseColumn(country, thirdArea.EndCol);
                break;
            }

            if (StringParser.StrictLike(_summ, thirdAreaText) || StringParser.ContainLike(thirdAreaText, _gpe))
            {
                //в строке оказались итого или газпром экспорт, значит парсим весь столбец
                ParseColumn(country, thirdArea.EndCol);
                continue;
            }

            //проверяем не аддон страны ли это
            var addon = country.Addons.FirstOrDefault(x =>
                x.Names.Any(n => StringParser.StrictLike(n, thirdAreaText)));
            if (addon != null)
            {
                ParseColumn(addon, thirdArea.EndCol);
                continue;
            }

            //проверяем не аддон гиса ли это
            var gisAddon = country.Gis.Addons.FirstOrDefault(x =>
                x.Names.Any(n => StringParser.StrictLike(n, thirdAreaText)));
            if (gisAddon != null)
            {
                ParseColumn(gisAddon, thirdArea.EndCol);
                continue;
            }
        }
    }

    private void ParseColumn(GisCountry country, int col)
    {
        var dateArea = _parser.GetExcelArea(_dateRow, _dateCol);
        for (var i = dateArea.EndRow + 1; i < dateArea.EndRow + 1 + 31; i++)
        {
            var dateTime = _parser.GetDateTime(i, _dateCol);
            if (dateTime == null) break;
            var date = DateOnly.FromDateTime(dateTime.Value);
            if (date > _parserResult.ReportDate)
            {
                break;
            }

            var value = _parser.GetCellDouble(i, col);
            value /= 1000d;
            var existed = _parserResult.Result
                .FirstOrDefault(x => x.GisId == country.GisId &&
                                     x.ValueId == country.Id &&
                                     x.InType == ReviewValueInput.InputType.Country &&
                                     x.ReportDate == date);
            if (existed == null)
            {
                _parserResult.Result.Add(new ReviewValueInput()
                {
                    GisId = country.GisId,
                    ValueId = country.Id,
                    ReportDate = date,
                    InType = ReviewValueInput.InputType.Country,
                    ValType = ReviewValueInput.ValueType.Fact,
                    Value = value
                });
            }
            else
            {
                existed.Value += value;
            }
        }
    }

    private void ParseColumn(GisCountryAddon addon, int col)
    {
        var dateArea = _parser.GetExcelArea(_dateRow, _dateCol);
        for (var i = dateArea.EndRow + 1; i < dateArea.EndRow + 1 + 31; i++)
        {
            var dateTime = _parser.GetDateTime(i, _dateCol);
            if (dateTime == null) break;
            var date = DateOnly.FromDateTime(dateTime.Value);
            if (date > _parserResult.ReportDate)
            {
                break;
            }

            var value = _parser.GetCellDouble(i, col);
            value /= 1000d;
            var existed = _parserResult.Result
                .FirstOrDefault(x => x.GisId == addon.GisCountry.GisId &&
                                     x.ValueId == addon.Id &&
                                     x.InType == ReviewValueInput.InputType.CountryAddon &&
                                     x.ReportDate == date);
            if (existed == null)
            {
                _parserResult.Result.Add(new ReviewValueInput()
                {
                    GisId = addon.GisCountry.GisId,
                    ValueId = addon.Id,
                    ReportDate = date,
                    InType = ReviewValueInput.InputType.CountryAddon,
                    ValType = ReviewValueInput.ValueType.Fact,
                    Value = value
                });
            }
            else
            {
                existed.Value += value;
            }
        }
    }

    private void ParseColumn(GisAddon addon, int col)
    {
        var dateArea = _parser.GetExcelArea(_dateRow, _dateCol);
        for (var i = dateArea.EndRow + 1; i < dateArea.EndRow + 1 + 31; i++)
        {
            var dateTime = _parser.GetDateTime(i, _dateCol);
            if (dateTime == null) break;
            var date = DateOnly.FromDateTime(dateTime.Value);
            if (date > _parserResult.ReportDate)
            {
                break;
            }

            var value = _parser.GetCellDouble(i, col);
            value /= 1000d;
            var existed = _parserResult.Result
                .FirstOrDefault(x => x.GisId == addon.GisId &&
                                     x.ValueId == addon.Id &&
                                     x.InType == ReviewValueInput.InputType.Addon &&
                                     x.ReportDate == date);
            if (existed == null)
            {
                _parserResult.Result.Add(new ReviewValueInput()
                {
                    GisId = addon.GisId,
                    ValueId = addon.Id,
                    ReportDate = date,
                    InType = ReviewValueInput.InputType.Addon,
                    ValType = ReviewValueInput.ValueType.Fact,
                    Value = value
                });
            }
            else
            {
                existed.Value += value;
            }
        }
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