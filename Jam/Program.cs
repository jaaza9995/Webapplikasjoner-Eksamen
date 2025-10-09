
using Microsoft.EntityFrameworkCore;
using Jam.DAL;

using Jam.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<StoryDbContext>(options =>
{
    options.UseSqlite(
        builder.Configuration["ConnectionStrings:StoryDbContextConnection"]);
});

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

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapDefaultControllerRoute();

app.Run();

