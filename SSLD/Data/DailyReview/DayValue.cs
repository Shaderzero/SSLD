using System.ComponentModel.DataAnnotations.Schema;

namespace SSLD.Data.DailyReview;

public class DayValue
{
    public long Id { get; set; }
    public DateOnly ReportDate { get; set; }
    [Column(TypeName = "decimal(16, 8)")]
    public decimal RequestedValue { get; set; }
    public long? RequestedValueTimeId { get; set; }
    public InputFileLog RequestedValueTime { get; set; }
    [Column(TypeName = "decimal(16, 8)")]
    public decimal AllocatedValue { get; set; }
    public long? AllocatedValueTimeId { get; set; }
    public InputFileLog AllocatedValueTime { get; set; }
    [Column(TypeName = "decimal(16, 8)")]
    public decimal EstimatedValue { get; set; }
    public long? EstimatedValueTimeId { get; set; }
    public InputFileLog EstimatedValueTime { get; set; }
    [Column(TypeName = "decimal(16, 8)")]
    public decimal FactValue { get; set; }
    public long? FactValueTimeId { get; set; }
    public InputFileLog FactValueTime { get; set; }
    [NotMapped]
    public DateTime ReportDateTime
    {
        get => ReportDate.ToDateTime(new TimeOnly(0));
        set => ReportDate = DateOnly.FromDateTime(value);
    }
}