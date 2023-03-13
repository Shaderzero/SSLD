using OfficeOpenXml;
using SSLD.Data.DailyReview;

namespace SSLD.Tools.DailyReviewGenerator;

public class BalanceSheetGenerator: IBalanceSheetGenerator
{
    private ExcelWorksheet _sheet;
    private Forecast[] _forecasts;
    private DateOnly _endDate;
    private DataHelper _dataHelper;

    public BalanceSheetGenerator(DataHelper dataHelper)
    {
        _dataHelper = dataHelper;
    }

    public void SetSheet(ExcelWorksheet sheet)
    {
        _sheet = sheet;
    }

    public void SetLastDate(DateOnly endDate)
    {
        _endDate = endDate;
    }

    public bool Generate()
    {
        throw new NotImplementedException();
    }
}