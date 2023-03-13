namespace SSLD.Data.BTM;

public class Countrerparty
{
    public int Id { get; set; }
    public string Name { get; set; }
    public List<Contract> Contracts { get; set; } = new();
}