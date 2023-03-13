using Microsoft.AspNetCore.Components.Forms;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using OfficeOpenXml;
using SSLD.Tools;

namespace SSLD.Parsers.Excel;

public class XlsParser: IExcelParser
{
    private readonly IBrowserFile _file;
    private ParserResult _parserResult;
    private IWorkbook _excel;
    private ISheet _sheet;
    private MemoryStream _ms;
    public XlsParser(IBrowserFile file, ParserResult parserResult)
    {
        _file = file;
        _parserResult = parserResult;
    }
    
    public async Task<bool> SetSheet(string name = null)
    {
        try
        {
            _ms = new MemoryStream();
            var stream = _file.OpenReadStream(_file.Size);
            await stream.CopyToAsync(_ms);
            stream.Close();
            _ms.Position = 0;
            _excel = new HSSFWorkbook(_ms, true);
            _sheet = name == null ? _excel.GetSheetAt(0) : _excel.GetSheet(name);
            return true;
        }
        catch (Exception ex)
        {
            _parserResult.Messages.Add("Ошибка открытия вкладки");
            Console.WriteLine(ex.Message);
            return false;
        }
    }

    public void GetStringEntry(List<string> names, out int row, out int col)
    {
        if (names == null || names.Count == 0)
        {
            row = 0;
            col = 0;
        }
        else
        {
            for (var i = 0; i <= _sheet.LastRowNum; i++)
            {
                var r = _sheet.GetRow(i);
                if (r == null) continue;
                for (var c = 0; c <= r.LastCellNum; c++)
                {
                    var cell = r.GetCell(c);
                    if (cell is not { CellType: CellType.String }) continue;
                    if (string.IsNullOrEmpty(cell.ToString())) continue;
                    if (!StringParser.NameEqualsAnyList(names, cell.ToString())) continue;
                    row = i;
                    col = c;
                    return;
                }
            }
        }

        row = 0;
        col = 0;
    }

    public void GetStringEntry(string name, out int row, out int col)
    {
        var names = new List<string>() { name };
        GetStringEntry(names, out row, out col);
    }

    public string GetCellString(int row, int col)
    {
        var r = _sheet.GetRow(row);
        var cell = r?.GetCell(col);
        return cell?.ToString();
    }

    public double? GetCellDoubleOrNull(int row, int col)
    {
        try
        {
            var r = _sheet.GetRow(row);
            var cell = r.GetCell(col);
            return cell.NumericCellValue;
        }
        catch (Exception ex)
        {
            return null;
        }
    }

    public double GetCellDouble(int row, int col)
    {
        var result = GetCellDoubleOrNull(row, col);
        return result ?? 0d;
    }

    public DateTime? GetDateTime(int row, int col)
    {
        var r = _sheet.GetRow(row);
        var cell = r.GetCell(col);
        if (DateTime.TryParse(cell.ToString(), out var dateTime)) return dateTime;
        return null;
    }

    public void GetLastCell(out int row, out int col)
    {
        row = _sheet.LastRowNum;
        col = _sheet.GetRow(row).LastCellNum;
    }

    public ExcelArea GetExcelArea(int row, int col)
    {
        var cell = _sheet.GetRow(row).GetCell(col);
        if (cell.IsMergedCell)
        {
            for (var i = 0; i < _sheet.NumMergedRegions; i++)
            {
                var region = _sheet.GetMergedRegion(i);
                if (!region.ContainsRow(cell.RowIndex) || !region.ContainsColumn(cell.ColumnIndex)) continue;
                return new ExcelArea(region.FirstRow, region.FirstColumn, region.LastRow, region.LastColumn);
            }
        }
        return new ExcelArea(row, col, row, col);
    }

    public async Task Dispose()
    {
        _excel.Close();
        await _ms.DisposeAsync();
    }
}