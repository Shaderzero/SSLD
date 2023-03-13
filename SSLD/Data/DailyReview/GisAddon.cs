namespace SSLD.Data.DailyReview;

public class GisAddon
{
    public int Id { get; set; }
    public int GisId { get; set; }
    public Gis Gis { get; set; }
    public string Name { get; set; }
    public string DailyReviewName { get; set; }
    public bool IsHidden { get; set; }
    public bool IsInput { get; set; }
    public bool IsOutput { get; set; }
    public List<string> Names { get; set; } = new List<string>();
    public List<GisAddonValue> Values { get; set; } = new List<GisAddonValue>();
}