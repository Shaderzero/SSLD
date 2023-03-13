using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore;
using Radzen;
using SSLD.Data;
using SSLD.Data.DailyReview;
using SSLD.DTO;
using SSLD.Parsers.Excel;
using SSLD.Tools;

namespace SSLD.Pages.DailyReview;

[Authorize(Roles = SD.Role_User)]
public partial class PageDailyReviewInput
{
    [Inject] public ApplicationDbContext Db { get; set; }
    [Inject] public NotificationService NotificationService { get; set; }
    [Inject] public AuthenticationStateProvider AuthProvider { get; set; }
    private string _currentFile;
    private bool _isReady = false;
    private List<ParserResult> _parserResults;
    private List<FileTypeSetting> _fileTypes;
    private string _userId;
    private IQueryable<GisCountryValue> _gisCountryValues;
    private IQueryable<GisCountryAddonValue> _gisCountryAddonValues;
    private IQueryable<GisAddonValue> _gisAddonValues;
    private IQueryable<GisInputValue> _gisInputValues;
    private IQueryable<GisOutputValue> _gisOutputValues;
    private bool _isForced = false;

    protected override async Task OnInitializedAsync()
    {
        _userId = await CurrentUserId();
        _fileTypes = await Db.FileTypeSettings.ToListAsync();
        _gisCountryValues = Db.GisCountryValues
            .Include(x => x.RequestedValueTime)
            .Include(x => x.AllocatedValueTime)
            .Include(x => x.EstimatedValueTime)
            .Include(x => x.FactValueTime)
            .AsQueryable();
        _gisCountryAddonValues = Db.GisCountryAddonValues
            .Include(x => x.RequestedValueTime)
            .Include(x => x.AllocatedValueTime)
            .Include(x => x.EstimatedValueTime)
            .Include(x => x.FactValueTime)
            .AsQueryable();
        _gisAddonValues = Db.GisAddonValues
            .Include(x => x.RequestedValueTime)
            .Include(x => x.AllocatedValueTime)
            .Include(x => x.EstimatedValueTime)
            .Include(x => x.FactValueTime)
            .AsQueryable();
        _gisInputValues = Db.GisInputValues
            .Include(x => x.RequestedValueTime)
            .Include(x => x.AllocatedValueTime)
            .Include(x => x.EstimatedValueTime)
            .Include(x => x.FactValueTime)
            .AsQueryable();
        _gisOutputValues = Db.GisOutputValues
            .Include(x => x.RequestedValueTime)
            .Include(x => x.AllocatedValueTime)
            .Include(x => x.EstimatedValueTime)
            .Include(x => x.FactValueTime)
            .AsQueryable();
    }

