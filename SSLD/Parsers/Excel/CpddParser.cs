using Microsoft.AspNetCore.Components.Forms;
using SSLD.Data.DailyReview;
using SSLD.Tools;

namespace SSLD.Parsers.Excel;

public class CpddParser : IFileParser
{
    private IBrowserFile _file;
    protected IExcelParser Parser;
    protected readonly IParserHelper Helper;
    protected FileTypeSetting FileTypeSetting;
    private Gis[] _gisArray;
    protected readonly ParserResultImpl<ReviewValueInput> ParserResult = new();
    protected int RequestedCol = 0;
    protected int AllocatedCol = 0;
    protected int EstimatedCol = 0;
    protected int FactCol = 0;
    private int _countryCol;
    private int _gisCol;

    protected CpddParser(IParserHelper helper)
    {
        Helper = helper;
    }

    public virtual async Task<bool> SetFileAsync(IBrowserFile file)
    {
        _file = file;
        ParserResult.Filename = _file.Name;
        var extension = ParserResult.Filename.Split(new char[] { '.' })[^1];
        Parser = extension switch
        {
            "xlsx" => new XlsxParser(_file, ParserResult),
            "xlsm" => new XlsxParser(_file, ParserResult),
            "xls" => new XlsParser(_file, ParserResult),
            _ => Parser
        };

        FileTypeSetting = await Helper.GetFileSettings(ParserResult.Filename);
        _gisArray = await Helper.GetGisDetailArrayAsync();
        return true;
    }

    protected virtual bool ParseFileName()
    {
        var revisionTime = ParserResult.Filename.GetDateTime();
        var hour = FileTypeSetting.LastHour;
        var timeSpan = new TimeOnly(hour, 0);
        var reportDate = ParserResult.Filename.GetFirstDate();
        if (reportDate is null || revisionTime is null)
        {
            ParserResult.Messages.Add("Не удалось определить время отчета или ревизии");
            return false;
        }

        ParserResult.ReportDate = reportDate.Value;
        ParserResult.FileTimeStamp = revisionTime.Value;
        Parser.GetStringEntry(FileTypeSetting.CountryEntry, out _, out _countryCol);
        Parser.GetStringEntry(FileTypeSetting.GisEntry, out _, out _gisCol);
        return true;
    }

    public Task ParseAsync()
    {
        ParseRows();
        return Task.CompletedTask;
    }

    private void ParseRows()
    {
        Gis gis = null;
        Parser.GetLastCell(out var lastRow, out _);
        for (var i = 1; i <= lastRow; i++)
        {
            var cellText = Parser.GetCellString(i, _gisCol);
            if (string.IsNullOrEmpty(cellText)) continue;
            // проверяем не гис ли нам попался в ячейке
            var isGis = _gisArray.FirstOrDefault(x =>
                StringParser.NameContainAnyList(x.Names, cellText));
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
            if (RequestedCol > 0)
            {
                var newValue = new ReviewValueInput()
                {
                    GisId = gisId,
                    ValueId = valueId,
                    ReportDate = ParserResult.ReportDate,
                    InType = inType,
                    ValType = ReviewValueInput.ValueType.Requsted,
                    Value = Parser.GetCellDouble(row, RequestedCol)
                };
                ParserResult.Result.Add(newValue);
            }

            if (AllocatedCol > 0)
            {
                var newValue = new ReviewValueInput()
                {
                    GisId = gisId,
                    ValueId = valueId,
                    ReportDate = ParserResult.ReportDate,
                    InType = inType,
                    ValType = ReviewValueInput.ValueType.Allocated,
                    Value = Parser.GetCellDouble(row, AllocatedCol)
                };
                ParserResult.Result.Add(newValue);
            }

            if (EstimatedCol > 0)
            {
                var newValue = new ReviewValueInput()
                {
                    GisId = gisId,
                    ValueId = valueId,
                    ReportDate = ParserResult.ReportDate,
                    InType = inType,
                    ValType = ReviewValueInput.ValueType.Estimated,
                    Value = Parser.GetCellDouble(row, EstimatedCol)
                };
                ParserResult.Result.Add(newValue);
            }

            if (FactCol > 0)
            {
                var newValue = new ReviewValueInput()
                {
                    GisId = gisId,
                    ValueId = valueId,
                    ReportDate = ParserResult.ReportDate,
                    InType = inType,
                    ValType = ReviewValueInput.ValueType.Fact,
                    Value = Parser.GetCellDouble(row, FactCol)
                };
                ParserResult.Result.Add(newValue);
            }
        }
        catch (Exception ex)
        {
            ParserResult.Messages.Add(ex.Message);
        }
    }

    private void ParseRowValue(Gis gis, int row)
    {
        var cellText = Parser.GetCellString(row, _countryCol);
        if (string.IsNullOrEmpty(cellText)) return;
        GetCellType(gis, cellText, out var valueId, out var inType);
        if (inType is null) return;
        AddInputValues(gis.Id, valueId, inType.Value, row);
    }

    public async Task<ParserResult> SaveResultAsync()
    {
        return await Helper.SaveResultAsync(ParserResult);
    }

    public async Task Dispose()
    {
        await Parser.Dispose();
    }
}