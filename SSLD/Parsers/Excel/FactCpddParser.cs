using Microsoft.AspNetCore.Components.Forms;

namespace SSLD.Parsers.Excel;

public class FactCpddParser : CpddParser
{
    private readonly string _sheetName = null;

    public FactCpddParser(IParserHelper helper) : base(helper)
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

        Parser.GetStringEntry(FileTypeSetting.FactValueEntry, out _, out FactCol);
        return true;
    }
}