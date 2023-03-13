namespace SSLD.Tools;

public class ReviewValueInput
{
    public enum InputType
    {
        Input,
        Output,
        Addon,
        Country,
        CountryAddon
    }

    public enum ValueType
    {
        Requsted,
        Allocated,
        Estimated,
        Fact
    }

    public bool LikeValue(ReviewValueInput value)
    {
        return GisId == value.GisId
               && ValueId == value.ValueId
               && InType == value.InType
               && ValType == value.ValType
               && ReportDate == value.ReportDate;
    }

    public int GisId { get; set; }
    public InputType InType { get; set; }
    public ValueType ValType { get; set; }
    public int ValueId { get; set; }
    public DateOnly ReportDate { get; set; }
    public double Value { get; set; }
}