    private async Task LoadExcelFiles(InputFileChangeEventArgs e)
    {
        _isReady = false;
        _parserResults = new List<ParserResult>();
        if (e.FileCount > 100)
        {
            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Error,
                Summary = "Слишком много файлов",
                Detail = "Максимум возможна обработка до 100 файлов за раз",
                Duration = 5000
            });
            _isReady = true;
            return;
        }
        foreach (var file in e.GetMultipleFiles(100))
        {
            _parserResults.Add(new ParserResult()
            {
                Filename = file.Name
            });
        }

        var helper = new ParserHelper(Db, _userId, _isForced);
        var parser = new FileParser(helper);
        foreach (var file in e.GetMultipleFiles(100))
        {
            _currentFile = file.Name;
            var parserResult = _parserResults.FirstOrDefault(x => x.Filename == _currentFile);
            var fileResult = await parser.SetFileAsync(file);
            if (!fileResult)
            {
                parserResult?.Messages.Add("файл не распознан");
                continue;
            }
            await parser.ParseAsync();
            await parser.Dispose();
            var result = await parser.SaveResultAsync();
            if (parserResult != null)
            {
                parserResult.Messages = result.Messages;
                parserResult.CreatedCount = result.CreatedCount;
                parserResult.UpdatedCount = result.UpdatedCount;
                parserResult.SendedCount = result.SendedCount;
            }
            
            StateHasChanged();
        }
        _currentFile = null;
        _isForced = false;
        _isReady = true;
    }

    private async Task<string> CurrentUserId()
    {
        var authState = await AuthProvider.GetAuthenticationStateAsync();
        var user = authState.User;
        var userId = user.FindFirst(c => c.Type.Contains("nameidentifier"))?.Value;
        return userId;
    }

    private async Task CreateOrUpdate(ReviewValueList list, FileMessage message)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var timeFile = list.FileTimeStamp;
        var logTime = await Db.InputFilesLogs.FirstOrDefaultAsync(x => x.Filename == list.Filename);
        if (logTime == null)
        {
            logTime = new InputFileLog()
            {
                Filename = list.Filename,
                InputTime = DateTime.Now,
                FileDate = today,
                FileTime = timeFile,
                UserId = _userId
            };
            logTime = (await Db.AddAsync(logTime)).Entity;
        }
        else
        {
            logTime.InputTime = DateTime.Now;
            logTime.UserId = _userId;
            logTime.FileTime = timeFile;
            logTime = Db.Update(logTime).Entity;
        }
        await Db.SaveChangesAsync();
        foreach (var value in list.Values)
        {
            switch (value.InType)
            {
                case ReviewValueInput.InputType.Country:
                    await CreateOrUpdateGcValues(value, logTime, message);
                    break;
                case ReviewValueInput.InputType.CountryAddon:
                    await CreateOrUpdateCountryAddonValues(value, logTime, message);
                    break;
                case ReviewValueInput.InputType.Addon:
                    await CreateOrUpdateAddonValues(value, logTime, message);
                    break;
                case ReviewValueInput.InputType.Input:
                    await CreateOrUpdateInputValues(value, logTime, message);
                    break;
                case ReviewValueInput.InputType.Output:
                    await CreateOrUpdateOutputValues(value, logTime, message);
                    break;
                default:
                    break;
            }
        }
    }

    private async Task CreateOrUpdateGcValues(ReviewValueInput value, InputFileLog logTime, FileMessage message)
    {
        var dbVal = await _gisCountryValues
            .Where(x => x.GisCountryId == value.ValueId && x.ReportDate == value.ReportDate)
            .FirstOrDefaultAsync();
        var val = Convert.ToDecimal(Math.Round(value.Value, 8));
        if (dbVal == null)
        {
            var newValue = new GisCountryValue()
            {
                ReportDate = value.ReportDate,
                GisCountryId = value.ValueId
            };
            switch (value.ValType)
            {
                case ReviewValueInput.ValueType.Requsted:
                    newValue.RequestedValue = val;
                    newValue.RequestedValueTimeId = logTime.Id;
                    break;
                case ReviewValueInput.ValueType.Allocated:
                    newValue.AllocatedValue = val;
                    newValue.AllocatedValueTimeId = logTime.Id;
                    break;
                case ReviewValueInput.ValueType.Estimated:
                    newValue.EstimatedValue = val;
                    newValue.EstimatedValueTimeId = logTime.Id;
                    break;
                case ReviewValueInput.ValueType.Fact:
                    newValue.FactValue = val;
                    newValue.FactValueTimeId = logTime.Id;
                    break;
                default:
                    break;
            }
            Db.Add(newValue);
            message.CreatedCount++;
        }
        else
        {
            switch (value.ValType)
            {
                case ReviewValueInput.ValueType.Requsted when (dbVal.RequestedValueTime == null || logTime.FileTime >= dbVal.RequestedValueTime?.FileTime || _isForced):
                    dbVal.RequestedValue = val;
                    dbVal.RequestedValueTimeId = logTime.Id;
                    Db.Update(dbVal);
                    message.UpdatedCount++;
                    break;
                case ReviewValueInput.ValueType.Allocated when (dbVal.AllocatedValueTime == null || logTime.FileTime >= dbVal.AllocatedValueTime?.FileTime || _isForced):
                    dbVal.AllocatedValue = val;
                    dbVal.AllocatedValueTimeId = logTime.Id;
                    Db.Update(dbVal);
                    message.UpdatedCount++;
                    break;
                case ReviewValueInput.ValueType.Estimated when (dbVal.EstimatedValueTime == null || logTime.FileTime >= dbVal.EstimatedValueTime?.FileTime || _isForced):
                    dbVal.EstimatedValue = val;
                    dbVal.EstimatedValueTimeId = logTime.Id;
                    Db.Update(dbVal);
                    message.UpdatedCount++;
                    break;
                case ReviewValueInput.ValueType.Fact when (dbVal.FactValueTime == null || logTime.FileTime >= dbVal.FactValueTime?.FileTime || _isForced):
                    dbVal.FactValue = val;
                    dbVal.FactValueTimeId = logTime.Id;
                    Db.Update(dbVal);
                    message.UpdatedCount++;
                    break;
                default:
                    break;
            }
        }
        await Db.SaveChangesAsync();
    }

    private async Task CreateOrUpdateAddonValues(ReviewValueInput value, InputFileLog logTime, FileMessage message)
    {
        var val = Convert.ToDecimal(Math.Round(value.Value, 8));
        var dbVal = await _gisAddonValues
            .Where(x => x.GisAddonId == value.ValueId && x.ReportDate == value.ReportDate)
            .FirstOrDefaultAsync();
        if (dbVal == null)
        {
            var newValue = new GisAddonValue()
            {
                ReportDate = value.ReportDate,
                GisAddonId = value.ValueId
            };
            switch (value.ValType)
            {
                case ReviewValueInput.ValueType.Requsted:
                    newValue.RequestedValue = val;
                    newValue.RequestedValueTimeId = logTime.Id;
                    break;
                case ReviewValueInput.ValueType.Allocated:
                    newValue.AllocatedValue = val;
                    newValue.AllocatedValueTimeId = logTime.Id;
                    break;
                case ReviewValueInput.ValueType.Estimated:
                    newValue.EstimatedValue = val;
                    newValue.EstimatedValueTimeId = logTime.Id;
                    break;
                case ReviewValueInput.ValueType.Fact:
                    newValue.FactValue = val;
                    newValue.FactValueTimeId = logTime.Id;
                    break;
                default:
                    break;
            }
            Db.Add(newValue);
            message.CreatedCount++;
        }
        else
        {
            switch (value.ValType)
            {
                case ReviewValueInput.ValueType.Requsted when (dbVal.RequestedValueTime == null || logTime.FileTime >= dbVal.RequestedValueTime?.FileTime || _isForced):
                    dbVal.RequestedValue = val;
                    dbVal.RequestedValueTimeId = logTime.Id;
                    Db.Update(dbVal);
                    message.UpdatedCount++;
                    break;
                case ReviewValueInput.ValueType.Allocated when (dbVal.AllocatedValueTime == null || logTime.FileTime >= dbVal.AllocatedValueTime?.FileTime || _isForced):
                    dbVal.AllocatedValue = val;
                    dbVal.AllocatedValueTimeId = logTime.Id;
                    Db.Update(dbVal);
                    message.UpdatedCount++;
                    break;
                case ReviewValueInput.ValueType.Estimated when (dbVal.EstimatedValueTime == null || logTime.FileTime >= dbVal.EstimatedValueTime?.FileTime || _isForced):
                    dbVal.EstimatedValue = val;
                    dbVal.EstimatedValueTimeId = logTime.Id;
                    Db.Update(dbVal);
                    message.UpdatedCount++;
                    break;
                case ReviewValueInput.ValueType.Fact when (dbVal.FactValueTime == null || logTime.FileTime >= dbVal.FactValueTime?.FileTime || _isForced):
                    dbVal.FactValue = val;
                    dbVal.FactValueTimeId = logTime.Id;
                    Db.Update(dbVal);
                    message.UpdatedCount++;
                    break;
                default:
                    break;
            }
        }
        await Db.SaveChangesAsync();
    }

    private async Task CreateOrUpdateCountryAddonValues(ReviewValueInput value, InputFileLog logTime, FileMessage message)
    {
        var val = Convert.ToDecimal(Math.Round(value.Value, 8));
        var dbVal = await _gisCountryAddonValues
            .Where(x => x.GisCountryAddonId == value.ValueId && x.ReportDate == value.ReportDate)
            .FirstOrDefaultAsync();
        if (dbVal == null)
        {
            var newValue = new GisCountryAddonValue()
            {
                ReportDate = value.ReportDate,
                GisCountryAddonId = value.ValueId
            };
            switch (value.ValType)
            {
                case ReviewValueInput.ValueType.Requsted:
                    newValue.RequestedValue = val;
                    newValue.RequestedValueTimeId = logTime.Id;
                    break;
                case ReviewValueInput.ValueType.Allocated:
                    newValue.AllocatedValue = val;
                    newValue.AllocatedValueTimeId = logTime.Id;
                    break;
                case ReviewValueInput.ValueType.Estimated:
                    newValue.EstimatedValue = val;
                    newValue.EstimatedValueTimeId = logTime.Id;
                    break;
                case ReviewValueInput.ValueType.Fact:
                    newValue.FactValue = val;
                    newValue.FactValueTimeId = logTime.Id;
                    break;
                default:
                    break;
            }
            Db.Add(newValue);
            message.CreatedCount++;
        }
        else
        {
            switch (value.ValType)
            {
                case ReviewValueInput.ValueType.Requsted when (dbVal.RequestedValueTime == null || logTime.FileTime >= dbVal.RequestedValueTime?.FileTime || _isForced):
                    dbVal.RequestedValue = val;
                    dbVal.RequestedValueTimeId = logTime.Id;
                    Db.Update(dbVal);
                    message.UpdatedCount++;
                    break;
                case ReviewValueInput.ValueType.Allocated when (dbVal.AllocatedValueTime == null || logTime.FileTime >= dbVal.AllocatedValueTime?.FileTime || _isForced):
                    dbVal.AllocatedValue = val;
                    dbVal.AllocatedValueTimeId = logTime.Id;
                    Db.Update(dbVal);
                    message.UpdatedCount++;
                    break;
                case ReviewValueInput.ValueType.Estimated when (dbVal.EstimatedValueTime == null || logTime.FileTime >= dbVal.EstimatedValueTime?.FileTime || _isForced):
                    dbVal.EstimatedValue = val;
                    dbVal.EstimatedValueTimeId = logTime.Id;
                    Db.Update(dbVal);
                    message.UpdatedCount++;
                    break;
                case ReviewValueInput.ValueType.Fact when (dbVal.FactValueTime == null || logTime.FileTime >= dbVal.FactValueTime?.FileTime || _isForced):
                    dbVal.FactValue = val;
                    dbVal.FactValueTimeId = logTime.Id;
                    Db.Update(dbVal);
                    message.UpdatedCount++;
                    break;
                default:
                    break;
            }
        }
        await Db.SaveChangesAsync();
    }

    private async Task CreateOrUpdateInputValues(ReviewValueInput value, InputFileLog logTime, FileMessage message)
    {
        var val = Convert.ToDecimal(Math.Round(value.Value, 8));
        var dbVal = await _gisInputValues
            .Where(x => x.GisId == value.GisId && x.ReportDate == value.ReportDate)
            .FirstOrDefaultAsync();
        if (dbVal == null)
        {
            var newValue = new GisInputValue()
            {
                ReportDate = value.ReportDate,
                GisId = value.GisId
            };
            switch (value.ValType)
            {
                case ReviewValueInput.ValueType.Requsted:
                    newValue.RequestedValue = val;
                    newValue.RequestedValueTimeId = logTime.Id;
                    break;
                case ReviewValueInput.ValueType.Allocated:
                    newValue.AllocatedValue = val;
                    newValue.AllocatedValueTimeId = logTime.Id;
                    break;
                case ReviewValueInput.ValueType.Estimated:
                    newValue.EstimatedValue = val;
                    newValue.EstimatedValueTimeId = logTime.Id;
                    break;
                case ReviewValueInput.ValueType.Fact:
                    newValue.FactValue = val;
                    newValue.FactValueTimeId = logTime.Id;
                    break;
                default:
                    break;
            }
            Db.Add(newValue);
            message.CreatedCount++;
        }
        else
        {
            switch (value.ValType)
            {
                case ReviewValueInput.ValueType.Requsted when (dbVal.RequestedValueTime == null || logTime.FileTime >= dbVal.RequestedValueTime?.FileTime || _isForced):
                    dbVal.RequestedValue = val;
                    dbVal.RequestedValueTimeId = logTime.Id;
                    Db.Update(dbVal);
                    message.UpdatedCount++;
                    break;
                case ReviewValueInput.ValueType.Allocated when (dbVal.AllocatedValueTime == null || logTime.FileTime >= dbVal.AllocatedValueTime?.FileTime || _isForced):
                    dbVal.AllocatedValue = val;
                    dbVal.AllocatedValueTimeId = logTime.Id;
                    Db.Update(dbVal);
                    message.UpdatedCount++;
                    break;
                case ReviewValueInput.ValueType.Estimated when (dbVal.EstimatedValueTime == null || logTime.FileTime >= dbVal.EstimatedValueTime?.FileTime || _isForced):
                    dbVal.EstimatedValue = val;
                    dbVal.EstimatedValueTimeId = logTime.Id;
                    Db.Update(dbVal);
                    message.UpdatedCount++;
                    break;
                case ReviewValueInput.ValueType.Fact when (dbVal.FactValueTime == null || logTime.FileTime >= dbVal.FactValueTime?.FileTime || _isForced):
                    dbVal.FactValue = val;
                    dbVal.FactValueTimeId = logTime.Id;
                    Db.Update(dbVal);
                    message.UpdatedCount++;
                    break;
            }
        }
        await Db.SaveChangesAsync();
    }

    private async Task CreateOrUpdateOutputValues(ReviewValueInput value, InputFileLog logTime, FileMessage message)
    {
        var val = Convert.ToDecimal(Math.Round(value.Value, 8));
        var dbVal = await _gisOutputValues
            .Where(x => x.GisId == value.GisId && x.ReportDate == value.ReportDate)
            .FirstOrDefaultAsync();
        if (dbVal == null)
        {
            var newValue = new GisOutputValue()
            {
                ReportDate = value.ReportDate,
                GisId = value.GisId
            };
            switch (value.ValType)
            {
                case ReviewValueInput.ValueType.Requsted:
                    newValue.RequestedValue = val;
                    newValue.RequestedValueTimeId = logTime.Id;
                    break;
                case ReviewValueInput.ValueType.Allocated:
                    newValue.AllocatedValue = val;
                    newValue.AllocatedValueTimeId = logTime.Id;
                    break;
                case ReviewValueInput.ValueType.Estimated:
                    newValue.EstimatedValue = val;
                    newValue.EstimatedValueTimeId = logTime.Id;
                    break;
                case ReviewValueInput.ValueType.Fact:
                    newValue.FactValue = val;
                    newValue.FactValueTimeId = logTime.Id;
                    break;
                default:
                    break;
            }
            Db.Add(newValue);
            message.CreatedCount++;
        }
        else
        {
            switch (value.ValType)
            {
                case ReviewValueInput.ValueType.Requsted when (dbVal.RequestedValueTime == null || logTime.FileTime >= dbVal.RequestedValueTime?.FileTime || _isForced):
                    dbVal.RequestedValue = val;
                    dbVal.RequestedValueTimeId = logTime.Id;
                    Db.Update(dbVal);
                    message.UpdatedCount++;
                    break;
                case ReviewValueInput.ValueType.Allocated when (dbVal.AllocatedValueTime == null || logTime.FileTime >= dbVal.AllocatedValueTime?.FileTime || _isForced):
                    dbVal.AllocatedValue = val;
                    dbVal.AllocatedValueTimeId = logTime.Id;
                    Db.Update(dbVal);
                    message.UpdatedCount++;
                    break;
                case ReviewValueInput.ValueType.Estimated when (dbVal.EstimatedValueTime == null || logTime.FileTime >= dbVal.EstimatedValueTime?.FileTime || _isForced):
                    dbVal.EstimatedValue = val;
                    dbVal.EstimatedValueTimeId = logTime.Id;
                    Db.Update(dbVal);
                    message.UpdatedCount++;
                    break;
                case ReviewValueInput.ValueType.Fact when (dbVal.FactValueTime == null || logTime.FileTime >= dbVal.FactValueTime?.FileTime || _isForced):
                    dbVal.FactValue = val;
                    dbVal.FactValueTimeId = logTime.Id;
                    Db.Update(dbVal);
                    message.UpdatedCount++;
                    break;
                default:
                    break;
            }
        }
        await Db.SaveChangesAsync();
    }
}