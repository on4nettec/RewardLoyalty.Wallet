using On4Net.Extensions.Data.Migration;
using Wallet.Core.Api;
using Wallet.Core.Data.Schema;

var builder = WebApplication.CreateBuilder(args);
var startup = new Startup(builder.Configuration);
startup.ConfigureServices(builder.Services);

var app = builder.Build();

if (app.Configuration.GetValue<bool>("RunMigrations"))
{
    using var scope = app.Services.CreateScope();
    scope.ServiceProvider.GetRequiredService<DataSchemaMigrator>().UpdateSchemas<Anchor>();
}

startup.Configure(app, app.Environment);
app.MapControllers();
app.Run();

public partial class Program { }
