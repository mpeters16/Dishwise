using Microsoft.EntityFrameworkCore;


namespace RecipeManager.Infrastructure.Data;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _db;

    public UserRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<User?> GetUserById(Guid id)
    {
        var user = await _db.Users.FindAsync(id);
        if (user == null || user.Id != id)
        {
            return null;
        }
        return user;
    }

    public async Task<User?> GetUserByEmail(string email)
    {
        var user = await _db.Users.Where(r => r.Email == email).FirstOrDefaultAsync();
        if (user == null)
        {
            return null;
        }
        return user;
    }

      public async Task<User?> GetUserByUsername(string username)
    {
        var user = await _db.Users.Where(r => r.Username == username).FirstOrDefaultAsync();
        if (user == null)
        {
            return null;
        }
        return user;
    }

    public async Task AddUser(User user)
    {
        _db.Users.Add(user);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateUser(User user)
    {
        _db.Users.Update(user);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteUser(User user)
    {
        _db.Users.Remove(user);
        await _db.SaveChangesAsync();
    }
}