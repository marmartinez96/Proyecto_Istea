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
        public UserController(ProdigyPlanningContext context)
        {
            _context = context;
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
    }
}
