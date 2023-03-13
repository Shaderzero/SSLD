using Microsoft.EntityFrameworkCore;
using SSLD.Data.DailyReview;

namespace SSLD.Data.DZZR;

[Index(nameof(OperatorGisId), nameof(Type), nameof(SupplyDate), nameof(ReportDate), IsUnique = true)]
[Index(nameof(ReportDate))]
[Index(nameof(SupplyDate))]
[Index(nameof(OperatorGisId))]
public class OperatorResource
{ 
    public long Id { get; set; }
    public int OperatorGisId { get; set; }
    public OperatorGis OperatorGis { get; set; }
    public OperatorResourceType Type { get; set; }
    public DateOnly SupplyDate { get; set; }
    public DateTime ReportDate { get; set; }
    public List<OperatorResourceHour> Hours { get; set; } = new List<OperatorResourceHour>();
}

public enum OperatorResourceType
{
    Dz,
    Zr
}