using Microsoft.AspNetCore.Components.Forms;

namespace SSLD.Parsers.Excel;

public interface IFileParser
{
    Task<bool> SetFileAsync(IBrowserFile file);
    Task ParseAsync();
    Task<ParserResult> SaveResultAsync();
    Task Dispose();
}