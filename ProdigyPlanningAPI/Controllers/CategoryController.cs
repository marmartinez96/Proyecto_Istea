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
        private IQueryable<Event> _activeEventQueryBP;
        private IQueryable<Event> _listedEventQueryBP;
        private IQueryable<Category> _listedCategoryQueryBP;
        public CategoryController(ProdigyPlanningContext context)
        {
            _context = context;

            _activeEventQueryBP = _context.Events.Include(x => x.CreatedByNavigation).Include(x => x.Categories).Include(x => x.Banner).Where(x => x.IsDeleted == false).Where(x => x.IsActive == true);
            _listedEventQueryBP = _context.Events.Include(x => x.CreatedByNavigation).Include(x => x.Categories).Include(x => x.Banner).Where(x => x.IsDeleted == false);

            _listedCategoryQueryBP = _context.Categories.Include(x => x.Events).Where(x => x.IsDeleted == false);
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetCategories()
        {
            bool success = true;
            string message = "Success";
            List<Category> result = null;
            try
            {
                result = await _listedCategoryQueryBP.ToListAsync();
            }
            catch (Exception e)
            {
                success = false;
                message = e.Message;
                return StatusCode(500, new { success = success, message = message });
            }

            return Ok(new { success = success, message = message, result = result });
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("GetEvents")]
        public dynamic GetEventsByCategory(Category category)
        {
            bool success = true;
            string message = "success";

            List<EventRetrievalModel> result = new List<EventRetrievalModel>();
            try
            {
                if(category.Id == 0)
                {
                    throw new Exception("Debe ingresar un id de categoria valido");
                }
                Category _category = _listedCategoryQueryBP.FirstOrDefault(c => c.Id == category.Id);
                if (_category == null)
                {
                    throw new Exception("La categoria que esta buscando no existe");
                }
                List<Event> _events = _activeEventQueryBP.Where(x => x.Categories.Contains(_category)).ToList();
                foreach (Event e in _events)
                {
                    EventRetrievalModel _event = EventRetrievalHelper.CreateRetrievalModel(_context, e);
                    result.Add(_event);
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
                data = result,
            };
        }

        [Authorize]
        [HttpPost]
        [Route("Add")]
        public async Task<IActionResult> AddCategory(Category category)
        {
            bool success = true;
            string message = "";
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var token = AuthorizationHelper.ValidateToken(identity, _context);
            if (!token.success) return Unauthorized(token);

            User user = token.result;

            if (user.Roles != "[ROLE_ADMIN]")
            {
                return Forbid("Necesita permisos de administrador para utilizar este recurso");
            }

            try
            {
                if (category.Id != 0)
                {
                    return BadRequest("El id debe estar vacio para que se otorgue uno en la base de datos");
                }
                if (string.IsNullOrWhiteSpace(category.Name))
                {
                    return BadRequest("No se puede crear una categoria con un nombre en blanco");
                }

                Category _category = await _listedCategoryQueryBP.FirstOrDefaultAsync(c => c.Name == category.Name);
                if (_category != null)
                {
                    if (_category.IsDeleted == true)
                    {
                        return BadRequest("Ya existe una categoria con ese nombre");
                    }
                    _category.IsDeleted = false;
                }
                else
                {
                    _category = new Category();
                    _category.Name = category.Name;
                    await _context.Categories.AddAsync(_category);
                }

                await _context.SaveChangesAsync();
                message = "Se ha creado la categoria " + _category.Name;
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
                    success = success,
                    message = message,
                });
            }
            else
            {
                return StatusCode(500, new
                {
                    success = success,
                    message = message,
                });
            }
        }


        [Authorize]
        [HttpPatch]
        [Route("Edit")]
        public async Task<IActionResult> EditCategory(ChangeCategoryModel changeCategoryModel)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var token = AuthorizationHelper.ValidateToken(identity, _context);
            if (!token.success) return Unauthorized(token);

            User user = token.result;

            if (user.Roles != "[ROLE_ADMIN]")
            {
                return Forbid("Necesita permisos de administrador para utilizar este recurso");
            }

            try
            {
                Category _category = await _listedCategoryQueryBP.FirstOrDefaultAsync(c => c.Id == changeCategoryModel.Id);
                if (_category == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "La categoria que desea modificar no existe"
                    });
                }

                if (changeCategoryModel.NewName == _category.Name)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "El nuevo nombre coincide con el anterior"
                    });
                }

                if (await _listedCategoryQueryBP.FirstOrDefaultAsync(c => c.Name == changeCategoryModel.NewName) != null)
                {
                    return Conflict(new
                    {
                        success = false,
                        message = "Ya existe una categoria con ese nombre"
                    });
                }

                string oldName = _category.Name;
                _category.Name = changeCategoryModel.NewName;
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "Se ha actualizado el nombre de la categoria " + oldName + " a: " + _category.Name
                });
            }
            catch (Exception e)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = e.Message
                });
            }
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
                Category _category = _listedCategoryQueryBP.FirstOrDefault(c => c.Id == category.Id);
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
