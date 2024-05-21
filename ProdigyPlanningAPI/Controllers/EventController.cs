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
using System.Data;
using System.Diagnostics.Tracing;
using System.Security.Claims;

namespace ProdigyPlanningAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]

    public class EventController : Controller
    {
        private readonly ProdigyPlanningContext _context;
        public EventController(ProdigyPlanningContext context)
        {
            _context = context;
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("/events")]
        public dynamic GetEvents()
        {
            bool success = true;
            string message = "Success";
            List<EventRetrievalModel> result = new List<EventRetrievalModel>();
            try
            {
                List<Event> _result = _context.Events.Include(x => x.CreatedByNavigation).Include(x => x.Categories).Where(x=> x.IsDeleted == false).OrderBy(x=> x.Date).ToList();
                foreach (Event e in _result)
                {
                    EventRetrievalModel eventResult = EventRetrievalHelper.CreateRetrievalModel(_context, e);
                    result.Add(eventResult);
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
                count = result.Count(),
                result = result
            };
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("GetFeatured")]
        public dynamic GetFeatured()
        {
            bool success = true;
            string message = "Success";
            List<EventRetrievalModel> result = new List<EventRetrievalModel>();
            try
            {
                List<Event> _events = _context.Events.Include(x => x.Banner).Include(x => x.CreatedByNavigation).Include(x => x.Categories).Where(x => x.IsDeleted == false).Where(x=> x.IsFeatured == true).ToList();
                
                foreach(Event e in _events)
                {
                    result.Add(EventRetrievalHelper.CreateRetrievalModel(_context, e));
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
                count = result.Count(),
                result = result
            };
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("GetById")]
        public dynamic GetEventById(Event evnt)
        {
            bool success = true;
            string message = "Success";
            EventRetrievalModel result = null;
            try
            {
                if(evnt.Id == null || evnt.Id <= 0)
                {
                    throw new Exception("Debe enviar un numero id valido");
                }
                Event _event = _context.Events.Include(x => x.Banner).Include(x => x.CreatedByNavigation).Include(x=> x.Categories).Where(x => x.IsDeleted == false).FirstOrDefault(x => x.Id == evnt.Id);
                if (_event == null)
                {
                    throw new Exception("No se encontro ese evento en la base de datos");
                }
                result = EventRetrievalHelper.CreateRetrievalModel(_context, _event);
                
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

        [AllowAnonymous]
        [HttpGet]
        [Route("GetByFilters")]
        public dynamic GetByfilter(FilterEventModel filter)
        {
            bool success = true;
            string message = "Success";
            List<EventRetrievalModel> result = new List<EventRetrievalModel>();
            try
            {
                IQueryable<Event> query = _context.Events.Include(x => x.CreatedByNavigation).Include(x => x.Categories).Include(x=> x.Banner).Where(x=> x.IsDeleted == false);
                if (filter.Name != null)
                {
                    query = query.Where(x=> x.IsDeleted == false).Where(x => x.Name == filter.Name);
                }
                if (filter.CategoryId != null)
                {
                    Category _cat = _context.Categories.FirstOrDefault(x => x.Id == filter.CategoryId);
                    if (_cat == null)
                    {
                        throw new Exception("La categoria que intenta buscar no existe");
                    }
                    query = query.Where(x => x.IsDeleted == false).Where(x => x.Categories.Contains(_cat));
                }
                if (filter.FromDate != DateOnly.MinValue)
                {
                    query = query.Where(x=> x.Date>= filter.FromDate);
                }
                if (filter.ToDate!= DateOnly.MaxValue) 
                {
                    query = query.Where(x=> x.Date<= filter.ToDate);
                }

                foreach (Event e in query.ToList())
                {
                    result.Add(EventRetrievalHelper.CreateRetrievalModel(_context, e));
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
                count = result.Count(),
                result = result
            };
        }

        ///
        ///Estoy seguro que aca faltan un monton de validaciones
        ///
        [AllowAnonymous]
        [HttpGet]
        [Route("GetByPeriod")]
        public dynamic GetEventByPeriod(PeriodModel? model)
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

                List<Event> _events = _context.Events.Include(x => x.Banner).
                    Include(x => x.CreatedByNavigation).
                    Include(x => x.Categories).
                    Where(x => x.IsDeleted == false).
                    Where(x => x.Date >= firstDayPeriod).
                    Where(x=> x.Date <= lastDayPeriod).
                    ToList();

                foreach (Event e in _events)
                {
                    result.Add(EventRetrievalHelper.CreateRetrievalModel(_context, e));
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
                count = result.Count(),
                result = result
            };
        }

        [Authorize]
        [HttpPost]
        [Route("Add")]
        public dynamic AddEvent(AddEventModel evnt)
        {
            bool success = true;
            string message = "";
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var token = AuthorizationHelper.ValidateToken(identity, _context);
            if (!token.success) return token;

            User user = token.result;

            if (user.Roles != "[ROLE_ORGANIZER]" && user.Roles != "[ROLE_ADMIN]")
            {
                return new
                {
                    success = false,
                    message = "Necesita permisos de organizador para utilizar este recurso",
                };
            }
            try
            {
                int currentMonth = DateTime.Now.Month;
                int currentYear = DateTime.Now.Year;
                DateTime firstDayOfMonth = new DateTime(currentYear, currentMonth, 1);
                DateTime lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddTicks(-1);
                int userMonthlyEventCount = _context.Events.Include(x => x.CreatedByNavigation).Where(x => x.IsDeleted == false).Where(x => x.CreatedByNavigation == user).Where(x=> x.CreatedAt >= firstDayOfMonth).Where(x=> x.CreatedAt <= lastDayOfMonth).Count();

                if (user.IsPremium == false && userMonthlyEventCount > 3)
                {
                    throw new Exception("Se alcanzo el limite de eventos creados para usuarios gratuitos");
                }
                if (evnt.Name == null || evnt.Name.Trim() == "")
                {
                    throw new Exception("El campo nombre no puede estar vacio");
                }
                if (evnt.Location == null || evnt.Location.Trim() == "")
                {
                    throw new Exception("El campo ubicacion no puede estar vacio");
                }
                if (evnt.Date == DateOnly.MinValue)
                {
                    throw new Exception("El campo fecha no puede estar vacio");
                }
                if (evnt.Time == TimeOnly.MinValue)
                {
                    throw new Exception("El campo horario no puede estar vacio");
                }
                if (evnt.CategoryId == null)
                {
                    throw new Exception("El campo categoria no puede estar vacio");
                }

                Event _event = new Event();
                _event.Name = evnt.Name;
                _event.Location = evnt.Location;
                _event.Date= evnt.Date;
                _event.Time= evnt.Time;
                _event.Description = evnt.Description;
                _event.CreatedBy= user.Id;
                _event.CreatedByNavigation = user;
                _event.Duration = evnt.Duration;
                user.Events.Add(_event);

                Category _cat = _context.Categories.Where(x => x.IsDeleted == false).FirstOrDefault(x => x.Id == evnt.CategoryId);
                if(_cat == null )
                {
                    throw new Exception("No se encontro la categoria con id:"+evnt.CategoryId);
                }

                _event.Categories.Add(_cat);
                _cat.Events.Add(_event);
                _context.Events.Add(_event);
                _context.SaveChanges();
                message = "Se ha creado el evento " + _event.Name;
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
        [Route("Edit")]
        public dynamic EditEvent(ChangeEventModel changeEventModel)
        {
            bool success = true;
            string message = "";
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var token = AuthorizationHelper.ValidateToken(identity, _context);
            if (!token.success) return token;

            User user = token.result;

            if (user.Roles != "[ROLE_ORGANIZER]" && user.Roles != "[ROLE_ADMIN]")
            {
                return new
                {
                    success = false,
                    message = "Necesita permisos de organizador para utilizar este recurso",
                };
            }
            Event _event = null;
            try
            {
                _event = _context.Events.Where(x => x.IsDeleted == false).FirstOrDefault(c => c.Id == changeEventModel.Id);
                if (_event == null)
                {
                    throw new Exception("El evento que desea modificar no existe");
                }
                if (_event.CreatedByNavigation != user)
                {
                    throw new Exception("El evento solo puede ser modificado por su creador");
                }
                if(changeEventModel.NewName != null && changeEventModel.NewName != _event.Name) { _event.Name = changeEventModel.NewName; }
                if(changeEventModel.NewDescription != null && changeEventModel.NewDescription != _event.Description) { _event.Description = changeEventModel.NewDescription;}
                if(changeEventModel.NewDate != DateOnly.MinValue && changeEventModel.NewDate != _event.Date) { _event.Date = changeEventModel.NewDate;}
                if(changeEventModel.NewTime != TimeOnly.MinValue && changeEventModel.NewTime != _event.Time) { _event.Time = changeEventModel.NewTime;}
                if(changeEventModel.NewLocation != null && changeEventModel.NewLocation != _event.Location) { _event.Location = changeEventModel.NewLocation;}
                if(changeEventModel.NewDuration != null && changeEventModel.NewDuration != _event.Duration) { _event.Duration = changeEventModel.NewDuration;}
                if(changeEventModel.NewCategoryId != null)
                {
                    Category _cat = _context.Categories.Where(x => x.IsDeleted == false).FirstOrDefault(x => x.Id == changeEventModel.NewCategoryId);
                    if (_cat == null)
                    {
                        throw new Exception("No se encontro la categoria con id: " + changeEventModel.NewCategoryId);
                    }
                    if (!_event.Categories.Contains(_cat))
                    {
                        _event.Categories.Add(_cat);
                        _cat.Events.Add(_event);
                    }
                }
                _context.SaveChanges();
                message = "Se ha actualizado el evento "+ _event.Name;
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
                data = EventRetrievalHelper.CreateRetrievalModel(_context, _event)
            };
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
                _event = _context.Events.Include(x=> x.CreatedByNavigation).Include(x=> x.Categories).Where(x => x.IsDeleted == false).FirstOrDefault(c => c.Id == evnt.Id);
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
                int userFeaturedEvents = _context.Events.Include(x => x.CreatedByNavigation).Where(x => x.IsDeleted == false).Where(x => x.IsFeatured == true).Where(x => x.CreatedByNavigation == user).Count();
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
        public dynamic DeleteEvent(Event evnt)
        {
            bool success = true;
            string message = "";
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var token = AuthorizationHelper.ValidateToken(identity, _context);
            if (!token.success) return token;

            User user = token.result;

            if (user.Roles != "[ROLE_ORGANIZER]" && user.Roles != "[ROLE_ADMIN]")
            {
                return new
                {
                    success = false,
                    message = "Necesita permisos de organizador para utilizar este recurso",
                };
            }
            try
            {
                if(evnt.Id == 0)
                {
                    throw new Exception("Es obligatorio proveer un id para identificar el evento a eliminar");
                }
                Event _event = _context.Events.Where(x => x.IsDeleted == false).FirstOrDefault(c => c.Id == evnt.Id);
                if (_event == null)
                {
                    throw new Exception("El evento que desea eliminar no existe");
                }
                if (_event.CreatedByNavigation != user)
                {
                    throw new Exception("El evento solo puede ser eliminado por su creador");
                }
                _event.IsDeleted = true;
                _event.IsFeatured = false;
                _context.SaveChanges();
                message = "Se ha eliminado el evento " + _event.Name;
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

            try
            {
                Event _event = _context.Events.Where(x => x.IsDeleted == false).FirstOrDefault(x => x.Id == id);
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

                if (formFile != null && _event != null)
                {
                    EventBanner _eventBanner = _context.EventBanners.FirstOrDefault(x => x.EventId == _event.Id);
                    if (_eventBanner != null)
                    {
                        _context.EventBanners.Remove(_eventBanner);
                    }
                    using (MemoryStream stream = new MemoryStream())
                    {
                        formFile.CopyTo(stream);
                        _eventBanner = new EventBanner()
                        {
                            EventId = _event.Id,
                            EventNavigation = _event,
                            EventImage = stream.ToArray()
                        };

                        _context.EventBanners.Add(_eventBanner);
                        _context.SaveChanges();
                    }
                    _event.Banner = _eventBanner;
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

                Event _event = _context.Events.Include(x=>x.Banner).Where(x => x.IsDeleted == false).FirstOrDefault(x => x.Id == evnt.Id);
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
                Event _event = _context.Events.Include(x=> x.Banner).Where(x => x.IsDeleted == false).FirstOrDefault(x => x.Id == evnt.Id);
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
                Event _evnt = _context.Events.Include(x => x.Categories).Where(x => x.IsDeleted == false).FirstOrDefault(x => x.Id == evnt.Id);
                if (_evnt == null)
                {
                    throw new Exception("Debe enviar un id de evento valido");
                }
                if (user != _evnt.CreatedByNavigation)
                {
                    throw new Exception("Solo el organizador puede modificar las categorias de un evento");
                }

                Category _cat = _context.Categories.Where(x=> x.IsDeleted == false).FirstOrDefault(x => x.Id == evnt.CategoryId);
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
                Event _evnt = _context.Events.Include(x => x.Categories).Where(x => x.IsDeleted == false).FirstOrDefault(x => x.Id == evnt.Id);
                if (_evnt == null)
                {
                    throw new Exception("Debe enviar un id de evento valido");
                }
                if (user != _evnt.CreatedByNavigation)
                {
                    throw new Exception("Solo el organizador puede modificar las categorias de un evento");
                }
                Category _cat = _context.Categories.Where(x => x.IsDeleted == false).FirstOrDefault(x => x.Id == evnt.CategoryId);
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