namespace SSLD.Data.DailyReview;

public class FileTypeSetting
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string TypeName { get; set; }
    public List<string> MustHave { get; set; } = new List<string>();
    public List<string> NotHave { get; set; } = new List<string>();
    public List<string> CountryEntry { get; set; } = new List<string>();
    public List<string> GisEntry { get; set; } = new List<string>();
    public List<string> RequestedValueEntry { get; set; } = new List<string>();
    public List<string> AllocatedValueEntry { get; set; } = new List<string>();
    public List<string> EstimatedValueEntry { get; set; } = new List<string>();
    public List<string> FactValueEntry { get; set; } = new List<string>();
    public List<string> DataEntry { get; set; } = new List<string>();
    public int LastHour { get; set; }
}