using Akvila.Web.Api.Domains.User;

namespace Akvila.Web.Api.Core.Repositories;

public interface IUserRepository {
    Task<User?> CheckExistUser(string login, string email);
    Task<User?> GetUser(string loginOrEmail, string password);
    Task<User> CreateUser(string email, string login, string password);
}