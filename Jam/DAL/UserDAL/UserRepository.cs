using Microsoft.EntityFrameworkCore;
using Jam.Models;

namespace Jam.DAL.UserDAL;

// Consider adding AsNoTracking where appropriate for read-only queries
// to improve performance by disabling change tracking 

public class UserRepository : IUserRepository
{
    private readonly StoryDbContext _db;
    private readonly ILogger<UserRepository> _logger;

    public UserRepository(StoryDbContext db, ILogger<UserRepository> logger)
    {
        _db = db;
        _logger = logger;
    }

    // --------------------------------------- Read / GET ---------------------------------------

    public async Task<IEnumerable<User>> GetAllUsers() // new method
    {
        try
        {
            return await _db.Users.ToListAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "[UserRepository -> GetAllUsers] An error occurred while retrieving all users.");
            return Enumerable.Empty<User>(); // not returning null to avoid null reference exceptions
        }
    }

    public async Task<User?> GetUserById(int userId)
    {
        if (userId <= 0)
        {
            _logger.LogWarning("[UserRepository -> GetUserById] Invalid user id {userId} provided", userId);
            return null;
        }

        try
        {
            var user = await _db.Users.FindAsync(userId);

            if (user == null)
            {
                _logger.LogInformation("[UserRepository -> GetUserById] No user found with id {userId}", userId);
            }

            return user;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "[UserRepository -> GetUserById] Failed to get user with id {userId}", userId);
            return null;
        }
    }

    public async Task<User?> GetUserByIdWithLoadedData(int userId)
    {
        if (userId <= 0)
        {
            _logger.LogWarning("[UserRepository -> GetUserByIdWithLoadedData] Invalid user id {userId} provided", userId);
            return null;
        }

        try
        {
            var user = await _db.Users
                .Include(u => u.Stories) // eager loading stories when fetching user 
                .Include(u => u.PlayingSessions) // eager loading playing sessions as well
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
            {
                _logger.LogInformation("[UserRepository -> GetUserByIdWithLoadedData] No user found with id {userId}", userId);
            }

            return user;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "[UserRepository -> GetUserByIdWithLoadedData] Failed to get user with id {userId}", userId);
            return null;
        }
    }

    public async Task<User?> GetUser(string username, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(passwordHash))
        {
            _logger.LogWarning("[UserRepository -> GetUser] Invalid username or password provided");
            return null;
        }

        try
        {
            var user = await _db.Users
                .Include(u => u.Stories) // eager loading stories when fetching user 
                .FirstOrDefaultAsync(u => u.Username == username && u.PasswordHash == passwordHash);

            if (user == null)
            {
                _logger.LogInformation("[UserRepository -> GetUser] No user found matching username {username}", username);
            }

            return user;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "[UserRepository -> GetUser] Failed to get user with username {username}", username);
            return null;
        }

        // This method must be changed (or removed) when adding authentication
    }

    public async Task<bool> UsernameExists(string username)
    {
        if (username == null)
        {
            _logger.LogWarning("[UserRepository -> UsernameExists] Attempted to check existence of a null username");
            return false;
        }

        try
        {
            bool exists = await _db.Users.AnyAsync(u => u.Username == username);
            return exists;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "[UserRepository -> UsernameExists] Failed to check if username {username} exists", username);
            return false;
        }
    }

    public async Task<bool> UserEmailExists(string email)
    {
        if (email == null)
        {
            _logger.LogWarning("[UserRepository -> UserEmailExists] Attempted to check existence of a null email");
            return false;
        }

        try
        {
            bool exists = await _db.Users.AnyAsync(u => u.Email == email);
            return exists;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "[UserRepository -> UserEmailExists] Failed to check if email {email} exists", email);
            return false;
        }
    }



    // --------------------------------------- Create ---------------------------------------

    public async Task<bool> AddUser(User user)
    {
        if (user == null)
        {
            _logger.LogWarning("[UserRepository -> AddUser] Attempted to add a null user object");
            return false;
        }

        try
        {
            _db.Users.Add(user);
            await _db.SaveChangesAsync();
            _logger.LogInformation("[UserRepository -> AddUser] Successfully created user {@User}", user);
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "[UserRepository -> AddUser] Failed to create user {@user}", user);
            return false;
        }
    }



    // --------------------------------------- Update ---------------------------------------    
    public async Task<bool> UpdateUser(User user)
    {
        if (user == null)
        {
            _logger.LogWarning("[UserRepository -> UpdateUser] Attempted to update a null user object");
            return false;
        }

        try
        {
            _db.Users.Update(user);
            await _db.SaveChangesAsync();
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "[UserRepository -> UpdateUser] Failed to update user {@user}", user);
            return false;
        }
    }



    // --------------------------------------- Delete ---------------------------------------

    public async Task<bool> DeleteUser(int userId)
    {
        if (userId <= 0)
        {
            _logger.LogWarning("[UserRepository -> DeleteUser] Invalid user id {userId} provided", userId);
            return false;
        }

        try
        {
            var user = await _db.Users.FindAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("[UserRepository -> DeleteUser] No user found with id {userId} to delete", userId);
                return false;
            }

            _db.Users.Remove(user);
            await _db.SaveChangesAsync();
            _logger.LogInformation("[UserRepository -> DeleteUser] Successfully deleted user with id {userId}", userId);
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "[UserRepository -> DeleteUser] Failed to delete user with id {userId}", userId);
            return false;
        }
    }
}