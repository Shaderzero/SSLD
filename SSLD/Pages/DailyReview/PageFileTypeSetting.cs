using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;
using SSLD.Data;
using SSLD.Data.DailyReview;
using SSLD.Tools;

namespace SSLD.Pages.DailyReview;

[Authorize(Roles = SD.Role_User)]
public partial class PageFileTypeSetting
{
    [Inject] public ApplicationDbContext Db { get; set; }
    [Inject] public NotificationService NotificationService { get; set; }
    [Inject] public DialogService DialogService { get; set; }

    private IList<FileTypeSetting> _selectedFileType;
    private RadzenDataGrid<FileTypeSetting> _fileTypesGrid;
    private List<string> _typeList;
    private FileTypeSetting _ftsToInsert;
    private IQueryable<FileTypeSetting> _fts;
    private List<string> _mustHaves;
    private List<string> _notHaves;
    private List<string> _countryEntries;
    private List<string> _gisEntries;
    private List<string> _requestEntries;
    private List<string> _allocatedEntries;
    private List<string> _estimatedEntries;
    private List<string> _factEntries;
    private List<string> _dataEntries;
    private bool _watchMode = true;

    protected override Task OnInitializedAsync()
    {
        _typeList = new List<string>
        {
            SD.File_Avt,
            SD.File_Balance_Cpdd,
            SD.File_Fact_Cpdd,
            SD.File_Fact_Supply,
            SD.File_Teterevka,
            SD.File_Ge_Mail,
            SD.File_Gas_Day,
            SD.File_Forecast,
            SD.File_Dz_Zr
        };

        _fts = Db.FileTypeSettings
            .OrderBy(x => x.Name)
            .AsQueryable();
        return Task.CompletedTask;
    }

    private void SetNameObjects(FileTypeSetting fts)
    {
        _mustHaves = fts.MustHave;
        _notHaves = fts.NotHave;
        _countryEntries = fts.CountryEntry;
        _gisEntries = fts.GisEntry;
        _requestEntries = fts.RequestedValueEntry;
        _allocatedEntries = fts.AllocatedValueEntry;
        _estimatedEntries = fts.EstimatedValueEntry;
        _factEntries = fts.FactValueEntry;
        _dataEntries = fts.DataEntry;
    }
        
    private async Task InsertRow()
    {
        _watchMode = false;
        _ftsToInsert = new FileTypeSetting();
        await _fileTypesGrid.InsertRow(_ftsToInsert);
    }
        
    private void RowExpand(FileTypeSetting fts)
    {
        _selectedFileType = new List<FileTypeSetting>() { fts };
        SetNameObjects(fts);
        _watchMode = true;
    }
        
    private async Task OnUpdateRow(FileTypeSetting fts)
    {
        Db.Update(fts);
        var result = await Db.SaveChangesAsync();
        if (result > 0)
        {
            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Success,
                Summary = "Тип файла обновлен",
                Detail = "Настройки файла " + fts.Name + " успешно обновлены",
                Duration = 3000
            });
        }
        else
        {
            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Error,
                Summary = "Ошибка обновления типа файла",
                Detail = "Настройки файла " + fts.Name + " не удалось обновить",
                Duration = 3000
            });
        }
        _watchMode = true;
    }
        
    private async Task OnCreateRow(FileTypeSetting fts)
    {
        await Db.AddAsync(fts);
        var result = await Db.SaveChangesAsync();
        if (result > 0)
        {
            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Success,
                Summary = "Тип файла создан",
                Detail = "Тип файла " + fts.Name + " успешно создан",
                Duration = 3000
            });
        }
        else
        {
            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Error,
                Summary = "Ошибка создания типа файла",
                Detail = "тип файла " + fts.Name + " не удалось сохранить в базе данных",
                Duration = 3000
            });
        }
        _watchMode = true;
    }
        
    private void EditRow(FileTypeSetting fts)
    {
        _watchMode = false;
        _fileTypesGrid.EditRow(fts);
    }
        
    private async Task DeleteRow(FileTypeSetting fts)
    {
        var valToDelete = _fts
            .FirstOrDefault(x => x.Id == fts.Id);
        var confirmation = await DialogService.Confirm($"Вы уверены, что хотите удалить тип файла {fts.Name}?", "Удаление",
            new ConfirmOptions() { OkButtonText = "Да", CancelButtonText = "Отмена" });
        if (valToDelete != null && confirmation == true)
        {
            Db.Remove(valToDelete);
            var result = await Db.SaveChangesAsync();
            if (result > 0)
            {
                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Success,
                    Summary = "Удаление",
                    Detail = "Тип файла " + fts.Name + " удален",
                    Duration = 3000
                });
            }
            else
            {
                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Error,
                    Summary = "Ошибка удаления",
                    Detail = "",
                    Duration = 3000
                });
            }
            await _fileTypesGrid.Reload();
        }
        else
        {
            _fileTypesGrid.CancelEditRow(fts);
        }
    }
        
    private async Task SaveRow(FileTypeSetting fts)
    {
        await _fileTypesGrid.UpdateRow(fts);
    }
        
    private void CancelEdit(FileTypeSetting fts)
    {
        _fileTypesGrid.CancelEditRow(fts);
        _watchMode = true;
    }
        
    private async Task SaveMustHave(List<string> names)
    {
        var fts = _selectedFileType.FirstOrDefault();
        if (fts != null)
        {
            fts.MustHave = names;
            await OnUpdateRow(fts);
        }
        _watchMode = true;
    }
        
    private async Task SaveNotHave(List<string> names)
    {
        var fts = _selectedFileType.FirstOrDefault();
        if (fts != null)
        {
            fts.NotHave = names;
            await OnUpdateRow(fts);
        }
        _watchMode = true;
    }
        
    private async Task SaveCountry(List<string> names)
    {
        var fts = _selectedFileType.FirstOrDefault();
        if (fts != null)
        {
            fts.CountryEntry = names;
            await OnUpdateRow(fts);
        }
        _watchMode = true;
    }

    private async Task SaveGis(List<string> names)
    {
        var fts = _selectedFileType.FirstOrDefault();
        if (fts != null)
        {
            fts.GisEntry = names;
            await OnUpdateRow(fts);
        }
        _watchMode = true;
    }
        
    private async Task SaveDate(List<string> names)
    {
        var fts = _selectedFileType.FirstOrDefault();
        if (fts != null)
        {
            fts.DataEntry = names;
            await OnUpdateRow(fts);
        }
        _watchMode = true;
    }
        
    private async Task SaveRequest(List<string> names)
    {
        var fts = _selectedFileType.FirstOrDefault();
        if (fts != null)
        {
            fts.RequestedValueEntry = names;
            await OnUpdateRow(fts);
        }
        _watchMode = true;
    }
        
    private async Task SaveAllocated(List<string> names)
    {
        var fts = _selectedFileType.FirstOrDefault();
        if (fts != null)
        {
            fts.AllocatedValueEntry = names;
            await OnUpdateRow(fts);
        }
        _watchMode = true;
    }
        
    private async Task SaveEstimated(List<string> names)
    {
        var fts = _selectedFileType.FirstOrDefault();
        if (fts != null)
        {
            fts.EstimatedValueEntry = names;
            await OnUpdateRow(fts);
        }
        _watchMode = true;
    }
        
    private async Task SaveFact(List<string> names)
    {
        var fts = _selectedFileType.FirstOrDefault();
        if (fts != null)
        {
            fts.FactValueEntry = names;
            await OnUpdateRow(fts);
        }
        _watchMode = true;
    }
}