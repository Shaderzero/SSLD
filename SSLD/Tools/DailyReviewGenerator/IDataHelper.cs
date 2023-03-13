using SSLD.Data.DailyReview;

namespace SSLD.Tools.DailyReviewGenerator;

public interface IDataHelper
{
    Forecast[] GetForecasts();
    decimal[] GetTovGas();
    decimal[] GetTovGasPhg();
    decimal[] GetGpS();
    decimal[] GetChina();
}