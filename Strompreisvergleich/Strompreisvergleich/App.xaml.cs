using System.IO;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StromDbLib;

namespace Strompreisvergleich;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private readonly IHost host;
    public IConfiguration Configuration { get; private set; }

    public App()
    {
        var builder = new ConfigurationBuilder()
         .SetBasePath(Directory.GetCurrentDirectory())
         .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

        Configuration = builder.Build();

        host = Host.CreateDefaultBuilder().ConfigureServices((context, services) =>
        {
            services.AddDbContext<StromDbContext>(x => x.UseSqlite("Data Source=Strom.db"));
            services.AddSingleton(Configuration);
            services.AddSingleton<MainWindow>();
        }).Build();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        await host.StartAsync();
        var mainwindow = host.Services.GetRequiredService<MainWindow>();
        mainwindow.Show();
        base.OnStartup(e);
    }
}
