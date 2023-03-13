[assembly: HostingStartup(typeof(SSLD.Areas.Identity.IdentityHostingStartup))]
namespace SSLD.Areas.Identity;

public class IdentityHostingStartup : IHostingStartup
{
    public void Configure(IWebHostBuilder builder)
    {
        builder.ConfigureServices((context, services) => {
        });
    }
}