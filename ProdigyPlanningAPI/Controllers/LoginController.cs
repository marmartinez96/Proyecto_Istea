using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using NuGet.Common;
using ProdigyPlanningAPI.Data;
using ProdigyPlanningAPI.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ProdigyPlanningAPI.Controllers
{
    [Route("/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private IConfiguration _config;
        private readonly ProdigyPlanningContext _context;
        public LoginController(IConfiguration configuration, ProdigyPlanningContext context)
        {
            _config = configuration;
            _context = context;
        }

        private User AuthenticateUser(User user)
        {
            User _user = null;
            try
            {
                _user = _context.Users.FirstOrDefault(a => a.Email == user.Email);
                if(_user != null && !BC.EnhancedVerify(user.Password, _user.Password))
                {
                    _user = null;
                }
            }
            catch(Exception e) 
            {

            }
            return _user;
        }

        private string GenerateToken(User user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, _config["Jwt:Subject"]),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTime.Now.ToString()),
                new Claim("id", user.Id.ToString())
            };

            var token = new JwtSecurityToken(_config["Jwt:Issuer"], _config["Jwt:Audience"], claims, expires: DateTime.Now.AddHours(3), signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        [AllowAnonymous]
        [HttpPost]
        public IActionResult Login(User user)
        {
            IActionResult response = Unauthorized(new
            {
                success = false,
                message = "Credenciales invalidas",
                token = ""
            });
            var _user = AuthenticateUser(user);
            if (_user != null)
            {
                var token = GenerateToken(_user);
                response = Ok(new {
                    success = true,
                    message = "success",
                    token = token 
                });
            }
            return response;
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("Signup")]
        public IActionResult SignUp(User user)
        {
            IActionResult response = BadRequest();
            try
            {
                var _user = _context.Users.FirstOrDefault(a => a.Email == user.Email);

                if (_user != null)
                {
                    throw new Exception("Ese email ya esta registrado");
                }

                _user = new User();
                _user.Name = user.Name;
                _user.Surname = user.Surname;
                _user.Password = BC.EnhancedHashPassword(user.Password, 13);
                _user.Email = user.Email;
                if (user.Roles == "Organizador")
                {
                    _user.Roles = "[ROLE_ORGANIZER]";
                }
                else
                {
                    _user.Roles = "[ROLE_USER]";
                }
                _context.Add(_user);
                _context.SaveChanges();
                response = Ok("Usuario creado con exito");

                return response;
            }
            catch (Exception ex) 
            {
                response = BadRequest(ex.Message);

                return response;
            }
        }
    }
}
