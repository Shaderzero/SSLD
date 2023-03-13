using Microsoft.EntityFrameworkCore;
using SSLD.Data;
using SSLD.Data.DailyReview;
using SSLD.Tools;

namespace SSLD.Parsers.Excel;

public class ForecastSaver
{
    private readonly ApplicationDbContext _db;
    private readonly ParserResultImpl<ForecastGisCountry> _parserResult;
    private readonly string _userId;
    private InputFileLog _log;
    private readonly bool _isForced;

    public ForecastSaver(ApplicationDbContext db, ParserResultImpl<ForecastGisCountry> result, string userId, bool isForced = false)
    {
        _db = db;
        _userId = userId;
        _isForced = isForced;
        _parserResult = result;
    }

    public async Task<ParserResult> CreateOrUpdateAsync()
    {
        var timeFile = _parserResult.FileTimeStamp;
        _parserResult.SendedCount = _parserResult.Result.Count;
        _log = await GetLog();
        var forecast = await _db.Forecasts
            .Include(x => x.Countries)
            .Where(x => x.InputFileLog.Id == _log.Id)
            .FirstOrDefaultAsync();

        if (forecast != null)
        {
            forecast.Countries = _parserResult.Result;
            _ = _db.Update(forecast);
            _parserResult.UpdatedCount = _parserResult.SendedCount;
        }
        else
        {
            var forecastYear = await GetForecastYear();
            forecast = new Forecast
            {
                Fullname = _parserResult.Filename,
                InputFileLogId = _log.Id,
                ForecastYearId = forecastYear.Id,
                ReportDate = DateOnly.FromDateTime(DateTime.Today),
                Countries = _parserResult.Result
            };
            _ = await _db.AddAsync(forecast);
            _parserResult.CreatedCount = _parserResult.SendedCount;
        }
        await _db.SaveChangesAsync();
        return _parserResult;
    }

    private async Task<InputFileLog> GetLog()
    {
        _log = await _db.InputFilesLogs.FirstOrDefaultAsync(x => x.Filename == _parserResult.Filename);
        if (_log == null)
        {
            _log = new InputFileLog()
            {
                Filename = _parserResult.Filename,
                InputTime = DateTime.Now,
                FileDate = DateOnly.FromDateTime(DateTime.Today),
                FileTime = _parserResult.FileTimeStamp,
                UserId = _userId
            };
            _log = _db.Add(_log).Entity;
        }
        else
        {
            _log.InputTime = DateTime.Now;
            _log.UserId = _userId;
            _log.FileTime = _parserResult.FileTimeStamp;
            _log = _db.Update(_log).Entity;
        }

        var result = await _db.SaveChangesAsync();
        return result > 0 ? _log : null;
    }

    private async Task<ForecastYear> GetForecastYear()
    {
        var year = _parserResult.ReportDate.Year;
        var forecastYear = _db.ForecastYears.FirstOrDefault(x => x.Year == year);
        if (forecastYear != null) return forecastYear;
        var fy = new ForecastYear()
        {
            Year = year
        };
        forecastYear = _db.ForecastYears.Add(fy).Entity;
        await _db.SaveChangesAsync();

        return forecastYear;
    }

}