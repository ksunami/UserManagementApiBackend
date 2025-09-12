using UserApi.Models;

namespace UserApi.Services;

public interface IUserService
{
    IEnumerable<User> GetAll();
    User? GetById(int id);
    void Create(User user);
    void Update(User user);
    void Delete(int id);
}