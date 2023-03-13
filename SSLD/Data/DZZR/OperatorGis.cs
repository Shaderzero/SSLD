namespace SSLD.Data.DZZR;

public class OperatorGis
{
    public int Id { get; set; }
    public string Name { get; set; }
    public List<OperatorResource> Resources { get; set; } = new List<OperatorResource>();
}