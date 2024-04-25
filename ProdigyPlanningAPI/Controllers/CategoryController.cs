using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using ProdigyPlanningAPI.Data;
using ProdigyPlanningAPI.Helpers;
using ProdigyPlanningAPI.Models;
using System.Security.Claims;

namespace ProdigyPlanningAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly ProdigyPlanningContext _context;
        public CategoryController(ProdigyPlanningContext context)
        {
            _context = context;
        }

        [Authorize]
        [HttpGet]
        public dynamic GetCategories() 
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var token = AuthorizationHelper.ValidateToken(identity, _context);
            if (!token.success) return token;

            User user = token.result;

            if (user.Roles != "[ROLE_ORGANIZER]")
            {
                return new
                {
                    success = false,
                    message = "No tiene permisos para ver este recurso",
                    result = ""
                };
            }

            return new
            {
                success = true,
                message = "",
                result = _context.Categories.ToList()
            };
        }
    }
}
