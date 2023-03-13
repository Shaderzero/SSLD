using SSLD.Data.BTM;

namespace SSLD.Tools.BTM;

public class NPConnectionWrapper
{
    public string CounterpartyName { get; set; }
    public string ContractName { get; set; }
    public string GisName { get; set; }
    public TimeOnly Hour { get; set; }
    public int Kwh { get; set; }
    public decimal Volume { get; set; }
}