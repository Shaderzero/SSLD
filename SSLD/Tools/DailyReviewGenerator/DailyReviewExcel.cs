using System.Drawing;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using Radzen;
using SSLD.Data;
using SSLD.Data.DailyReview;
using FileInfo = System.IO.FileInfo;

namespace SSLD.Tools.DailyReviewGenerator;

public class DailyReviewExcel
{
    private readonly ApplicationDbContext _db;
    private NotificationService _notificationService;
    private ExcelWorksheet _ws;
    private ExcelWorksheet _fp;
    private ExcelWorksheet _bp;
    private ExcelWorkbook _wb;
    private readonly DateOnly _startDate;
    private readonly DateOnly _finishDate;
    private const int Round = 4;
    private const int StartRow = 1;
    private const int StartCol = 2;
    private int _row;
    private int _count;
    private IList<Gis> _gisList;
    private IList<GisInputValue> _gisInputValues;
    private IList<GisOutputValue> _gisOutputValues;
    private IList<GisAddonValue> _gisAddonValues;
    private IList<GisCountryValue> _gisCountryValues;
    private IList<GisCountryAddonValue> _gisCountryAddonValues;
    private IList<ForecastGisCountry> _forecast;

    public DailyReviewExcel(ApplicationDbContext db, NotificationService notificationService, DateOnly startDate, DateOnly finishDate)
    {
        _db = db;
        _startDate = startDate;
        _finishDate = finishDate;
        _notificationService = notificationService;
    }

    private async Task CreateList()
    {
        _gisList = await _db.Gises
            .Include(x => x.Addons)
            .Include(x => x.Countries).ThenInclude(x => x.Country)
            .Include(x => x.Countries).ThenInclude(x => x.Addons)
            .ToListAsync();
        _gisInputValues = await _db.GisInputValues
            .Include(x => x.Gis)
            .Where(x => x.ReportDate >= _startDate && x.ReportDate <= _finishDate)
            .ToListAsync();
        _gisOutputValues = await _db.GisOutputValues
            .Include(x => x.Gis)
            .Where(x => x.ReportDate >= _startDate && x.ReportDate <= _finishDate)
            .ToListAsync();
        _gisAddonValues = await _db.GisAddonValues
            .Where(x => x.ReportDate >= _startDate && x.ReportDate <= _finishDate)
            .ToListAsync();
        _gisCountryValues = await _db.GisCountryValues
            .Include(x => x.GisCountry).ThenInclude(x => x.Gis)
            .Where(x => x.ReportDate >= _startDate && x.ReportDate <= _finishDate)
            .ToListAsync();
        _gisCountryAddonValues = await _db.GisCountryAddonValues
            .Include(x => x.GisCountryAddon)
            .Where(x => x.ReportDate >= _startDate && x.ReportDate <= _finishDate)
            .ToListAsync();
        var lastForecastDate = await _db.Forecasts.Where(x => x.ReportDate.Year == _finishDate.Year).MaxAsync(x => x.ReportDate);
        var lastForecast = await _db.Forecasts.FirstOrDefaultAsync(x => x.ReportDate == lastForecastDate);
        _forecast = await _db.ForecastGisCountries
            .Include(x => x.GisCountry)
            .Where(x => x.ForecastId == lastForecast.Id)
            .ToListAsync();
    }

    public async Task<byte[]> GenerateExcelReport()
    {
        SetCount();
        await CreateList();
        //ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        //ExcelPackage package = new ExcelPackage();
        const string fileName = "operativka.xlsm";
        var dir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "files", fileName);
        var infile = new FileInfo(dir);
        var package = new ExcelPackage(infile);
        _wb = package.Workbook;
        _ws = _wb.Worksheets.FirstOrDefault(x => x.Name == "ОПЕРАТИВКА пн-пн");
        _fp = _wb.Worksheets.FirstOrDefault(x => x.Name == "WORD MONDAY");
        _bp = _wb.Worksheets.FirstOrDefault(x => x.Name == "баланс");
        if (_ws != null)
        {
            _ws.DefaultRowHeight = 12;
            _ws.Cells[_ws.Dimension.Start.Row, _ws.Dimension.Start.Column, _ws.Dimension.End.Row,
                _ws.Dimension.End.Column].Clear();
            //ws = package.Workbook.Worksheets.Add("Review");
            var col = StartCol + 2;

            //оформляем заголовки
            for (var d = _startDate; d <= _finishDate; d = d.AddDays(1))
            {
                _row = StartRow;
                _ws.Cells[_row, ++col].Value = d;
                _ws.Cells[_row, col, _row, col + 4].Merge = true;
                //ws.Cells[row, col, row, col + 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.CenterContinuous;
                _ws.Cells[_row, col, _row, col + 4].Style.Numberformat.Format = "dd.MM.yyyy";
                _ws.Cells[++_row, col].Value = d.ToString("dddd");
                _ws.Cells[_row, col, _row, col + 4].Merge = true;
                //ws.Cells[row, col, row, col + 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.CenterContinuous;
                // _ws.Cells[_row, col, _row, col + 4].Style.Numberformat.Format = "dddd";
                _ws.Cells[++_row, col].Value = "Заявки утренние";
                _ws.Cells[_row, col + 1].Value = "Выделено";
                _ws.Cells[_row, col + 2].Value = "Факт оценка";
                _ws.Cells[_row, col + 3].Value = "Факт";
                _ws.Cells[_row, col + 4].Value = "Факт к графику";
                _ws.Column(col).Width = 8;
                _ws.Column(++col).Width = 8;
                _ws.Column(++col).Width = 8;
                _ws.Column(++col).Width = 8;
                _ws.Column(++col).Width = 8;
                _ws.Cells[StartRow, col - 4, _row, col].Style.Font.Bold = true;
                _ws.Cells[StartRow, col - 4, _row, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
                _ws.Cells[StartRow, col - 4, _row, col].Style.Fill.BackgroundColor.SetColor(Color.LightGreen);
                _ws.Cells[_row, col - 1, _row, col - 1].Style.Fill.BackgroundColor.SetColor(Color.LightYellow);
                _ws.Column(col).PageBreak = true;
            }

            SetBorder(_ws.Cells[StartRow, StartCol + 3, _row, col]);
            _ws.Cells[StartRow, StartCol, _row, col].Style.HorizontalAlignment =
                ExcelHorizontalAlignment.CenterContinuous;
            _ws.Cells[StartRow, StartCol, _row, col].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            _ws.Row(3).Style.WrapText = true;
            _ws.Column(StartCol).Width = 35;

            //оформляем график поставок
            col = StartCol + 1;
            _row = StartRow;
            _ws.Cells[_row, col].Value = _finishDate.AddMonths(-1).ToString("MMMM");
            _ws.Column(col).Width = 8;
            _ws.Cells[_row, col + 1].Value = _finishDate.ToString("MMMM");
            _ws.Column(col + 1).Width = 8;
            _ws.Cells[_row, col, _row + 1, col].Merge = true;
            _ws.Cells[_row, col + 1, _row + 1, col + 1].Merge = true;
            _ws.Cells[_row + 2, col].Value = "График ГПЭ";
            _ws.Cells[_row + 2, col + 1].Value = "График ГПЭ";
            _ws.Cells[_row, col, _row + 2, col + 1].Style.Font.Color.SetColor(Color.Red);

            //подача ресура всего
            _row = StartRow + 3;
            SetMainRow();
            SetInputRow();
            SetUkraineRow();

            //строки для расположения наверху (например Молдавия)
            SetTopRows();

            //товарный газ всего;
            SetGasRow();

            //товарный газ без ГПШ;
            SetGasRowNoGps();

            //трейдинг
            SetEtpRow();

            //отбор
            SetOutputRow();

            //гисы со странами и пхг
            SetGisAndCountryRows();

            //строки для расположения внизу (например Китай)
            SetBottomRows();
            SetLastRow();

            var valueCells = _ws.Cells[StartRow + 3, StartCol + 1, _ws.Dimension.End.Row, _ws.Dimension.End.Column];
            valueCells.Style.Numberformat.Format = "_-* #,##0.0_-;-* #,##0.0_-;_-* \"-\"??_-;_-@_-";

            var allCells = _ws.Cells[1, 1, _ws.Dimension.End.Row, _ws.Dimension.End.Column];
            // _ws.Rows[1, _ws.Dimension.End.Row].Height = 12;
            for (var r = 1; r <= _ws.Dimension.End.Row; r++)
            {
                _ws.Row(r).Height = 12;
            }
            _ws.Row(StartRow + 2).Height = 24;
            allCells.Style.Font.Size = 8;
            // var dayCount = _finishDate.DayNumber - _startDate.DayNumber;
            _ws.PrinterSettings.RepeatColumns = _ws.Cells["A:C"];
            _ws.PrinterSettings.PaperSize = ePaperSize.A4;
            _ws.PrinterSettings.Orientation = eOrientation.Portrait;
            _ws.PrinterSettings.HorizontalCentered = true;
            _ws.PrinterSettings.LeftMargin = 0.5M;
            _ws.PrinterSettings.RightMargin = 0.5M;
            _ws.PrinterSettings.HorizontalCentered = true;
        }

        //ws.PrinterSettings.FitToWidth = dayCount;
        //ws.PrinterSettings.FitToHeight = 1;
        //ws.PrinterSettings.FitToPage = true;

        GenerateFirstPage();
        GenerateBalancePage();
        _wb.Calculate();

        return await package.GetAsByteArrayAsync();
    }

    private void GenerateFirstPage()
    {
        _fp.Cells[1, 11].Value = _startDate;
        _fp.Cells[1, 12].Value = _finishDate;
    }

