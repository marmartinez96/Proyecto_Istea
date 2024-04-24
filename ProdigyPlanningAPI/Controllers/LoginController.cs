using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using ProdigyPlanningAPI.Data;
using ProdigyPlanningAPI.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace ProdigyPlanningAPI.Controllers
{
    [Route("api/[controller]")]
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
                if(user.Password != _user.Password)
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

            var token = new JwtSecurityToken(_config["Jwt:Issuer"], _config["Jwt:Audience"], null, expires: DateTime.Now.AddMinutes(1), signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        [AllowAnonymous]
        [HttpPost]
        public IActionResult Login(User user)
        {
            IActionResult response = Unauthorized();
            var _user = AuthenticateUser(user);
            if (_user != null)
            {
                var token = GenerateToken(user);
                response = Ok(new { token = token });
            }
            return response;
        }
    }
}
