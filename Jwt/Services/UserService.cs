using Jwt.Model;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Jwt.Services
{
    public interface IUserService
    {
        UserModel Authenticate(string username, string password);
        IEnumerable<UserModel> GetAll();
        UserModel GetById(int id);
    }
    public class UserService : IUserService
    {
        private List<User> _users = new List<User>
        {
            new User { Id = 1, FirstName = "Admin", LastName = "User", Username = "admin", Password = "admin", Role = Role.Admin },
            new User { Id = 2, FirstName = "Normal", LastName = "User", Username = "user", Password = "user", Role = Role.User }
        };

        private readonly AppSettings _appSettings;

        public UserService(IOptions<AppSettings> appSettings)
        {
            _appSettings = appSettings.Value;
        }
        public UserModel Authenticate(string username, string password)
        {
            var user = _users.SingleOrDefault(x => x.Username == username && x.Password == password);

            // return null if user not found
            if (user == null)
                return null;
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.Secret);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                            {
                    new Claim(ClaimTypes.Name, user.Id.ToString()),
                    new Claim(ClaimTypes.Role, user.Role)
                            }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            user.Token = tokenHandler.WriteToken(token);

            return new UserModel{
                Id= user.Id, 
                FirstName=user.FirstName, 
                LastName=user.LastName, 
                Username=user.Username, 
                Token=user.Token,
                Role=user.Role
                };
        }

        public IEnumerable<UserModel> GetAll()
        {
            return _users.Select(u => new UserModel {
                Id=u.Id, 
                FirstName=u.FirstName, 
                LastName=u.LastName,
                Username=u.Username,
                Token=u.Token,
                Role=u.Role
            });
        }

        public UserModel GetById(int id)
        {
            var user = _users.FirstOrDefault(x => x.Id == id);

            if (user != null)
                return new UserModel{
                    Id= user.Id, 
                    FirstName=user.FirstName, 
                    LastName=user.LastName, 
                    Username=user.Username, 
                    Token=user.Token,
                    Role=user.Role
                    };
            return null;
        }
    }
}
