using Microsoft.EntityFrameworkCore;
using SSLD.Data;
using SSLD.Data.DailyReview;

namespace SSLD.Shared;

public class Consolidation
{
    private readonly ApplicationDbContext _db;
    private IQueryable<GisAddonValue> _addonValues;
    private IQueryable<GisInputValue> _inputValues;
    private IQueryable<GisOutputValue> _outputValues;
    private IQueryable<GisCountryValue> _countryValues;
    private IQueryable<GisCountryAddonValue> _countryAddonValues;

    public Consolidation(ApplicationDbContext db)
    {
        _db = db;
        _addonValues = _db.GisAddonValues
            .AsQueryable();
        _inputValues = _db.GisInputValues
            .AsQueryable();
        _outputValues = _db.GisOutputValues
            .AsQueryable();
        _countryValues = _db.GisCountryValues
            .AsQueryable();
        _countryAddonValues = _db.GisCountryAddonValues
            .AsQueryable();
    }

    public async Task<DayValue> SumOnDateAsync(DateOnly reportDate)
    {
        var d = reportDate;
        decimal requestedValue = 0;
        decimal allocatedValue = 0;
        decimal estimatedValue = 0;
        decimal factValue = 0;
        requestedValue = await _inputValues.Where(x => x.ReportDate == d).SumAsync(x => x.RequestedValue);
        requestedValue -= await _outputValues.Where(x => x.ReportDate == d).SumAsync(x => x.RequestedValue);
        requestedValue += await _countryValues.Where(x => x.ReportDate == d && !x.GisCountry.IsNotCalculated)
            .SumAsync(x => x.RequestedValue);
        requestedValue += await _addonValues.Where(x => x.ReportDate == d && x.GisAddon.IsInput)
            .SumAsync(x => x.RequestedValue);
        requestedValue -= await _addonValues.Where(x => x.ReportDate == d && x.GisAddon.IsOutput)
            .SumAsync(x => x.RequestedValue);
        requestedValue += await _countryAddonValues.Where(x => x.ReportDate == d).SumAsync(x => x.RequestedValue);
        allocatedValue = await _inputValues.Where(x => x.ReportDate == d).SumAsync(x => x.AllocatedValue);
        allocatedValue -= await _outputValues.Where(x => x.ReportDate == d).SumAsync(x => x.AllocatedValue);
        allocatedValue += await _countryValues.Where(x => x.ReportDate == d && !x.GisCountry.IsNotCalculated)
            .SumAsync(x => x.AllocatedValue);
        allocatedValue += await _addonValues.Where(x => x.ReportDate == d && x.GisAddon.IsInput)
            .SumAsync(x => x.AllocatedValue);
        allocatedValue -= await _addonValues.Where(x => x.ReportDate == d && x.GisAddon.IsOutput)
            .SumAsync(x => x.AllocatedValue);
        allocatedValue += await _countryAddonValues.Where(x => x.ReportDate == d).SumAsync(x => x.AllocatedValue);
        estimatedValue = await _inputValues.Where(x => x.ReportDate == d).SumAsync(x => x.EstimatedValue);
        estimatedValue -= await _outputValues.Where(x => x.ReportDate == d).SumAsync(x => x.EstimatedValue);
        estimatedValue += await _countryValues.Where(x => x.ReportDate == d && !x.GisCountry.IsNotCalculated)
            .SumAsync(x => x.EstimatedValue);
        estimatedValue += await _addonValues.Where(x => x.ReportDate == d && x.GisAddon.IsInput)
            .SumAsync(x => x.EstimatedValue);
        estimatedValue -= await _addonValues.Where(x => x.ReportDate == d && x.GisAddon.IsOutput)
            .SumAsync(x => x.EstimatedValue);
        estimatedValue += await _countryAddonValues.Where(x => x.ReportDate == d).SumAsync(x => x.EstimatedValue);
        factValue = await _inputValues.Where(x => x.ReportDate == d).SumAsync(x => x.FactValue);
        factValue -= await _outputValues.Where(x => x.ReportDate == d).SumAsync(x => x.FactValue);
        factValue += await _countryValues.Where(x => x.ReportDate == d && !x.GisCountry.IsNotCalculated)
            .SumAsync(x => x.FactValue);
        factValue += await _addonValues.Where(x => x.ReportDate == d && x.GisAddon.IsInput)
            .SumAsync(x => x.FactValue);
        factValue -= await _addonValues.Where(x => x.ReportDate == d && x.GisAddon.IsOutput)
            .SumAsync(x => x.FactValue);
        factValue += await _countryAddonValues.Where(x => x.ReportDate == d).SumAsync(x => x.FactValue);
        var dayValue = new DayValue()
        {
            ReportDate = d,
            RequestedValue = requestedValue,
            AllocatedValue = allocatedValue,
            EstimatedValue = estimatedValue,
            FactValue = factValue
        };
        return dayValue;
    }

