using System.ComponentModel.DataAnnotations.Schema;

namespace SSLD.Data.BTM;

public class NominationHour
{
    public int Id { get; set; }
    public int NominationId { get; set; }
    public Nomination Nomination { get; set; }
    public TimeOnly Hour { get; set; }
    public int Power { get; set; }
    [Column(TypeName = "decimal(16, 8)")]
    public decimal Volume { get; set; }
}