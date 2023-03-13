using OfficeOpenXml;

namespace SSLD.Parsers.Excel;

public interface IExcelParser
{
    Task<bool> SetSheet(string name = null);
    void GetStringEntry(List<string> names, out int row, out int col);
    void GetStringEntry(string name, out int row, out int col);
    string GetCellString(int row, int col);
    double? GetCellDoubleOrNull(int row, int col);
    double GetCellDouble(int row, int col);
    DateTime? GetDateTime(int row, int col);
    void GetLastCell(out int row, out int col);
    ExcelArea GetExcelArea(int row, int col);
    Task Dispose();
}

public class ExcelArea
{
    public int StartRow;
    public int StartCol;
    public int EndRow;
    public int EndCol;

    public ExcelArea()
    {
    }

    public ExcelArea(int startRow, int startCol, int endRow, int endCol)
    {
        StartRow = startRow;
        StartCol = startCol;
        EndRow = endRow;
        EndCol = endCol;
    }
}