    public async Task<List<DayValue>> SumOnDateRangeAsync(DateOnly startDate,
        DateOnly finishDate)
    {
        var values = new List<DayValue>();
        for (var d = finishDate; d >= startDate; d = d.AddDays(-1))
        {
            var val = await SumOnDateAsync(d);
            values.Add(val);
        }

        return values;
    }

    public async Task<DayValue> GisSumOnDateAsync(Gis gis, DateOnly reportDate)
    {
        var d = reportDate;
        decimal requestedValue = 0;
        decimal allocatedValue = 0;
        decimal estimatedValue = 0;
        decimal factValue = 0;
        requestedValue = await _inputValues.Where(x => x.ReportDate == d && x.GisId == gis.Id)
            .SumAsync(x => x.RequestedValue);
        requestedValue -= await _outputValues.Where(x => x.ReportDate == d && x.GisId == gis.Id)
            .SumAsync(x => x.RequestedValue);
        requestedValue += await _countryValues.Where(x =>
                x.ReportDate == d && !x.GisCountry.IsNotCalculated && x.GisCountry.GisId == gis.Id)
            .SumAsync(x => x.RequestedValue);
        requestedValue += await _addonValues
            .Where(x => x.ReportDate == d && x.GisAddon.IsInput && x.GisAddon.GisId == gis.Id)
            .SumAsync(x => x.RequestedValue);
        requestedValue -= await _addonValues
            .Where(x => x.ReportDate == d && x.GisAddon.IsOutput && x.GisAddon.GisId == gis.Id)
            .SumAsync(x => x.RequestedValue);
        requestedValue += await _countryAddonValues
            .Where(x => x.ReportDate == d && x.GisCountryAddon.GisCountry.GisId == gis.Id)
            .SumAsync(x => x.RequestedValue);
        allocatedValue = await _inputValues.Where(x => x.ReportDate == d && x.GisId == gis.Id)
            .SumAsync(x => x.AllocatedValue);
        allocatedValue -= await _outputValues.Where(x => x.ReportDate == d && x.GisId == gis.Id)
            .SumAsync(x => x.AllocatedValue);
        allocatedValue += await _countryValues.Where(x =>
                x.ReportDate == d && !x.GisCountry.IsNotCalculated && x.GisCountry.GisId == gis.Id)
            .SumAsync(x => x.AllocatedValue);
        allocatedValue += await _addonValues
            .Where(x => x.ReportDate == d && x.GisAddon.IsInput && x.GisAddon.GisId == gis.Id)
            .SumAsync(x => x.AllocatedValue);
        allocatedValue -= await _addonValues
            .Where(x => x.ReportDate == d && x.GisAddon.IsOutput && x.GisAddon.GisId == gis.Id)
            .SumAsync(x => x.AllocatedValue);
        allocatedValue += await _countryAddonValues
            .Where(x => x.ReportDate == d && x.GisCountryAddon.GisCountry.GisId == gis.Id)
            .SumAsync(x => x.AllocatedValue);
        estimatedValue = await _inputValues.Where(x => x.ReportDate == d && x.GisId == gis.Id)
            .SumAsync(x => x.EstimatedValue);
        estimatedValue -= await _outputValues.Where(x => x.ReportDate == d && x.GisId == gis.Id)
            .SumAsync(x => x.EstimatedValue);
        estimatedValue += await _countryValues.Where(x =>
                x.ReportDate == d && !x.GisCountry.IsNotCalculated && x.GisCountry.GisId == gis.Id)
            .SumAsync(x => x.EstimatedValue);
        estimatedValue += await _addonValues
            .Where(x => x.ReportDate == d && x.GisAddon.IsInput && x.GisAddon.GisId == gis.Id)
            .SumAsync(x => x.EstimatedValue);
        estimatedValue -= await _addonValues
            .Where(x => x.ReportDate == d && x.GisAddon.IsOutput && x.GisAddon.GisId == gis.Id)
            .SumAsync(x => x.EstimatedValue);
        estimatedValue += await _countryAddonValues
            .Where(x => x.ReportDate == d && x.GisCountryAddon.GisCountry.GisId == gis.Id)
            .SumAsync(x => x.EstimatedValue);
        factValue = await _inputValues.Where(x => x.ReportDate == d && x.GisId == gis.Id)
            .SumAsync(x => x.FactValue);
        factValue -= await _outputValues.Where(x => x.ReportDate == d && x.GisId == gis.Id)
            .SumAsync(x => x.FactValue);
        factValue += await _countryValues.Where(x =>
                x.ReportDate == d && !x.GisCountry.IsNotCalculated && x.GisCountry.GisId == gis.Id)
            .SumAsync(x => x.FactValue);
        factValue += await _addonValues
            .Where(x => x.ReportDate == d && x.GisAddon.IsInput && x.GisAddon.GisId == gis.Id)
            .SumAsync(x => x.FactValue);
        factValue -= await _addonValues
            .Where(x => x.ReportDate == d && x.GisAddon.IsOutput && x.GisAddon.GisId == gis.Id)
            .SumAsync(x => x.FactValue);
        factValue += await _countryAddonValues
            .Where(x => x.ReportDate == d && x.GisCountryAddon.GisCountry.GisId == gis.Id)
            .SumAsync(x => x.FactValue);
        var dayValue = new DayValue()
        {
            ReportDate = d,
            RequestedValue = requestedValue,
            AllocatedValue = allocatedValue,
            EstimatedValue = estimatedValue,
            FactValue = factValue
        };
        return dayValue;
    }

