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
using SixLabors.ImageSharp.Formats.Png;
using System.Data;
using System.Diagnostics.Tracing;
using System.Security.Claims;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing;

namespace ProdigyPlanningAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]

    public class EventController : Controller
    {
        private readonly ProdigyPlanningContext _context;
        private IQueryable<Event> _activeEventQueryBP;
        private IQueryable<Event> _listedEventQueryBP;
        private IQueryable<Category> _listedCategoryQueryBP;
        public EventController(ProdigyPlanningContext context)
        {
            _context = context;

            _activeEventQueryBP = _context.Events.Include(x => x.CreatedByNavigation).Include(x => x.Categories).Include(x=> x.Banner).Where(x => x.IsDeleted == false).Where(x => x.IsActive == true);
            _listedEventQueryBP = _context.Events.Include(x => x.CreatedByNavigation).Include(x => x.Categories).Include(x => x.Banner).Where(x => x.IsDeleted == false);

            _listedCategoryQueryBP = _context.Categories.Include(x => x.Events).Where(x => x.IsDeleted == false);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("/events")]
        public async Task<IActionResult> GetEvents()
        {
            try
            {
                var result = new List<EventRetrievalModel>();
                var events = await _listedEventQueryBP.OrderBy(x => x.Date).ThenBy(x => x.IsFeatured).ToListAsync();

                foreach (var e in events)
                {
                    var eventResult = EventRetrievalHelper.CreateRetrievalModel(_context, e);
                    result.Add(eventResult);
                }

                return Ok(new
                {
                    success = true,
                    message = "Success",
                    count = result.Count,
                    result = result
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

        [AllowAnonymous]
        [HttpGet]
        [Route("GetFeatured")]
        public async Task<IActionResult> GetFeatured()
        {
            bool success = true;
            string message = "Success";
            List<EventRetrievalModel> result = new List<EventRetrievalModel>();

            try
            {
                List<Event> _events = await _activeEventQueryBP.Where(x => x.IsFeatured == true).ToListAsync();

                foreach (Event e in _events)
                {
                    result.Add(EventRetrievalHelper.CreateRetrievalModel(_context, e));
                }
            }
            catch (Exception e)
            {
                success = false;
                message = e.Message;
                return StatusCode(500, new { success = success, message = message });
            }

            return Ok(new
            {
                success = success,
                message = message,
                count = result.Count(),
                result = result
            });
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("GetById")]
        public async Task<IActionResult> GetEventById(Event evnt)
        {
            bool success = true;
            string message = "Success";
            EventRetrievalModel result = null;
            try
            {
                if (evnt.Id == null || evnt.Id <= 0)
                {
                    return BadRequest(new { success = false, message = "Debe enviar un numero id valido" });
                }
                
                Event _event = await _activeEventQueryBP.FirstOrDefaultAsync(x => x.Id == evnt.Id);
                if (evnt.Id == null)
                {
                    return NotFound(new { success = false, message = "No se encontró ese evento en la base de datos" });
                }

                result = EventRetrievalHelper.CreateRetrievalModel(_context, _event);
            }
            catch (Exception e)
            {
                success = false;
                message = e.Message;
            }

            return Ok(new
            {
                success = success,
                message = message,
                result = result
            });
        }

        //
        //Este metodo no puede hacer uso de los queryBP porque necesita modificar los filtros dinamicamente y no queremos alterar los BP a nivel clase.
        //
        [AllowAnonymous]
        [HttpGet]
        [Route("GetByFilters")]
        public async Task<IActionResult> GetByfilter(FilterEventModel filter)
        {
            bool success = true;
            string message = "Success";
            List<EventRetrievalModel> result = new List<EventRetrievalModel>();
            try
            {
                IQueryable<Event> query = _context.Events
                    .Include(x => x.CreatedByNavigation)
                    .Include(x => x.Categories)
                    .Include(x => x.Banner)
                    .Where(x => x.IsDeleted == false);

                if (filter.Name != null)
                {
                    query = query.Where(x => x.Name == filter.Name);
                }

                if (filter.CategoryId != null)
                {
                    Category _cat = await _listedCategoryQueryBP.FirstOrDefaultAsync(x => x.Id == filter.CategoryId);
                    if (_cat == null)
                    {
                        return NotFound(new { success = false, message = "La categoria que intenta buscar no existe" });
                    }
                    query = query.Where(x => x.Categories.Contains(_cat));
                }
                
                if (filter.FromDate != DateOnly.MinValue)
                {
                    query = query.Where(x => x.Date >= filter.FromDate);
                }

                if (filter.ToDate != DateOnly.MaxValue)
                {
                    query = query.Where(x => x.Date <= filter.ToDate);
                }

                if (filter.IsActive == true)
                {
                    query = query.Where(x => x.IsActive == true);
                }

                var events = await query.ToListAsync();

                foreach (Event e in events)
                {
                    result.Add(EventRetrievalHelper.CreateRetrievalModel(_context, e));
                }
            }
            catch (Exception e)
            {
                success = false;
                message = e.Message;
            }
            return Ok(new
            {
                success = success,
                message = message,
                count = result.Count,
                result = result
            });
        }

        ///
        ///Estoy seguro que aca faltan un monton de validaciones
        ///
        [AllowAnonymous]
        [HttpGet]
        [Route("GetByPeriod")]
        public async Task<IActionResult> GetEventByPeriod(PeriodModel? model)
        {
            bool success = true;
            string message = "Success";
            string cdPeriod;
            List<EventRetrievalModel> result = new List<EventRetrievalModel>();
            try
            {
                if (model == null || model.cd == null || model.cd.Trim() == "" || model.cd.Count() > 6)
                {
                    model = new PeriodModel();
                }
                cdPeriod = model.cd.Trim();

                DateOnly? firstDayPeriod = cdPeriodToDateTime(cdPeriod);
                DateOnly? lastDayPeriod = cdPeriodToDateTime(cdPeriod).AddMonths(1).AddDays(-1);

                List<Event> _events = await _listedEventQueryBP
                    .Where(x => x.Date >= firstDayPeriod)
                    .Where(x => x.Date <= lastDayPeriod)
                    .ToListAsync();

                foreach (Event e in _events)
                {
                    result.Add(EventRetrievalHelper.CreateRetrievalModel(_context, e));
                }
            }
            catch (Exception e)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = e.Message,
                    count = result.Count,
                    result = result
                });
            }
            return Ok(new
            {
                success = success,
                message = message,
                count = result.Count(),
                result = result
            });
        }

        [Authorize]
        [HttpPost]
        [Route("Add")]
        public async Task<IActionResult> AddEventAsync(AddEventModel evnt)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var token = AuthorizationHelper.ValidateToken(identity, _context);
            if (!token.success) return Unauthorized(token);

            User user = token.result;

            if (user.Roles != "[ROLE_ORGANIZER]" && user.Roles != "[ROLE_ADMIN]")
            {
                return Forbid("Necesita permisos de organizador para utilizar este recurso");
            }
            try
            {
                int currentMonth = DateTime.Now.Month;
                int currentYear = DateTime.Now.Year;
                DateTime firstDayOfMonth = new DateTime(currentYear, currentMonth, 1);
                DateTime lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddTicks(-1);
                int userMonthlyEventCount = await _activeEventQueryBP
                    .Where(x => x.CreatedByNavigation == user)
                    .Where(x => x.CreatedAt >= firstDayOfMonth)
                    .Where(x => x.CreatedAt <= lastDayOfMonth)
                    .CountAsync();

                if (user.IsPremium == false && userMonthlyEventCount > 3)
                {
                    return BadRequest("Se alcanzó el límite de eventos creados para usuarios gratuitos");
                }
                if (string.IsNullOrWhiteSpace(evnt.Name))
                {
                    return BadRequest("El campo nombre no puede estar vacío");
                }
                if (string.IsNullOrWhiteSpace(evnt.Location))
                {
                    return BadRequest("El campo ubicación no puede estar vacío");
                }
                if (evnt.Date == DateOnly.MinValue)
                {
                    return BadRequest("El campo fecha no puede estar vacío");
                }
                if (evnt.Time == TimeOnly.MinValue)
                {
                    return BadRequest("El campo horario no puede estar vacío");
                }
                if (evnt.CategoryId == null)
                {
                    return BadRequest("El campo categoría no puede estar vacío");
                }

                Event _event = new Event
                {
                    Name = evnt.Name,
                    Location = evnt.Location,
                    Date = evnt.Date,
                    Time = evnt.Time,
                    Description = evnt.Description,
                    CreatedBy = user.Id,
                    CreatedByNavigation = user,
                    Duration = evnt.Duration
                };
                user.Events.Add(_event);

                Category _cat = await _listedCategoryQueryBP
                    .FirstOrDefaultAsync(x => x.Id == evnt.CategoryId);
                if (_cat == null)
                {
                    return NotFound("No se encontró la categoría con id:" + evnt.CategoryId);
                }

                _event.Categories.Add(_cat);
                _cat.Events.Add(_event);
                _context.Events.Add(_event);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "Se ha creado el evento " + _event.Name
                });
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    success = false,
                    message = "Error interno del servidor",
                    error = e.Message
                });
            }
        }



        [Authorize]
        [HttpPatch]
        [Route("Edit")]
        public async Task<IActionResult> EditEventAsync(ChangeEventModel changeEventModel)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var token = AuthorizationHelper.ValidateToken(identity, _context);
            if (!token.success)
            {
                return Unauthorized(new { success = false, message = "Token inválido" });
            }

            User user = token.result;

            if (user.Roles != "[ROLE_ORGANIZER]" && user.Roles != "[ROLE_ADMIN]")
            {
                return Forbid("Necesita permisos de organizador para utilizar este recurso");
            }

            Event _event = null;
            try
            {
                _event = await _listedEventQueryBP.FirstOrDefaultAsync(c => c.Id == changeEventModel.Id);
                if (_event == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "El evento que desea modificar no existe"
                    });
                }

                if (_event.CreatedByNavigation != user)
                {
                    return Forbid("El evento solo puede ser modificado por su creador");
                }
                
                if (changeEventModel.NewName != null && changeEventModel.NewName != _event.Name) { _event.Name = changeEventModel.NewName; }
                if (changeEventModel.NewDescription != null && changeEventModel.NewDescription != _event.Description) { _event.Description = changeEventModel.NewDescription; }
                if (changeEventModel.NewDate != DateOnly.MinValue && changeEventModel.NewDate != _event.Date) { _event.Date = changeEventModel.NewDate; }
                if (changeEventModel.NewTime != TimeOnly.MinValue && changeEventModel.NewTime != _event.Time) { _event.Time = changeEventModel.NewTime; }
                if (changeEventModel.NewLocation != null && changeEventModel.NewLocation != _event.Location) { _event.Location = changeEventModel.NewLocation; }
                if (changeEventModel.NewDuration != null && changeEventModel.NewDuration != _event.Duration) { _event.Duration = changeEventModel.NewDuration; }

                if (changeEventModel.NewCategoryId != null)
                {
                    Category _cat = await _listedCategoryQueryBP.FirstOrDefaultAsync(x => x.Id == changeEventModel.NewCategoryId);
                    if (_cat == null)
                    {
                        return NotFound(new
                        {
                            success = false,
                            message = "No se encontró la categoría con id: " + changeEventModel.NewCategoryId
                        });
                    }

                    if (!_event.Categories.Contains(_cat))
                    {
                        _event.Categories.Add(_cat);
                        _cat.Events.Add(_event);
                    }
                }

                await _context.SaveChangesAsync();
                return Ok(new
                {
                    success = true,
                    message = "Se ha actualizado el evento " + _event.Name,
                    data = EventRetrievalHelper.CreateRetrievalModel(_context, _event)
                });
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    success = false,
                    message = "Error interno del servidor",
                    error = e.Message
                });
            }
        }

        [Authorize]
        [HttpPatch]
        [Route("SetFeatured")]
        public dynamic SetFeatured(Event evnt)
        {
            bool success = true;
            string message = "";
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var token = AuthorizationHelper.ValidateToken(identity, _context);
            if (!token.success) return token;

            User user = token.result;
            Event _event = new Event();
            try
            {
                if (evnt.Id == 0)
                {
                    throw new Exception("Debe enviar un id para identificar al evento");
                }
                _event = _activeEventQueryBP.FirstOrDefault(c => c.Id == evnt.Id);
                if (_event == null)
                {
                    throw new Exception("El evento que desea destacar no existe");
                }
                if (_event.CreatedByNavigation != user)
                {
                    throw new Exception("El evento solo puede ser destacado por su creador");
                }
                if (!user.IsPremium)
                {
                    throw new Exception("Solo los usuarios premiun pueden destacar eventos");
                }
                _event.IsFeatured = !_event.IsFeatured;
                int userFeaturedEvents = _activeEventQueryBP.Where(x => x.CreatedByNavigation == user).Count();
                if (userFeaturedEvents > 2 && _event.IsFeatured == true)
                {
                    throw new Exception("Solo se puede destacar un maximo de tres eventos");
                }
                _context.SaveChanges();
                if (_event.IsFeatured)
                {
                    message = "Se destaco el evento " + _event.Name;
                }
                else
                {
                    message = "Se quito el destacado al evento " + _event.Name;
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
                data = EventRetrievalHelper.CreateRetrievalModel(_context, _event)
            };
        }


        [Authorize]
        [HttpDelete]
        [Route("Delete")]
        public async Task<IActionResult> DeleteEventAsync(Event evnt)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var token = AuthorizationHelper.ValidateToken(identity, _context);
            if (!token.success)
            {
                return Unauthorized(new { success = false, message = "Token inválido" });
            }

            User user = token.result;

            if (user.Roles != "[ROLE_ORGANIZER]" && user.Roles != "[ROLE_ADMIN]")
            {
                return Forbid("Necesita permisos de organizador para utilizar este recurso");
            }

            try
            {
                if (evnt.Id == 0)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Es obligatorio proveer un id para identificar el evento a eliminar"
                    });
                }

                Event _event = await _listedEventQueryBP.FirstOrDefaultAsync(c => c.Id == evnt.Id);
                if (_event == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "El evento que desea eliminar no existe"
                    });
                }

                if (_event.CreatedByNavigation != user)
                {
                    return Forbid("El evento solo puede ser eliminado por su creador");
                }
                
                _event.IsDeleted = true;
                _event.IsFeatured = false;
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "Se ha eliminado el evento " + _event.Name
                });
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    success = false,
                    message = "Error interno del servidor",
                    error = e.Message
                });
            }
        }

        [Authorize]
        [HttpPost]
        [Route("AddBanner")]
        public dynamic AddBanner([FromForm] IFormFile formFile, [FromForm] int id)
        {
            bool success = true;
            string message = "El banner fue agregado con exito";
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var token = AuthorizationHelper.ValidateToken(identity, _context);
            if (!token.success) return token;

            User user = token.result;
            EventRetrievalModel result = null; 

            try
            {
                Event _event = _activeEventQueryBP.FirstOrDefault(x => x.Id == id);
                if (formFile == null)
                {
                    throw new Exception("Debe enviar una imagen valida");
                }
                if (_event == null)
                {
                    throw new Exception("Debe enviar un id de evento valido");
                }
                if (user != _event.CreatedByNavigation)
                {
                    throw new Exception("Solo el organizador de un evento puedo modificar el banner");
                }
                EventBanner _eventBanner = _context.EventBanners.FirstOrDefault(x => x.EventId == _event.Id);
                if (_eventBanner != null)
                {
                    _context.EventBanners.Remove(_eventBanner);
                }

                var encoder = new PngEncoder();
                using (var stream = formFile.OpenReadStream())
                {
                    using (var output = new MemoryStream())
                    using (Image image = Image.Load(stream))
                    {
                        if(image.Height > 90 || image.Width > 728) { throw new Exception("La imagen debe tener un tamaño de 728x90 como maximo"); }

                        _eventBanner = new EventBanner()
                        {
                            EventId = _event.Id,
                            EventNavigation = _event,
                            EventImage = output.ToArray()
                        };

                        _context.EventBanners.Add(_eventBanner);
                        _context.SaveChanges();
                    }
                }
                _event.Banner = _eventBanner;
                _context.SaveChanges();
                result = EventRetrievalHelper.CreateRetrievalModel(_context, _event);
                //using (MemoryStream stream = new MemoryStream())
                //{
                //    formFile.CopyTo(stream);
                //    _eventBanner = new EventBanner()
                //    {
                //        EventId = _event.Id,
                //        EventNavigation = _event,
                //        EventImage = stream.ToArray()
                //    };
                //
                //    _context.EventBanners.Add(_eventBanner);
                //    _context.SaveChanges();
                //}
                //_event.Banner = _eventBanner;
                //_context.SaveChanges();
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
                result= result
            };
        }

        [Authorize]
        [HttpDelete]
        [Route("RemoveBanner")]
        public dynamic RemoveBanner(Event evnt)
        {
            bool success = true;
            string message = "El banner fue eliminado con exito";
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var token = AuthorizationHelper.ValidateToken(identity, _context);
            if (!token.success) return token;

            User user = token.result;

            try
            {

                Event _event = _activeEventQueryBP.FirstOrDefault(x => x.Id == evnt.Id);
                if (_event == null)
                {
                    throw new Exception("Debe enviar un id de evento valido");
                }
                if (_event.Banner == null)
                {
                    throw new Exception("El evento enviado no cuenta con un banner");
                }
                EventBanner eventBanner = _event.Banner;
                if (eventBanner == null)
                {
                    throw new Exception("El evento enviado no cuenta con un banner");
                }
                if (user != _event.CreatedByNavigation)
                {
                    throw new Exception("El banner del evento solo puede ser eliminado por su organizador");
                }
                _event.Banner = null;
                _context.EventBanners.Remove(eventBanner);
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
            };
        }

        [HttpGet]
        [Route("GetBanner")]
        public dynamic GetBanner(Event evnt)
        {
            bool success = true;
            string message = "Imagen recuperada con exito";
            string data = "";

            try
            {
                Event _event = _activeEventQueryBP.FirstOrDefault(x => x.Id == evnt.Id);
                if (_event == null || evnt.Id == null)
                {
                    throw new Exception("Debe enviar un id de evento valido");
                }
                EventBanner image = _event.Banner;
                if (image == null)
                {
                    throw new Exception("El evento que envio no tiene un banner cargado");
                }
                data = Convert.ToBase64String(image.EventImage);
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
                data = data
            };
        }

        [Authorize]
        [HttpPatch]
        [Route("AddCategory")]
        public dynamic AddCategory(EventCategoryModel evnt) 
        {
            bool success = true;
            string message = "La categoria fue agregada al evento con exito";
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var token = AuthorizationHelper.ValidateToken(identity, _context);
            if (!token.success) return token;

            User user = token.result;

            try
            {
                Event _evnt = _activeEventQueryBP.FirstOrDefault(x => x.Id == evnt.Id);
                if (_evnt == null)
                {
                    throw new Exception("Debe enviar un id de evento valido");
                }
                if (user != _evnt.CreatedByNavigation)
                {
                    throw new Exception("Solo el organizador puede modificar las categorias de un evento");
                }

                Category _cat = _listedCategoryQueryBP.FirstOrDefault(x => x.Id == evnt.CategoryId);
                if (_cat == null)
                {
                    throw new Exception("La categoria que desea agregar no existe");
                }
                if (_evnt.Categories.Contains(_cat))
                {
                    throw new Exception("Este evento ya contiene la categoria "+_cat.Name);
                }
                if (_evnt.Categories.Count()>1)
                {
                    throw new Exception("Este evento ya pertenece al numero maximo de categorias posibles. Quite una categoria y vuelva a intentar");
                }
                _evnt.Categories.Add(_cat);
                _cat.Events.Add(_evnt);
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
            };
        }

        [Authorize]
        [HttpPatch]
        [Route("RemoveCategory")]
        public dynamic RemoveCategory(EventCategoryModel evnt)
        {
            bool success = true;
            string message = "La categoria fue quitada del evento con exito";
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var token = AuthorizationHelper.ValidateToken(identity, _context);
            if (!token.success) return token;

            User user = token.result;

            try
            {
                Event _evnt = _activeEventQueryBP.FirstOrDefault(x => x.Id == evnt.Id);
                if (_evnt == null)
                {
                    throw new Exception("Debe enviar un id de evento valido");
                }
                if (user != _evnt.CreatedByNavigation)
                {
                    throw new Exception("Solo el organizador puede modificar las categorias de un evento");
                }
                Category _cat = _listedCategoryQueryBP.FirstOrDefault(x => x.Id == evnt.CategoryId);
                if (_cat == null)
                {
                    throw new Exception("La categoria que desea eliminar del evento no existe");
                }
                if (!_evnt.Categories.Contains(_cat))
                {
                    throw new Exception("El evento no contiene la categoria que desea eliminar");
                }
                if (_evnt.Categories.Count() < 2)
                {
                    throw new Exception("Si se quita la categoria "+_cat.Name+" el evento se qeudaria sin categorias. Agrege por lo menos una categoria mas antes de quitar esta o utilice el metodo de reemplazo.");
                }
                _evnt.Categories.Remove(_cat);
                _cat.Events.Remove(_evnt);
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
            };
        }

        [Authorize]
        [HttpPatch]
        [Route("ReplaceCategory")]
        public dynamic ReplaceCategory(EventCategoryReplaceModel evnt)
        {
            bool success = true;
            string message = "";
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var token = AuthorizationHelper.ValidateToken(identity, _context);
            if (!token.success) return token;

            User user = token.result;

            Event _evnt = null;
            try
            {
                _evnt = _activeEventQueryBP.FirstOrDefault(x => x.Id == evnt.EventId);
                if (_evnt == null)
                {
                    throw new Exception("Debe enviar un id de evento valido");
                }
                if (user != _evnt.CreatedByNavigation)
                {
                    throw new Exception("Solo el organizador puede modificar las categorias de un evento");
                }

                Category _cat0 = _listedCategoryQueryBP.FirstOrDefault(x => x.Id == evnt.ReplaceId);
                if (_cat0 == null)
                {
                    throw new Exception("La categoria que desea reemplazar no existe");
                }
                if (!_evnt.Categories.Contains(_cat0))
                {
                    throw new Exception("Este evento no contiene la categoria " + _cat0.Name);
                }

                Category _cat1 = _listedCategoryQueryBP.FirstOrDefault(x => x.Id == evnt.ReplacementId);
                if (_cat1 == null)
                {
                    throw new Exception("La categoria con la que desea reemplazar a "+ _cat0.Name+" no existe");
                }
                if (_evnt.Categories.Contains(_cat1))
                {
                    throw new Exception("Este evento ya contiene la categoria " + _cat1.Name);
                }

                _evnt.Categories.Remove(_cat0);
                _cat0.Events.Remove(_evnt);

                _evnt.Categories.Add(_cat1);
                _cat1.Events.Add(_evnt);

                message = "Se reemplazo la categoria " + _cat0.Name + " a " + _cat1.Name;
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
                data = EventRetrievalHelper.CreateRetrievalModel(_context, _evnt)
            };
        }

        private DateOnly cdPeriodToDateTime(string cdPeriod)
        {
            DateOnly result = DateOnly.MinValue;
            try
            {
                int month;
                int year;
                int.TryParse(cdPeriod.Substring(0, 2), out month);
                int.TryParse(cdPeriod.Substring(2), out year);
                result = new DateOnly(year, month, 1);
            }
            catch (Exception e)
            {
                throw new Exception("Hubo un error tratando de convertir el periodo");
            }
            return result;
        }

    }

}