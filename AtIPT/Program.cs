using System.Globalization;
using System.Threading;
using AtIPT.Application.Abstractions;
using AtIPT.Application.Services;
using AtIPT.Components;
using AtIPT.Infrastructure.Excel;
using AtIPT.Infrastructure.Import;
using AtIPT.Infrastructure.Licensing;
using AtIPT.Infrastructure.Persistence;
using AtIPT.Infrastructure.Simulation;
using AtIPT.Services;

CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;
CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;
Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;  

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();

builder.Services.AddSingleton<IImportService, TextFileImportService>();
builder.Services.AddSingleton<IExcelExportService, ClosedXmlExcelExportService>();
builder.Services.AddSingleton<ILicensingService, LicensingService>();
builder.Services.AddSingleton<IParameterFileService, ParameterFileService>();

var exeDir = Path.Combine(builder.Environment.ContentRootPath, "Exe");

builder.Services.AddSingleton<ISimulationRunner>(sp =>
    new FortranProcessRunner(exeDir,
        sp.GetRequiredService<ILogger<FortranProcessRunner>>()));

builder.Services.AddSingleton<ISolverFileService>(sp => new SolverFileService(exeDir));
builder.Services.AddSingleton<IProjectRepository>(sp =>
    new ProjectRepository(builder.Environment.ContentRootPath));

builder.Services.AddScoped<IAnalysisService, AnalysisService>();
builder.Services.AddScoped<AppState>();
builder.Services.AddScoped<IThemeService, ThemeService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
   .AddInteractiveServerRenderMode();

Directory.CreateDirectory(exeDir);
Directory.CreateDirectory(Path.Combine(app.Environment.ContentRootPath, "Projects"));

using (var scope = app.Services.CreateScope())
{
    var runner = scope.ServiceProvider.GetRequiredService<ISimulationRunner>();
    try { await runner.KillAllAsync(); } catch { }
    var files = scope.ServiceProvider.GetRequiredService<ISolverFileService>();
    await files.InitialiseWorkingFilesAsync();
}

app.Run();