    public async Task<List<DayValue>> GisSumOnDateRangeAsync(Gis gis, DateOnly startDate,
        DateOnly finishDate)
    {
        var values = new List<DayValue>();
        for (var d = finishDate; d >= startDate; d = d.AddDays(-1))
        {
            var val = await GisSumOnDateAsync(gis, d);
            values.Add(val);
        }

        return values;
    }

    public async Task<DayValue> ResourceSubmissionOnDateAsync(DateOnly reportDate)
    {
        var d = reportDate;
        decimal requestedValue = 0;
        decimal allocatedValue = 0;
        decimal estimatedValue = 0;
        decimal factValue = 0;
        requestedValue += await _inputValues.Where(x => x.ReportDate == d && !x.Gis.IsTop && !x.Gis.IsBottom)
            .SumAsync(x => x.RequestedValue);
        requestedValue -= await _outputValues.Where(x => x.ReportDate == d && !x.Gis.IsTop && !x.Gis.IsBottom)
            .SumAsync(x => x.RequestedValue);
        requestedValue += await _countryValues
            .Where(x => x.ReportDate == d && !x.GisCountry.IsNotCalculated && !x.GisCountry.Gis.IsTop &&
                        !x.GisCountry.Gis.IsBottom).SumAsync(x => x.RequestedValue);
        requestedValue += await _addonValues.Where(x => x.ReportDate == d && x.GisAddon.IsInput)
            .SumAsync(x => x.RequestedValue);
        requestedValue -= await _addonValues.Where(x => x.ReportDate == d && x.GisAddon.IsOutput)
            .SumAsync(x => x.RequestedValue);
        requestedValue += await _countryAddonValues.Where(x => x.ReportDate == d).SumAsync(x => x.RequestedValue);
        allocatedValue = await _inputValues.Where(x => x.ReportDate == d && !x.Gis.IsTop && !x.Gis.IsBottom)
            .SumAsync(x => x.AllocatedValue);
        allocatedValue -= await _outputValues.Where(x => x.ReportDate == d && !x.Gis.IsTop && !x.Gis.IsBottom)
            .SumAsync(x => x.AllocatedValue);
        allocatedValue += await _countryValues
            .Where(x => x.ReportDate == d && !x.GisCountry.IsNotCalculated && !x.GisCountry.Gis.IsTop &&
                        !x.GisCountry.Gis.IsBottom).SumAsync(x => x.AllocatedValue);
        allocatedValue += await _addonValues.Where(x => x.ReportDate == d && x.GisAddon.IsInput)
            .SumAsync(x => x.AllocatedValue);
        allocatedValue -= await _addonValues.Where(x => x.ReportDate == d && x.GisAddon.IsOutput)
            .SumAsync(x => x.AllocatedValue);
        allocatedValue += await _countryAddonValues.Where(x => x.ReportDate == d).SumAsync(x => x.AllocatedValue);
        estimatedValue = await _inputValues.Where(x => x.ReportDate == d && !x.Gis.IsTop && !x.Gis.IsBottom)
            .SumAsync(x => x.EstimatedValue);
        estimatedValue -= await _outputValues.Where(x => x.ReportDate == d && !x.Gis.IsTop && !x.Gis.IsBottom)
            .SumAsync(x => x.EstimatedValue);
        estimatedValue += await _countryValues
            .Where(x => x.ReportDate == d && !x.GisCountry.IsNotCalculated && !x.GisCountry.Gis.IsTop &&
                        !x.GisCountry.Gis.IsBottom).SumAsync(x => x.EstimatedValue);
        estimatedValue += await _addonValues.Where(x => x.ReportDate == d && x.GisAddon.IsInput)
            .SumAsync(x => x.EstimatedValue);
        estimatedValue -= await _addonValues.Where(x => x.ReportDate == d && x.GisAddon.IsOutput)
            .SumAsync(x => x.EstimatedValue);
        estimatedValue += await _countryAddonValues.Where(x => x.ReportDate == d).SumAsync(x => x.EstimatedValue);
        factValue = await _inputValues.Where(x => x.ReportDate == d && !x.Gis.IsTop && !x.Gis.IsBottom)
            .SumAsync(x => x.FactValue);
        factValue -= await _outputValues.Where(x => x.ReportDate == d && !x.Gis.IsTop && !x.Gis.IsBottom)
            .SumAsync(x => x.FactValue);
        factValue += await _countryValues
            .Where(x => x.ReportDate == d && !x.GisCountry.IsNotCalculated && !x.GisCountry.Gis.IsTop &&
                        !x.GisCountry.Gis.IsBottom).SumAsync(x => x.FactValue);
        factValue += await _addonValues.Where(x => x.ReportDate == d && x.GisAddon.IsInput)
            .SumAsync(x => x.FactValue);
        factValue -= await _addonValues.Where(x => x.ReportDate == d && x.GisAddon.IsOutput)
            .SumAsync(x => x.FactValue);
        factValue += await _countryAddonValues.Where(x => x.ReportDate == d).SumAsync(x => x.FactValue);
        var dayValue = new DayValue()
        {
            ReportDate = d,
            RequestedValue = requestedValue,
            AllocatedValue = allocatedValue,
            EstimatedValue = estimatedValue,
            FactValue = factValue
        };
        return dayValue;
    }

