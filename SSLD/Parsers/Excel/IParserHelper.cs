using SSLD.Data.DailyReview;
using SSLD.Data.DZZR;
using SSLD.Tools;

namespace SSLD.Parsers.Excel;

public interface IParserHelper
{
    Task<Gis[]> GetGisArrayAsync();
    Task<Gis[]> GetGisDetailArrayAsync();
    Task<OperatorGis[]> GetOperatorGisArrayAsync();
    Task<FileTypeSetting> GetFileSettings(string filename);
    bool IsForced();
    Task<ParserResult> SaveResultAsync(ParserResultImpl<ReviewValueInput> parserResultImpl);
    Task<ParserResult> SaveResultAsync(ParserResultImpl<OperatorResource> parserResultImpl);
    Task<ParserResult> SaveResultAsync(ParserResultImpl<ForecastGisCountry> parserResultImpl);
}