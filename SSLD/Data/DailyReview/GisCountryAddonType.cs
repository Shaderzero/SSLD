using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SSLD.Data.DailyReview;

[Index(nameof(StartDate))]
public class GisCountryAddonType
{
    public int Id { get; set; }
    public int GisCountryAddonId { get; set; }
    public GisCountryAddon GisCountryAddon { get; set; }
    public DateOnly StartDate { get; set; }
    public bool IsCommGas { get; set; }
    [NotMapped]
    public DateTime StartDateTime
    {
        get => StartDate.ToDateTime(new TimeOnly(0));
        set => StartDate = DateOnly.FromDateTime(value);
    }
}