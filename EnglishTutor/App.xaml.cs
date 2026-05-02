using System.Windows;
using EnglishTutor.Data;
using Microsoft.Extensions.Configuration;
namespace EnglishTutor
{
    public partial class App : Application
    {
        public static IConfiguration Configuration { get; private set; } = null!;
        public static string ConnectionString { get; private set; } = null!;
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            Configuration = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();
            ConnectionString = Configuration.GetConnectionString("DefaultConnection")!;
            DbInitializer.Initialize();
        }
    }
}
