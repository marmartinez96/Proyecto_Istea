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
        [Route("GetRoles")]
        public dynamic GetRoles()
        {
            bool success = true;
            string message = "Success";
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var token = AuthorizationHelper.ValidateToken(identity, _context);
            if (!token.success) return token;

            User user = token.result;

            return new
            {
                success = success,
                message = message,
                data = user.Roles
            };
        }

        [Authorize]
        [HttpGet]
        [Route("IsPremium")]
        public dynamic GetIsPremium()
        {
            bool success = true;
            string message = "Success";
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var token = AuthorizationHelper.ValidateToken(identity, _context);
            if (!token.success) return token;

            User user = token.result;

            return new
            {
                success = success,
                message = message,
                data = user.IsPremium
            };
        }

        [Authorize]
        [HttpPatch]
        [Route("UpdatePassword")]
        public dynamic UpdatePassword(ChangePasswordModel passwordModel)
        {
            bool success = true;
            string message = "Contraseña cambiada exitosamente";
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var token = AuthorizationHelper.ValidateToken(identity, _context);
            if (!token.success) return token;

            User user = token.result;
            try
            {
                if (passwordModel.OldPassword != passwordModel.ConfirmPassword)
                {
                    throw new Exception("Las contraseñas no coinciden");
                }

                if (!BC.EnhancedVerify(passwordModel.OldPassword, user.Password))
                {
                    throw new Exception("La contraseña enviada es incorrecta");
                }

                if (passwordModel.OldPassword == passwordModel.ConfirmPassword && BC.EnhancedVerify(passwordModel.OldPassword, user.Password))
                {
                    user.Password = BC.EnhancedHashPassword(passwordModel.NewPassword, 13);
                    _context.SaveChanges();
                }
            }
            catch (Exception e)
            {
                success = false;
                message = e.Message;
            }

            return new
            {
                success = success,
                message = message,
            };
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
        public dynamic UpdateUser(User user)
        {
            bool success = true;
            string message = "Campos actualizados exitosamente";
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var token = AuthorizationHelper.ValidateToken(identity, _context);
            if (!token.success) return token;
        
            User _user = token.result;
        
            try
            {
                var emailValidation = _context.Users.Where(x => x.IsDeleted == false).FirstOrDefault(a => a.Email == user.Email);

                if (emailValidation != null && emailValidation != _user)
                {
                    throw new Exception("Ese email ya esta registrado");
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
                _context.SaveChanges();
            }
            catch (Exception e)
            {
                success = false;
                message = e.Message;
            }
            
            return new
            {
                success = success,
                message = message,
                data = _user
            };
        }

        [Authorize]
        [HttpDelete]
        [Route("DeleteUser")]
        public dynamic DeleteUser()
        {
            bool success = true;
            string message = "Usuario eliminado exitosamente";
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var token = AuthorizationHelper.ValidateToken(identity, _context);
            if (!token.success) return token;

            User _user = token.result;

            try
            {
                _user.IsDeleted = true;
                _context.SaveChanges();
            }
            catch(Exception e)
            {
                success = false;
                message = e.Message;
            }

            return new
            {
                success = success,
                message = message,
            };
        }

        [Authorize]
        [HttpGet]
        [Route("GetOwnedEvents")]
        public dynamic GetOwnedEvents(Event evnt)
        {
            bool success = true;
            string message = "success";
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var token = AuthorizationHelper.ValidateToken(identity, _context);
            if (!token.success) return token;

            User _user = token.result;

            List<EventRetrievalModel> result = new List<EventRetrievalModel>();
            try
            {
                List<Event> _events = new List<Event>();
                if (evnt.IsActive == true) 
                {
                    _events = _activeEventQueryBP.Where(x => x.CreatedByNavigation == _user).ToList();
                }
                else
                {
                    _events = _listedEventQueryBP.Where(x => x.CreatedByNavigation == _user).ToList();
                }
                foreach (Event e in _events)
                {
                    EventRetrievalModel _event = EventRetrievalHelper.CreateRetrievalModel(_context, e);
                    result.Add(_event);
                }
            }
            catch(Exception e)
            {
                success = false;
                message = e.Message; 
            }
            return new
            {
                success = success,
                message = message,
                count = result.Count(),
                data = result,
            };
        }

        [Authorize]
        [HttpPatch]
        [Route("SetPremium")]
        public dynamic SetPremium()
        {
            bool success = true;
            string message = "success";
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var token = AuthorizationHelper.ValidateToken(identity, _context);
            if (!token.success) return token;

            User _user = token.result;

            try
            {
                _user.IsPremium = !_user.IsPremium;
                _context.SaveChanges();
            }
            catch (Exception e)
            {
                success = false;
                message = e.Message;
            }
            return new
            {
                success = success,
                message = message,
                data = _user,
            };
        }
    }
}
