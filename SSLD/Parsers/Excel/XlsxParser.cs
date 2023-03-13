using Microsoft.AspNetCore.Components.Forms;
using OfficeOpenXml;
using SSLD.Tools;

namespace SSLD.Parsers.Excel;

public class XlsxParser : IExcelParser
{
    private readonly IBrowserFile _file;
    private ExcelPackage _excel;
    private ExcelWorksheet _sheet;
    private readonly ParserResult _parserResult;

    public XlsxParser(IBrowserFile file, ParserResult parserResult)
    {
        _file = file;
        _parserResult = parserResult;
    }

    public async Task<bool> SetSheet(string name = null)
    {
        try
        {
            _excel = new ExcelPackage();
            var stream = _file.OpenReadStream(_file.Size);
            await _excel.LoadAsync(stream);
            await stream.DisposeAsync();
            var sheets = _excel.Workbook.Worksheets;
            _sheet = name is null ? sheets.First() : sheets.First(x => x.Name == name);
            return true;
        }
        catch (Exception ex)
        {
            _parserResult.Messages.Add("Ошибка открытия вкладки");
            Console.WriteLine(ex.Message);
            return false;
        }
    }

    public Task Dispose()
    {
        _excel.Dispose();
        return Task.CompletedTask;
    }
    
    public void GetStringEntry(List<string> names, out int row, out int col)
    {
        var result = _sheet.Cells.FirstOrDefault(x => StringParser.NameContainAnyList(names, x.Text));
        if (result == null)
        {
            row = 0;
            col = 0;
        }
        else
        {
            row = result.Start.Row;
            col = result.Start.Column;
        }
    }

    public void GetStringEntry(string name, out int row, out int col)
    {
        var names = new List<string>() { name };
        GetStringEntry(names, out row, out col);
    }

    public string GetCellString(int row, int col)
    {
        var cell = _sheet.Cells[row, col];
        return cell?.Text;
    }

    public double GetCellDouble(int row, int col)
    {
        var result = GetCellDoubleOrNull(row, col);
        return result ?? 0d;
    }

    public double? GetCellDoubleOrNull(int row, int col)
    {
        var cell = _sheet.Cells[row, col];
        if (double.TryParse(cell.Text, out var value)) return value;
        return null;
    }

    public DateTime? GetDateTime(int row, int col)
    {
        try
        {
            var dateTime = _sheet.Cells[row, col].GetValue<DateTime>();
            return dateTime;
        }
        catch (Exception ex)
        {
            return null;
        }
    }

    public void GetLastCell(out int row, out int col)
    {
        row = _sheet.Dimension.End.Row;
        col = _sheet.Dimension.End.Column;
    }

    public ExcelArea GetExcelArea(int row, int col)
    {
        var cell = _sheet.Cells[row, col];
        var idx = _sheet.GetMergeCellId(cell.Start.Row, cell.Start.Column);
        if (idx <= 0) return new ExcelArea(row, col, row, col);
        var address = _sheet.MergedCells[idx - 1];
        var range = _sheet.Cells[address];
        return new ExcelArea(range.Start.Row, range.Start.Column, range.End.Row, range.End.Column);

    }

}