using UserApi.Models;

namespace UserApi.Services;

public class UserService : IUserService
{
    private readonly Dictionary<int, User> _users = new();
    private int _nextId = 1;

    public IEnumerable<User> GetAll() => _users.Values;

    public User? GetById(int id) => _users.TryGetValue(id, out var user) ? user : null;

    public void Create(User user)
    {
        user.Id = _nextId++;
        _users[user.Id] = user;
    }


    public void Update(User user)
    {
        var existing = GetById(user.Id);
        if (existing is null) return;

        existing.Name = user.Name;
        existing.Email = user.Email;
    }

   public void Delete(int id) => _users.Remove(id);

}