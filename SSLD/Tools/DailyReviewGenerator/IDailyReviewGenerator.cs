namespace SSLD.Tools.DailyReviewGenerator;

public interface IDailyReviewGenerator
{
    public void SetDateRange(DateOnly _startDate, DateOnly _endDate);
    public byte[] GetResult();
}