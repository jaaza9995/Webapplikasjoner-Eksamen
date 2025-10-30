using Jam.Models;

namespace Jam.DAL.UserDAL;

// Note: there will probalby be some changes with IUserRepository and UserRepository
// once we add authentication. Will start working on this ASAP. 

public interface IUserRepository
{
    // Read / GET
    Task<IEnumerable<User>> GetAllUsers(); // new method
    Task<User?> GetUserById(int userId);
    Task<User?> GetUserByIdWithLoadedData(int userId); // new method
    Task<User?> GetUser(string username, string passwordHash); // will be removed
    Task<bool> UsernameExists(string username);
    Task<bool> UserEmailExists(string email);


    // Create
    Task<bool> AddUser(User user);


    // Update
    Task<bool> UpdateUser(User user);


    // Delete
    Task<bool> DeleteUser(int userId);
}