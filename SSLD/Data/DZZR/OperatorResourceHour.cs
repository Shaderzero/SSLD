using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SSLD.Data.DZZR;

[Index(nameof(OperatorResourceId), nameof(Hour), IsUnique = true)]
public class OperatorResourceHour
{
    public long Id { get; set; }
    public long OperatorResourceId { get; set; }
    public OperatorResource OperatorResource { get; set; }
    public int Hour { get; set; }
    [Column(TypeName = "decimal(16, 8)")]
    public decimal Volume { get; set; }
}