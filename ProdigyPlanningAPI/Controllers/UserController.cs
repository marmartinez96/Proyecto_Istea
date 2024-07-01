using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using ProdigyPlanningAPI.Data;
using ProdigyPlanningAPI.FormModels;
using ProdigyPlanningAPI.Helpers;
using ProdigyPlanningAPI.Models;
using System.Security.Claims;


namespace ProdigyPlanningAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly ProdigyPlanningContext _context;
        private IQueryable<Event> _activeEventQueryBP;
        private IQueryable<Event> _listedEventQueryBP;
        private IQueryable<Category> _listedCategoryQueryBP;
        private IEmailSender _emailSender;
        public UserController(ProdigyPlanningContext context, IEmailSender emailSender)
        {
            _context = context;

            _activeEventQueryBP = _context.Events.Include(x => x.CreatedByNavigation).Include(x => x.Categories).Include(x => x.Banner).Where(x => x.IsDeleted == false).Where(x => x.IsActive == true);
            _listedEventQueryBP = _context.Events.Include(x => x.CreatedByNavigation).Include(x => x.Categories).Include(x => x.Banner).Where(x => x.IsDeleted == false);

            _listedCategoryQueryBP = _context.Categories.Include(x => x.Events).Where(x => x.IsDeleted == false);
            _emailSender = emailSender;
        }

        [Authorize]
        [HttpGet]
        [Route("GetUser")]
        public async Task<IActionResult> GetUser()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var token = AuthorizationHelper.ValidateToken(identity, _context);
            if (!token.success) return token;

            User user = token.result;

            return Ok(new
            {
                success = true,
                message = "Success",
                data = UserRetrievalHelper.CreateRetrievalModel(_context, user)
            });
        }

        [Authorize]
        [HttpGet]
        [Route("GetRoles")]
        public async Task<IActionResult> GetRoles()
        {
            bool success = true;
            string message = "Success";
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var token = AuthorizationHelper.ValidateToken(identity, _context);
            if (!token.success) return Unauthorized(token);

            User user = token.result;

            var result = new
            {
                success = success,
                message = message,
                data = user.Roles
            };

            return Ok(await Task.FromResult(result));
        }

        [Authorize]
        [HttpGet]
        [Route("IsPremium")]
        public async Task<IActionResult> GetIsPremium()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var token = AuthorizationHelper.ValidateToken(identity, _context);
            if (!token.success)
                return Unauthorized(new { success = false, message = token.message });

            User user = token.result;

            return Ok(new
            {
                success = true,
                message = "Success",
                data = user.IsPremium
            });
        }

        [Authorize]
        [HttpPatch]
        [Route("UpdatePassword")]
        public async Task<IActionResult> UpdatePassword(ChangePasswordModel passwordModel)
        {
            bool success = true;
            string message = "Contraseña cambiada exitosamente";
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var token = AuthorizationHelper.ValidateToken(identity, _context);
            if (!token.success) return Unauthorized(token);

            User user = token.result;
            try
            {
                if (passwordModel.OldPassword != passwordModel.ConfirmPassword)
                {
                    return BadRequest(new { success = false, message = "Las contraseñas no coinciden" });
                }

                if (!BC.EnhancedVerify(passwordModel.OldPassword, user.Password))
                {
                    return BadRequest(new { success = false, message = "La contraseña enviada es incorrecta" });
                }

                if (passwordModel.OldPassword == passwordModel.ConfirmPassword && BC.EnhancedVerify(passwordModel.OldPassword, user.Password))
                {
                    user.Password = BC.EnhancedHashPassword(passwordModel.NewPassword, 13);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception e)
            {
                success = false;
                message = e.Message;
            }

            if (success)
            {
                return Ok(new { success = true, message = message });
            }
            else
            {
                return BadRequest(new { success = false, message = message });
            }
        }

        [AllowAnonymous]
        [HttpPatch]
        [Route("RecoverPassword")]
        public async Task<IActionResult> RecoverPassword(PasswordRecoveryModel user)
        {
            if (user.Email == null)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Se debe proporcionar un correo electronico para el reseteo de contraseña",
                });
            }
            if (user.QuestionId == 0)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Se debe proporcionar una pregunta para el reseteo de contraseña",
                });
            }
            if (user.Answer == null)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Se debe proporcionar una respuesta para el reseteo de contraseña",
                });
            }
            User _user = await _context.Users.FirstOrDefaultAsync(x => x.Email == user.Email);
            if (_user == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "No se encontro un usuario asociado con ese correo electronico",
                });
            }
            SecurityQuestion securityQuestion = await _context.SecurityQuestions.FirstOrDefaultAsync(x => x.Id == user.QuestionId);
            if (securityQuestion == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "No se encontro ninguna pregunta de seguridad con el id proporcionado",
                });
            }
            UserQuestion userQuestion = await _context.UserQuestions.Include(x=> x.User).Include(x => x.Question).FirstOrDefaultAsync(x => x.User == _user);
            if (userQuestion == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "No se encontro ninguna pregunta de seguridad asociada al correo enviado",
                });
            }
            if (userQuestion.Question != securityQuestion || !BC.EnhancedVerify(user.Answer, userQuestion.Answer))
            {
                return Unauthorized(new
                {
                    success = false,
                    message = "La pregunta o la respuesta son incorrectos",
                });
            }

            int length = 10;
            string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*()_+";
            var random = new Random();
            string password = new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());

            _user.Password = BC.EnhancedHashPassword(password, 13);
            await _context.SaveChangesAsync();

            
            string receiver = _user.Email;
            string subject = "No-Responder Recuperacion de contraseña";
            string message = "<p>Se ha solicitado el cambio de contraseña </p>" +
                $"<p>Nombre: {_user.Name} {_user.Surname} </p>" +
                $"<p>Nueva contraseña generada aleatoriamente: {password} </p>" +
                $"<p>Se recomienda cambiar la contraseña la proxima vez que inicie sesion</p>" +
                $"<p>Equipo de Prodigy Panning</p>" +
                $"<p>Por favor no responda a este mensaje.</p>";

            await _emailSender.SendEmailAsync(receiver, subject, message);
            return Ok(new
            {
                success = true,
                message = "Email de recuperacion de contraseña enviado con exito",
            });
        }

        [Authorize]
        [HttpPatch]
        [Route("UpdateUser")]
        public async Task<IActionResult> UpdateUser(User user)
        {
            bool success = true;
            string message = "Campos actualizados exitosamente";
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var token = AuthorizationHelper.ValidateToken(identity, _context);
            if (!token.success) return Unauthorized();

            User _user = token.result;

            try
            {
                var emailValidation = await _context.Users
                    .Where(x => !x.IsDeleted)
                    .FirstOrDefaultAsync(a => a.Email == user.Email);

                if (emailValidation != null && emailValidation != _user)
                {
                    throw new Exception("Ese email ya está registrado");
                }

                if (user.Name != null && user.Name != _user.Name) { _user.Name = user.Name; }
                if (user.Surname != null && user.Surname != _user.Surname) { _user.Surname = user.Surname; }
                if (user.Email != null && user.Email != _user.Email) { _user.Email = user.Email; }
                if (user.Roles != null && user.Roles == "Organizador" && _user.Roles != "[ROLE_ORGANIZER]")
                {
                    _user.Roles = "[ROLE_ORGANIZER]";
                }
                else if (user.Roles != null && _user.Roles != "[ROLE_USER]")
                {
                    _user.Roles = "[ROLE_USER]";
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                success = false;
                message = e.Message;
            }

            if (success)
            {
                return Ok(new
                {
                    success = true,
                    message = message,
                    data = _user
                });
            }
            else
            {
                return BadRequest(new
                {
                    success = false,
                    message = message
                });
            }
        }

        [Authorize]
        [HttpDelete]
        [Route("DeleteUser")]
        public async Task<IActionResult> DeleteUser()
        {
            bool success = true;
            string message = "Usuario eliminado exitosamente";
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var token = AuthorizationHelper.ValidateToken(identity, _context);
            if (!token.success) return Unauthorized(token);

            User _user = token.result;

            try
            {
                _user.IsDeleted = true;
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                success = false;
                message = e.Message;
                return BadRequest(new { success = success, message = message });
            }

            return Ok(new { success = success, message = message });
        }

        [Authorize]
        [HttpGet]
        [Route("GetOwnedEvents")]
        public async Task<IActionResult> GetOwnedEvents([FromQuery] Event evnt)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var token = AuthorizationHelper.ValidateToken(identity, _context);
            if (!token.success) return Unauthorized(token);

            User _user = token.result;

            try
            {
                List<Event> _events;
                if (evnt.IsActive == true)
                {
                    _events = await _activeEventQueryBP.Where(x => x.CreatedByNavigation == _user).ToListAsync();
                }
                else
                {
                    _events = await _listedEventQueryBP.Where(x => x.CreatedByNavigation == _user).ToListAsync();
                }

                List<EventRetrievalModel> result = new List<EventRetrievalModel>();
                foreach (Event e in _events)
                {
                    EventRetrievalModel _event = EventRetrievalHelper.CreateRetrievalModel(_context, e);
                    result.Add(_event);
                }

                return Ok(new
                {
                    success = true,
                    message = "success",
                    count = result.Count(),
                    data = result,
                });
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    success = false,
                    message = e.Message
                });
            }
        }

        [Authorize]
        [HttpPatch]
        [Route("SetPremium")]
        public async Task<IActionResult> SetPremium()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var token = AuthorizationHelper.ValidateToken(identity, _context);
            if (!token.success)
                return Unauthorized(new { success = false, message = token.message });

            User _user = token.result;

            try
            {
                _user.IsPremium = !_user.IsPremium;
                await _context.SaveChangesAsync();
                return Ok(new
                {
                    success = true,
                    message = "success",
                    data = _user,
                });
            }
            catch (Exception e)
            {
                return StatusCode(500, new { success = false, message = e.Message });
            }
        }

    }
}