    private void GenerateBalancePage()
    {
        var lastDate = new DateOnly(_finishDate.Year, _finishDate.Month, 1);
        lastDate = lastDate.AddMonths(1).AddDays(-1);
        for (var d = _startDate; d <= lastDate; d = d.AddMonths(1))
        {
            var prognos = CalculateBalance(d);
            SetBalance(d, prognos);
        }
        var gasMonth = CalculateGasMonth(lastDate);
        SetGasMonth(gasMonth);
        var gpsMonth = CalculateGpsMonth(lastDate);
        SetGpsMonth(gpsMonth);
        var chinaMonth = CalculateChinaMonth(lastDate);
        SetChinaMonth(chinaMonth);
    }

    private void SetBalance(DateOnly date, decimal value)
    {
        var balanceRow = 0;
        var balanceCol = 0;
        var month = date.Month;
        for (var row = 1; row <= _bp.Dimension.Rows; row++)
        {
            var cell = _bp.Cells[row, 1].Text;
            if (!cell.Equals("товарный")) continue;
            balanceRow = row;
            break;
        }
        for (var col = 1; col <= _bp.Dimension.Columns; col++)
        {
            var cell = _bp.Cells[3, col].Text;
            if (!cell.Equals(month.ToString())) continue;
            balanceCol = col;
            break;
        }
        _bp.Cells[balanceRow, balanceCol].Value = value / 1000;
    }

    private void SetGasMonth(decimal value)
    {
        var gasMonthRow = 0;
        var gasMonthCol = 3;
        for (var row = 1; row <= _bp.Dimension.Rows; row++)
        {
            var cell = _bp.Cells[row, 1].Text;
            if (!cell.Equals("товарный_месяц")) continue;
            gasMonthRow = row;
            break;
        }
        _bp.Cells[gasMonthRow, gasMonthCol].Value = value / 1000;
    }

    private void SetGpsMonth(decimal value)
    {
        var gasMonthRow = 0;
        var gasMonthCol = 3;
        for (var row = 1; row <= _bp.Dimension.Rows; row++)
        {
            var cell = _bp.Cells[row, 1].Text;
            if (!cell.Equals("товарный_гпш_месяц")) continue;
            gasMonthRow = row;
            break;
        }
        _bp.Cells[gasMonthRow, gasMonthCol+1].Value = value / 1000;
        _bp.Cells[gasMonthRow, gasMonthCol].Formula = $"{_bp.Cells[gasMonthRow - 1, gasMonthCol].Address}+{_bp.Cells[gasMonthRow, gasMonthCol + 1].Address}";
    }

    private void SetChinaMonth(decimal value)
    {
        var gasMonthRow = 0;
        var gasMonthCol = 3;
        for (var row = 1; row <= _bp.Dimension.Rows; row++)
        {
            var cell = _bp.Cells[row, 1].Text;
            if (!cell.Equals("товарный_китай_месяц")) continue;
            gasMonthRow = row;
            break;
        }
        _bp.Cells[gasMonthRow, gasMonthCol + 1].Value = value / 1000;
        _bp.Cells[gasMonthRow, gasMonthCol].Formula = $"{_bp.Cells[gasMonthRow - 1, gasMonthCol].Address}+{_bp.Cells[gasMonthRow, gasMonthCol + 1].Address}";
    }

    private decimal CalculateGasMonth(DateOnly date)
    {
        decimal result = 0;
        var startDate = new DateOnly(date.Year, date.Month, 1);
        var finishDate = date;
        for (var d = startDate; d <= finishDate; d = d.AddDays(1))
        {
            decimal value = 0;
            var values = _gisCountryValues.Where(x => x.ReportDate == d && !x.GisCountry.IsNotCalculated && !x.GisCountry.Gis.IsTop && !x.GisCountry.Gis.IsBottom).ToList();
            value += values.Sum(x => x.FactValue);
            if (value == 0)
            {
                value += values.Sum(x => x.EstimatedValue);
                if (value == 0)
                {
                    value += values.Sum(x => x.AllocatedValue);
                }
            }
            result += value;
        }
        return result;
    }

    private decimal CalculateGpsMonth(DateOnly date)
    {
        decimal result = 0;
        var startDate = new DateOnly(date.Year, date.Month, 1);
        for (var d = startDate; d <= date; d = d.AddDays(1))
        {
            decimal value = 0;
            var values = _gisCountryAddonValues.Where(x => x.ReportDate == d && !x.GisCountryAddon.Types.Any(t => t.StartDate == d && t.IsCommGas)).ToList();
            value += values.Sum(x => x.FactValue);
            if (value == 0)
            {

                value += values.Sum(x => x.EstimatedValue);
                if (value == 0)
                {
                    value += values.Sum(x => x.AllocatedValue);
                }
            }
            result += value;
        }
        return result;
    }

    private decimal CalculateChinaMonth(DateOnly date)
    {
        decimal result = 0;
        var startDate = new DateOnly(date.Year, date.Month, 1);
        for (var d = startDate; d <= date; d = d.AddDays(1))
        {
            decimal value = 0;
            var values = _gisCountryValues.Where(x => x.ReportDate == d && !x.GisCountry.IsNotCalculated && x.GisCountry.Gis.IsBottom).ToList();
            //var values = _db.GisCountryValues.Where(x => x.ReportDate == d && !x.GisCountry.IsNotCalculated && x.GisCountry.Gis.IsBottom).AsQueryable();
            value += values.Sum(x => x.FactValue);
            if (value == 0)
            {

                value += values.Sum(x => x.EstimatedValue);
                if (value == 0)
                {
                    value += values.Sum(x => x.AllocatedValue);
                }
            }
            result += value;
        }
        return result;
    }

    private decimal CalculateBalance(DateOnly date)
    {
        decimal result = 0;
        var startDate = new DateOnly(date.Year, date.Month, 1);
        var finishDate = startDate.AddMonths(1).AddDays(-1);
        for (var d = startDate; d <= finishDate; d = d.AddDays(1))
        {
            decimal value = 0;
            var values = _gisCountryValues.Where(x => x.ReportDate == d && !x.GisCountry.IsNotCalculated && !x.GisCountry.Gis.IsTop && !x.GisCountry.Gis.IsBottom).ToList();
            //var values = _db.GisCountryValues.Where(x => x.ReportDate == d && !x.GisCountry.IsNotCalculated && !x.GisCountry.Gis.IsTop && !x.GisCountry.Gis.IsBottom).AsQueryable();
            value += values.Sum(x => x.FactValue);
            if (value == 0)
            {
                value += values.Sum(x => x.EstimatedValue);
                if (value == 0)
                {
                    value += values.Sum(x => x.AllocatedValue);
                    if (value == 0)
                    {
                        value += CalculateBalancePerDay(d);
                    }
                }
            }
            result += value;
        }
        return result;
    }

    private decimal CalculateBalancePerDay(DateOnly date)
    {
        var dayCount = DateTime.DaysInMonth(date.Year, date.Month);
        var monthResult = _forecast.Where(x => x.Month == date.Month).Sum(x => x.Value);
        return monthResult / dayCount;
    }

    private void SetCount()
    {
        _count = 6;
        _count += _db.Gises.Count(x => !x.IsNotCalculated);
        _count += _db.GisCountries.Count(x => !x.IsHidden && !x.Gis.IsOneRow);
        _count += _db.Gises.Count(x => !x.IsNoPhg) * 2;
        _count += _db.GisAddons.Count();
    }

