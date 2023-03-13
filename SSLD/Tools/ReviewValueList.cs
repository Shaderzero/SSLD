namespace SSLD.Tools;

public class ReviewValueList
{
    public List<ReviewValueInput> Values { get; set; } = new();
    public string Filename { get; set; }
    public DateTime FileTimeStamp { get; set; }
    public string Message { get; set; }
}