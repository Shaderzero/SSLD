using Microsoft.AspNetCore.Components.Forms;
using SSLD.Data.DailyReview;
using SSLD.Tools;

namespace SSLD.Parsers.Excel;

public class BalanceCpddParser: CpddParser
{
    private readonly string _sheetName = null;
    
    public BalanceCpddParser(IParserHelper helper) : base(helper)
    {
    }

    public override async Task<bool> SetFileAsync(IBrowserFile file)
    {
        await base.SetFileAsync(file);
        
        var setSheetResult = await Parser.SetSheet(_sheetName);
        return setSheetResult && ParseFileName();
    }
    
    protected override bool ParseFileName()
    {
        if (!base.ParseFileName()) return false;

        if (Helper.IsForced())
        {
            Parser.GetStringEntry(FileTypeSetting.RequestedValueEntry, out _, out RequestedCol);
            Parser.GetStringEntry(FileTypeSetting.AllocatedValueEntry, out _, out AllocatedCol);
            Parser.GetStringEntry(FileTypeSetting.EstimatedValueEntry, out _, out EstimatedCol);
            Parser.GetStringEntry(FileTypeSetting.FactValueEntry, out _, out FactCol);
            return true;
        }

        var hour = FileTypeSetting.LastHour;
        var timeSpan = new TimeOnly(hour, 0);
        var minTime = ParserResult.ReportDate.AddDays(-1).ToDateTime(new TimeOnly(0, 0));
        var maxTime = ParserResult.ReportDate.ToDateTime(timeSpan);
        if (minTime < ParserResult.FileTimeStamp && ParserResult.FileTimeStamp < maxTime)
        {
            Parser.GetStringEntry(FileTypeSetting.RequestedValueEntry, out _, out RequestedCol);
            Parser.GetStringEntry(FileTypeSetting.AllocatedValueEntry, out _, out AllocatedCol);
        }
        else
        {
            Parser.GetStringEntry(FileTypeSetting.EstimatedValueEntry, out _, out EstimatedCol);
            Parser.GetStringEntry(FileTypeSetting.FactValueEntry, out _, out FactCol);
        }
        return true;
    }
}