using Microsoft.AspNetCore.Components.Forms;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;
using Radzen;
using SSLD.Data.DailyReview;
using SSLD.Tools;

namespace SSLD.Parsers;

public class ExcelAvtParser
{
    private readonly NotificationService _notificationService;
    private readonly IBrowserFile _file;
    private ISheet _sheet;
    private string _filename;
    private DateOnly _reportDate;
    private readonly IEnumerable<Gis> _gisList;
    private Gis _gisTurkey;
    private Gis _gisBlue;
    private readonly List<ReviewValueInput> _valueList = new();
    private readonly FileTypeSetting _settings;
    private CellRangeAddress _dateCell;
    private string _message;

    public ExcelAvtParser(IBrowserFile file, IEnumerable<Gis> gisList, FileTypeSetting settings,
        NotificationService notificationService)
    {
        _file = file;
        _gisList = gisList;
        _settings = settings;
        _notificationService = notificationService;
    }

    public async Task<List<ReviewValueInput>> GetResult()
    {
        await Parse();
        return _valueList;
    }

    public string GetMessage()
    {
        return _message;
    }

    private async Task Parse()
    {
        _filename = _file.Name;
        var reportDate = StringParser.GetFirstDateOnlyFromString(_filename);
        if (reportDate == null)
        {
            _message = "В результате парсинга файла " + _filename + " не удалось установить дату";
            return;
        }
        _reportDate = reportDate.Value;
        var startMonth = new DateOnly(_reportDate.Year, _reportDate.Month, 1);
        //берем Турецкий поток
        _gisTurkey = _gisList.FirstOrDefault(x => x.Names.Any(n => n.ToLower().Equals("турецкий поток")));
        if (_gisTurkey == null)
        {
            _message = "В БД не обнаружен Турецкий поток";
            return;
        }

        //голубой поток - только Турция
        _gisBlue = _gisList.FirstOrDefault(x => x.Names.Any(n => n.ToLower().Equals("голубой поток")));
        if (_gisBlue == null)
        {
            _message = "В БД не обнаружен Голубой поток";
            return;
        }

        var ms = new MemoryStream();
        var stream = _file.OpenReadStream(_file.Size);
        await stream.CopyToAsync(ms);
        stream.Close();
        ms.Position = 0;
        var xssWorkbook = new XSSFWorkbook(ms);
        _sheet = xssWorkbook.GetSheetAt(0);
        AvtFindDateEntry();
        if (_dateCell == null)
        {
            _message = "не удалось установить координаты поля \"Дата\"";
            return;
        }

        var r = _dateCell.FirstRow;
        var row = _sheet.GetRow(r);
        for (var col = _dateCell.LastColumn + 1; col <= row.LastCellNum; col++)
        {
            var cell = row.GetCell(col);
            var firstRange = GetMergedRegionContainingCell(cell);
            if (firstRange == null)
            {
                ParseFirstRange(cell);
            }
            else
            {
                ParseFirstRange(firstRange);
                col = firstRange.LastColumn;
            }
        }

        xssWorkbook.Close();
        await ms.DisposeAsync();
    }

    private string GetCellRangeAddressText(CellRangeAddressBase range)
    {
        var r = range.FirstRow;
        var row = _sheet.GetRow(r);
        var firstCell = row?.GetCell(range.FirstColumn);
        var text = firstCell?.StringCellValue;
        return text;
    }