    public async Task<List<DayValue>> ResourceSubmissionOnDateRangeAsync(DateOnly startDate,
        DateOnly finishDate)
    {
        var values = new List<DayValue>();
        for (var d = finishDate; d >= startDate; d = d.AddDays(-1))
        {
            var val = await ResourceSubmissionOnDateAsync(d);
            values.Add(val);
        }

        return values;
    }

    public async Task<DayValue> ComGisOnDateAsync(Gis gis, DateOnly reportDate)
    {
        var d = reportDate;
        decimal requestedValue = 0;
        decimal allocatedValue = 0;
        decimal estimatedValue = 0;
        decimal factValue = 0;
        requestedValue += await _countryValues
            .Where(x => !x.GisCountry.IsNotCalculated && x.GisCountry.GisId == gis.Id && x.ReportDate == d)
            .SumAsync(x => x.RequestedValue);
        requestedValue += await _countryAddonValues
            .Where(x => x.GisCountryAddon.GisCountry.GisId == gis.Id && x.ReportDate == d)
            .SumAsync(x => x.RequestedValue);
        allocatedValue += await _countryValues
            .Where(x => !x.GisCountry.IsNotCalculated && x.GisCountry.GisId == gis.Id && x.ReportDate == d)
            .SumAsync(x => x.AllocatedValue);
        allocatedValue += await _countryAddonValues
            .Where(x => x.GisCountryAddon.GisCountry.GisId == gis.Id && x.ReportDate == d)
            .SumAsync(x => x.AllocatedValue);
        estimatedValue += await _countryValues
            .Where(x => !x.GisCountry.IsNotCalculated && x.GisCountry.GisId == gis.Id && x.ReportDate == d)
            .SumAsync(x => x.EstimatedValue);
        estimatedValue += await _countryAddonValues
            .Where(x => x.GisCountryAddon.GisCountry.GisId == gis.Id && x.ReportDate == d)
            .SumAsync(x => x.EstimatedValue);
        factValue += await _countryValues
            .Where(x => !x.GisCountry.IsNotCalculated && x.GisCountry.GisId == gis.Id && x.ReportDate == d)
            .SumAsync(x => x.FactValue);
        factValue += await _countryAddonValues
            .Where(x => x.GisCountryAddon.GisCountry.GisId == gis.Id && x.ReportDate == d)
            .SumAsync(x => x.FactValue);
        var dayValue = new DayValue()
        {
            ReportDate = d,
            RequestedValue = requestedValue,
            AllocatedValue = allocatedValue,
            EstimatedValue = estimatedValue,
            FactValue = factValue
        };
        return dayValue;
    }

