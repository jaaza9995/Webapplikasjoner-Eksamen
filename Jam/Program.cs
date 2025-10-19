
using Microsoft.EntityFrameworkCore;
using Jam.DAL;
using Jam.DAL.SceneDAL;
using Jam.DAL.PlayingSessionDAL;
using Jam.DAL.AnswerOptionDAL;
using Jam.DAL.StoryDAL;
using Jam.DAL.UserDAL;
using Jam.DAL.QuestionDAL;


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
builder.Services.AddScoped<IQuestionRepository, QuestionRepository>();


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

using (var scope = app.Services.CreateScope()) // skal fjernes når ting er inne
{
    var db = scope.ServiceProvider.GetRequiredService<StoryDbContext>();
    db.Database.EnsureCreated(); // creates the database + tables if not exist
    Console.WriteLine($"Database created/verified at: {System.IO.Path.GetFullPath("StoryDatabase.db")}");
}

//app.UseStaticFiles();
app.MapControllers();   

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapDefaultControllerRoute();

app.Run();

