using SSLD.Tools;
using System.ComponentModel.DataAnnotations.Schema;

namespace SSLD.Data.DailyReview;

public class Country
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string DailyReviewName { get; set; }
    public string NameEn { get; set; }
    public string ShortName { get; set; }
    public List<string> Names { get; set; } = new List<string>();
    public List<GisCountry> GisCountries { get; set; } = new List<GisCountry>();
    [NotMapped]
    public List<NameObject> NameObjects
    {
        get => NameObject.StringToObject(Names);
        set => Names = NameObject.ObjectToString(value);
    }
}