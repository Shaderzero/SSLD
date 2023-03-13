using Microsoft.AspNetCore.Components;
using Radzen.Blazor;
using SSLD.Tools;

namespace SSLD.Pages.Shared;

public partial class ComponentNameObject
{
    [Parameter] public List<string> Names { get; set; }
    [Parameter] public EventCallback<List<string>> SaveValue { get; set; }

    [Parameter]
    public bool WatchMode
    {
        get => _watchMode;
        set
        {
            if (_watchMode == value) return;
            _watchMode = value;
            WatchModeChanged.InvokeAsync(value);
        }
    }
    [Parameter] public EventCallback<bool> WatchModeChanged { get; set; }
    [Parameter] public string Title { get; set; }
    private List<NameObject> _names;
    private bool _watchMode;
    private RadzenDataGrid<NameObject> _namesGrid;

    protected override void OnInitialized()
    {
        _names = Names.ToListObject();
    }

    private async Task Insert()
    {
        WatchMode = false;
        await _namesGrid.InsertRow(new NameObject(""));
    }

    private async Task Delete(NameObject name)
    {
        // Names.Remove(name.Name);
        _names.Remove(name);
        Names = _names.ToListString();
        await SaveValue.InvokeAsync(Names);
        await _namesGrid.Reload();
    }
    
    private async Task Edit(NameObject name)
    {
        await _namesGrid.EditRow(name);
        WatchMode = false;
    }
    
    private void OnCreateRow(NameObject name)
    {
        _names.Add(name);
        Names = _names.ToListString();
    }
    
    private async Task Save(NameObject name)
    {
        await _namesGrid.UpdateRow(name);
        Names = _names.ToListString();
        await SaveValue.InvokeAsync(Names);
    }
    
    private Task Cancel(NameObject name)
    {
        _namesGrid.CancelEditRow(name);
        WatchMode = true;
        return Task.CompletedTask;
    }
}