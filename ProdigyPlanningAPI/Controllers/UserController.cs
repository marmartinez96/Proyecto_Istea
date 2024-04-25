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

        [HttpPost]
        [Route("UpdatePassword")]
        public dynamic UpdatePassword(ChangePasswordModel passwordModel)
        {
            bool success = true;
            string message = "Contraseña cambiada exitosamente";
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var token = AuthorizationHelper.ValidateToken(identity, _context);
            if (!token.success) return token;
            

            User user = token.result;

            if (passwordModel.OldPassword == passwordModel.ConfirmPassword && BC.EnhancedVerify(passwordModel.OldPassword, user.Password)) 
            {
                user.Password = BC.EnhancedHashPassword(passwordModel.NewPassword, 13);
                _context.SaveChanges();
            }
            else if(passwordModel.OldPassword != passwordModel.ConfirmPassword)
            {
                success = false;
                message = "Las contraseñas no coinciden";
            }
            else if(!BC.EnhancedVerify(passwordModel.OldPassword, user.Password))
            {
                success = false;
                message = "La contraseña enviada es incorrecta";
            }

            return new
            {
                success = success,
                message = message,
            };
        }
    }
}
