using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SSLD.Data.DailyReview;

[Index(nameof(Year), IsUnique = true)]
public class ForecastYear
{
    public int Id { get; set; }
    public int Year { get; set; }
    public double[] GasTovValues { get; set; } = new double[12];
    public double[] GasPhgValues { get; set; } = new double[12];
    public double[] GpsValues { get; set; } = new double[12];
    public double[] ChinaValues { get; set; } = new double[12];
    public double[] RepoValues { get; set; } = new double[12];
    public string[] GasTovTxts { get; set; } = new string[12];
    public string[] GasPhgTxts { get; set; } = new string[12];
    public string[] GpsTxts { get; set; } = new string[12];
    public string[] ChinaTxts { get; set; } = new string[12];
    public string[] RepoTxts { get; set; } = new string[12];
    public List<Forecast> Forecasts { get; set; } = new ();

    [NotMapped]
    public double[] DailyFactNoPhg
    {
        get
        {
            var array = new double[12];
            for (var i = 0; i < 12; i++)
            {
                var days = DateTime.DaysInMonth(Year, i+1);
                array[i] = (GasTovValues[i] - GpsValues[i]) / days;
            }

            return array;
        }
    }
}