using System.Drawing;
using OfficeOpenXml;
using OfficeOpenXml.Drawing.Chart;
using OfficeOpenXml.Style;
using SSLD.Data.DZZR;
using SSLD.DTO;

namespace SSLD.Tools;

public class DzZrExcel
{
    private IList<OperatorResource> _resources;
    private IList<OperatorResourceOnDate> _sResources;
    private const int StartRow = 2;
    private DateTime _minDate;
    private DateTime _maxDate;
    private string _name;

    public DzZrExcel(IList<OperatorResource> resources)
    {
        _resources = resources;
    }
    
    public DzZrExcel(IList<OperatorResourceOnDate> sResources)
    {
        _sResources = sResources;
    }
    
    public async Task<byte[]> GenerateExcelReport()
    {
        using var pck = new ExcelPackage();
        var ws = pck.Workbook.Worksheets.Add("result");
        // ws.Cells.Style.Font.Name = "Droid Sans";
        ws.Cells.Style.Font.Name = "Arial";
        var gisName = _resources.FirstOrDefault()!.OperatorGis.Name;
        _minDate = _resources.Min(x => x.ReportDate);
        _maxDate = _resources.Max(x => x.ReportDate);
        _name = $"График ДЗ и ДР за период с {_minDate.ToString("dd.MM.yy")} по {_maxDate.ToString("dd.MM.yy")} по направлению {gisName}";
        ws.Cells[1, 1].Value = _name;
        ws.Cells[1, 1].Style.Font.Bold = true;
        ws.Cells[1, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        ws.Cells[1, 1, 1, 1 + 3 + 24].Merge = true;
        var row = StartRow;
        ws.Cells[row, 1].Value = "Дата поставки";
        ws.Column(1).Width = 16;
        ws.Cells[row, 2].Value = "Дата ревизии";
        ws.Column(2).Width = 20;
        ws.Cells[row, 3].Value = "Тип";
        ws.Column(3).Width = 6;
        ws.Cells[row, 4].Value = "Сумма";
        ws.Column(4).Width = 12;
        for (var i = 1; i <= 24; i++)
        {
            ws.Cells[row, 4 + i].Value = StringParser.IntToCevTime(i);
            ws.Column(4 + i).Width = 10;
        }
        ws.Cells[row, 1, row, 28].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        ws.Cells[row, 1, row, 28].Style.Font.Bold = true;
        row++;
        foreach (var resource in _resources)
        {
            ws.Cells[row, 1].Value = resource.SupplyDate.ToString("dd.MM.yyyy");
            ws.Cells[row, 1].Style.Numberformat.Format = "dd.MM.yy";
            ws.Cells[row, 2].Value = resource.ReportDate.ToString("dd.MM.yyyy hh:mm:ss");
            ws.Cells[row, 2].Style.Numberformat.Format = "dd.MM hh:mm:ss";
            ws.Cells[row, 3].Value = resource.Type switch
            {
                OperatorResourceType.Dz => "ДЗ",
                OperatorResourceType.Zr => "ЗР",
                _ => ws.Cells[row, 3].Value
            };
            ws.Cells[row, 3].Style.Font.Color.SetColor(resource.Type switch
            {
                OperatorResourceType.Dz => Color.Green,
                OperatorResourceType.Zr => Color.Blue,
                _ => Color.Black
            });
            ws.Cells[row, 4].Value = resource.Hours.Sum(x => x.Volume);
            ws.Cells[row, 4].Style.Numberformat.Format = "_-* #,##0.00_-;-* #,##0.00_-;_-* \"-\"??_-;_-@_-";
            for (var i = 1; i <= 24; i++)
            {
                decimal val = 0;
                var hour = resource.Hours.FirstOrDefault(x => x.Hour == i);
                if (hour != null) val = hour.Volume;
                ws.Cells[row, i + 4].Value = val;
                ws.Cells[row, i + 4].Style.Numberformat.Format = "_-* #,##0.000_-;-* #,##0.000_-;_-* \"-\"??_-;_-@_-";
            }
            row++;
        }
        row--;
        ws.Cells[StartRow, 1, row, 28].Style.Border.Top.Style = ExcelBorderStyle.Thin;
        ws.Cells[StartRow, 1, row, 28].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
        ws.Cells[StartRow, 1, row, 28].Style.Border.Left.Style = ExcelBorderStyle.Thin;
        ws.Cells[StartRow, 1, row, 28].Style.Border.Right.Style = ExcelBorderStyle.Thin;
        ws.PrinterSettings.RepeatRows = new ExcelAddress("2:2");
        ws.PrinterSettings.PrintArea = ws.Cells[1, 1, row, 28];
        ws.PrinterSettings.FitToPage = true;
        ws.PrinterSettings.FitToWidth = 1;
        ws.PrinterSettings.FitToHeight = 0;
        ws.PrinterSettings.PaperSize = ePaperSize.A4;
        ws.PrinterSettings.Orientation = eOrientation.Landscape;
        ws.PrinterSettings.HorizontalCentered = true;
        ws.PrinterSettings.LeftMargin = 0.5M;
        ws.PrinterSettings.RightMargin = 0.5M;
        await GenerateLineChart(pck);
        var excelBytes = pck.GetAsByteArray();
        return await Task.FromResult(excelBytes);
    }

    public async Task<byte[]> GenerateExcelOverallReport()
    {
        using var pck = new ExcelPackage();
        var ws = pck.Workbook.Worksheets.Add("result");
        // ws.Cells.Style.Font.Name = "Droid Sans";
        ws.Cells.Style.Font.Name = "Arial";
        var minDate = _sResources.Min(x => x.SupplyDate);
        var maxDate = _sResources.Max(x => x.SupplyDate);
        _name = $"График ДЗ и ДР за период с {minDate:dd.MM.yy} по {maxDate:dd.MM.yy}";
        if (_sResources == null) return null;
        var gises = _sResources.FirstOrDefault()!.Operators;
        ws.Cells[1, 1].Value = _name;
        ws.Cells[1, 1].Style.Font.Bold = true;
        ws.Cells[1, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        ws.Cells[1, 1].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
        ws.Cells[1, 1, 1, 1 + 1 + gises.Count].Merge = true;
        var row = StartRow;
        ws.Cells[row, 1].Value = "Дата поставки";
        ws.Column(1).Width = 20;
        ws.Cells[row, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        ws.Cells[row, 1].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
        ws.Cells[row, 2].Value = "Тип";
        ws.Column(2).Width = 6;
        ws.Cells[row, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        ws.Cells[row, 2].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
        ws.Row(2).Height = 50;
        ws.Row(2).Style.WrapText = true;
        ws.Row(2).Style.Font.Bold = true;
        var gisNumber = 1;
        for (var date = maxDate; date >= minDate; date = date.AddDays(-1))
        {
            ws.Cells[++row, 1].Value = date.ToString("dd.MM.yyyy");
            ws.Cells[row, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            ws.Cells[row, 2].Value = "ДЗ";
            ws.Cells[row, 2].Style.Font.Color.SetColor(Color.Green);
            ws.Cells[row, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            ws.Cells[++row, 1].Value = date.ToString("dd.MM.yyyy");
            ws.Cells[row, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            ws.Cells[row, 2].Value = "ЗР";
            ws.Cells[row, 2].Style.Font.Color.SetColor(Color.Blue);
            ws.Cells[row, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

        }
        foreach (var gis in gises)
        {
            row = StartRow;
            ws.Cells[row, 2 + gisNumber].Value = gis.OperatorGis.Name;
            ws.Column(2 + gisNumber).Width = 20;
            ws.Cells[row, 2 + gisNumber].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            ws.Cells[row, 2 + gisNumber].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            for (var date = maxDate; date >= minDate; date = date.AddDays(-1))
            {
                var op = _sResources.FirstOrDefault(x => x.SupplyDate == date && x.Type == OperatorResourceType.Dz);
                if (op == null)
                {
                    ws.Cells[++row, 2 + gisNumber].Value = 0;
                }
                else
                {
                    var oper = op.Operators.FirstOrDefault(x => x.OperatorGis.Id == gis.OperatorGis.Id);
                    ws.Cells[++row, 2 + gisNumber].Value = oper?.Volume ?? 0;
                }
                ws.Cells[row, 2 + gisNumber].Style.Numberformat.Format = "_-* #,##0.00_-;-* #,##0.00_-;_-* \"-\"??_-;_-@_-";
                op = _sResources.FirstOrDefault(x => x.SupplyDate == date && x.Type == OperatorResourceType.Zr);
                if (op == null)
                {
                    ws.Cells[++row, 2 + gisNumber].Value = 0;
                }
                else
                {
                    var oper = op.Operators.FirstOrDefault(x => x.OperatorGis.Id == gis.OperatorGis.Id);
                    ws.Cells[++row, 2 + gisNumber].Value = oper?.Volume ?? 0;
                }
                ws.Cells[row, 2 + gisNumber].Style.Numberformat.Format = "_-* #,##0.00_-;-* #,##0.00_-;_-* \"-\"??_-;_-@_-";
            }
            gisNumber++;
        }
        ws.Cells[StartRow, 1, row, 1 + gisNumber].Style.Border.Top.Style = ExcelBorderStyle.Thin;
        ws.Cells[StartRow, 1, row, 1 + gisNumber].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
        ws.Cells[StartRow, 1, row, 1 + gisNumber].Style.Border.Left.Style = ExcelBorderStyle.Thin;
        ws.Cells[StartRow, 1, row, 1 + gisNumber].Style.Border.Right.Style = ExcelBorderStyle.Thin;
        ws.PrinterSettings.RepeatRows = new ExcelAddress("2:2");
        ws.PrinterSettings.PrintArea = ws.Cells[1, 1, row, 1 + gisNumber];
        ws.PrinterSettings.FitToPage = true;
        ws.PrinterSettings.FitToWidth = 1;
        ws.PrinterSettings.FitToHeight = 0;
        ws.PrinterSettings.PaperSize = ePaperSize.A4;
        ws.PrinterSettings.Orientation = eOrientation.Landscape;
        ws.PrinterSettings.HorizontalCentered = true;
        ws.PrinterSettings.LeftMargin = 0.5M;
        ws.PrinterSettings.RightMargin = 0.5M;
        var excelBytes = pck.GetAsByteArray();
        return await Task.FromResult(excelBytes);
    }
    
    private Task GenerateLineChart(ExcelPackage pck)
    {
        // var data = pck.Workbook.Worksheets.FirstOrDefault(x => x.Name == "result");
        var ws = pck.Workbook.Worksheets.Add("chart");
        ws.Cells.Style.Font.Name = "Arial";
        _resources = _resources.OrderBy(x => x.ReportDate).ToList();
        ws.Cells[2, 22].Value = "ДЗ";
        ws.Cells[2, 24].Value = "ЗР";
        var dzLastRow = 2;
        var zrLastRow = 2;
        foreach (var val in _resources)
        {
            switch (val.Type)
            {
                case OperatorResourceType.Dz:
                    dzLastRow++;
                    ws.Cells[dzLastRow, 21].Value = val.ReportDate;
                    ws.Cells[dzLastRow, 21].Style.Numberformat.Format = "dd.MM.yy hh:mm";
                    ws.Cells[dzLastRow, 22].Value = val.Hours.Sum(x => x.Volume);
                    break;
                case OperatorResourceType.Zr:
                    zrLastRow++;
                    ws.Cells[zrLastRow, 23].Value = val.ReportDate;
                    ws.Cells[zrLastRow, 23].Style.Numberformat.Format = "dd.MM.yy hh:mm";
                    ws.Cells[zrLastRow, 24].Value = val.Hours.Sum(x => x.Volume);
                    break;
                default:
                    continue;
            }
        }
        var chart = ws.Drawings.AddChart("Name", eChartType.XYScatterSmoothNoMarkers);
        var serie1 = chart.Series.Add(ws.Cells[3, 22, dzLastRow, 22], ws.Cells[3, 21, dzLastRow, 21]);
        var serie2 = chart.Series.Add(ws.Cells[3, 24, zrLastRow, 24], ws.Cells[3, 23, zrLastRow, 23]);
        serie1.Header = "ДЗ";
        serie2.Header = "ЗР";
        chart.XAxis.MinValue = _minDate.ToOADate();
        chart.XAxis.MaxValue = _maxDate.ToOADate();
        chart.XAxis.Format = "dd.MM.yy";
        chart.SetPosition(0, 0, 0, 0);
        chart.SetSize(1200, 800);
        chart.Title.Text = _name;
        chart.RoundedCorners = true;
        ws.PrinterSettings.PrintArea = ws.Cells[1, 1, 40, 19];
        ws.PrinterSettings.FitToPage = true;
        ws.PrinterSettings.PaperSize = ePaperSize.A4;
        ws.PrinterSettings.Orientation = eOrientation.Landscape;
        ws.PrinterSettings.HorizontalCentered = true;
        ws.PrinterSettings.LeftMargin = 0.5M;
        ws.PrinterSettings.RightMargin = 0.5M;
        return Task.CompletedTask;
        // chart.DropLine.Border.Width = 2;
        //Set style 12
        // chart.StyleManager.SetChartStyle(ePresetChartStyle.LineChartStyle12);
    }
}