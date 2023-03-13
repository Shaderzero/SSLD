using Microsoft.EntityFrameworkCore;
using SSLD.Data;
using SSLD.Data.DailyReview;
using SSLD.Tools;

namespace SSLD.Parsers.Excel;

public class DailyReviewSaver
{
    private readonly ApplicationDbContext _db;
    private readonly ParserResultImpl<ReviewValueInput> _parserResult;
    private readonly string _userId;
    private InputFileLog _log;
    private GisCountryValue[] _gisCountryValues;
    private GisCountryAddonValue[] _gisCountryAddonValues;
    private GisAddonValue[] _gisAddonValues;
    private GisInputValue[] _gisInputValues;
    private GisOutputValue[] _gisOutputValues;
    private readonly bool _isForced;

    public DailyReviewSaver(ApplicationDbContext db, ParserResultImpl<ReviewValueInput> result, string userId, bool isForced = false)
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
        _log = await _db.InputFilesLogs.FirstOrDefaultAsync(x => x.Filename == _parserResult.Filename);
        var dates = _parserResult.Result.GroupBy(x => x.ReportDate).Select(x => x.Key).ToArray();
        _gisCountryValues = await _db.GisCountryValues
            .Include(x => x.RequestedValueTime)
            .Include(x => x.AllocatedValueTime)
            .Include(x => x.EstimatedValueTime)
            .Include(x => x.FactValueTime)
            .Where(x => dates.Any(d => d == x.ReportDate))
            .ToArrayAsync();
        _gisCountryAddonValues = await _db.GisCountryAddonValues
            .Include(x => x.RequestedValueTime)
            .Include(x => x.AllocatedValueTime)
            .Include(x => x.EstimatedValueTime)
            .Include(x => x.FactValueTime)
            .Where(x => dates.Any(d => d == x.ReportDate))
            .ToArrayAsync();
        _gisAddonValues = await _db.GisAddonValues
            .Include(x => x.RequestedValueTime)
            .Include(x => x.AllocatedValueTime)
            .Include(x => x.EstimatedValueTime)
            .Include(x => x.FactValueTime)
            .Where(x => dates.Any(d => d == x.ReportDate))
            .ToArrayAsync();
        _gisInputValues = await _db.GisInputValues
            .Include(x => x.RequestedValueTime)
            .Include(x => x.AllocatedValueTime)
            .Include(x => x.EstimatedValueTime)
            .Include(x => x.FactValueTime)
            .Where(x => dates.Any(d => d == x.ReportDate))
            .ToArrayAsync();
        _gisOutputValues = await _db.GisOutputValues
            .Include(x => x.RequestedValueTime)
            .Include(x => x.AllocatedValueTime)
            .Include(x => x.EstimatedValueTime)
            .Include(x => x.FactValueTime)
            .Where(x => dates.Any(d => d == x.ReportDate))
            .ToArrayAsync();
        if (_log == null)
        {
            _log = CreateLog();
            _log = _db.Add(_log).Entity;
        }
        else
        {
            _log.InputTime = DateTime.Now;
            _log.UserId = _userId;
            _log.FileTime = timeFile;
            _log = _db.Update(_log).Entity;
        }

        await _db.SaveChangesAsync();
        foreach (var value in _parserResult.Result)
        {
            switch (value.InType)
            {
                case ReviewValueInput.InputType.Country:
                    CreateOrUpdateGcValues(value);
                    break;
                case ReviewValueInput.InputType.CountryAddon:
                    CreateOrUpdateCountryAddonValues(value);
                    break;
                case ReviewValueInput.InputType.Addon:
                    CreateOrUpdateAddonValues(value);
                    break;
                case ReviewValueInput.InputType.Input:
                    CreateOrUpdateInputValues(value);
                    break;
                case ReviewValueInput.InputType.Output:
                    CreateOrUpdateOutputValues(value);
                    break;
                default:
                    break;
            }
        }
        await _db.SaveChangesAsync();