    private void SetMainRow()
    {
        var cell = _ws.Cells[_row, StartCol];
        cell.Value = "Подача ресурса - всего";
        _ws.Cells[_row, StartCol-1].Formula = cell.Address;
        cell.Style.Font.Bold = true;
        cell.Style.Font.Color.SetColor(Color.Red);
        SetBorder(cell);
        var col = StartCol + 3;
        for (var d = _startDate; d <= _finishDate; d = d.AddDays(1))
        {
            var requestedValue = _gisInputValues.Where(x => x.ReportDate == d && !x.Gis.IsTop && !x.Gis.IsBottom).Sum(x => x.RequestedValue);
            requestedValue -= _gisOutputValues.Where(x => x.ReportDate == d && !x.Gis.IsTop && !x.Gis.IsBottom).Sum(x => x.RequestedValue);
            requestedValue += _gisCountryValues.Where(x => x.ReportDate == d && !x.GisCountry.IsNotCalculated && !x.GisCountry.Gis.IsTop && !x.GisCountry.Gis.IsBottom).Sum(x => x.RequestedValue);
            requestedValue += _gisAddonValues.Where(x => x.ReportDate == d && x.GisAddon.IsInput).Sum(x => x.RequestedValue);
            requestedValue -= _gisAddonValues.Where(x => x.ReportDate == d && x.GisAddon.IsOutput).Sum(x => x.RequestedValue);
            requestedValue += _gisCountryAddonValues.Where(x => x.ReportDate == d).Sum(x => x.RequestedValue);
            var allocatedValue = _gisInputValues.Where(x => x.ReportDate == d && !x.Gis.IsTop && !x.Gis.IsBottom).Sum(x => x.AllocatedValue);
            allocatedValue -= _gisOutputValues.Where(x => x.ReportDate == d && !x.Gis.IsTop && !x.Gis.IsBottom).Sum(x => x.AllocatedValue);
            allocatedValue += _gisCountryValues.Where(x => x.ReportDate == d && !x.GisCountry.IsNotCalculated && !x.GisCountry.Gis.IsTop && !x.GisCountry.Gis.IsBottom).Sum(x => x.AllocatedValue);
            allocatedValue += _gisAddonValues.Where(x => x.ReportDate == d && x.GisAddon.IsInput).Sum(x => x.AllocatedValue);
            allocatedValue -= _gisAddonValues.Where(x => x.ReportDate == d && x.GisAddon.IsOutput).Sum(x => x.AllocatedValue);
            allocatedValue += _gisCountryAddonValues.Where(x => x.ReportDate == d).Sum(x => x.AllocatedValue);
            var estimatedValue = _gisInputValues.Where(x => x.ReportDate == d && !x.Gis.IsTop && !x.Gis.IsBottom).Sum(x => x.EstimatedValue);
            estimatedValue -= _gisOutputValues.Where(x => x.ReportDate == d && !x.Gis.IsTop && !x.Gis.IsBottom).Sum(x => x.EstimatedValue);
            estimatedValue += _gisCountryValues.Where(x => x.ReportDate == d && !x.GisCountry.IsNotCalculated && !x.GisCountry.Gis.IsTop && !x.GisCountry.Gis.IsBottom).Sum(x => x.EstimatedValue);
            estimatedValue += _gisAddonValues.Where(x => x.ReportDate == d && x.GisAddon.IsInput).Sum(x => x.EstimatedValue);
            estimatedValue -= _gisAddonValues.Where(x => x.ReportDate == d && x.GisAddon.IsOutput).Sum(x => x.EstimatedValue);
            estimatedValue += _gisCountryAddonValues.Where(x => x.ReportDate == d).Sum(x => x.EstimatedValue);
            var factValue = _gisInputValues.Where(x => x.ReportDate == d && !x.Gis.IsTop && !x.Gis.IsBottom).Sum(x => x.FactValue);
            factValue -= _gisOutputValues.Where(x => x.ReportDate == d && !x.Gis.IsTop && !x.Gis.IsBottom).Sum(x => x.FactValue);
            factValue += _gisCountryValues.Where(x => x.ReportDate == d && !x.GisCountry.IsNotCalculated && !x.GisCountry.Gis.IsTop && !x.GisCountry.Gis.IsBottom).Sum(x => x.FactValue);
            factValue += _gisAddonValues.Where(x => x.ReportDate == d && x.GisAddon.IsInput).Sum(x => x.FactValue);
            factValue -= _gisAddonValues.Where(x => x.ReportDate == d && x.GisAddon.IsOutput).Sum(x => x.FactValue);
            factValue += _gisCountryAddonValues.Where(x => x.ReportDate == d).Sum(x => x.FactValue);
            _ws.Cells[_row, col].Value = Math.Round(requestedValue, Round);
            _ws.Cells[_row, col].Style.Font.Bold = true;
            SetBorder(_ws.Cells[_row, col]);
            _ws.Cells[_row, ++col].Value = Math.Round(allocatedValue, Round);
            _ws.Cells[_row, col].Style.Font.Bold = true;
            SetBorder(_ws.Cells[_row, col]);
            if (d == _finishDate)
            {
                _fp.Cells[10, 11].Formula = _ws.Cells[_row, col].FullAddress;
            }
            _ws.Cells[_row, ++col].Value = Math.Round(estimatedValue, Round);
            _ws.Cells[_row, col].Style.Font.Bold = true;
            SetBorder(_ws.Cells[_row, col]);
            _ws.Cells[_row, ++col].Value = Math.Round(factValue, Round);
            _ws.Cells[_row, col].Style.Font.Bold = true;
            SetBorder(_ws.Cells[_row, col]);
            _ws.Cells[_row, ++col].Value = 0;
            col++;
        }

        _row++;
    }

    private void SetLastRow()
    {
        var cell = _ws.Cells[_row, StartCol];
        cell.Value = "ТОВАРНЫЙ ГАЗ - ВСЕГО С УЧЕТОМ КИТАЯ и ГПШ";
        cell.Style.Font.Bold = true;
        cell.Style.Font.Color.SetColor(Color.Red);
        SetBorder(cell);
        var col = StartCol + 3;
        for (var d = _startDate; d <= _finishDate; d = d.AddDays(1))
        {
            decimal requestedValue = 0;
            decimal allocatedValue = 0;
            decimal estimatedValue = 0;
            decimal factValue = 0;
            var values = _gisCountryValues.Where(x => x.ReportDate == d && !x.GisCountry.IsNotCalculated && !x.GisCountry.Gis.IsTop).ToList();
            // var addonDate = new DateOnly(d.Year, d.Month, 1);
            var addonValues = _gisCountryAddonValues.Where(x => x.ReportDate == d).ToList();
            requestedValue += values.Sum(x => x.RequestedValue);
            allocatedValue += values.Sum(x => x.AllocatedValue);
            estimatedValue += values.Sum(x => x.EstimatedValue);
            factValue += values.Sum(x => x.FactValue);
            requestedValue += addonValues.Sum(x => x.RequestedValue);
            allocatedValue += addonValues.Sum(x => x.AllocatedValue);
            estimatedValue += addonValues.Sum(x => x.EstimatedValue);
            factValue += addonValues.Sum(x => x.FactValue);
            _ws.Cells[_row, col].Value = Math.Round(requestedValue, Round);
            _ws.Cells[_row, col].Style.Font.Bold = true;
            SetBorder(_ws.Cells[_row, col]);
            _ws.Cells[_row, ++col].Value = Math.Round(allocatedValue, Round);
            _ws.Cells[_row, col].Style.Font.Bold = true;
            SetBorder(_ws.Cells[_row, col]);
            if (d == _finishDate)
            {
                _fp.Cells[9, 11].Formula = _ws.Cells[_row, col].FullAddress;
            }
            _ws.Cells[_row, ++col].Value = Math.Round(estimatedValue, Round);
            _ws.Cells[_row, col].Style.Font.Bold = true;
            SetBorder(_ws.Cells[_row, col]);
            _ws.Cells[_row, ++col].Value = Math.Round(factValue, Round);
            _ws.Cells[_row, col].Style.Font.Bold = true;
            SetBorder(_ws.Cells[_row, col]);
            _ws.Cells[_row, ++col].Value = 0;
            col++;
        }

        _row++;
    }

