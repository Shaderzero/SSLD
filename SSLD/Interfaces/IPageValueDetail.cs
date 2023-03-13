using SSLD.Data.DailyReview;

namespace SSLD.Interfaces;

public interface IPageValueDetail
{
    Task DeleteRow(long id);
    Task SaveRow(DayValue value);
    void ShowValueInfo(long? logId);
    void OnChange(DayValue val, int type, string input);
    Task ExportToExcel();
}