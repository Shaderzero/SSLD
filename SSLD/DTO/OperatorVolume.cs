using SSLD.Data.DZZR;

namespace SSLD.DTO;

public class OperatorVolume
{
    public OperatorGis OperatorGis { get; set; }
    public decimal Volume { get; set; }

    public OperatorVolume(){}

    public OperatorVolume(OperatorResource resource)
    {
        OperatorGis = resource.OperatorGis;
        Volume = resource.Hours.Sum(x => x.Volume);
    }
}