    private void ParseFirstRange(CellRangeAddressBase firstRange)
    {
        var r = firstRange.FirstRow;
        var row = _sheet.GetRow(r);
        var firstCell = row?.GetCell(firstRange.FirstColumn);
        if (firstCell == null) return;
        var firstRangeText = firstCell.StringCellValue;
        //листаем диапазон справа на лево
        for (var col = firstRange.LastColumn; col >= firstRange.FirstColumn; col--)
        {
            var secondCell = _sheet.GetRow(r + 1).GetCell(col);
            var secondRange = GetMergedRegionContainingCell(secondCell);
            var secondRangeText = "";
            secondRangeText = secondRange != null
                ? GetCellRangeAddressText(secondRange)
                : secondCell.StringCellValue;
            if (!string.IsNullOrEmpty(secondRangeText))
            {
                if (StringParser.StrictLike(secondRangeText, "Итого"))
                {
                    //оказалось значение "Итого", парсим весь столбец и выходим из диапазона
                    ParseColumn("турецкий поток", firstRangeText, col);
                    break;
                }

                if (StringParser.StrictLike(secondRangeText, "Топл. газ"))
                {
                    //оказалось значение "Итого", парсим весь столбец и выходим из диапазона
                    ParseColumn("турецкий поток", firstRangeText, col);
                    break;
                }

                col = secondRange != null ? (short)secondRange.FirstColumn : (short)secondCell.ColumnIndex;
                var gis = _gisList.FirstOrDefault(
                    x => x.Names.Any(n => StringParser.StrictLike(n, secondRangeText)));
                if (gis == null)
                {
                    var gisAddon = _gisTurkey.Addons.FirstOrDefault(x => x.Names.Any(n => n == firstRangeText));
                    if (gisAddon == null) continue;
                    ParseGisAddon(firstRange, gisAddon);
                }
                else
                {
                    if (secondRange != null)
                    {
                        ParseSecondRange(firstRange, secondRange);
                    }
                    else
                    {
                        ParseSecondRange(firstRange, secondCell);
                    }
                }
            }
            else
            {
                var gisAddon = _gisTurkey.Addons.FirstOrDefault(x => x.Names.Any(n => n == firstRangeText));
                if (gisAddon == null) continue;
                ParseGisAddon(firstRange, gisAddon);
                if (secondRange != null) col = (short)secondRange.FirstColumn;
            }
        }
    }

    private void ParseFirstRange(ICell firstCell)
    {
        if (firstCell == null) return;
        var col = firstCell.ColumnIndex;
        var firstRangeText = firstCell.StringCellValue;
        var secondCell = _sheet.GetRow(firstCell.RowIndex + 1).GetCell(col);
        var secondRange = GetMergedRegionContainingCell(secondCell);
        var secondRangeText = "";
        secondRangeText = secondRange != null ? GetCellRangeAddressText(secondRange) : secondCell.StringCellValue;
        if (!string.IsNullOrEmpty(secondRangeText))
        {
            if (StringParser.StrictLike(secondRangeText, "Итого"))
            {
                //оказалось значение "Итого", парсим весь столбец и выходим из диапазона
                ParseColumn("турецкий поток", firstRangeText, col);
                return;
            }
            if (StringParser.StrictLike(secondRangeText, "Топл. газ"))
            {
                //оказалось значение "Итого", парсим весь столбец и выходим из диапазона
                ParseColumn("турецкий поток", firstRangeText, col);
                return;
            }
            var gis = _gisList.FirstOrDefault(x => x.Names.Any(n => StringParser.StrictLike(n, secondRangeText)));
            if (gis == null)
            {
                var gisAddon = _gisTurkey.Addons.FirstOrDefault(x => x.Names.Any(n => n == firstRangeText));
                if (gisAddon == null) return;
                ParseGisAddon(firstCell, gisAddon);
            }
            else
            {
                if (secondRange != null)
                {
                    ParseSecondRange(firstCell, secondRange);
                }
                else
                {
                    ParseSecondRange(firstCell, secondCell);
                }
            }
        }
        else
        {
            var gisAddon = _gisTurkey.Addons.FirstOrDefault(x => x.Names.Any(n => n == firstRangeText));
            if (gisAddon == null) return;
            ParseGisAddon(firstCell, gisAddon);
        }
    }

