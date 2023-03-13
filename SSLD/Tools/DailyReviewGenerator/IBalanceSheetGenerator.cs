using OfficeOpenXml;

namespace SSLD.Tools.DailyReviewGenerator;

public interface IBalanceSheetGenerator
{
    void SetSheet(ExcelWorksheet sheet);
    void SetLastDate(DateOnly endDate);
    bool Generate();
}