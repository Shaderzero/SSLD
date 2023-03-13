using Microsoft.EntityFrameworkCore;
using SSLD.Data;
using SSLD.Data.DailyReview;
using SSLD.Data.DZZR;

namespace SSLD.Parsers.Excel;

public class DzZrSaver
{
    private readonly ApplicationDbContext _db;
    private readonly ParserResultImpl<OperatorResource> _parserResult;
    private readonly string _userId;
    private InputFileLog _log;
    private readonly bool _isForced;

    public DzZrSaver(ApplicationDbContext db, ParserResultImpl<OperatorResource> result, string userId, bool isForced = false)
    {
        _db = db;
        _userId = userId;
        _isForced = isForced;
        _parserResult = result;
    }

    public async Task<ParserResult> CreateOrUpdateAsync()
    {
        _parserResult.SendedCount = _parserResult.Result.Count;
        var dbValues = await _db.OperatorResources
            .Include(x => x.Hours)
            .Where(x => x.SupplyDate == _parserResult.ReportDate)
            .ToListAsync();
        foreach (var value in _parserResult.Result)
        {
            if (dbValues.Count > 0)
            {
                var dbVal = dbValues
                    .FirstOrDefault(x => x.OperatorGisId == value.OperatorGisId &&
                                         x.ReportDate == value.ReportDate &&
                                         x.SupplyDate == value.SupplyDate &&
                                         x.Type == value.Type);
                if (dbVal != null)
                {
                    dbVal.Hours = value.Hours;
                    _ = _db.Update(dbVal);
                    _parserResult.UpdatedCount++;
                    continue;
                }
            }
            _ = await _db.AddAsync(value);
            _parserResult.CreatedCount++;
        }
        await _db.SaveChangesAsync();
        return _parserResult;
    }
}