    private void ParseSecondRange(CellRangeAddressBase firstRange, ICell secondRange)
    {
        var r = firstRange.FirstRow;
        var col = secondRange.ColumnIndex;
        //проверяем наличие Qсутки
        var row = _sheet.GetRow(secondRange.RowIndex + 1);
        var bottomCell = row.GetCell(col);
        var bottomCellText = bottomCell.StringCellValue;
        if (StringParser.NameContainAnyList(_settings.FactValueEntry, bottomCellText))
        {
            //в строке оказались Qсутки, значит персим весь столбец
            ParseColumn(secondRange.StringCellValue, GetCellRangeAddressText(firstRange), col);
            return;
        }

        //проверяем не аддон ли это
        var gis = _gisList.FirstOrDefault(x =>
            x.Names.Any(n => StringParser.StrictLike(n, secondRange.StringCellValue)));
        var country = gis?.Countries.FirstOrDefault(x =>
            x.Country.Names.Any(n => StringParser.StrictLike(n, GetCellRangeAddressText(firstRange))));
        if (country == null) return;
        var addon = country.Addons.FirstOrDefault(x =>
            x.Names.Any(n => StringParser.StrictLike(n, bottomCellText)));
        var lastRow = _sheet.GetRow(secondRange.RowIndex + 2).GetCell(col);
        var lastRowText = lastRow.StringCellValue;
        if (addon != null)
        {
            if (StringParser.NameContainAnyList(_settings.FactValueEntry, lastRowText))
            {
                ParseColumn(secondRange.StringCellValue, GetCellRangeAddressText(firstRange), col, addon.Id);
            }
        }
        else if (bottomCellText.ToLower().Contains("экспорт"))
        {
            //добавляем к стране если ниже строка Qсутки
            if (StringParser.NameContainAnyList(_settings.FactValueEntry, lastRowText))
            {
                ParseColumn(secondRange.StringCellValue, GetCellRangeAddressText(firstRange), col);
            }
        }
    }

    private void ParseSecondRange(CellRangeAddressBase firstRange, CellRangeAddressBase secondRange)
    {
        var r = firstRange.FirstRow;
        for (var col = secondRange.LastColumn; col >= secondRange.FirstColumn; col--)
        {
            //проверяем наличие Qсутки
            var row = _sheet.GetRow(secondRange.LastRow + 1);
            var bottomCell = row.GetCell(col);
            var bottomCellText = bottomCell.StringCellValue;
            if (StringParser.NameContainAnyList(_settings.FactValueEntry, bottomCellText))
            {
                //в строке оказались Qсутки, значит персим весь столбец
                ParseColumn(GetCellRangeAddressText(secondRange), GetCellRangeAddressText(firstRange), col);
                break;
            }

            //проверяем не аддон ли это
            var gis = _gisList.FirstOrDefault(x =>
                x.Names.Any(n => StringParser.StrictLike(n, GetCellRangeAddressText(secondRange))));
            var country = gis?.Countries.FirstOrDefault(x =>
                x.Country.Names.Any(n => StringParser.StrictLike(n, GetCellRangeAddressText(firstRange))));
            if (country == null) continue;
            var addon = country.Addons.FirstOrDefault(x =>
                x.Names.Any(n => StringParser.StrictLike(n, bottomCellText)));
            var lastRow = _sheet.GetRow(secondRange.LastRow + 2).GetCell(col);
            var lastRowText = lastRow.StringCellValue;
            if (addon != null)
            {
                if (StringParser.NameContainAnyList(_settings.FactValueEntry, lastRowText))
                {
                    ParseColumn(GetCellRangeAddressText(secondRange), GetCellRangeAddressText(firstRange), col,
                        addon.Id);
                }
            }
            else if (bottomCellText.ToLower().Contains("экспорт") || bottomCellText.ToLower().Contains("итого"))
            {
                //добавляем к стране если ниже строка Qсутки
                if (StringParser.NameContainAnyList(_settings.FactValueEntry, lastRowText))
                {
                    ParseColumn(GetCellRangeAddressText(secondRange), GetCellRangeAddressText(firstRange), col);
                }
            }
        }
    }

