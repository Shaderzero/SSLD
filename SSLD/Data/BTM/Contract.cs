namespace SSLD.Data.BTM;

public class Contract
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int CounterpartyId { get; set; }
    public Countrerparty Counterparty { get; set; }
    public List<Nomination> Nominations { get; set; } = new();
}