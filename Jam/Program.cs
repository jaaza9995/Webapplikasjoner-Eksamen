
using Microsoft.EntityFrameworkCore;
using Jam.DAL;
using Jam.DAL.SceneDAL;
using Jam.DAL.PlayingSessionDAL;
using Jam.DAL.AnswerOptionDAL;
using Jam.DAL.StoryDAL;
using Jam.DAL.UserDAL;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<StoryDbContext>(options =>
{
    options.UseSqlite(
        builder.Configuration["ConnectionStrings:StoryDbContextConnection"]);
});

builder.Services.AddScoped<ISceneRepository, SceneRepository>();
builder.Services.AddScoped<IPlayingSessionRepository, PlayingSessionRepository>();
builder.Services.AddScoped<IAnswerOptionRepository, AnswerOptionRepository>();
builder.Services.AddScoped<IStoryRepository, StoryRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    DBInit.Seed(app);
}

//app.UseStaticFiles();
app.MapControllers();   

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapDefaultControllerRoute();



app.Run();

