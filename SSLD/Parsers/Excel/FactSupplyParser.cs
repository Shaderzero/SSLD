using SSLD.Data.DailyReview;
using SSLD.Tools;

namespace SSLD.Parsers.Excel;

public class FactSupplyParser : GasDayParser
{
    public FactSupplyParser(IParserHelper helper) : base(helper)
    {
    }

    protected override bool GetAddonValue(GisAddon addon, int r)
    {
        if (_factCol <= 0) return false;
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

        return true;
    }
}