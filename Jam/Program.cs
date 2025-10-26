using Microsoft.EntityFrameworkCore;
using Jam.DAL;
using Jam.DAL.SceneDAL;
using Jam.DAL.PlayingSessionDAL;
using Jam.DAL.AnswerOptionDAL;
using Jam.DAL.StoryDAL;
using Jam.DAL.UserDAL;
using Microsoft.AspNetCore.Identity;
using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("StoryDbContextConnection") ?? throw new InvalidOperationException("Connection string 'StoryDbContextConnection' not found.");

builder.Services.AddDefaultIdentity<IdentityUser>().AddEntityFrameworkStores<StoryDbContext>();

// DbContext (SQLite)
builder.Services.AddDbContext<StoryDbContext>(options =>
    options.UseSqlite(builder.Configuration["ConnectionStrings:StoryDbContextConnection"])
);

// Repositories
builder.Services.AddScoped<ISceneRepository, SceneRepository>();
builder.Services.AddScoped<IPlayingSessionRepository, PlayingSessionRepository>();
builder.Services.AddScoped<IAnswerOptionRepository, AnswerOptionRepository>();
builder.Services.AddScoped<IStoryRepository, StoryRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

var loggerConfiguration = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.File($"Logs/app_{DateTime.Now:yyyyMMdd_HHmmss}.log");

loggerConfiguration.Filter.ByExcluding(e => e.Properties.TryGetValue("SourceContext", out var value) &&
                            e.Level == LogEventLevel.Information &&
                            e.MessageTemplate.Text.Contains("Executed dbCommand"));

var logger = loggerConfiguration.CreateLogger();
builder.Logging.AddSerilog(logger);

builder.Services.AddRazorPages();
builder.Services.AddSession();

// Session (for å lagre wizard-utkast mellom Next/Back)
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(o =>
{
    o.IdleTimeout = TimeSpan.FromHours(2);
    o.Cookie.HttpOnly = true;
    o.Cookie.IsEssential = true;
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    DBInit.Seed(app);
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

// Viktig for css/js og _ValidationScriptsPartial
//app.UseStaticFiles();

app.UseAuthorization();
app.UseAuthentication();
app.MapRazorPages();
app.UseRouting();
app.UseSession();// Session må være før Authorization/Endpoints

// Én standard MVC-rute holder
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