    private void ParseSecondRange(ICell firstCell, ICell secondRange)
    {
        var col = secondRange.ColumnIndex;
        //проверяем наличие Qсутки
        var row = _sheet.GetRow(secondRange.RowIndex + 1);
        var bottomCell = row.GetCell(col);
        var bottomCellText = bottomCell.StringCellValue;
        if (StringParser.NameContainAnyList(_settings.FactValueEntry, bottomCellText))
        {
            //в строке оказались Qсутки, значит персим весь столбец
            ParseColumn(secondRange.StringCellValue, firstCell.StringCellValue, col);
            return;
        }

        //проверяем не аддон ли это
        var gis = _gisList.FirstOrDefault(x =>
            x.Names.Any(n => StringParser.StrictLike(n, secondRange.StringCellValue)));
        var country = gis?.Countries.FirstOrDefault(x =>
            x.Country.Names.Any(n => StringParser.StrictLike(n, firstCell.StringCellValue)));
        if (country == null) return;
        var addon = country.Addons.FirstOrDefault(x =>
            x.Names.Any(n => StringParser.StrictLike(n, bottomCellText)));
        var lastRow = _sheet.GetRow(secondRange.RowIndex + 2).GetCell(col);
        var lastRowText = lastRow.StringCellValue;
        if (addon != null)
        {
            if (StringParser.NameContainAnyList(_settings.FactValueEntry, lastRowText))
            {
                ParseColumn(secondRange.StringCellValue, firstCell.StringCellValue, col, addon.Id);
            }
        }
        else if (bottomCellText.ToLower().Contains("экспорт"))
        {
            //добавляем к стране если ниже строка Qсутки
            if (StringParser.NameContainAnyList(_settings.FactValueEntry, lastRowText))
            {
                ParseColumn(secondRange.StringCellValue, firstCell.StringCellValue, col);
            }
        }
    }

    private void ParseSecondRange(ICell firstCell, CellRangeAddressBase secondRange)
    {
        for (var col = secondRange.LastColumn; col >= secondRange.FirstColumn; col--)
        {
            //проверяем наличие Qсутки
            var row = _sheet.GetRow(secondRange.LastRow + 1);
            var bottomCell = row.GetCell(col);
            var bottomCellText = bottomCell.StringCellValue;
            if (StringParser.NameContainAnyList(_settings.FactValueEntry, bottomCellText))
            {
                //в строке оказались Qсутки, значит персим весь столбец
                ParseColumn(GetCellRangeAddressText(secondRange), firstCell.StringCellValue, col);
                break;
            }

            //проверяем не аддон ли это
            var gis = _gisList.FirstOrDefault(x =>
                x.Names.Any(n => StringParser.StrictLike(n, GetCellRangeAddressText(secondRange))));
            var country = gis?.Countries.FirstOrDefault(x =>
                x.Country.Names.Any(n => StringParser.StrictLike(n, firstCell.StringCellValue)));
            if (country == null) continue;
            var addon = country.Addons.FirstOrDefault(x =>
                x.Names.Any(n => StringParser.StrictLike(n, bottomCellText)));
            var lastRow = _sheet.GetRow(secondRange.LastRow + 2).GetCell(col);
            var lastRowText = lastRow.StringCellValue;
            if (addon != null)
            {
                if (StringParser.NameContainAnyList(_settings.FactValueEntry, lastRowText))
                {
                    ParseColumn(GetCellRangeAddressText(secondRange), firstCell.StringCellValue, col,
                        addon.Id);
                }
            }
            else if (bottomCellText.ToLower().Contains("экспорт") || bottomCellText.ToLower().Contains("итого"))
            {
                //добавляем к стране если ниже строка Qсутки
                if (StringParser.NameContainAnyList(_settings.FactValueEntry, lastRowText))
                {
                    ParseColumn(GetCellRangeAddressText(secondRange), firstCell.StringCellValue, col);
                }
            }
        }
    }

