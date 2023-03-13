namespace SSLD.Parsers.Excel;

public class ParserResult
{
    public string Filename { get; set; }
    public List<string> Messages { get; set; } = new();
    public DateOnly ReportDate { get; set; }
    public DateTime FileTimeStamp { get; set; }
    public int CreatedCount { get; set; }
    public int UpdatedCount { get; set; }
    public int SendedCount { get; set; }
}