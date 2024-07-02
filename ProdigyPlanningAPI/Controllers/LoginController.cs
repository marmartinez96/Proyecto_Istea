using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using NuGet.Common;
using ProdigyPlanningAPI.Data;
using ProdigyPlanningAPI.FormModels;
using ProdigyPlanningAPI.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;


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
        
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Login(User user)
        {
            IActionResult response = Unauthorized(new
            {
                success = false,
                message = "Credenciales inválidas",
                token = ""
            });

            try
            {
                var _user = await AuthenticateUser(user);
                if (_user != null)
                {
                    var token = GenerateToken(_user);
                    response = Ok(new
                    {
                        success = true,
                        message = "success",
                        token = token
                    });
                }
                else
                {
                    response = Unauthorized(new
                    {
                        success = false,
                        message = "Credenciales inválidas",
                        token = ""
                    });
                }
            }
            catch (Exception ex)
            {
                response = StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    success = false,
                    message = "Error interno del servidor",
                    error = ex.Message
                });
            }

            return response;
        }

        private async Task<User> AuthenticateUser(User user)
        {
            User _user = null;
            try
            {
                _user = await _context.Users
                                      .Where(x => !x.IsDeleted && x.Email == user.Email)
                                      .FirstOrDefaultAsync();

                if (_user != null && !BC.EnhancedVerify(user.Password, _user.Password))
                {
                    _user = null;
                }
            }
            catch (Exception e)
            {
                
                throw new Exception("Error autenticando el usuario", e);
            }
            return _user;
        }

        private async Task<string> GenerateToken(User user)
        {
            
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            
            var claimsTask = Task.FromResult(new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, _config["Jwt:Subject"]),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTime.Now.ToString()),
                new Claim("id", user.Id.ToString())
            });

            
            var claims = await claimsTask;

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(3),
                signingCredentials: credentials);

            
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        


        [AllowAnonymous]
        [HttpPost]
        [Route("Signup")]
        public async Task<IActionResult> SignUp(SignupModel user)
        {
            try
            {
                if (user.Email == null)
                {
                    return BadRequest(new { success = false, message = "El campo email no puede estar vacio" });
                }

                var _user = await _context.Users.FirstOrDefaultAsync(a => a.Email == user.Email);

                if (_user != null)
                {
                    return BadRequest(new { success = false, message = "Ese email ya esta registrado" });
                }
                if (user.Name == null)
                {
                    return BadRequest(new { success = false, message = "El campo nombre no puede estar vacio" });
                }
                if (user.Surname == null)
                {
                    return BadRequest(new { success = false, message = "El campo apellido no puede estar vacio" });
                }
                if (user.Password == null)
                {
                    return BadRequest(new { success = false, message = "El campo contraseña no puede estar vacio" });
                }
                SecurityQuestion _question = await _context.SecurityQuestions.FirstOrDefaultAsync(x => x.Id == user.UserQuestionId);
                if (_question == null)
                {
                    return BadRequest(new { success = false, message = "Se debe establecer una pregunta de seguridad" });
                }
                if (user.UserQuestionAnswer == null)
                {
                    return BadRequest(new { success = false, message = "Se debe establecer una respuesta a la pregunta de seguridad" });
                }

                _user = new User
                {
                    Name = user.Name,
                    Surname = user.Surname,
                    Password = BC.EnhancedHashPassword(user.Password, 13),
                    Email = user.Email,
                    Roles = user.Roles == "Organizador" ? "[ROLE_ORGANIZER]" : "[ROLE_USER]"
                };

                _context.Add(_user);

                UserQuestion securityQuestion = new UserQuestion
                {
                    UserId = _user.Id,
                    User = _user,
                    QuestionId = user.UserQuestionId,
                    Question = _question,
                    Answer = BC.EnhancedHashPassword(user.UserQuestionAnswer, 13)
                };
                _context.Add(securityQuestion);

                await _context.SaveChangesAsync();
                return Ok(new { success = true, message = "Usuario creado con exito" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}
