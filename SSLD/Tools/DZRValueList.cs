using SSLD.Data.DZZR;

namespace SSLD.Tools;

public class DZRValueList
{
    public List<OperatorResource> Values { get; set; }
    public string Filename { get; set; }
    public DateTime FileTimeStamp { get; set; }
    public string Message { get; set; }
}