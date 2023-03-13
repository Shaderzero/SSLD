using Microsoft.EntityFrameworkCore;

namespace SSLD.Data.DailyReview;

[Index(nameof(Filename))]
[Index(nameof(InputTime))]
[Index(nameof(FileDate))]
[Index(nameof(FileTime))]
public class InputFileLog
{
    public long Id { get; set; }
    public string Filename { get; set; }
    public string UserId { get; set; }
    public ApplicationUser User { get; set; }
    public DateTime InputTime { get; set; }
    public DateOnly FileDate { get; set; }
    public DateTime FileTime { get; set; }
}