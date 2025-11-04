using Microsoft.EntityFrameworkCore;
using Jam.DAL;
using Jam.DAL.SceneDAL;
using Jam.DAL.PlayingSessionDAL;
using Jam.DAL.AnswerOptionDAL;
using Jam.DAL.StoryDAL;
using Jam.DAL.UserDAL;
using Microsoft.AspNetCore.Identity;


var builder = WebApplication.CreateBuilder(args);

// Repositories
builder.Services.AddScoped<ISceneRepository, SceneRepository>();
builder.Services.AddScoped<IPlayingSessionRepository, PlayingSessionRepository>();
builder.Services.AddScoped<IAnswerOptionRepository, AnswerOptionRepository>();
builder.Services.AddScoped<IStoryRepository, StoryRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();


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
builder.Services.AddDbContext<StoryDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("StoryDbContextConnection")));

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

app.UseRouting();
app.UseSession();// Session må være før Authorization/Endpoints

// Én standard MVC-rute holder
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