    private void SetInputRow()
    {
        var cell = _ws.Cells[_row, StartCol];
        cell.Value = "в т.ч. закачка - всего";
        _ws.Cells[_row, StartCol-1].Formula = cell.Address;
        cell.Style.Font.Bold = true;
        cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
        SetBorder(cell);
        var col = StartCol + 3;
        for (var d = _startDate; d <= _finishDate; d = d.AddDays(1))
        {
            var values = _gisInputValues.Where(x => x.ReportDate == d).ToList();
            var requestedValue = values.Sum(x => x.RequestedValue);
            var allocatedValue = values.Sum(x => x.AllocatedValue);
            var estimatedValue = values.Sum(x => x.EstimatedValue);
            var factValue = values.Sum(x => x.FactValue);
            _ws.Cells[_row, col].Value = Math.Round(requestedValue, Round);
            _ws.Cells[_row, col].Style.Font.Bold = true;
            _ws.Cells[_row, ++col].Value = Math.Round(allocatedValue, Round);
            if (d == _finishDate)
            {
                _fp.Cells[11, 11].Formula = _ws.Cells[_row, col].FullAddress;
            }
            _ws.Cells[_row, col].Style.Font.Bold = true;
            _ws.Cells[_row, ++col].Value = Math.Round(estimatedValue, Round);
            _ws.Cells[_row, col].Style.Font.Bold = true;
            _ws.Cells[_row, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
            _ws.Cells[_row, col].Style.Fill.BackgroundColor.SetColor(Color.LightGreen);
            _ws.Cells[_row, ++col].Value = Math.Round(factValue, Round);
            _ws.Cells[_row, col].Style.Font.Bold = true;
            _ws.Cells[_row, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
            _ws.Cells[_row, col].Style.Fill.BackgroundColor.SetColor(Color.LightYellow);
            _ws.Cells[_row, ++col].Value = 0;
            col++;
        }
        _row++;
    }

    private void SetEtpRow()
    {
        var cell = _ws.Cells[_row, StartCol];
        cell.Value = "в т.ч. спот продажи через трейдинг/ЭТП";
        cell.Style.Font.Bold = true;
        cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
        SetBorder(cell);
        var col = StartCol + 3;
        for (var d = _startDate; d <= _finishDate; d = d.AddDays(1))
        {
            _ws.Cells[_row, col].Style.Font.Bold = true;
            _ws.Cells[_row, ++col].Style.Font.Bold = true;
            _ws.Cells[_row, ++col].Style.Font.Bold = true;
            _ws.Cells[_row, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
            _ws.Cells[_row, col].Style.Fill.BackgroundColor.SetColor(Color.LightGreen);
            _ws.Cells[_row, ++col].Style.Font.Bold = true;
            _ws.Cells[_row, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
            _ws.Cells[_row, col].Style.Fill.BackgroundColor.SetColor(Color.LightYellow);
            _ws.Cells[_row, ++col].Value = 0;
            col++;
        }
        _row++;
    }

    private void SetOutputRow()
    {
        var cell = _ws.Cells[_row, StartCol];
        cell.Value = "в т.ч. отбор - всего";
        cell.Style.Font.Bold = true;
        cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
        SetBorder(cell);
        var col = StartCol + 3;
        for (var d = _startDate; d <= _finishDate; d = d.AddDays(1))
        {
            var values = _gisOutputValues.Where(x => x.ReportDate == d).ToList();
            var requestedValue = values.Sum(x => x.RequestedValue);
            var allocatedValue = values.Sum(x => x.AllocatedValue);
            var estimatedValue = values.Sum(x => x.EstimatedValue);
            var factValue = values.Sum(x => x.FactValue);
            _ws.Cells[_row, col].Value = Math.Round(requestedValue, Round);
            _ws.Cells[_row, col].Style.Font.Bold = true;
            _ws.Cells[_row, ++col].Value = Math.Round(allocatedValue, Round);
            _ws.Cells[_row, col].Style.Font.Bold = true;
            if (d == _finishDate)
            {
                _fp.Cells[12, 11].Formula = _ws.Cells[_row, col].FullAddress;
            }
            _ws.Cells[_row, ++col].Value = Math.Round(estimatedValue, Round);
            _ws.Cells[_row, col].Style.Font.Bold = true;
            _ws.Cells[_row, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
            _ws.Cells[_row, col].Style.Fill.BackgroundColor.SetColor(Color.LightGreen);
            _ws.Cells[_row, ++col].Value = Math.Round(factValue, Round);
            _ws.Cells[_row, col].Style.Font.Bold = true;
            _ws.Cells[_row, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
            _ws.Cells[_row, col].Style.Fill.BackgroundColor.SetColor(Color.LightYellow);
            _ws.Cells[_row, ++col].Value = 0;
            col++;
        }
        _row++;
    }

    private void SetUkraineRow()
    {
        var cell = _ws.Cells[_row, StartCol];
        cell.Value = "транспорт через Украину";
        cell.Style.Font.Bold = true;
        cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
        SetBorder(cell);
        var col = StartCol + 3;
        for (var d = _startDate; d <= _finishDate; d = d.AddDays(1))
        {
            var requestedValue = _gisInputValues.Where(x => x.ReportDate == d && x.Gis.IsUkraineTransport).Sum(x => x.RequestedValue);
            requestedValue -= _gisOutputValues.Where(x => x.ReportDate == d && x.Gis.IsUkraineTransport).Sum(x => x.RequestedValue);
            requestedValue += _gisCountryValues.Where(x => x.ReportDate == d && !x.GisCountry.IsNotCalculated && x.GisCountry.Gis.IsUkraineTransport).Sum(x => x.RequestedValue);
            var allocatedValue = _gisInputValues.Where(x => x.ReportDate == d && x.Gis.IsUkraineTransport).Sum(x => x.AllocatedValue);
            allocatedValue -= _gisOutputValues.Where(x => x.ReportDate == d && x.Gis.IsUkraineTransport).Sum(x => x.AllocatedValue);
            allocatedValue += _gisCountryValues.Where(x => x.ReportDate == d && !x.GisCountry.IsNotCalculated && x.GisCountry.Gis.IsUkraineTransport).Sum(x => x.AllocatedValue);
            var estimatedValue = _gisInputValues.Where(x => x.ReportDate == d && x.Gis.IsUkraineTransport).Sum(x => x.EstimatedValue);
            estimatedValue -= _gisOutputValues.Where(x => x.ReportDate == d && x.Gis.IsUkraineTransport).Sum(x => x.EstimatedValue);
            estimatedValue += _gisCountryValues.Where(x => x.ReportDate == d && !x.GisCountry.IsNotCalculated && x.GisCountry.Gis.IsUkraineTransport).Sum(x => x.EstimatedValue);
            var factValue = _gisInputValues.Where(x => x.ReportDate == d && x.Gis.IsUkraineTransport).Sum(x => x.FactValue);
            factValue -= _gisOutputValues.Where(x => x.ReportDate == d && x.Gis.IsUkraineTransport).Sum(x => x.FactValue);
            factValue += _gisCountryValues.Where(x => x.ReportDate == d && !x.GisCountry.IsNotCalculated && x.GisCountry.Gis.IsUkraineTransport).Sum(x => x.FactValue);
            _ws.Cells[_row, col].Value = Math.Round(requestedValue, Round);
            _ws.Cells[_row, col].Style.Font.Bold = true;
            _ws.Cells[_row, ++col].Value = Math.Round(allocatedValue, Round);
            _ws.Cells[_row, col].Style.Font.Bold = true;
            _ws.Cells[_row, ++col].Value = Math.Round(estimatedValue, Round);
            _ws.Cells[_row, col].Style.Font.Bold = true;
            _ws.Cells[_row, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
            _ws.Cells[_row, col].Style.Fill.BackgroundColor.SetColor(Color.LightGreen);
            _ws.Cells[_row, ++col].Value = Math.Round(factValue, Round);
            _ws.Cells[_row, col].Style.Font.Bold = true;
            _ws.Cells[_row, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
            _ws.Cells[_row, col].Style.Fill.BackgroundColor.SetColor(Color.LightYellow);
            _ws.Cells[_row, ++col].Value = 0;
            col++;
        }
        _row++;
    }

    private void SetTopRows()
    {
        var gisList = _gisList.Where(x => x.IsOneRow && x.IsTop);
        foreach (var gis in gisList)
        {
            var cell = _ws.Cells[_row, StartCol];
            cell.Value = gis.DailyReviewName;
            cell.Style.Font.Bold = true;
            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
            SetBorder(cell);
            var col = StartCol + 3;
            for (DateOnly d = _startDate; d <= _finishDate; d = d.AddDays(1))
            {
                decimal requestedValue = 0;
                decimal allocatedValue = 0;
                decimal estimatedValue = 0;
                decimal factValue = 0;
                requestedValue = _gisInputValues.Where(x => x.GisId == gis.Id && x.ReportDate == d).Sum(x => x.RequestedValue);
                requestedValue -= _gisOutputValues.Where(x => x.GisId == gis.Id && x.ReportDate == d).Sum(x => x.RequestedValue);
                requestedValue += _gisCountryValues.Where(x => x.GisCountry.GisId == gis.Id && x.ReportDate == d && !x.GisCountry.IsNotCalculated).Sum(x => x.RequestedValue);
                allocatedValue = _gisInputValues.Where(x => x.GisId == gis.Id && x.ReportDate == d).Sum(x => x.AllocatedValue);
                allocatedValue -= _gisOutputValues.Where(x => x.GisId == gis.Id && x.ReportDate == d).Sum(x => x.AllocatedValue);
                allocatedValue += _gisCountryValues.Where(x => x.GisCountry.GisId == gis.Id && x.ReportDate == d && !x.GisCountry.IsNotCalculated).Sum(x => x.AllocatedValue);
                estimatedValue = _gisInputValues.Where(x => x.GisId == gis.Id && x.ReportDate == d).Sum(x => x.EstimatedValue);
                estimatedValue -= _gisOutputValues.Where(x => x.GisId == gis.Id && x.ReportDate == d).Sum(x => x.EstimatedValue);
                estimatedValue += _gisCountryValues.Where(x => x.GisCountry.GisId == gis.Id && x.ReportDate == d && !x.GisCountry.IsNotCalculated).Sum(x => x.EstimatedValue);
                factValue = _gisInputValues.Where(x => x.GisId == gis.Id && x.ReportDate == d).Sum(x => x.FactValue);
                factValue -= _gisOutputValues.Where(x => x.GisId == gis.Id && x.ReportDate == d).Sum(x => x.FactValue);
                factValue += _gisCountryValues.Where(x => x.GisCountry.GisId == gis.Id && x.ReportDate == d && !x.GisCountry.IsNotCalculated).Sum(x => x.FactValue);
                _ws.Cells[_row, col].Value = Math.Round(requestedValue, Round);
                _ws.Cells[_row, col].Style.Font.Bold = true;
                _ws.Cells[_row, ++col].Value = Math.Round(allocatedValue, Round);
                _ws.Cells[_row, col].Style.Font.Bold = true;
                _ws.Cells[_row, ++col].Value = Math.Round(estimatedValue, Round);
                _ws.Cells[_row, col].Style.Font.Bold = true;
                _ws.Cells[_row, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
                _ws.Cells[_row, col].Style.Fill.BackgroundColor.SetColor(Color.LightGreen);
                _ws.Cells[_row, ++col].Value = Math.Round(factValue, Round);
                _ws.Cells[_row, col].Style.Font.Bold = true;
                _ws.Cells[_row, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
                _ws.Cells[_row, col].Style.Fill.BackgroundColor.SetColor(Color.LightYellow);
                _ws.Cells[_row, ++col].Value = 0;
                col++;
            }
            _row++;
        }
    }

    private void SetBottomRows()
    {
        var gisList = _gisList.Where(x => x.IsOneRow && x.IsBottom);
        foreach (var gis in gisList)
        {
            var cell = _ws.Cells[_row, StartCol];
            cell.Value = gis.DailyReviewName;
            cell.Style.Font.Bold = true;
            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
            SetBorder(cell);
            var col = StartCol + 3;
            for (DateOnly d = _startDate; d <= _finishDate; d = d.AddDays(1))
            {
                decimal requestedValue = 0;
                decimal allocatedValue = 0;
                decimal estimatedValue = 0;
                decimal factValue = 0;
                requestedValue = _gisInputValues.Where(x => x.GisId == gis.Id && x.ReportDate == d).Sum(x => x.RequestedValue);
                requestedValue -= _gisOutputValues.Where(x => x.GisId == gis.Id && x.ReportDate == d).Sum(x => x.RequestedValue);
                requestedValue += _gisCountryValues.Where(x => x.GisCountry.GisId == gis.Id && x.ReportDate == d && !x.GisCountry.IsNotCalculated).Sum(x => x.RequestedValue);
                allocatedValue = _gisInputValues.Where(x => x.GisId == gis.Id && x.ReportDate == d).Sum(x => x.AllocatedValue);
                allocatedValue -= _gisOutputValues.Where(x => x.GisId == gis.Id && x.ReportDate == d).Sum(x => x.AllocatedValue);
                allocatedValue += _gisCountryValues.Where(x => x.GisCountry.GisId == gis.Id && x.ReportDate == d && !x.GisCountry.IsNotCalculated).Sum(x => x.AllocatedValue);
                estimatedValue = _gisInputValues.Where(x => x.GisId == gis.Id && x.ReportDate == d).Sum(x => x.EstimatedValue);
                estimatedValue -= _gisOutputValues.Where(x => x.GisId == gis.Id && x.ReportDate == d).Sum(x => x.EstimatedValue);
                estimatedValue += _gisCountryValues.Where(x => x.GisCountry.GisId == gis.Id && x.ReportDate == d && !x.GisCountry.IsNotCalculated).Sum(x => x.EstimatedValue);
                factValue = _gisInputValues.Where(x => x.GisId == gis.Id && x.ReportDate == d).Sum(x => x.FactValue);
                factValue -= _gisOutputValues.Where(x => x.GisId == gis.Id && x.ReportDate == d).Sum(x => x.FactValue);
                factValue += _gisCountryValues.Where(x => x.GisCountry.GisId == gis.Id && x.ReportDate == d && !x.GisCountry.IsNotCalculated).Sum(x => x.FactValue);
                _ws.Cells[_row, col].Value = Math.Round(requestedValue, Round);
                _ws.Cells[_row, col].Style.Font.Bold = true;
                SetBorder(_ws.Cells[_row, col]);
                _ws.Cells[_row, ++col].Value = Math.Round(allocatedValue, Round);
                _ws.Cells[_row, col].Style.Font.Bold = true;
                SetBorder(_ws.Cells[_row, col]);
                _ws.Cells[_row, ++col].Value = Math.Round(estimatedValue, Round);
                _ws.Cells[_row, col].Style.Font.Bold = true;
                SetBorder(_ws.Cells[_row, col]);
                _ws.Cells[_row, ++col].Value = Math.Round(factValue, Round);
                _ws.Cells[_row, col].Style.Font.Bold = true;
                SetBorder(_ws.Cells[_row, col]);
                _ws.Cells[_row, ++col].Value = 0;
                col++;
            }
            _row++;
        }
    }

    private void SetGasRow()
    {
        var cell = _ws.Cells[_row, StartCol];
        cell.Value = "Товарный газ - всего";
        cell.Style.Font.Bold = true;
        cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
        SetBorder(cell);
        var col = StartCol + 3;
        for (var d = _startDate; d <= _finishDate; d = d.AddDays(1))
        {
            decimal requestedValue = 0;
            decimal allocatedValue = 0;
            decimal estimatedValue = 0;
            decimal factValue = 0;
            var values = _gisCountryValues
                .Where(x => x.ReportDate == d && !x.GisCountry.IsNotCalculated && !x.GisCountry.Gis.IsTop && !x.GisCountry.Gis.IsBottom)
                .ToList();
            requestedValue += values.Sum(x => x.RequestedValue);
            allocatedValue += values.Sum(x => x.AllocatedValue);
            estimatedValue += values.Sum(x => x.EstimatedValue);
            factValue += values.Sum(x => x.FactValue);
            var addonValues = _gisCountryAddonValues
                .Where(x => x.ReportDate == d)
                .ToList();
            requestedValue += addonValues.Sum(x => x.RequestedValue);
            allocatedValue += addonValues.Sum(x => x.AllocatedValue);
            estimatedValue += addonValues.Sum(x => x.EstimatedValue);
            factValue += addonValues.Sum(x => x.FactValue);
            _ws.Cells[_row, col].Value = Math.Round(requestedValue, Round);
            _ws.Cells[_row, col].Style.Font.Bold = true;
            _ws.Cells[_row, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
            _ws.Cells[_row, col].Style.Fill.BackgroundColor.SetColor(Color.LightCyan);
            SetBorder(_ws.Cells[_row, col]);
            _ws.Cells[_row, ++col].Value = Math.Round(allocatedValue, Round);
            _ws.Cells[_row, col].Style.Font.Bold = true;
            _ws.Cells[_row, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
            _ws.Cells[_row, col].Style.Fill.BackgroundColor.SetColor(Color.LightCyan);
            SetBorder(_ws.Cells[_row, col]);
            if (d == _finishDate)
            {
                _fp.Cells[7, 11].Formula = _ws.Cells[_row, col].FullAddress;
            }
            _ws.Cells[_row, ++col].Value = Math.Round(estimatedValue, Round);
            _ws.Cells[_row, col].Style.Font.Bold = true;
            _ws.Cells[_row, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
            _ws.Cells[_row, col].Style.Fill.BackgroundColor.SetColor(Color.LightCyan);
            SetBorder(_ws.Cells[_row, col]);
            _ws.Cells[_row, ++col].Value = Math.Round(factValue, Round);
            _ws.Cells[_row, col].Style.Font.Bold = true;
            _ws.Cells[_row, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
            _ws.Cells[_row, col].Style.Fill.BackgroundColor.SetColor(Color.LightCyan);
            SetBorder(_ws.Cells[_row, col]);
            _ws.Cells[_row, ++col].Value = 0;
            col++;
        }
        _row++;
    }

    private void SetGasRowNoGps()
    {
        var cell = _ws.Cells[_row, StartCol];
        cell.Value = "Товарный газ ГПЭ без учета ГПШ";
        cell.Style.Font.Bold = true;
        cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
        SetBorder(cell);
        var col = StartCol + 1;
        var firstResource = _forecast.Where(x => x.Month == _startDate.Month).Sum(x => x.Value);
        firstResource /= DateTime.DaysInMonth(_startDate.Year, _startDate.Month);
        var secondResource = _forecast.Where(x => x.Month == _finishDate.Month).Sum(x => x.Value);
        secondResource /= DateTime.DaysInMonth(_finishDate.Year, _finishDate.Month);
        _ws.Cells[_row, col].Value = Math.Round(firstResource, Round);
        _ws.Cells[_row, col].Style.Font.Bold = true;
        _ws.Cells[_row, col].Style.Font.Color.SetColor(Color.Red);
        col++;
        _ws.Cells[_row, col].Value = Math.Round(secondResource, Round);
        _ws.Cells[_row, col].Style.Font.Bold = true;
        _ws.Cells[_row, col].Style.Font.Color.SetColor(Color.Red);
        col++;
        var list = _gisCountryValues
            .Where(x => !x.GisCountry.IsNotCalculated && !x.GisCountry.Gis.IsTop && !x.GisCountry.Gis.IsBottom)
            .ToList();
        for (var d = _startDate; d <= _finishDate; d = d.AddDays(1))
        {
            decimal requestedValue = 0;
            decimal allocatedValue = 0;
            decimal estimatedValue = 0;
            decimal factValue = 0;
            var values = list.Where(x => x.ReportDate == d).ToList();
            requestedValue += values.Sum(x => x.RequestedValue);
            allocatedValue += values.Sum(x => x.AllocatedValue);
            estimatedValue += values.Sum(x => x.EstimatedValue);
            factValue += values.Sum(x => x.FactValue);
            var addonDate = new DateOnly(d.Year, d.Month, 1);
            var addonValues = _gisCountryAddonValues
                .Where(x => x.ReportDate == d && x.GisCountryAddon.Types.Any(t => t.StartDate == addonDate && t.IsCommGas))
                .ToList();
            requestedValue += addonValues.Sum(x => x.RequestedValue);
            allocatedValue += addonValues.Sum(x => x.AllocatedValue);
            estimatedValue += addonValues.Sum(x => x.EstimatedValue);
            factValue += addonValues.Sum(x => x.FactValue);
            _ws.Cells[_row, col].Value = Math.Round(requestedValue, Round);
            _ws.Cells[_row, col].Style.Font.Bold = true;
            _ws.Cells[_row, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
            _ws.Cells[_row, col].Style.Fill.BackgroundColor.SetColor(Color.LightCyan);
            SetBorder(_ws.Cells[_row, col]);
            _ws.Cells[_row, ++col].Value = Math.Round(allocatedValue, Round);
            _ws.Cells[_row, col].Style.Font.Bold = true;
            _ws.Cells[_row, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
            _ws.Cells[_row, col].Style.Fill.BackgroundColor.SetColor(Color.LightCyan);
            SetBorder(_ws.Cells[_row, col]);
            if (d == _finishDate)
            {
                _fp.Cells[8, 11].Formula = _ws.Cells[_row, col].FullAddress;
            }
            _ws.Cells[_row, ++col].Value = Math.Round(estimatedValue, Round);
            _ws.Cells[_row, col].Style.Font.Bold = true;
            _ws.Cells[_row, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
            _ws.Cells[_row, col].Style.Fill.BackgroundColor.SetColor(Color.LightCyan);
            SetBorder(_ws.Cells[_row, col]);
            _ws.Cells[_row, ++col].Value = Math.Round(factValue, Round);
            _ws.Cells[_row, col].Style.Font.Bold = true;
            _ws.Cells[_row, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
            _ws.Cells[_row, col].Style.Fill.BackgroundColor.SetColor(Color.LightCyan);
            SetBorder(_ws.Cells[_row, col]);
            col++;
            var grCell = _ws.Cells[_row, col];
            grCell.Style.Font.Bold = true;
            var monthCol = 0;
            if (d.Month == _finishDate.Month)
            {
                monthCol = StartCol + 2;
            }
            else
            {
                monthCol = StartCol + 1;
            }
            if (factValue > 0)
            {
                grCell.Formula = $"{_ws.Cells[_row, col - 1].Address}-{_ws.Cells[_row, monthCol]}";
            }
            else if (estimatedValue > 0)
            {
                grCell.Formula = $"{_ws.Cells[_row, col - 2].Address}-{_ws.Cells[_row, monthCol]}";
            }
            else if (allocatedValue > 0)
            {
                grCell.Formula = $"{_ws.Cells[_row, col - 3].Address}-{_ws.Cells[_row, monthCol]}";
            }
            else
            {
                _ws.Cells[_row, col].Value = 0;
            }
            col++;
        }
        _row++;
    }

    private void SetGisAndCountryRows()
    {
        var gises = _gisList
            .Where(x => !x.IsHidden && !x.IsTop && !x.IsBottom)
            .OrderBy(x => x.DailyReviewOrder);
        foreach (var gis in gises)
        {
            var cell = _ws.Cells[_row, StartCol];
            cell.Value = gis.DailyReviewName;
            cell.Style.Font.Bold = true;
            cell.Style.Font.Color.SetColor(Color.Red);
            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            SetBorder(cell);
            var col = StartCol + 3;
            if (gis.IsOneRow)
            {
                var firstResource = _forecast.Where(x => x.Month == _startDate.Month && x.GisCountry.GisId == gis.Id).Sum(x => x.Value);
                firstResource /= DateTime.DaysInMonth(_startDate.Year, _startDate.Month);
                var secondResource = _forecast.Where(x => x.Month == _finishDate.Month && x.GisCountry.GisId == gis.Id).Sum(x => x.Value);
                secondResource /= DateTime.DaysInMonth(_finishDate.Year, _finishDate.Month);
                _ws.Cells[_row, StartCol + 1].Value = Math.Round(firstResource, Round);
                _ws.Cells[_row, StartCol + 1].Style.Font.Bold = true;
                _ws.Cells[_row, StartCol + 1].Style.Font.Color.SetColor(Color.Red);
                _ws.Cells[_row, StartCol + 2].Value = Math.Round(secondResource, Round);
                _ws.Cells[_row, StartCol + 2].Style.Font.Bold = true;
                _ws.Cells[_row, StartCol + 2].Style.Font.Color.SetColor(Color.Red);
            }
            for (var d = _startDate; d <= _finishDate; d = d.AddDays(1))
            {
                decimal requestedValue = 0;
                decimal allocatedValue = 0;
                decimal estimatedValue = 0;
                decimal factValue = 0;
                requestedValue = _gisInputValues.Where(x => x.ReportDate == d && x.GisId == gis.Id).Sum(x => x.RequestedValue);
                requestedValue -= _gisOutputValues.Where(x => x.ReportDate == d && x.GisId == gis.Id).Sum(x => x.RequestedValue);
                requestedValue += _gisCountryValues.Where(x => x.ReportDate == d && !x.GisCountry.IsNotCalculated && x.GisCountry.GisId == gis.Id)
                    .Sum(x => x.RequestedValue);
                requestedValue += _gisAddonValues.Where(x => x.ReportDate == d && x.GisAddon.GisId == gis.Id && x.GisAddon.IsInput).Sum(x => x.RequestedValue);
                requestedValue -= _gisAddonValues.Where(x => x.ReportDate == d && x.GisAddon.GisId == gis.Id && x.GisAddon.IsOutput).Sum(x => x.RequestedValue);
                requestedValue += _gisCountryAddonValues.Where(x => x.ReportDate == d && x.GisCountryAddon.GisCountry.GisId == gis.Id).Sum(x => x.RequestedValue);
                allocatedValue = _gisInputValues.Where(x => x.ReportDate == d && x.GisId == gis.Id).Sum(x => x.AllocatedValue);
                allocatedValue -= _gisOutputValues.Where(x => x.ReportDate == d && x.GisId == gis.Id).Sum(x => x.AllocatedValue);
                allocatedValue += _gisCountryValues.Where(x => x.ReportDate == d && !x.GisCountry.IsNotCalculated && x.GisCountry.GisId == gis.Id)
                    .Sum(x => x.AllocatedValue);
                allocatedValue += _gisAddonValues.Where(x => x.ReportDate == d && x.GisAddon.GisId == gis.Id && x.GisAddon.IsInput).Sum(x => x.AllocatedValue);
                allocatedValue -= _gisAddonValues.Where(x => x.ReportDate == d && x.GisAddon.GisId == gis.Id && x.GisAddon.IsOutput).Sum(x => x.AllocatedValue);
                allocatedValue += _gisCountryAddonValues.Where(x => x.ReportDate == d && x.GisCountryAddon.GisCountry.GisId == gis.Id).Sum(x => x.AllocatedValue);
                estimatedValue = _gisInputValues.Where(x => x.ReportDate == d && x.GisId == gis.Id).Sum(x => x.EstimatedValue);
                estimatedValue -= _gisOutputValues.Where(x => x.ReportDate == d && x.GisId == gis.Id).Sum(x => x.EstimatedValue);
                estimatedValue += _gisCountryValues.Where(x => x.ReportDate == d && !x.GisCountry.IsNotCalculated && x.GisCountry.GisId == gis.Id)
                    .Sum(x => x.EstimatedValue);
                estimatedValue += _gisAddonValues.Where(x => x.ReportDate == d && x.GisAddon.GisId == gis.Id && x.GisAddon.IsInput).Sum(x => x.EstimatedValue);
                estimatedValue -= _gisAddonValues.Where(x => x.ReportDate == d && x.GisAddon.GisId == gis.Id && x.GisAddon.IsOutput).Sum(x => x.EstimatedValue);
                estimatedValue += _gisCountryAddonValues.Where(x => x.ReportDate == d && x.GisCountryAddon.GisCountry.GisId == gis.Id).Sum(x => x.EstimatedValue);
                factValue = _gisInputValues.Where(x => x.ReportDate == d && x.GisId == gis.Id).Sum(x => x.FactValue);
                factValue -= _gisOutputValues.Where(x => x.ReportDate == d && x.GisId == gis.Id).Sum(x => x.FactValue);
                factValue += _gisCountryValues.Where(x => x.ReportDate == d && !x.GisCountry.IsNotCalculated && x.GisCountry.GisId == gis.Id)
                    .Sum(x => x.FactValue);
                factValue += _gisAddonValues.Where(x => x.ReportDate == d && x.GisAddon.GisId == gis.Id && x.GisAddon.IsInput).Sum(x => x.FactValue);
                factValue -= _gisAddonValues.Where(x => x.ReportDate == d && x.GisAddon.GisId == gis.Id && x.GisAddon.IsOutput).Sum(x => x.FactValue);
                factValue += _gisCountryAddonValues.Where(x => x.ReportDate == d && x.GisCountryAddon.GisCountry.GisId == gis.Id).Sum(x => x.FactValue);
                _ws.Cells[_row, col].Value = Math.Round(requestedValue, Round);
                _ws.Cells[_row, col].Style.Font.Bold = true;
                if (gis.IsOneRow)
                {
                    _ws.Cells[_row, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    _ws.Cells[_row, col].Style.Fill.BackgroundColor.SetColor(Color.LightCyan);
                }
                SetBorder(_ws.Cells[_row, col]);
                _ws.Cells[_row, ++col].Value = Math.Round(allocatedValue, Round);
                _ws.Cells[_row, col].Style.Font.Bold = true;
                if (gis.IsOneRow)
                {
                    _ws.Cells[_row, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    _ws.Cells[_row, col].Style.Fill.BackgroundColor.SetColor(Color.LightCyan);
                }
                SetBorder(_ws.Cells[_row, col]);
                _ws.Cells[_row, ++col].Value = Math.Round(estimatedValue, Round);
                _ws.Cells[_row, col].Style.Font.Bold = true;
                if (gis.IsOneRow)
                {
                    _ws.Cells[_row, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    _ws.Cells[_row, col].Style.Fill.BackgroundColor.SetColor(Color.LightCyan);
                }
                SetBorder(_ws.Cells[_row, col]);
                _ws.Cells[_row, ++col].Value = Math.Round(factValue, Round);
                _ws.Cells[_row, col].Style.Font.Bold = true;
                if (gis.IsOneRow)
                {
                    _ws.Cells[_row, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    _ws.Cells[_row, col].Style.Fill.BackgroundColor.SetColor(Color.LightCyan);
                }
                SetBorder(_ws.Cells[_row, col]);
                col++;
                var grCell = _ws.Cells[_row, col];
                grCell.Style.Font.Bold = true;
                if (gis.IsOneRow)
                {
                    var monthCol = 0;
                    if (d.Month == _finishDate.Month)
                    {
                        monthCol = StartCol + 2;
                    }
                    else
                    {
                        monthCol = StartCol + 1;
                    }
                    if (factValue > 0)
                    {
                        grCell.Formula = $"{_ws.Cells[_row, col - 1].Address} - {_ws.Cells[_row, monthCol]}";
                    }
                    else if (estimatedValue > 0)
                    {
                        grCell.Formula = $"{_ws.Cells[_row, col - 2].Address} - {_ws.Cells[_row, monthCol]}";
                    }
                    else if (allocatedValue > 0)
                    {
                        grCell.Formula = $"{_ws.Cells[_row, col - 3].Address} - {_ws.Cells[_row, monthCol]}";
                    }
                    else
                    {
                        grCell.Value = 0;
                    }
                }
                col++;
            }
            _row++;
            if (!gis.IsNoPhg && !gis.IsOneRow)
            {
                SetGisInputRow(gis.Id);
                SetGisOutputRow(gis.Id);
                SetGisAddonRows(gis.Id);
            }

            if (gis.IsOneRow) continue;
            SetGisGasRow(gis.Id);
            foreach (var gc in gis.Countries)
            {
                SetGisCountryRow(gc);
            }
        }
    }

    private void SetGisInputRow(int gisId)
    {
        var cell = _ws.Cells[_row, StartCol];
        cell.Value = "Закачка в ПХГ";
        cell.Style.Font.Bold = true;
        cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
        SetBorder(cell);
        var values = _gisInputValues
            .Where(x => x.GisId == gisId)
            .ToList();
        var sum = values.Sum(x => x.RequestedValue);
        sum += values.Sum(x => x.AllocatedValue);
        sum += values.Sum(x => x.EstimatedValue);
        sum += values.Sum(x => x.FactValue);
        var col = StartCol + 3;
        for (DateOnly d = _startDate; d <= _finishDate; d = d.AddDays(1))
        {
            decimal requestedValue = 0;
            decimal allocatedValue = 0;
            decimal estimatedValue = 0;
            decimal factValue = 0;
            if (sum > 0)
            {
                requestedValue = values.Where(x => x.ReportDate == d).Sum(x => x.RequestedValue);
                allocatedValue = values.Where(x => x.ReportDate == d).Sum(x => x.AllocatedValue);
                estimatedValue = values.Where(x => x.ReportDate == d).Sum(x => x.EstimatedValue);
                factValue = values.Where(x => x.ReportDate == d).Sum(x => x.FactValue);
            }
            _ws.Cells[_row, col].Value = Math.Round(requestedValue, Round);
            _ws.Cells[_row, col].Style.Font.Bold = true;
            _ws.Cells[_row, ++col].Value = Math.Round(allocatedValue, Round);
            _ws.Cells[_row, col].Style.Font.Bold = true;
            _ws.Cells[_row, ++col].Value = Math.Round(estimatedValue, Round);
            _ws.Cells[_row, col].Style.Font.Bold = true;
            _ws.Cells[_row, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
            _ws.Cells[_row, col].Style.Fill.BackgroundColor.SetColor(Color.LightGreen);
            _ws.Cells[_row, ++col].Value = Math.Round(factValue, Round);
            _ws.Cells[_row, col].Style.Font.Bold = true;
            _ws.Cells[_row, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
            _ws.Cells[_row, col].Style.Fill.BackgroundColor.SetColor(Color.LightYellow);
            _ws.Cells[_row, ++col].Value = 0;
            col++;
        }
        if (sum == 0)
        {
            _ws.Row(_row).OutlineLevel = 1;
            _ws.Row(_row).Collapsed = true;
        }
        _row++;
    }

    private void SetGisAddonRows(int gisId)
    {
        var gis = _gisList.FirstOrDefault(x => x.Id == gisId);
        if (gis == null) return;
        var addons = gis.Addons;
        foreach (var addon in addons)
        {
            if (addon.IsHidden) continue;
            var cell = _ws.Cells[_row, StartCol];
            cell.Value = addon.DailyReviewName;
            cell.Style.Font.Bold = true;
            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
            SetBorder(cell);
            var values = _gisAddonValues
                .Where(x => x.GisAddonId == addon.Id)
                .ToList();
            var sum = values.Sum(x => x.RequestedValue);
            sum += values.Sum(x => x.AllocatedValue);
            sum += values.Sum(x => x.EstimatedValue);
            sum += values.Sum(x => x.FactValue);
            var col = StartCol + 3;
            for (var d = _startDate; d <= _finishDate; d = d.AddDays(1))
            {
                decimal requestedValue = 0;
                decimal allocatedValue = 0;
                decimal estimatedValue = 0;
                decimal factValue = 0;
                if (sum > 0)
                {
                    requestedValue = values.Where(x => x.ReportDate == d).Sum(x => x.RequestedValue);
                    allocatedValue = values.Where(x => x.ReportDate == d).Sum(x => x.AllocatedValue);
                    estimatedValue = values.Where(x => x.ReportDate == d).Sum(x => x.EstimatedValue);
                    factValue = values.Where(x => x.ReportDate == d).Sum(x => x.FactValue);
                }
                _ws.Cells[_row, col].Value = Math.Round(requestedValue, Round);
                _ws.Cells[_row, col].Style.Font.Bold = true;
                _ws.Cells[_row, ++col].Value = Math.Round(allocatedValue, Round);
                _ws.Cells[_row, col].Style.Font.Bold = true;
                _ws.Cells[_row, ++col].Value = Math.Round(estimatedValue, Round);
                _ws.Cells[_row, col].Style.Font.Bold = true;
                _ws.Cells[_row, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
                _ws.Cells[_row, col].Style.Fill.BackgroundColor.SetColor(Color.LightGreen);
                _ws.Cells[_row, ++col].Value = Math.Round(factValue, Round);
                _ws.Cells[_row, col].Style.Font.Bold = true;
                _ws.Cells[_row, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
                _ws.Cells[_row, col].Style.Fill.BackgroundColor.SetColor(Color.LightYellow);
                _ws.Cells[_row, ++col].Value = 0;
                col++;
            }
            if (sum == 0)
            {
                _ws.Row(_row).OutlineLevel = 1;
                _ws.Row(_row).Collapsed = true;
            }
            _row++;
        }
    }

    private void SetGisOutputRow(int gisId)
    {
        var cell = _ws.Cells[_row, StartCol];
        cell.Value = "Отбор из ПХГ";
        cell.Style.Font.Bold = true;
        cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
        SetBorder(cell);
        var values = _gisOutputValues
            .Where(x => x.GisId == gisId)
            .ToList();
        var sum = values.Sum(x => x.RequestedValue);
        sum += values.Sum(x => x.AllocatedValue);
        sum += values.Sum(x => x.EstimatedValue);
        sum += values.Sum(x => x.FactValue);
        var col = StartCol + 3;
        for (var d = _startDate; d <= _finishDate; d = d.AddDays(1))
        {
            decimal requestedValue = 0;
            decimal allocatedValue = 0;
            decimal estimatedValue = 0;
            decimal factValue = 0;
            if (sum > 0)
            {
                requestedValue = values.Where(x => x.ReportDate == d).Sum(x => x.RequestedValue);
                allocatedValue = values.Where(x => x.ReportDate == d).Sum(x => x.AllocatedValue);
                estimatedValue = values.Where(x => x.ReportDate == d).Sum(x => x.EstimatedValue);
                factValue = values.Where(x => x.ReportDate == d).Sum(x => x.FactValue);
            }
            _ws.Cells[_row, col].Value = Math.Round(requestedValue, Round);
            _ws.Cells[_row, col].Style.Font.Bold = true;
            _ws.Cells[_row, ++col].Value = Math.Round(allocatedValue, Round);
            _ws.Cells[_row, col].Style.Font.Bold = true;
            _ws.Cells[_row, ++col].Value = Math.Round(estimatedValue, Round);
            _ws.Cells[_row, col].Style.Font.Bold = true;
            _ws.Cells[_row, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
            _ws.Cells[_row, col].Style.Fill.BackgroundColor.SetColor(Color.LightGreen);
            _ws.Cells[_row, ++col].Value = Math.Round(factValue, Round);
            _ws.Cells[_row, col].Style.Font.Bold = true;
            _ws.Cells[_row, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
            _ws.Cells[_row, col].Style.Fill.BackgroundColor.SetColor(Color.LightYellow);
            _ws.Cells[_row, ++col].Value = 0;
            col++;
        }

        if (sum == 0)
        {
            _ws.Row(_row).OutlineLevel = 1;
            _ws.Row(_row).Collapsed = true;
        }
        _row++;
    }

    private void SetGisGasRow(int gisId)
    {
        var cell = _ws.Cells[_row, StartCol];
        cell.Value = "ТОВАРНЫЙ ГАЗ";
        cell.Style.Font.Bold = true;
        cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
        SetBorder(cell);
        var col = StartCol + 1;
        var firstResource = _forecast.Where(x => x.Month == (_startDate.Month - 1) && x.GisCountry.GisId == gisId).Sum(x => x.Value);
        firstResource /= DateTime.DaysInMonth(_startDate.Year, _startDate.Month);
        var secondResource = _forecast.Where(x => x.Month == _finishDate.Month && x.GisCountry.GisId == gisId).Sum(x => x.Value);
        secondResource /= DateTime.DaysInMonth(_finishDate.Year, _finishDate.Month);
        _ws.Cells[_row, col].Value = Math.Round(firstResource, Round);
        _ws.Cells[_row, col].Style.Font.Bold = true;
        _ws.Cells[_row, col].Style.Font.Color.SetColor(Color.Red);
        col++;
        _ws.Cells[_row, col].Value = Math.Round(secondResource, Round);
        _ws.Cells[_row, col].Style.Font.Bold = true;
        _ws.Cells[_row, col].Style.Font.Color.SetColor(Color.Red);
        var values = _gisCountryValues
            .Where(x => !x.GisCountry.IsNotCalculated && x.GisCountry.GisId == gisId)
            .ToList();
        var addonValues = _gisCountryAddonValues
            .Where(x => x.GisCountryAddon.GisCountry.GisId == gisId)
            .ToList();
        var sum = values.Sum(x => x.RequestedValue);
        sum += values.Sum(x => x.AllocatedValue);
        sum += values.Sum(x => x.EstimatedValue);
        sum += values.Sum(x => x.FactValue);
        sum += addonValues.Sum(x => x.RequestedValue);
        sum += addonValues.Sum(x => x.AllocatedValue);
        sum += addonValues.Sum(x => x.EstimatedValue);
        sum += addonValues.Sum(x => x.FactValue);
        col++;
        for (var d = _startDate; d <= _finishDate; d = d.AddDays(1))
        {
            decimal requestedValue = 0;
            decimal allocatedValue = 0;
            decimal estimatedValue = 0;
            decimal factValue = 0;
            if (sum > 0)
            {
                requestedValue += values.Where(x => x.ReportDate == d).Sum(x => x.RequestedValue);
                allocatedValue += values.Where(x => x.ReportDate == d).Sum(x => x.AllocatedValue);
                estimatedValue += values.Where(x => x.ReportDate == d).Sum(x => x.EstimatedValue);
                factValue += values.Where(x => x.ReportDate == d).Sum(x => x.FactValue);
                requestedValue += addonValues.Where(x => x.ReportDate == d).Sum(x => x.RequestedValue);
                allocatedValue += addonValues.Where(x => x.ReportDate == d).Sum(x => x.AllocatedValue);
                estimatedValue += addonValues.Where(x => x.ReportDate == d).Sum(x => x.EstimatedValue);
                factValue += addonValues.Where(x => x.ReportDate == d).Sum(x => x.FactValue);
            }
            _ws.Cells[_row, col].Value = Math.Round(requestedValue, Round);
            _ws.Cells[_row, col].Style.Font.Bold = true;
            _ws.Cells[_row, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
            _ws.Cells[_row, col].Style.Fill.BackgroundColor.SetColor(Color.LightCyan);
            SetBorder(_ws.Cells[_row, col]);
            _ws.Cells[_row, ++col].Value = Math.Round(allocatedValue, Round);
            _ws.Cells[_row, col].Style.Font.Bold = true;
            _ws.Cells[_row, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
            _ws.Cells[_row, col].Style.Fill.BackgroundColor.SetColor(Color.LightCyan);
            SetBorder(_ws.Cells[_row, col]);
            _ws.Cells[_row, ++col].Value = Math.Round(estimatedValue, Round);
            _ws.Cells[_row, col].Style.Font.Bold = true;
            _ws.Cells[_row, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
            _ws.Cells[_row, col].Style.Fill.BackgroundColor.SetColor(Color.LightCyan);
            SetBorder(_ws.Cells[_row, col]);
            _ws.Cells[_row, ++col].Value = Math.Round(factValue, Round);
            _ws.Cells[_row, col].Style.Font.Bold = true;
            _ws.Cells[_row, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
            _ws.Cells[_row, col].Style.Fill.BackgroundColor.SetColor(Color.LightCyan);
            SetBorder(_ws.Cells[_row, col]);
            col++;
            var grCell = _ws.Cells[_row, col];
            grCell.Style.Font.Bold = true;
            var monthCol = 0;
            if (d.Month == _finishDate.Month)
            {
                monthCol = StartCol + 2;
            }
            else
            {
                monthCol = StartCol + 1;
            }
            if (factValue > 0)
            {
                grCell.Formula = $"{_ws.Cells[_row, col - 1].Address} - {_ws.Cells[_row, monthCol]}";
            }
            else if (estimatedValue > 0)
            {
                grCell.Formula = $"{_ws.Cells[_row, col - 2].Address} - {_ws.Cells[_row, monthCol]}";
            }
            else if (allocatedValue > 0)
            {
                grCell.Formula = $"{_ws.Cells[_row, col - 3].Address} - {_ws.Cells[_row, monthCol]}";
            }
            else
            {
                _ws.Cells[_row, col].Value = 0;
            }
            col++;
        }
        if (sum == 0)
        {
            _ws.Row(_row).OutlineLevel = 1;
            _ws.Row(_row).Collapsed = true;
        }
        _row++;
    }

    private void SetGisCountryRow(GisCountry gisCountry)
    {
        var cell = _ws.Cells[_row, StartCol];
        if (gisCountry.IsHidden) return;
        cell.Value = gisCountry.Country.DailyReviewName;
        cell.Style.Font.Bold = true;
        cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
        SetBorder(cell);
        var col = StartCol + 1;
        var firstResource = _forecast
            .Where(x => x.Month == _startDate.Month && x.GisCountryId == gisCountry.Id)
            .Sum(x => x.Value);
        firstResource /= DateTime.DaysInMonth(_startDate.Year, _startDate.Month);
        var secondResource = _forecast
            .Where(x => x.Month == _finishDate.Month && x.GisCountryId == gisCountry.Id)
            .Sum(x => x.Value);
        secondResource /= DateTime.DaysInMonth(_finishDate.Year, _finishDate.Month);
        _ws.Cells[_row, col].Value = Math.Round(firstResource, Round);
        _ws.Cells[_row, col].Style.Font.Bold = true;
        _ws.Cells[_row, col].Style.Font.Color.SetColor(Color.Red);
        col++;
        _ws.Cells[_row, col].Value = Math.Round(secondResource, Round);
        _ws.Cells[_row, col].Style.Font.Bold = true;
        _ws.Cells[_row, col].Style.Font.Color.SetColor(Color.Red);
        var values = _gisCountryValues
            .Where(x => x.GisCountryId == gisCountry.Id)
            .ToList();
        var sum = values.Sum(x => x.RequestedValue);
        sum += values.Sum(x => x.AllocatedValue);
        sum += values.Sum(x => x.EstimatedValue);
        sum += values.Sum(x => x.FactValue);
        col++;
        for (var d = _startDate; d <= _finishDate; d = d.AddDays(1))
        {
            decimal requestedValue = 0;
            decimal allocatedValue = 0;
            decimal estimatedValue = 0;
            decimal factValue = 0;
            if (sum > 0)
            {
                requestedValue = values.Where(x => x.ReportDate == d).Sum(x => x.RequestedValue);
                allocatedValue = values.Where(x => x.ReportDate == d).Sum(x => x.AllocatedValue);
                estimatedValue = values.Where(x => x.ReportDate == d).Sum(x => x.EstimatedValue);
                factValue = values.Where(x => x.ReportDate == d).Sum(x => x.FactValue);
            }

            _ws.Cells[_row, col].Value = Math.Round(requestedValue, Round);
            _ws.Cells[_row, col].Style.Font.Bold = true;
            _ws.Cells[_row, ++col].Value = Math.Round(allocatedValue, Round);
            _ws.Cells[_row, col].Style.Font.Bold = true;
            _ws.Cells[_row, ++col].Value = Math.Round(estimatedValue, Round);
            _ws.Cells[_row, col].Style.Font.Bold = true;
            _ws.Cells[_row, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
            _ws.Cells[_row, col].Style.Fill.BackgroundColor.SetColor(Color.LightGreen);
            _ws.Cells[_row, ++col].Value = Math.Round(factValue, Round);
            _ws.Cells[_row, col].Style.Font.Bold = true;
            _ws.Cells[_row, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
            _ws.Cells[_row, col].Style.Fill.BackgroundColor.SetColor(Color.LightYellow);
            _ws.Cells[_row, ++col].Value = 0;
            col++;
        }
        if (sum == 0)
        {
            _ws.Row(_row).OutlineLevel = 1;
            _ws.Row(_row).Collapsed = true;
        }
        _row++;
        if (gisCountry.Addons is not { Count: > 0 }) return;
        foreach (var addon in gisCountry.Addons)
        {
            SetGisCountryAddonRow(addon);
        }
    }

    private void SetGisCountryAddonRow(GisCountryAddon addon)
    {
        if (addon == null) return;
        var cell = _ws.Cells[_row, StartCol];
        cell.Value = addon.DailyReviewName;
        cell.Style.Font.Bold = true;
        cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
        SetBorder(cell);
        var col = StartCol + 1;
        col++;
        var values = _gisCountryAddonValues
            .Where(x => x.GisCountryAddonId == addon.Id)
            .ToList();
        var sum = values.Sum(x => x.RequestedValue);
        sum += values.Sum(x => x.AllocatedValue);
        sum += values.Sum(x => x.EstimatedValue);
        sum += values.Sum(x => x.FactValue);
        col++;
        for (var d = _startDate; d <= _finishDate; d = d.AddDays(1))
        {
            decimal requestedValue = 0;
            decimal allocatedValue = 0;
            decimal estimatedValue = 0;
            decimal factValue = 0;
            if (sum > 0)
            {
                requestedValue = values.Where(x => x.ReportDate == d).Sum(x => x.RequestedValue);
                allocatedValue = values.Where(x => x.ReportDate == d).Sum(x => x.AllocatedValue);
                estimatedValue = values.Where(x => x.ReportDate == d).Sum(x => x.EstimatedValue);
                factValue = values.Where(x => x.ReportDate == d).Sum(x => x.FactValue);
            }
            _ws.Cells[_row, col].Value = Math.Round(requestedValue, Round);
            _ws.Cells[_row, col].Style.Font.Bold = true;
            _ws.Cells[_row, ++col].Value = Math.Round(allocatedValue, Round);
            _ws.Cells[_row, col].Style.Font.Bold = true;
            _ws.Cells[_row, ++col].Value = Math.Round(estimatedValue, Round);
            _ws.Cells[_row, col].Style.Font.Bold = true;
            _ws.Cells[_row, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
            _ws.Cells[_row, col].Style.Fill.BackgroundColor.SetColor(Color.LightGreen);
            _ws.Cells[_row, ++col].Value = Math.Round(factValue, Round);
            _ws.Cells[_row, col].Style.Font.Bold = true;
            _ws.Cells[_row, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
            _ws.Cells[_row, col].Style.Fill.BackgroundColor.SetColor(Color.LightYellow);
            _ws.Cells[_row, ++col].Value = 0;
            col++;
        }
        if (sum == 0)
        {
            _ws.Row(_row).OutlineLevel = 1;
            _ws.Row(_row).Collapsed = true;
        }
        _row++;
    }

    private static void SetBorder(ExcelRangeBase cell)
    {
        cell.Style.Border.Top.Style = ExcelBorderStyle.Thin;
        cell.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
        cell.Style.Border.Left.Style = ExcelBorderStyle.Thin;
        cell.Style.Border.Right.Style = ExcelBorderStyle.Thin;
    }
}