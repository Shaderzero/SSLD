using Microsoft.EntityFrameworkCore;
using SSLD.Data;
using SSLD.Data.DailyReview;
using SSLD.Data.DZZR;
using SSLD.Tools;

namespace SSLD.Parsers.Excel;

public class ParserHelper : IParserHelper
{
    private ApplicationDbContext _db;
    private readonly string _userId;
    private readonly bool _isForced;
    private Gis[] _gisArray;
    private OperatorGis[] _operatorGisArray;
    private Gis[] _gisDetailArray;
    private FileTypeSetting[] _fileTypeSettings;

    public ParserHelper(ApplicationDbContext db, string userId, bool isForced)
    {
        _db = db;
        _userId = userId;
        _isForced = isForced;
    }


    public async Task<Gis[]> GetGisArrayAsync()
    {
        // if (_gisArray == null || _gisArray.Length == 0)
        // {
        //     _gisArray = await _db.Gises.ToArrayAsync();
        // }
        _gisArray ??= await _db.Gises.ToArrayAsync();
        return _gisArray;
    }

    public async Task<Gis[]> GetGisDetailArrayAsync()
    {
        // if (_gisDetailArray == null || _gisDetailArray.Length == 0)
        // {
        //     _gisDetailArray = await _db.Gises
        //         .Include(x => x.Addons)
        //         .Include(x => x.Countries).ThenInclude(gc => gc.Country)
        //         .Include(x => x.Countries).ThenInclude(gc => gc.Addons)
        //         .ToArrayAsync();
        // }
        _gisDetailArray ??= await _db.Gises
            .Include(x => x.Addons)
            .Include(x => x.Countries).ThenInclude(gc => gc.Country)
            .Include(x => x.Countries).ThenInclude(gc => gc.Addons)
            .ToArrayAsync();
        return _gisDetailArray;
    }

    public async Task<OperatorGis[]> GetOperatorGisArrayAsync()
    {
        // if (_operatorGisArray == null || _operatorGisArray.Length == 0)
        // {
        //     _operatorGisArray = await _db.OperatorGises.ToArrayAsync();
        // }
        _operatorGisArray ??= await _db.OperatorGises.ToArrayAsync();
        return _operatorGisArray;
    }

    public bool IsForced()
    {
        return _isForced;
    }

    public async Task<FileTypeSetting> GetFileSettings(string filename)
    {
        // if (_fileTypeSettings == null || _fileTypeSettings.Length == 0)
        // {
        //     _fileTypeSettings = await _db.FileTypeSettings.ToArrayAsync();
        // }
        _fileTypeSettings ??= await _db.FileTypeSettings.ToArrayAsync();
        var fileSettings = _fileTypeSettings.FirstOrDefault(x =>
                               StringParser.NameContainAllList(x.MustHave, filename) &&
                               !StringParser.NameContainAnyList(x.NotHave, filename))
                           ?? _fileTypeSettings.FirstOrDefault(x => StringParser.NameContainAnyList(x.MustHave, filename));
        return fileSettings;
    }

    public async Task<ParserResult> SaveResultAsync(ParserResultImpl<ReviewValueInput> parserResultImpl)
    {
        var saver = new DailyReviewSaver(_db, parserResultImpl, _userId, _isForced);
        return await saver.CreateOrUpdateAsync();
    }

    public async Task<ParserResult> SaveResultAsync(ParserResultImpl<OperatorResource> parserResultImpl)
    {
        var saver = new DzZrSaver(_db, parserResultImpl, _userId, _isForced);
        return await saver.CreateOrUpdateAsync();
    }
    
    public async Task<ParserResult> SaveResultAsync(ParserResultImpl<ForecastGisCountry> parserResultImpl)
    {
        var saver = new ForecastSaver(_db, parserResultImpl, _userId, _isForced);
        return await saver.CreateOrUpdateAsync();
    }
}