    public async Task<List<DayValue>> ComGisOnDateRangeAsync(Gis gis, DateOnly startDate,
        DateOnly finishDate)
    {
        var values = new List<DayValue>();
        for (var d = finishDate; d >= startDate; d = d.AddDays(-1))
        {
            var val = await ComGisOnDateAsync(gis, d);
            values.Add(val);
        }

        return values;
    }
        
    public async Task<DayValue> GisInputOnDateAsync(DateOnly reportDate)
    {
        var d = reportDate;
        decimal requestedValue = 0;
        decimal allocatedValue = 0;
        decimal estimatedValue = 0;
        decimal factValue = 0;
        requestedValue = await _inputValues.Where(x => x.ReportDate == d).SumAsync(x => x.RequestedValue);
        allocatedValue = await _inputValues.Where(x => x.ReportDate == d).SumAsync(x => x.AllocatedValue);
        estimatedValue = await _inputValues.Where(x => x.ReportDate == d).SumAsync(x => x.EstimatedValue);
        factValue = await _inputValues.Where(x => x.ReportDate == d).SumAsync(x => x.FactValue);
        var dayValue = new DayValue()
        {
            ReportDate = d,
            RequestedValue = requestedValue,
            AllocatedValue = allocatedValue,
            EstimatedValue = estimatedValue,
            FactValue = factValue
        };
        return dayValue;
    }

    public async Task<List<DayValue>> GisInputOnDateRangeAsync(DateOnly startDate,
        DateOnly finishDate)
    {
        var values = new List<DayValue>();
        for (var d = finishDate; d >= startDate; d = d.AddDays(-1))
        {
            var val = await SumOnDateAsync(d);
            values.Add(val);
        }

        return values;
    }
}