    private void ParseGisAddon(CellRangeAddressBase firstRange, GisAddon gisAddon)
    {
        var col = firstRange.LastColumn;
        var lastRow = _sheet.GetRow(_dateCell.FirstRow + 2);
        var lastRowText = lastRow.GetCell(firstRange.LastColumn).StringCellValue;
        if (!StringParser.NameContainAnyList(_settings.FactValueEntry, lastRowText))
        {
            return;
        }

        for (var i = _dateCell.LastRow + 1; i <= _dateCell.LastRow + 1 + 31; i++)
        {
            try
            {
                var row = _sheet.GetRow(i);
                var dayCell = row.GetCell(_dateCell.FirstColumn);
                if (!DateUtil.IsCellDateFormatted(dayCell)) break;
                var dateTime = dayCell.DateCellValue;
                var date = DateOnly.FromDateTime(dateTime);
                if (date > _reportDate)
                {
                    break;
                }

                var valueCell = row.GetCell(col);
                double value = 0;
                if (valueCell.CellType == CellType.Numeric)
                {
                    value = valueCell.NumericCellValue;
                    value /= 1000d;
                }

                var existed = _valueList.FirstOrDefault(x => x.GisId == gisAddon.GisId &&
                                                             x.ValueId == gisAddon.Id &&
                                                             x.InType == ReviewValueInput.InputType.Addon &&
                                                             x.ReportDate == date);
                if (existed == null)
                {
                    _valueList.Add(new ReviewValueInput()
                    {
                        GisId = gisAddon.GisId,
                        ValueId = gisAddon.Id,
                        ReportDate = date,
                        InType = ReviewValueInput.InputType.Addon,
                        ValType = ReviewValueInput.ValueType.Fact,
                        Value = value
                    });
                }
                else
                {
                    existed.Value += value;
                }
            }
            catch (Exception ex)
            {
                _notificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Error,
                    Summary = "Ошибка с ГПШ",
                    Detail = ex.Message,
                    Duration = 3000
                });
            }
        }
    }

    private void ParseGisAddon(ICell firstCell, GisAddon gisAddon)
    {
        var col = firstCell.ColumnIndex;
        var lastRow = _sheet.GetRow(_dateCell.FirstRow + 2);
        var lastRowText = lastRow.GetCell(col).StringCellValue;
        if (!StringParser.NameContainAnyList(_settings.FactValueEntry, lastRowText))
        {
            return;
        }

        for (var i = _dateCell.LastRow + 1; i <= _dateCell.LastRow + 1 + 31; i++)
        {
            try
            {
                var row = _sheet.GetRow(i);
                var dayCell = row.GetCell(_dateCell.FirstColumn);
                if (!DateUtil.IsCellDateFormatted(dayCell)) break;
                var dateTime = dayCell.DateCellValue;
                var date = DateOnly.FromDateTime(dateTime);
                if (date > _reportDate)
                {
                    break;
                }

                var valueCell = row.GetCell(col);
                double value = 0;
                if (valueCell.CellType == CellType.Numeric)
                {
                    value = valueCell.NumericCellValue;
                    value /= 1000d;
                }

                var existed = _valueList.FirstOrDefault(x => x.GisId == gisAddon.GisId &&
                                                             x.ValueId == gisAddon.Id &&
                                                             x.InType == ReviewValueInput.InputType.Addon &&
                                                             x.ReportDate == date);
                if (existed == null)
                {
                    _valueList.Add(new ReviewValueInput()
                    {
                        GisId = gisAddon.GisId,
                        ValueId = gisAddon.Id,
                        ReportDate = date,
                        InType = ReviewValueInput.InputType.Addon,
                        ValType = ReviewValueInput.ValueType.Fact,
                        Value = value
                    });
                }
                else
                {
                    existed.Value += value;
                }
            }
            catch (Exception ex)
            {
                _notificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Error,
                    Summary = "Ошибка с ГПШ",
                    Detail = ex.Message,
                    Duration = 3000
                });
            }
        }
    }

    private void ParseColumn(string gisName, string countryName, int col, int addonId)
    {
        var gis = _gisList.FirstOrDefault(x => x.Names.Any(n => StringParser.StrictLike(n, gisName)));
        var gisCountry =
            gis?.Countries.FirstOrDefault(x => x.Country.Names.Any(n => StringParser.StrictLike(n, countryName)));
        var addon = gisCountry?.Addons.FirstOrDefault(x => x.Id == addonId);
        if (addon == null) return;
        for (var i = _dateCell.LastRow + 1; i <= _dateCell.LastRow + 1 + 31; i++)
        {
            try
            {
                var row = _sheet.GetRow(i);
                var dayCell = row.GetCell(_dateCell.FirstColumn);
                DateUtil.IsCellDateFormatted(dayCell);
                if (!DateUtil.IsCellDateFormatted(dayCell)) break;
                var dateTime = dayCell.DateCellValue;
                var date = DateOnly.FromDateTime(dateTime);
                if (date > _reportDate)
                {
                    break;
                }
                var valueCell = row.GetCell(col);
                double value = 0;
                if (valueCell.CellType == CellType.Numeric)
                {
                    value = valueCell.NumericCellValue;
                    value /= 1000d;
                }

                var existed = _valueList.FirstOrDefault(x => x.GisId == gisCountry.GisId && x.ValueId == addon.Id &&
                                                             x.InType == ReviewValueInput.InputType.CountryAddon &&
                                                             x.ReportDate == date);
                if (existed == null)
                {
                    _valueList.Add(new ReviewValueInput()
                    {
                        GisId = gisCountry.GisId,
                        ValueId = addon.Id,
                        ReportDate = date,
                        InType = ReviewValueInput.InputType.CountryAddon,
                        ValType = ReviewValueInput.ValueType.Fact,
                        Value = value
                    });
                }
                else
                {
                    existed.Value += value;
                }
            }
            catch (Exception ex)
            {
                _notificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Error,
                    Summary = "Ошибка",
                    Detail = ex.Message,
                    Duration = 3000
                });
            }
        }
    }

    private void ParseColumn(string gisName, string countryName, int col)
    {
        var gis = _gisList.FirstOrDefault(x => x.Names.Any(n => StringParser.StrictLike(n, gisName)));
        if (gis == null) return;
        {
            var gisCountry = gis.Countries.FirstOrDefault(x =>
                x.Country.Names.Any(n => StringParser.StrictLike(n, countryName)));
            if (gisCountry == null) return;
            var r = _dateCell.LastRow;
            for (var i = r + 1; i <= r + 1 + 31; i++)
            {
                try
                {
                    var row = _sheet.GetRow(i);
                    var dayCell = row.GetCell(_dateCell.FirstColumn);
                    if (!DateUtil.IsCellDateFormatted(dayCell)) break;
                    var dateTime = dayCell.DateCellValue;
                    var date = DateOnly.FromDateTime(dateTime);
                    if (date > _reportDate)
                    {
                        break;
                    }

                    var valueCell = row.GetCell(col);
                    double value = 0;
                    if (valueCell.CellType == CellType.Numeric)
                    {
                        value = valueCell.NumericCellValue;
                        value /= 1000d;
                    }

                    var existed = _valueList.FirstOrDefault(x => x.GisId == gisCountry.GisId &&
                                                                 x.ValueId == gisCountry.Id &&
                                                                 x.InType == ReviewValueInput.InputType.Country &&
                                                                 x.ReportDate == date);
                    if (existed == null)
                    {
                        _valueList.Add(new ReviewValueInput()
                        {
                            GisId = gisCountry.GisId,
                            ValueId = gisCountry.Id,
                            ReportDate = date,
                            InType = ReviewValueInput.InputType.Country,
                            ValType = ReviewValueInput.ValueType.Fact,
                            Value = value
                        });
                    }
                    else
                    {
                        existed.Value += value;
                    }
                }
                catch (Exception ex)
                {
                    _notificationService.Notify(new NotificationMessage
                    {
                        Severity = NotificationSeverity.Error,
                        Summary = "Ошибка со страной",
                        Detail = ex.Message,
                        Duration = 3000
                    });
                }
            }
        }
    }

    private void AvtFindDateEntry()
    {
        for (var i = 0; i <= _sheet.LastRowNum; i++)
        {
            var row = _sheet.GetRow(i);
            if (row == null) continue;
            for (var col = 0; col <= row.LastCellNum; col++)
            {
                var cell = row.GetCell(col);
                if (cell is not { CellType: CellType.String }) continue;
                if (string.IsNullOrEmpty(cell.StringCellValue)) continue;
                if (StringParser.NameEqualsAnyList(_settings.DataEntry, cell.StringCellValue))
                {
                    _dateCell = GetMergedRegionContainingCell(cell);
                }
            }
        }
    }

    private static CellRangeAddress GetMergedRegionContainingCell(ICell cell)
    {
        if (cell is not { IsMergedCell: true }) return null;
        var sheet = cell.Sheet;
        for (var i = 0; i < sheet.NumMergedRegions; i++)
        {
            var region = sheet.GetMergedRegion(i);
            if (region.ContainsRow(cell.RowIndex) &&
                region.ContainsColumn(cell.ColumnIndex))
            {
                return region;
            }
        }

        return null;
    }
}