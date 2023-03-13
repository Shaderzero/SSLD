using System.Drawing;
using OfficeOpenXml;
using OfficeOpenXml.Drawing.Chart;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using OfficeOpenXml.Style;
using SSLD.Data.DailyReview;
using SSLD.Data.DZZR;
using SSLD.DTO;

namespace SSLD.Tools;

public class DayValueExcel
{
    private readonly IList<DayValue> _values;
    private const int StartRow = 2;
    private readonly DateOnly _minDate;
    private readonly DateOnly _maxDate;
    private readonly string _name;
    
    public DayValueExcel(IList<DayValue> values, string name, DateOnly minDate, DateOnly maxDate)
    {
        _values = values;
        _name = name;
        _minDate = minDate;
        _maxDate = maxDate;
    }
    
    public async Task<byte[]> GenerateExcelReport()
    {
        using var pck = new ExcelPackage();
        var ws = pck.Workbook.Worksheets.Add("result");
        // ws.Cells.Style.Font.Name = "Droid Sans";
        ws.Cells.Style.Font.Name = "Arial";
        ws.Cells[1, 1].Value = _name;
        ws.Cells[1, 1].Style.Font.Bold = true;
        ws.Cells[1, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        ws.Cells[1, 1, 1, 5].Merge = true;
        var row = StartRow;
        ws.Cells[row, 1].Value = "Дата";
        ws.Column(1).Width = 20;
        ws.Cells[row, 2].Value = "Заявлено";
        ws.Column(2).Width = 30;
        ws.Cells[row, 3].Value = "Выделено";
        ws.Column(3).Width = 30;
        ws.Cells[row, 4].Value = "Оценка";
        ws.Column(4).Width = 30;
        ws.Cells[row, 5].Value = "Факт";
        ws.Column(5).Width = 30;
        ws.Cells[row, 1, row, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        ws.Cells[row, 1, row, 5].Style.Font.Bold = true;
        row++;
        for (var d = _maxDate; d >= _minDate; d = d.AddDays(-1))
        {
            var value = _values.FirstOrDefault(x => x.ReportDate == d);
            ws.Cells[row, 1].Value = d.ToString("dd.MM.yyyy");
            ws.Cells[row, 1].Style.Numberformat.Format = "dd.MM.yy";
            ws.Cells[row, 2].Value = value?.RequestedValue ?? 0;
            ws.Cells[row, 2].Style.Numberformat.Format = "_-* #,##0.00_-;-* #,##0.00_-;_-* \"-\"??_-;_-@_-";
            ws.Cells[row, 3].Value = value?.AllocatedValue ?? 0;
            ws.Cells[row, 3].Style.Numberformat.Format = "_-* #,##0.00_-;-* #,##0.00_-;_-* \"-\"??_-;_-@_-";
            ws.Cells[row, 4].Value = value?.EstimatedValue ?? 0;
            ws.Cells[row, 4].Style.Numberformat.Format = "_-* #,##0.00_-;-* #,##0.00_-;_-* \"-\"??_-;_-@_-";
            ws.Cells[row, 5].Value = value?.FactValue ?? 0;
            ws.Cells[row, 5].Style.Numberformat.Format = "_-* #,##0.00_-;-* #,##0.00_-;_-* \"-\"??_-;_-@_-";
            row++;
        }
        row--;
        ws.Cells[StartRow, 1, row, 5].Style.Border.Top.Style = ExcelBorderStyle.Thin;
        ws.Cells[StartRow, 1, row, 5].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
        ws.Cells[StartRow, 1, row, 5].Style.Border.Left.Style = ExcelBorderStyle.Thin;
        ws.Cells[StartRow, 1, row, 5].Style.Border.Right.Style = ExcelBorderStyle.Thin;
        ws.PrinterSettings.RepeatRows = new ExcelAddress("2:2");
        ws.PrinterSettings.PrintArea = ws.Cells[1, 1, row, 5];
        ws.PrinterSettings.FitToPage = true;
        ws.PrinterSettings.FitToWidth = 1;
        ws.PrinterSettings.FitToHeight = 0;
        ws.PrinterSettings.PaperSize = ePaperSize.A4;
        ws.PrinterSettings.Orientation = eOrientation.Portrait;
        ws.PrinterSettings.HorizontalCentered = true;
        ws.PrinterSettings.LeftMargin = 0.5M;
        ws.PrinterSettings.RightMargin = 0.5M;
        var excelBytes = pck.GetAsByteArray();
        return await Task.FromResult(excelBytes);
    }
}