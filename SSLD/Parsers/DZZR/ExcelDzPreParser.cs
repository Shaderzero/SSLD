using Microsoft.AspNetCore.Components.Forms;
using Radzen;
using SSLD.Data.DZZR;
using SSLD.Tools;

namespace SSLD.Parsers.DZZR;

public class ExcelDzPreParser
{
    private readonly NotificationService _notificationService;
    // private readonly ApplicationDbContext _db;
    private readonly IBrowserFile _file;
    private readonly IEnumerable<OperatorGis> _gisList;
    private List<OperatorResource> _valueList = new();
    private string _message;
    private OperatorResourceType fileType;

    public ExcelDzPreParser(IBrowserFile file, NotificationService notificationService, IEnumerable<OperatorGis> gisList)
    {
        _file = file;
        _gisList = gisList;
        _notificationService = notificationService;
    }

    public async Task ParseFile()
    {
        var filename = _file.Name;
        var reportDate = StringParser.GetFirstDateOnlyFromString(filename);
        if (reportDate == null)
        {
            _message = "В файле " + filename + " даже даты нет!";
            return;
        }
        if (filename.Contains("ДЗ"))
        {
            fileType = OperatorResourceType.Dz;
        }
        else if (filename.Contains("ЗР"))
        {
            fileType = OperatorResourceType.Zr;
        }
        else
        {
            _message = "В файле " + filename + " нет ДЗ или ЗР";
            return;
        }
        var parser = new ExcelDzParser(_file, _notificationService, _gisList);
        // parser.ParseFile();
        _valueList = await parser.GetResult();
        foreach (var val in _valueList)
        {
            val.Type = fileType;
        }
    }

    public DZRValueList GetResult()
    {
        var result = new DZRValueList()
        {
            Values = _valueList,
            Filename = _file.Name,
            FileTimeStamp = _file.LastModified.DateTime,
            Message = _message
        };
        return result;
    }
}