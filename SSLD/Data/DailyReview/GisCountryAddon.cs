namespace SSLD.Data.DailyReview;

public class GisCountryAddon
{
    public int Id { get; set; }
    public int GisCountryId { get; set; }
    public GisCountry GisCountry { get; set; }
    public string Name { get; set; }
    public string DailyReviewName { get; set; }
    public List<string> Names { get; set; } = new List<string>();
    public List<GisCountryAddonType> Types { get; set; } = new List<GisCountryAddonType>();
    public List<GisCountryAddonValue> Values { get; set; } = new List<GisCountryAddonValue>();
}