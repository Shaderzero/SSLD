using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Radzen;
using Radzen.Blazor;
using SSLD.Data;
using SSLD.Data.DailyReview;
using SSLD.Tools;

namespace SSLD.Pages.DailyReview;

[Authorize(Roles = SD.Role_User)]
public partial class PageFileTypeSettingOld
{
    [Inject] public ApplicationDbContext Db { get; set; }

    private FileTypeSetting _selectedFileType;
    private RadzenDataGrid<FileTypeSetting> _fileTypesGrid;
    private RadzenDataGrid<NameObject> _valuesGrid;
    private IList<FileTypeSetting> _fileTypes;
    private List<string> _typeList;
    private List<NameObject> _valueList;
    private List<NameObject> _typeParameters;
    private NameObject _selectedTypeParameter;
    private FileTypeSetting _ftsToInsert;
    private NameObject _nameToInsert;

    protected override async Task OnInitializedAsync()
    {
        _typeList = new List<string>
        {
            SD.File_Avt,
            SD.File_Balance_Cpdd,
            SD.File_Fact_Cpdd,
            SD.File_Fact_Supply,
            SD.File_Teterevka,
            SD.File_Ge_Mail,
            SD.File_Gas_Day
        };
        _typeParameters = new List<NameObject>
        {
            new NameObject("До этого часа"),
            new NameObject("Включает"),
            new NameObject("Исключает"),
            new NameObject("Страна"),
            new NameObject("ГИС"),
            new NameObject("Заявлено"),
            new NameObject("Выделено"),
            new NameObject("Оценка"),
            new NameObject("Факт"),
            new NameObject("Дата")
        };

        _fileTypes = await Db.FileTypeSettings
            .OrderBy(x => x.Name)
            .ToListAsync();

        _selectedFileType = _fileTypes.FirstOrDefault();
    }

    private void OnSelect(FileTypeSetting fts)
    {
        _selectedFileType = fts;
        _valueList = NameObject.StringToObject(_selectedFileType.MustHave);
    }

    private void OnSelect(NameObject tp)
    {
        _selectedTypeParameter = tp;
        var name = tp.Name;
        _valueList = name switch
        {
            "Включает" => NameObject.StringToObject(_selectedFileType.MustHave),
            "Исключает" => NameObject.StringToObject(_selectedFileType.NotHave),
            "Страна" => NameObject.StringToObject(_selectedFileType.CountryEntry),
            "ГИС" => NameObject.StringToObject(_selectedFileType.GisEntry),
            "Заявлено" => NameObject.StringToObject(_selectedFileType.RequestedValueEntry),
            "Выделено" => NameObject.StringToObject(_selectedFileType.AllocatedValueEntry),
            "Оценка" => NameObject.StringToObject(_selectedFileType.EstimatedValueEntry),
            "Факт" => NameObject.StringToObject(_selectedFileType.FactValueEntry),
            "Дата" => NameObject.StringToObject(_selectedFileType.DataEntry),
            _ => new List<NameObject>()
        };
    }

    //private void OnSelectValue(NameObject tp)
    //{
    //    _selectedValue = tp.Name;
    //}

    private async Task InsertFileType()
    {
        _ftsToInsert = new FileTypeSetting();
        await _fileTypesGrid.InsertRow(_ftsToInsert);
    }

    private async Task OnUpdateFileType(FileTypeSetting fts)
    {
        Db.Update(fts);
        await Db.SaveChangesAsync();
    }

    private async Task OnCreateFileType(FileTypeSetting fts)
    {
        Db.Add(fts);
        await Db.SaveChangesAsync();
    }

    private async Task InsertValue()
    {
        _nameToInsert = new NameObject("");
        await _valuesGrid.InsertRow(_nameToInsert);
    }

