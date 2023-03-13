using SSLD.Data;

namespace SSLD.Tools.DailyReviewGenerator;

public class DataHelper
{
    private ApplicationDbContext _db;

    public DataHelper(ApplicationDbContext db)
    {
        _db = db;
    }
    
    
}