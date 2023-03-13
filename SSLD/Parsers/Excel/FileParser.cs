using Microsoft.AspNetCore.Components.Forms;
using SSLD.Data.DailyReview;
using SSLD.Tools;

namespace SSLD.Parsers.Excel;

public class FileParser: IFileParser
{
    private IParserHelper _helper;
    private FileTypeSetting _fileTypeSetting;
    private IFileParser _parser;

    public FileParser(IParserHelper helper)
    {
        _helper = helper;
    }
    
    public async Task<bool> SetFileAsync(IBrowserFile file)
    {
        _fileTypeSetting = await _helper.GetFileSettings(file.Name);
        if (_fileTypeSetting == null) return false;
        switch (_fileTypeSetting.TypeName)
        {
            case SD.File_Balance_Cpdd:
            {
                _parser = new BalanceCpddParser(_helper);
                break;
            }
            case SD.File_Fact_Cpdd:
            {
                _parser = new FactCpddParser(_helper);
                break;
            }
            case SD.File_Avt:
            {
                _parser = new AvtParser(_helper);
                break;
            }
            case SD.File_Ge_Mail:
            {
                _parser = new GeMailParser(_helper);
                break;
            }
            case SD.File_Gas_Day:
            {
                _parser = new GasDayParser(_helper);
                break;
            }
            case SD.File_Fact_Supply:
            {
                _parser = new FactSupplyParser(_helper);
                break;
            }
            case SD.File_Teterevka:
            {
                _parser = new TeterevkaParser(_helper);
                break;
            }
            case SD.File_Forecast:
            {
                _parser = new ForecastParser(_helper);
                break;
            }
            case SD.File_Dz_Zr:
            {
                _parser = new DzZrParser(_helper);
                break;
            }
        }
        return await _parser.SetFileAsync(file);
    }

    public async Task ParseAsync()
    {
        await _parser.ParseAsync();
    }

    public async Task<ParserResult> SaveResultAsync()
    {
        return await _parser.SaveResultAsync();
    }

    public async Task Dispose()
    {
        await _parser.Dispose();
    }
}