    private async Task OnCreateValue(NameObject nameObject)
    {
        var parameterName = _selectedTypeParameter.Name;
        var name = nameObject.Name;
        switch (parameterName)
        {
            case "Включает":
                _selectedFileType.MustHave.Add(name);
                break;
            case "Исключает":
                _selectedFileType.NotHave.Add(name);
                break;
            case "Страна":
                _selectedFileType.CountryEntry.Add(name);
                break;
            case "ГИС":
                _selectedFileType.GisEntry.Add(name);
                break;
            case "Заявлено":
                _selectedFileType.RequestedValueEntry.Add(name);
                break;
            case "Выделено":
                _selectedFileType.AllocatedValueEntry.Add(name);
                break;
            case "Оценка":
                _selectedFileType.EstimatedValueEntry.Add(name);
                break;
            case "Факт":
                _selectedFileType.FactValueEntry.Add(name);
                break;
            case "Дата":
                _selectedFileType.DataEntry.Add(name);
                break;
            default:
                break;
        }
        Db.Update(_selectedFileType);
        await Db.SaveChangesAsync();
    }

    private void EditRow(FileTypeSetting fts)
    {
        _fileTypesGrid.EditRow(fts);
    }

    private async Task DeleteRow(FileTypeSetting fts)
    {
        if (_selectedFileType == fts)
        {
            Db.Remove(fts);
            await Db.SaveChangesAsync();
            await _fileTypesGrid.Reload();
        }
        else
        {
            _fileTypesGrid.CancelEditRow(fts);
        }
    }

    private async Task DeleteRow(NameObject nameObject)
    {
        var parameterName = _selectedTypeParameter.Name;
        var name = nameObject.Name;
        switch (parameterName)
        {
            case "Включает":
                _selectedFileType.MustHave.Remove(name);
                _valueList = NameObject.StringToObject(_selectedFileType.MustHave);
                break;
            case "Исключает":
                _selectedFileType.NotHave.Remove(name);
                _valueList = NameObject.StringToObject(_selectedFileType.NotHave);
                break;
            case "Страна":
                _selectedFileType.CountryEntry.Remove(name);
                _valueList = NameObject.StringToObject(_selectedFileType.CountryEntry);
                break;
            case "ГИС":
                _selectedFileType.GisEntry.Remove(name);
                _valueList = NameObject.StringToObject(_selectedFileType.GisEntry);
                break;
            case "Заявлено":
                _selectedFileType.RequestedValueEntry.Remove(name);
                _valueList = NameObject.StringToObject(_selectedFileType.RequestedValueEntry);
                break;
            case "Выделено":
                _selectedFileType.AllocatedValueEntry.Remove(name);
                _valueList = NameObject.StringToObject(_selectedFileType.AllocatedValueEntry);
                break;
            case "Оценка":
                _selectedFileType.EstimatedValueEntry.Remove(name);
                _valueList = NameObject.StringToObject(_selectedFileType.EstimatedValueEntry);
                break;
            case "Факт":
                _selectedFileType.FactValueEntry.Remove(name);
                _valueList = NameObject.StringToObject(_selectedFileType.FactValueEntry);
                break;
            case "Дата":
                _selectedFileType.DataEntry.Remove(name);
                _valueList = NameObject.StringToObject(_selectedFileType.DataEntry);
                break;
            default:
                break;
        }
        Db.Update(_selectedFileType);
        await Db.SaveChangesAsync();
        await _valuesGrid.Reload();
    }

    private void CancelEdit(FileTypeSetting fts)
    {
        _fileTypesGrid.CancelEditRow(fts);
    }

    private void CancelEdit(NameObject tp)
    {
        _valuesGrid.CancelEditRow(tp);
    }

    private async Task SaveRow(FileTypeSetting fts)
    {
        if (fts == _ftsToInsert)
        {
            _ftsToInsert = null;
        }
        await _fileTypesGrid.UpdateRow(fts);
    }

    private async Task SaveRow(NameObject nameObject)
    {
        if (nameObject == _nameToInsert)
        {
            _nameToInsert = null;
        }
        await _valuesGrid.UpdateRow(nameObject);
    }
}