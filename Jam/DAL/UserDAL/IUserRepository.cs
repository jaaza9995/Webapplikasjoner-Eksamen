using Jam.Models;

namespace Jam.DAL.UserDAL;

public interface IUserRepository
{
    // Read
    Task<User?> GetUserById(int id);
    Task<User?> GetUser(string username, string passwordHash);
    Task<bool> UsernameExists(string username);
    Task<bool> UserEmailExists(string? email);


    // Create
    Task AddUser(User user);


    // Update
    Task UpdateUser(User user);


    // Delete
    Task<bool> DeleteUser(int id);
} 