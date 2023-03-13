namespace SSLD.Data.DailyReview;

public class GisCountry
{
    public int Id { get; set; }
    public int CountryId { get; set; }
    public Country Country { get; set; }
    public bool IsHidden { get; set; }
    public bool IsNotCalculated { get; set; }
    public List<GisCountryAddon> Addons { get; set; } = new List<GisCountryAddon>();
    public int GisId { get; set; }
    public Gis Gis { get; set; }
    public List<GisCountryValue> Values { get; set; } = new List<GisCountryValue>();
}