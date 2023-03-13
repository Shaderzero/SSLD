using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using SSLD.Data;
using SSLD.Data.DailyReview;
using SSLD.DTO;
using SSLD.Interfaces;
using SSLD.Tools;

namespace SSLD.Parsers;

public abstract class ExcelParser: IExcelParser
{
    protected ExcelPackage Excel;
    protected readonly ApplicationDbContext Db;
    private readonly string _userId;
    protected IEnumerable<Gis> GisList;
    protected readonly  FileTypeSetting Settings;
    protected InputFileLog LogTime;
    private readonly FileMessage _fileMessage = new FileMessage();
    protected ExcelWorksheet Sheet;
    private DateOnly _reportDate;

    protected ExcelParser(ApplicationDbContext db, FileTypeSetting settings, string userId)
    {
        Db = db;
        _userId = userId;
        Settings = settings;
    }
    
    public async Task InitilalizeAsync()
    {
        GisList = await Db.Gises
            .Include(x => x.Addons)
            .Include(x => x.Countries).ThenInclude(gc => gc.Country)
            .Include(x => x.Countries).ThenInclude(gc => gc.Addons)
            .ToListAsync();
    }

    public async Task SetFileAsync(IBrowserFile file)
    {
        _fileMessage.FileTimeStamp = file.LastModified.DateTime;
        _fileMessage.Filename = file.Name;
        var reportDate = StringParser.GetFirstDateOnlyFromString(_fileMessage.Filename);
        if (reportDate == null)
        {
            _fileMessage.Message = "В результате парсинга файла " + _fileMessage.Filename + " не удалось установить дату";
            return;
        }
        _reportDate = reportDate.Value;
        await SetLog();
        Excel = new ExcelPackage();
        var stream = file.OpenReadStream(file.Size);
        await Excel.LoadAsync(stream);
        stream.Close();
    }

    public Task<FileMessage> GetResultAsync()
    {
        return Task.FromResult(_fileMessage);
    }

    protected virtual int[] GetExcelRange()
    {
        var topRow = GetRowEntry(Settings.CountryEntry);;
        var leftCol = GetColumnEntry(Settings.CountryEntry);;
        var rightCol = GetColumnEntry(Settings.FactValueEntry);
        var bottomRow = GetRowEntry(Settings.GisEntry);
        if (topRow == 0 || leftCol == 0 || rightCol == 0 || bottomRow == 0) return null;
        return new[] { topRow, leftCol, bottomRow, rightCol };
    }

    internal int GetColumnEntry(List<string> names)
    {
        var result = Sheet.Cells.FirstOrDefault(x => StringParser.NameContainAnyList(names, x.Value?.ToString()));
        return result != null ? result.Start.Column : 0;
    }

    internal int GetRowEntry(List<string> names)
    {
        var result = Sheet.Cells.FirstOrDefault(x => StringParser.NameContainAnyList(names, x.Value?.ToString()));
        return result != null ? result.Start.Row : 0;
    }

    private async Task SetLog()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        LogTime = await Db.InputFilesLogs.FirstOrDefaultAsync(x => x.Filename == _fileMessage.Filename);
        if (LogTime == null)
        {
            LogTime = new InputFileLog()
            {
                Filename = _fileMessage.Filename,
                InputTime = DateTime.Now,
                FileDate = today,
                FileTime = _fileMessage.FileTimeStamp,
                UserId = _userId
            };
            LogTime = (await Db.AddAsync(LogTime)).Entity;
        }
        else
        {
            LogTime.InputTime = DateTime.Now;
            LogTime.UserId = _userId;
            LogTime.FileTime = _fileMessage.FileTimeStamp;
            LogTime = Db.Update(LogTime).Entity;
        }
        await Db.SaveChangesAsync();
    }
}