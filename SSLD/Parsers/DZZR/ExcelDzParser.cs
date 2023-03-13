using Microsoft.AspNetCore.Components.Forms;
using OfficeOpenXml;
using Radzen;
using SSLD.Data.DZZR;
using SSLD.Tools;

namespace SSLD.Parsers.DZZR;

public class ExcelDzParser
{
    private NotificationService NotificationService { get; set; }
    private readonly IBrowserFile _file;
    private ExcelWorksheet _sheet;
    private string _filename;
    private DateTime _reportDate;
    private DateOnly _supplyDate;
    private readonly List<OperatorResource> _valueList = new();
    private readonly IEnumerable<OperatorGis> _gisList;
    private int _startRow;
    private int _startCol;
    private int _finishRow;
    private int _finishCol;
    private decimal _divider;
    
    public ExcelDzParser(IBrowserFile file, NotificationService notificationService, IEnumerable<OperatorGis> gisList)
    {
        _file = file;
        _gisList = gisList;
        NotificationService = notificationService;
    }
    
    public async Task<List<OperatorResource>> GetResult()
    {
        _filename = _file.Name;
        await Parse();
        return _valueList;
    }

    private async Task Parse()
    {
        var ms = new MemoryStream();
        var stream = _file.OpenReadStream(_file.Size);
        await stream.CopyToAsync(ms);
        stream.Close();
        ms.Position = 0;
        var excelPackage = new ExcelPackage();
        // var stream = _file.OpenReadStream(_file.Size);
        excelPackage.Load(ms);
        _sheet = excelPackage.Workbook.Worksheets.FirstOrDefault();
        if (_sheet == null) return;
        var endRow = _sheet.Dimension.End.Row;
        var endCol = _sheet.Dimension.End.Column;
        var reportDate = StringParser.GetFirstDateOnlyFromString(_file.Name);
        if (reportDate == null)
        {
            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Error,
                Summary = "В результате парсинга файла " + _filename + " не удалось установить дату",
                Duration = 3000
            });
            return;
        }
        var revisionTime = StringParser.GetDateWithTimeFromString(_filename); //европейское время
        var fileTime = _file.LastModified.DateTime; //время UTC

        FindReportDate();
        DetectStartCells();
        DetectFinishCells();
        FindDivider();
        
        for (var col = _startCol; col <= _finishCol; col += 3)
        {
            var gisName = _sheet.Cells[_startRow, col].Text;
            var operatorGis = _gisList.FirstOrDefault(x => StringParser.StrictLike(x.Name, gisName));
            if (operatorGis == null) continue;
            var diff = 0;
            if (!IsColumnCet(col+1))
            {
                diff = TimeParser.TimeDiff(_supplyDate.ToDateTime(new TimeOnly(12)));
            }
            var operatorResource = new OperatorResource
            {
                OperatorGis = operatorGis,
                OperatorGisId = operatorGis.Id,
                SupplyDate = _supplyDate,
                ReportDate = _reportDate
            };
            for (var row = _startRow; row <= _finishRow; row++)
            {
                var hour = _sheet.Cells[row, col + 1].Text;
                var value = _sheet.Cells[row, col + 2].Value.ToString();
                var operatorHour = new OperatorResourceHour()
                {
                    OperatorResource = operatorResource,
                    Hour = CalculateHour(hour, diff),
                    Volume = StringParser.TryGetDecimal(value) / _divider
                };
                // проверка перехода времени
                var check = operatorResource.Hours.FirstOrDefault(x => x.Hour == operatorHour.Hour);
                if (check != null)
                {
                    check.Volume += operatorHour.Volume;
                }
                else
                {
                    operatorResource.Hours.Add(operatorHour);
                }
            }
            _valueList.Add(operatorResource);
        }
        excelPackage.Dispose();
        await ms.DisposeAsync();
    }

    private static int CalculateHour(string hour, int diff)
    {
        var result = StringParser.CevTimeToInt(hour);
        result -= diff;
        return result switch
        {
            0 => 24,
            -1 => 23,
            -2 => 22,
            _ => result
        };
    }

    private bool IsColumnCet(int col)
    {
        for (var row = 1; row <= _startRow; row++)
        {
            var cellText = _sheet.Cells[row, col].Text;
            if (string.IsNullOrEmpty(cellText)) continue;
            if (StringParser.ContainLike(cellText, "мск"))
            {
                return false;
            }
            else if (StringParser.ContainLike(cellText, "цев"))
            {
                return true;
            }
        }
        return true;
    }

    private void FindReportDate()
    {
        for (var col = 1; col <= _sheet.Dimension.End.Column; col++)
        {
            for (var row = 1; row <= _sheet.Dimension.End.Row; row++)
            {
                var cellText = _sheet.Cells[row, col].Text;
                if (string.IsNullOrEmpty(cellText)) continue;
                if (!StringParser.ContainLike("Дата поставки:", cellText)) continue;
                var supplyDateTime = _sheet.Cells[row, col+1].GetValue<DateTime>();
                var reportDate = _sheet.Cells[row + 1, col+1].GetValue<DateTime>();
                var reportTime = _sheet.Cells[row + 2, col+1].GetValue<DateTime>();
                _supplyDate = DateOnly.FromDateTime(supplyDateTime);
                if (reportDate.Hour == 0 && reportDate.Minute == 0 && reportDate.Second == 0)
                {
                    _reportDate = reportDate.AddHours(reportTime.Hour).AddMinutes(reportTime.Minute).AddSeconds(reportTime.Second);
                }
                else
                {
                    _reportDate = reportDate;
                }
                return;
            }
        }
    }
    
    private void FindDivider()
    {
        for (var col = 1; col <= _sheet.Dimension.End.Column; col++)
        {
            for (var row = 1; row <= _sheet.Dimension.End.Row; row++)
            {
                var cellText = _sheet.Cells[row, col].Text;
                if (string.IsNullOrEmpty(cellText)) continue;
                if (StringParser.ContainLike(cellText, "млн"))
                {
                    _divider = 1;
                    return;
                }
                else if (StringParser.ContainLike(cellText, "тыс"))
                {
                    _divider = 1000;
                    return;
                }
            }
        }
    }

    private void DetectStartCells()
    {
        for (var col = 1; col <= _sheet.Dimension.End.Column; col++)
        {
            for (var row = 1; row <= _sheet.Dimension.End.Row; row++)
            {
                var cellText = _sheet.Cells[row, col].Text;
                if (string.IsNullOrEmpty(cellText)) continue;
                if (!StringParser.StrictLike(cellText, "ГИС")) continue;
                _startRow = row + 3;
                _startCol = col;
                return;
            }
        }
    }
    
    private void DetectFinishCells()
    {
        for (var col = _sheet.Dimension.End.Column; col >= 1; col--)
        {
            for (var row = _sheet.Dimension.End.Row; row >= 1; row--)
            {
                var cellText = _sheet.Cells[row, col].Text;
                if (string.IsNullOrEmpty(cellText)) continue;
                if (!StringParser.ContainLike(cellText, "Итого")) continue;
                _finishRow = row - 1;
                _finishCol = col + 2;
                return;
            }
        }
    }
}