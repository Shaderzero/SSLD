using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SSLD.Data.DailyReview;
using SSLD.Data.DZZR;

namespace SSLD.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        AppContext.SetSwitch("Npgsql.DisableDateTimeInfinityConversions", true);
    }

    public DbSet<Country> Countries { get; set; }
    public DbSet<Gis> Gises { get; set; }
    public DbSet<GisCountry> GisCountries { get; set; }
    public DbSet<GisCountryValue> GisCountryValues { get; set; }
    public DbSet<GisAddon> GisAddons { get; set; }
    public DbSet<GisAddonValue> GisAddonValues { get; set; }
    public DbSet<GisCountryAddon> GisCountryAddons { get; set; }
    public DbSet<GisCountryAddonType> GisCountryAddonTypes { get; set; }
    public DbSet<GisCountryAddonValue> GisCountryAddonValues { get; set; }
    public DbSet<GisInputValue> GisInputValues { get; set; }
    public DbSet<GisOutputValue> GisOutputValues { get; set; }
    public DbSet<FileTypeSetting> FileTypeSettings { get; set; }
    public DbSet<InputFileLog> InputFilesLogs { get; set; }
    public DbSet<OperatorGis> OperatorGises { get; set; }
    public DbSet<OperatorResource> OperatorResources { get; set; }
    public DbSet<OperatorResourceHour> OperatorResourceHours { get; set; }
    public DbSet<Forecast> Forecasts { get; set; }
    public DbSet<ForecastGisCountry> ForecastGisCountries { get; set; }
    public DbSet<ForecastYear> ForecastYears { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<GisCountry>(x =>
        {
            x.HasIndex(a => new { a.GisId, a.CountryId })
                .IsUnique();
        });

        modelBuilder.Entity<GisCountryValue>(x =>
        {
            x.HasIndex(a => new { a.GisCountryId, a.ReportDate })
                .IsUnique();
        });

        modelBuilder.Entity<GisAddonValue>(x =>
        {
            x.HasIndex(a => new { a.GisAddonId, a.ReportDate })
                .IsUnique();
        });

        modelBuilder.Entity<GisCountryValue>(x =>
        {
            x.HasIndex(a => new { a.GisCountryId, a.ReportDate })
                .IsUnique();
        });

        modelBuilder.Entity<GisInputValue>(x =>
        {
            x.HasIndex(a => new { a.GisId, a.ReportDate })
                .IsUnique();
        });

        modelBuilder.Entity<GisOutputValue>(x =>
        {
            x.HasIndex(a => new { a.GisId, a.ReportDate })
                .IsUnique();
        });

        modelBuilder.Entity<FileTypeSetting>()
            .HasIndex(u => u.Name)
            .IsUnique();
            
        modelBuilder.Entity<OperatorResourceHour>(x =>
        {
            x.HasIndex(a => new { a.OperatorResourceId, a.Hour })
                .IsUnique();
        });
    }
}