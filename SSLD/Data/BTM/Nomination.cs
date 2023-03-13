using SSLD.Data.DailyReview;
using System.ComponentModel.DataAnnotations.Schema;

namespace SSLD.Data.BTM;

public class Nomination
{
    public int Id { get; set; }
    public int ContractId { get; set; }
    public Contract Contract { get; set; }
    public int GisBtmId { get; set; }
    public GisBtm GisBtm { get; set; }
    public DateOnly ReportDate { get; set; }
    public DateTime RevisionTime { get; set; }
    public InputFileLog ValueTime { get; set; }
    public List<NominationHour> NominationHours { get; set; }
    [NotMapped]
    public int DailyPower
    {
        get => NominationHours.Where(x => x.Id == Id).Sum(x => x.Power);
    }

    [NotMapped]
    public decimal DailyVolume
    {
        get => NominationHours.Where(x => x.Id == Id).Sum(x => x.Volume);
    }
}