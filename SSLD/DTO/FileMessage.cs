namespace SSLD.DTO;

public class FileMessage
{
    public string Filename { get; set; }
    public string Message { get; set; }
    public DateOnly ReportDate { get; set; }
    public DateTime FileTimeStamp { get; set; }
    public int CreatedCount { get; set; } = 0;
    public int UpdatedCount { get; set; } = 0;
    public int SendedCount { get; set; } = 0;
}