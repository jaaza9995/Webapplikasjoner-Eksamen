using Microsoft.AspNetCore.Identity;

namespace Jam.Models;

public class User
{
    public int UserId { get; set; }
    public string Firstname { get; set; } = string.Empty;
    public string Lastname { get; set; } = string.Empty;
    public string? Email { get; set; } // optional because of kids
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public List<Story> Stories { get; set; } = new(); // Navigation property
    public List<PlayingSession> PlayingSessions { get; set; } = new(); // Navigation property
}

/* 
    Later, when adding authentication:
        change the name of file from: User.cs
        to: ApplicationUser.cs (still in Models-direcotry)

        change this: public class User {...} 
        to this: public class ApplicationUser : IdentityUser {...}
        

    For Example:
        using Microsoft.AspNetCore.Identity.EntityFrameworkCore; // important!

        namespace Jam.Models;

        public class ApplicationUser : IdentityUser 
        {
            public string Firstname { get; set; } = string.Empty;
            public string Lastname { get; set; } = string.Empty;

            // UserId, Email, Username, and PasswordHash are inherited from IdentityUser,
            // so these properties are not needed here anymore.

            public List<Story> Stories { get; set; } = new(); // Navigation property
            public List<PlayingSession> PlayingSessions { get; set; } = new(); // Navigation property
        }

    Rememer in DAL/StoryDbContext.cs:
        using Microsoft.AspNetCore.Identity.EntityFrameworkCore; // important! 
        change this: public class StoryDbContext : DbContext {}
        to this: public class StoryDbContext : IdentityDbContext<ApplicationUser> {}

        also, change this:
            public DbSet<Story> Stories { get; set; }
            public DbSet<User> Users { get; set; }
            public DbSet<Scene> Scenes { get; set; }
            public DbSet<Question> Questions { get; set; }
            public DbSet<AnswerOption> AnswerOptions { get; set; }
            public DbSet<PlayingSession> PlayingSessions { get; set; }
        to this:
            public DbSet<Story> Stories { get; set; }
            // We no longer need a DbSet for User, IdentityDbContext includes this automatically:
            // public virtual DbSet<ApplicationUser> Users { get; set; }
            public DbSet<Scene> Scenes { get; set; }
            public DbSet<Question> Questions { get; set; }
            public DbSet<AnswerOption> AnswerOptions { get; set; }
            public DbSet<PlayingSession> PlayingSessions { get; set; }


    Remember: 
        anywhere we use (Story and PlayingSession): 
            public int? UserId { get; set; } 
            public User? User { get; set; }
        we have to instead use:
            public string? UserId { get; set; } // Identity uses string Id (GUID)
            public ApplicationUser? User { get; set; }

    
    ChatGPT recommendation for scaffolding authentication:
        Scaffold all Identity pages once (like Baifan did in the Authentication demo)
        Then delete the ones you don’t need (keep Login, Register, Logout, AccessDenied)
        Later, when your main functionality works, you can replace those scaffolded pages 
        with your own custom ones if you want a cleaner/more fitting design.
       
*/