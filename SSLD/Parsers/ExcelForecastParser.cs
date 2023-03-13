using Microsoft.EntityFrameworkCore;
using NPOI.OpenXmlFormats;
using NPOI.OpenXmlFormats.Dml;
using SSLD.Data;
using SSLD.Data.DailyReview;
using SSLD.Tools;

namespace SSLD.Parsers;

public class ExcelForecastParser : ExcelParser
{
    private Forecast _forecast;
    private int[] _intRange;
    
    public ExcelForecastParser(ApplicationDbContext db, FileTypeSetting settings, string userId) : base(db, settings, userId)
    {
    }
    
    public async Task<bool> GetResult()
    {
        await Parse();
        return true;
    }

    private async Task Parse()
    {
        Sheet = Excel.Workbook.Worksheets.FirstOrDefault();
        if (Sheet == null)
        {
            Excel.Dispose();
            return;
        }
        _intRange = GetExcelRange();
        _forecast = new Forecast
        {
            Fullname = GetName(),
            InputFileLog = LogTime,
            ReportDate = DateOnly.FromDateTime(DateTime.Today)
        };
        GetCountryValues();
        await SaveOrUpdate();
    }

    private void GetCountryValues()
    {
        var countryCol = _intRange[1];
        var januaryCol = GetMonthCol("январь");
        var februaryCol = GetMonthCol("февраль");
        var marchCol = GetMonthCol("март");
        var aprilCol = GetMonthCol("апрель");
        var mayCol = GetMonthCol("май");
        var juneCol = GetMonthCol("июнь");
        var julyCol = GetMonthCol("июль");
        var augustCol = GetMonthCol("август");
        var septemberCol = GetMonthCol("сентябрь");
        var octoberCol = GetMonthCol("октябрь");
        var novemberCol = GetMonthCol("ноябрь");
        var decemberCol = GetMonthCol("декабрь");
        for (var row = _intRange[2]; row >= _intRange[0]; row--)
        {
            var txt = Sheet.Cells[row, countryCol].Text;
            if (txt == null) continue;
            var gis = StringParser.FindGisByName(GisList, txt);
            if (gis == null) continue;
            for (row = row - 1; row >= _intRange[0]; row--)
            {
                if (GisList.FirstOrDefault(x =>
                        StringParser.NameContainAnyList(x.Names, Sheet.Cells[row, countryCol].Text)) != null)
                {
                    row++;
                    break;
                }
                var gisCountry = gis.Countries.FirstOrDefault(x => StringParser.NameContainAnyList(x.Country.Names, Sheet.Cells[row, countryCol].Text));
                if (gisCountry == null)
                {
                    continue;
                }
                _forecast.Countries.Add(new ForecastGisCountry(){GisCountryId = gisCountry.Id, Month = 1, Value = (decimal) Sheet.Cells[row, januaryCol].GetValue<double>()});
                _forecast.Countries.Add(new ForecastGisCountry(){GisCountryId = gisCountry.Id, Month = 2, Value = (decimal) Sheet.Cells[row, februaryCol].GetValue<double>()});
                _forecast.Countries.Add(new ForecastGisCountry(){GisCountryId = gisCountry.Id, Month = 3, Value = (decimal) Sheet.Cells[row, marchCol].GetValue<double>()});
                _forecast.Countries.Add(new ForecastGisCountry(){GisCountryId = gisCountry.Id, Month = 4, Value = (decimal) Sheet.Cells[row, aprilCol].GetValue<double>()});
                _forecast.Countries.Add(new ForecastGisCountry(){GisCountryId = gisCountry.Id, Month = 5, Value = (decimal) Sheet.Cells[row, mayCol].GetValue<double>()});
                _forecast.Countries.Add(new ForecastGisCountry(){GisCountryId = gisCountry.Id, Month = 6, Value = (decimal) Sheet.Cells[row, juneCol].GetValue<double>()});
                _forecast.Countries.Add(new ForecastGisCountry(){GisCountryId = gisCountry.Id, Month = 7, Value = (decimal) Sheet.Cells[row, julyCol].GetValue<double>()});
                _forecast.Countries.Add(new ForecastGisCountry(){GisCountryId = gisCountry.Id, Month = 8, Value = (decimal) Sheet.Cells[row, augustCol].GetValue<double>()});
                _forecast.Countries.Add(new ForecastGisCountry(){GisCountryId = gisCountry.Id, Month = 9, Value = (decimal) Sheet.Cells[row, septemberCol].GetValue<double>()});
                _forecast.Countries.Add(new ForecastGisCountry(){GisCountryId = gisCountry.Id, Month = 10, Value = (decimal) Sheet.Cells[row, octoberCol].GetValue<double>()});
                _forecast.Countries.Add(new ForecastGisCountry(){GisCountryId = gisCountry.Id, Month = 11, Value = (decimal) Sheet.Cells[row, novemberCol].GetValue<double>()});
                _forecast.Countries.Add(new ForecastGisCountry(){GisCountryId = gisCountry.Id, Month = 12, Value = (decimal) Sheet.Cells[row, decemberCol].GetValue<double>()});
            }
        }
    }

    private string GetName()
    {
        var cell = Sheet.Cells.FirstOrDefault(x => StringParser.ContainLike(x.Value?.ToString()?.ToLower(), "прогноз поставок "));
        return cell != null ? cell.Value.ToString() : "";
    }
    
    protected override int[] GetExcelRange()
    {
        var topRow = GetRowEntry(Settings.CountryEntry);;
        var leftCol = GetColumnEntry(Settings.CountryEntry);;
        var rightCol = GetColumnEntry(Settings.FactValueEntry);
        var bottomRow = GetRowEntry(Settings.GisEntry);
        if (topRow == 0 || leftCol == 0 || rightCol == 0 || bottomRow == 0) return null;
        return new[] { topRow, leftCol, bottomRow, rightCol };
    }
    
    private int GetMonthCol(string monthName)
    {
        var checkRange = Sheet.Cells[_intRange[0], _intRange[1], _intRange[0], _intRange[3]];
        var firstEntry = checkRange.FirstOrDefault(x => x.Value.ToString()?.ToLower() == monthName);
        return firstEntry == null ? 0 : firstEntry.Start.Column;
    }

    private async Task SaveOrUpdate()
    {
        var forecast = await Db.Forecasts
            .Include(x => x.Countries)
            .Where(x => x.InputFileLog.Id == LogTime.Id)
            .FirstOrDefaultAsync();
        if (forecast != null)
        {
            forecast.Countries = _forecast.Countries;
            _ = Db.Update(forecast);
        }
        else
        {
            _ = await Db.AddAsync(_forecast);
        }
        _ = await Db.SaveChangesAsync();
    }
}