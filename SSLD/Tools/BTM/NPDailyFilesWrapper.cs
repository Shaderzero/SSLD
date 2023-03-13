namespace SSLD.Tools.BTM;

public class NPDailyFilesWrapper
{
    public List<NPConnectionWrapper> FileData { get; set; } = new List<NPConnectionWrapper>();
    public string FileName { get; set; }
    public DateTime RevisionTime { get; set; }
    public DateOnly ReportDate { get; set; }
}