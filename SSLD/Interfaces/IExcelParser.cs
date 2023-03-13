using Microsoft.AspNetCore.Components.Forms;
using SSLD.DTO;

namespace SSLD.Interfaces;

public interface IExcelParser
{
    Task<FileMessage> GetResultAsync();
    Task SetFileAsync(IBrowserFile file);
}