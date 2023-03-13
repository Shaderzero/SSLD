namespace SSLD.Data.DailyReview;

public class Gis
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string DailyReviewName { get; set; }
    public int DailyReviewOrder { get; set; }
    public bool IsHidden { get; set; }
    public bool IsNotCalculated { get; set; }
    public bool IsUkraineTransport { get; set; }
    public bool IsTop { get; set; }
    public bool IsBottom { get; set; }
    public bool IsOneRow { get; set; }
    public bool IsNoPhg { get; set; }
    public List<string> Names { get; set; } = new List<string>();
    public List<GisAddon> Addons { get; set; } = new List<GisAddon>();
    public List<GisCountry> Countries { get; set; } = new List<GisCountry>();
    public List<string> GisInputNames { get; set; } = new List<string>();
    public List<string> GisOutputNames { get; set; } = new List<string>();
    public List<GisInputValue> GisInputValues { get; set; } = new List<GisInputValue>();
    public List<GisOutputValue> GisOutputValues { get; set; } = new List<GisOutputValue>();
}