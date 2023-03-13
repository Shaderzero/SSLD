using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore;
using Radzen;
using SSLD.Data;
using SSLD.Data.DailyReview;
using SSLD.Tools;

namespace SSLD.Parsers;

public class ExcelDailyReviewParser
{
    private readonly NotificationService _notificationService;
    private readonly ApplicationDbContext _db;
    private readonly IBrowserFile _file;
    private FileTypeSetting _fileSettings;
    private IEnumerable<Gis> _gisList;
    private List<ReviewValueInput> _valueList = new();
    private List<ForecastGisCountry> _forecastValueList = new();
    private string _message;
    private string _userId;

    public ExcelDailyReviewParser(IBrowserFile file, NotificationService notificationService, ApplicationDbContext db, string userId)
    {
        _file = file;
        _db = db;
        _notificationService = notificationService;
        _userId = userId;
    }

    public async Task ParseFile()
    {
        var filename = _file.Name;
        var fileTypes = await _db.FileTypeSettings.ToListAsync();
        _fileSettings = fileTypes.FirstOrDefault(x => StringParser.NameContainAllList(x.MustHave, filename) && !StringParser.NameContainAnyList(x.NotHave, filename));
        if (_fileSettings == null)
        {
            _fileSettings = fileTypes.FirstOrDefault(x => StringParser.NameContainAnyList(x.MustHave, filename));
            if (_fileSettings == null)
            {
                _message = "Файл не опознан";
                return;
            }
        }
        _gisList = await _db.Gises
            .Include(x => x.Addons)
            .Include(x => x.Countries).ThenInclude(gc => gc.Country)
            .Include(x => x.Countries).ThenInclude(gc => gc.Addons)
            .ToListAsync();
        switch (_fileSettings.TypeName)
        {
            case SD.File_Balance_Cpdd:
            {
                var parser = new ExcelBalanceCpddParser(_file, _gisList, _fileSettings, _notificationService);
                _valueList = await parser.GetResult();
                _message = parser.GetMessage();
                break;
            }
            case SD.File_Fact_Cpdd:
            {
                var parser = new ExcelFactCpddParser(_file, _gisList, _fileSettings, _notificationService);
                _valueList = await parser.GetResult();
                _message = parser.GetMessage();
                break;
            }
            case SD.File_Avt:
            {
                var parser = new ExcelAvtParser(_file, _gisList, _fileSettings, _notificationService);
                _valueList = await parser.GetResult();
                _message = parser.GetMessage();
                break;
            }
            case SD.File_Ge_Mail:
            {
                var parser = new ExcelGeMailParser(_file, _gisList, _fileSettings, _notificationService);
                _valueList = await parser.GetResult();
                _message = parser.GetMessage();
                break;
            }
            case SD.File_Gas_Day:
            {
                var parser = new ExcelGasDayParser(_file, _gisList, _fileSettings, _notificationService);
                _valueList = await parser.GetResult();
                _message = parser.GetMessage();
                break;
            }
            case SD.File_Fact_Supply:
            {
                var parser = new ExcelFactSupplyParser(_file, _gisList, _fileSettings, _notificationService);
                _valueList = await parser.GetResult();
                _message = parser.GetMessage();
                break;
            }
            case SD.File_Teterevka:
            {
                var parser = new ExcelTeterevkaParser(_file, _gisList, _fileSettings, _notificationService);
                _valueList = await parser.GetResult();
                _message = parser.GetMessage();
                break;
            }
            case SD.File_Forecast:
            {
                var parser = new ExcelForecastParser(_db, _fileSettings, _userId);
                await parser.InitilalizeAsync();
                await parser.SetFileAsync(_file);
                var forecastValueList  = await parser.GetResult();
                break;
            }
        }
    }

    public ReviewValueList GetResult()
    {
        var result = new ReviewValueList()
        {
            Values = _valueList,
            Filename = _file.Name,
            FileTimeStamp = _file.LastModified.DateTime,
            Message = _message
        };
        return result;
    }
}