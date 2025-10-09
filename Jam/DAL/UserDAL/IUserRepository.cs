using Jam.Models;

namespace Jam.DAL.UserDAL;

public interface IUserRepository
{
    Task<User?> GetUserById(int id);
    Task<User?> GetUser(string username, string passwordHash);
    Task<bool> UsernameExists(string username);
    Task<bool> UserEmailExists(string? email);

    Task CreateUser(User user);
    Task UpdateUser(User user);
    Task<bool> DeleteUser(int id);
} 