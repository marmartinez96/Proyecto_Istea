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
    public class CategoryController : ControllerBase
    {
        private readonly ProdigyPlanningContext _context;
        public CategoryController(ProdigyPlanningContext context)
        {
            _context = context;
        }

        [AllowAnonymous]
        [HttpGet]
        public dynamic GetCategories() 
        {
            bool success = true;
            string message = "Success";
            List<Category> result = null;
            try
            {
                result = _context.Categories.Where(x=> x.IsDeleted == false).ToList();
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
                result = result
            };
        }

        [Authorize]
        [HttpPost]
        [Route("Add")]
        public dynamic AddCategory(Category category) 
        {
            bool success = true;
            string message = "";
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var token = AuthorizationHelper.ValidateToken(identity, _context);
            if (!token.success) return token;

            User user = token.result;

            if (user.Roles != "[ROLE_ADMIN]")
            {
                return new
                {
                    success = false,
                    message = "Necesita permisos de administrador para utilizar este recurso",
                };
            }

            try
            { 
                if(category.Name == null || category.Name.Trim() == "")
                {
                    throw new Exception("No se puede crear una categoria con un nombre en blanco");
                }
                if(_context.Categories.FirstOrDefault(c => c.Name == category.Name) != null)
                {
                    throw new Exception("Ya existe una categoria con ese nombre");
                }

                Category _category = new Category();
                _category.Name = category.Name;
                _context.Categories.Add(_category);
                _context.SaveChanges();
                message = "Se ha creado la categoria " + _category.Name;
            }
            catch (Exception e)
            {
                success= false;
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
        [Route("Edit")]
        public dynamic EditCategory(ChangeCategoryModel changeCategoryModel)
        {
            bool success = true;
            string message = "";
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var token = AuthorizationHelper.ValidateToken(identity, _context);
            if (!token.success) return token;

            User user = token.result;

            if (user.Roles != "[ROLE_ADMIN]")
            {
                return new
                {
                    success = false,
                    message = "Necesita permisos de administrador para utilizar este recurso",
                };
            }

            try
            {
                Category _category = _context.Categories.Where(x => x.IsDeleted == false).FirstOrDefault(c => c.Name == changeCategoryModel.OldName);
                if (_category == null) 
                {
                    throw new Exception("La categoria que desea modificar no existe");
                }
                if (changeCategoryModel.NewName == _category.Name)
                {
                    throw new Exception("El nuevo nombre coincide con el anterior");
                }
                if (_context.Categories.Where(x => x.IsDeleted == false).FirstOrDefault(c => c.Name == changeCategoryModel.NewName) != null)
                {
                    throw new Exception("Ya existe una categoria con ese nombre");
                }
                _category.Name = changeCategoryModel.NewName;
                _context.SaveChanges();
                message = "Se ha actualizado el nombre de la categoria " + changeCategoryModel.OldName + " a: " + _category.Name;
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
        [HttpDelete]
        [Route("Delete")]
        public dynamic DeleteCategory(Category category)
        {
            bool success = true;
            string message = "";
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var token = AuthorizationHelper.ValidateToken(identity, _context);
            if (!token.success) return token;

            User user = token.result;

            if (user.Roles != "[ROLE_ADMIN]")
            {
                return new
                {
                    success = false,
                    message = "Necesita permisos de administrador para utilizar este recurso",
                };
            }
            try
            {
                Category _category = _context.Categories.Where(x => x.IsDeleted == false).FirstOrDefault(c => c.Name == category.Name);
                if (_category == null)
                {
                    throw new Exception("La categoria que desea eliminar no existe");
                }
                _category.IsDeleted= true;
                _context.SaveChanges();
                message = "Se ha eliminado la categoria " + category.Name;
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
    }
}
