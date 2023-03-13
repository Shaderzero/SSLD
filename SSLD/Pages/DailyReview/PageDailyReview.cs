using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Radzen;
using SSLD.Data;
using SSLD.Data.DailyReview;
using SSLD.Tools;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace SSLD.Pages.DailyReview;

[Authorize(Roles = SD.Role_User)]
public partial class PageDailyReview
{
    [Inject] public ApplicationDbContext Db { get; set; }
    private Gis _selectedGis;
    private GisAddon _selectedAddon;
    private GisCountry _selectedCountry;
    private GisCountryAddon _selectedCountryAddon;
    private IList<Gis> _gises;
    private IList<GisCountryAddon> _gisCountryAddons;
    private string _show = "none";
    private List<NameObject> _phgList;
    private List<NameObject> _specialList;
    private List<DateRange> _dateRange;

    protected override async Task OnInitializedAsync()
    {
        var dateRange = new DateRange()
        {
            StartDate = DateTime.Now.AddDays(-14),
            FinishDate = DateTime.Now
        };
        _dateRange = new List<DateRange> {dateRange};
        _phgList = new List<NameObject>
        {
            new NameObject() {Name = "Закачка в ПХГ"},
            new NameObject() {Name = "Отбор из ПХГ"}
        };
        _specialList = new List<NameObject>
        {
            new NameObject() {Name = "Все направление"},
            new NameObject() {Name = "Товарный газ"}
        };
        _gises = await Db.Gises
            .OrderBy(x => x.Name)
            .ToListAsync();
        _selectedGis = _gises.FirstOrDefault();
    }

    public class DateRange
    {
        public DateTime StartDate { get; set; }
        public DateTime FinishDate { get; set; }
    }

    private async Task OnSelect(Gis gis)
    {
        _show = "none";
        _selectedGis = await Db.Gises
            .Include(x => x.Addons)
            .Include(x => x.Countries).ThenInclude(x => x.Country)
            .Where(x => x.Id == gis.Id)
            .FirstOrDefaultAsync();
        _gisCountryAddons = await Db.GisCountryAddons
            .Include(x => x.GisCountry).ThenInclude(x => x.Country)
            .Where(x => x.GisCountry.GisId == gis.Id).ToListAsync();
    }

    private void OnSelect(NameObject phg)
    {
        _show = phg.Name switch
        {
            "Закачка в ПХГ" => "input",
            "Отбор из ПХГ" => "output",
            "Все направление" => "gis",
            "Товарный газ" => "comgas",
            _ => _show
        };
    }

    private void OnSelect(GisAddon addon)
    {
        _selectedAddon = addon;
        _show = "addon";
    }

    private void OnSelect(GisCountry gc)
    {
        _selectedCountry = gc;
        _show = "country";
    }

    private void OnSelect(GisCountryAddon gcAddon)
    {
        _selectedCountryAddon = gcAddon;
        _show = "countryAddon";
    }

    private void OnStartDateChange(DateTime? date)
    {
        var dateRange = _dateRange.FirstOrDefault();
        if (date == null || dateRange == null) return;
        if (dateRange!.FinishDate <= date)
        {
            dateRange.FinishDate = date.Value.AddDays(1);
        }
    }
        
    private void OnFinishDateChange(DateTime? date)
    {
        var dateRange = _dateRange.FirstOrDefault();
        if (date == null || dateRange == null) return;
        if (dateRange!.StartDate >= date)
        {
            dateRange.StartDate = date.Value.AddDays(-1);
        }
    }
}