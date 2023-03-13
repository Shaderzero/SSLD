using SSLD.Data.DZZR;

namespace SSLD.DTO;

public class OperatorResourceOnDate
{
    public DateOnly SupplyDate { get; set; }
    public OperatorResourceType Type { get; set; }
    public List<OperatorVolume> Operators { get; set; } = new();
}