        return _parserResult;
    }

    private void DayValueSetup(DayValue value, ReviewValueInput input)
    {
        var val = Convert.ToDecimal(Math.Round(input.Value, 8));
        switch (input.ValType)
        {
            case ReviewValueInput.ValueType.Requsted when (value.RequestedValueTime == null ||
                                                           _log.FileTime >= 
                                                           value.RequestedValueTime?.FileTime || _isForced):
                value.RequestedValue = val;
                value.RequestedValueTimeId = _log.Id;
                break;
            case ReviewValueInput.ValueType.Allocated when (value.AllocatedValueTime == null ||
                                                            _log.FileTime >=
                                                            value.AllocatedValueTime?.FileTime || _isForced):
                value.AllocatedValue = val;
                value.AllocatedValueTimeId = _log.Id;
                break;
            case ReviewValueInput.ValueType.Estimated when (value.EstimatedValueTime == null ||
                                                            _log.FileTime >=
                                                            value.EstimatedValueTime?.FileTime || _isForced):
                value.EstimatedValue = val;
                value.EstimatedValueTimeId = _log.Id;
                break;
            case ReviewValueInput.ValueType.Fact when (value.FactValueTime == null ||
                                                       _log.FileTime >= 
                                                       value.FactValueTime?.FileTime || _isForced):
                value.FactValue = val;
                value.FactValueTimeId = _log.Id;
                break;
            default:
                break;
        }
    }

    private void CreateOrUpdateGcValues(ReviewValueInput value)
    {
        var dbVal = _gisCountryValues
            .FirstOrDefault(x => x.GisCountryId == value.ValueId && x.ReportDate == value.ReportDate);
        if (dbVal == null)
        {
            var newValue = new GisCountryValue()
            {
                ReportDate = value.ReportDate,
                GisCountryId = value.ValueId
            };
            DayValueSetup(newValue, value);
            _db.Add(newValue);
            _parserResult.CreatedCount++;
        }
        else
        {
            DayValueSetup(dbVal, value);
            _db.Update(dbVal);
            _parserResult.UpdatedCount++;
        }
    }

    private void CreateOrUpdateAddonValues(ReviewValueInput value)
    {
        var dbVal = _gisAddonValues
            .FirstOrDefault(x => x.GisAddonId == value.ValueId);
        if (dbVal == null)
        {
            var newValue = new GisAddonValue()
            {
                ReportDate = value.ReportDate,
                GisAddonId = value.ValueId
            };
            DayValueSetup(newValue, value);
            _db.Add(newValue);
            _parserResult.CreatedCount++;
        }
        else
        {
            DayValueSetup(dbVal, value);
            _db.Update(dbVal);
            _parserResult.UpdatedCount++;
        }
    }

    private void CreateOrUpdateCountryAddonValues(ReviewValueInput value)
    {
        var dbVal = _gisCountryAddonValues
            .FirstOrDefault(x => x.GisCountryAddonId == value.ValueId);
        if (dbVal == null)
        {
            var newValue = new GisCountryAddonValue()
            {
                ReportDate = value.ReportDate,
                GisCountryAddonId = value.ValueId
            };
            DayValueSetup(newValue, value);
            _db.Add(newValue);
            _parserResult.CreatedCount++;
        }
        else
        {
            DayValueSetup(dbVal, value);
            _db.Update(dbVal);
            _parserResult.UpdatedCount++;
        }
    }

    private void CreateOrUpdateInputValues(ReviewValueInput value)
    {
        var dbVal = _gisInputValues
            .FirstOrDefault(x => x.GisId == value.GisId);
        if (dbVal == null)
        {
            var newValue = new GisInputValue()
            {
                ReportDate = value.ReportDate,
                GisId = value.GisId
            };
            DayValueSetup(newValue, value);
            _db.Add(newValue);
            _parserResult.CreatedCount++;
        }
        else
        {
            DayValueSetup(dbVal, value);
            _db.Update(dbVal);
            _parserResult.UpdatedCount++;
        }
    }

    private void CreateOrUpdateOutputValues(ReviewValueInput value)
    {
        var dbVal = _gisOutputValues
            .FirstOrDefault(x => x.GisId == value.GisId);
        if (dbVal == null)
        {
            var newValue = new GisOutputValue()
            {
                ReportDate = value.ReportDate,
                GisId = value.GisId
            };
            DayValueSetup(newValue, value);
            _db.Add(newValue);
            _parserResult.CreatedCount++;
        }
        else
        {
            DayValueSetup(dbVal, value);
            _db.Update(dbVal);
            _parserResult.UpdatedCount++;
        }
    }

    private InputFileLog CreateLog()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var log = new InputFileLog()
        {
            Filename = _parserResult.Filename,
            InputTime = DateTime.Now,
            FileDate = today,
            FileTime = _parserResult.FileTimeStamp,
            UserId = _userId
        };
        return log;
    }
}