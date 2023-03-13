namespace SSLD.Parsers.Excel;

public class ParserResultImpl<T>: ParserResult
{
    public List<T> Result { get; set; } = new();
}