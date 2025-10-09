using Microsoft.EntityFrameworkCore;
using Jam.Models;

namespace Jam.DAL.UserDAL;

public class UserRepository : IUserRepository
{
    private readonly StoryDbContext _db;

    public UserRepository(StoryDbContext db)
    {
        _db = db;
    }
    public async Task<User?> GetUserById(int id)
    {
        return await _db.Users.FindAsync(id);
    }

    public async Task<User?> GetUser(string username, string passwordHash)
    {
        return await _db.Users
            .Include(u => u.Stories) // eager loading stories when fetching user 
            .FirstOrDefaultAsync(u => u.Username == username && u.PasswordHash == passwordHash);
    }

    public async Task<bool> UsernameExists(string username)
    {
        return await _db.Users.AnyAsync(u => u.Username == username);
    }

    public async Task<bool> UserEmailExists(string? email)
    {
        if (email == null) return false;
        return await _db.Users.AnyAsync(u => u.Email == email);
    }

    public async Task CreateUser(User user)
    {
        _db.Users.Add(user);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateUser(User user)
    {
        _db.Users.Update(user);
        await _db.SaveChangesAsync();
    }

    public async Task<bool> DeleteUser(int id)
    {
        var user = await _db.Users.FindAsync(id);
        if (user == null)
        {
            return false;
        }

        _db.Users.Remove(user);
        await _db.SaveChangesAsync();
        return true;
    }
}