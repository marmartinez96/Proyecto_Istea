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
        public dynamic GetEvents()
        {
            bool success = true;
            string message = "Success";
            List<EventRetrievalModel> result = new List<EventRetrievalModel>();
            try
            {
                List<Event> _result = _context.Events.Include(x => x.CreatedByNavigation).Include(x => x.Categories).Where(x=> x.IsDeleted == false).ToList();
                foreach (Event e in _result)
                {
                    EventRetrievalModel eventResult = CreateRetrievalModel(_context, e);
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
                result = CreateRetrievalModel(_context, _event);
                
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
                if (evnt.Name == null || evnt.Name.Trim() == "")
                {
                    throw new Exception("El campo nombre no puede estar vacio");
                }
                if (evnt.Location == null || evnt.Location.Trim() == "")
                {
                    throw new Exception("El campo ubicacion no puede estar vacio");
                }
                if (evnt.Date == null)
                {
                    throw new Exception("El campo fecha no puede estar vacio");
                }
                if (evnt.Category == null)
                {
                    throw new Exception("El campo categoria no puede estar vacio");
                }

                Event _event = new Event();
                _event.Name = evnt.Name;
                _event.Location = evnt.Location;
                _event.Date= evnt.Date;
                _event.Description = evnt.Description;
                _event.CreatedBy= user.Id;
                _event.CreatedByNavigation = user;
                user.Events.Add(_event);

                Category _cat = _context.Categories.Where(x => x.IsDeleted == false).FirstOrDefault(x => x.Name.ToLower() == evnt.Category.ToLower());
                if(_cat == null )
                {
                    throw new Exception("No se encontro la categoria "+evnt.Category);
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

            try
            {
                Event _event = _context.Events.Where(x => x.IsDeleted == false).FirstOrDefault(c => c.Id == changeEventModel.Id);
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
                if(changeEventModel.NewDate != null && changeEventModel.NewDate != _event.Date) { _event.Date = changeEventModel.NewDate;}
                if(changeEventModel.NewLocation != null && changeEventModel.NewLocation != _event.Location) { _event.Location = changeEventModel.NewLocation;}
                if (changeEventModel.NewCategory != null)
                {
                    Category _cat = _context.Categories.Where(x => x.IsDeleted == false).FirstOrDefault(x => x.Name.ToLower() == changeEventModel.NewCategory.ToLower());
                    if (_cat == null)
                    {
                        throw new Exception("No se encontro la categoria " + changeEventModel.NewCategory);
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
                Event _event = _context.Events.Where(x => x.IsDeleted == false).FirstOrDefault(c => c.Name == evnt.Name);
                if (_event == null)
                {
                    throw new Exception("El evento que desea eliminar no existe");
                }
                if (_event.CreatedByNavigation != user)
                {
                    throw new Exception("El evento solo puede ser eliminado por su creador");
                }
                
                _event.IsDeleted = true;
                _context.SaveChanges();
                message = "Se ha eliminado el evento " + evnt.Name;
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

                Category _cat = _context.Categories.Where(x=> x.IsDeleted == false).FirstOrDefault(x => x.Name == evnt.Category);
                if (_cat == null)
                {
                    throw new Exception("La categoria que desea agregar no existe");
                }
                if (_evnt.Categories.Contains(_cat))
                {
                    throw new Exception("Este evento ya contiene la categoria "+evnt.Category);
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
                Category _cat = _context.Categories.Where(x => x.IsDeleted == false).FirstOrDefault(x => x.Name == evnt.Category);
                if (_cat == null)
                {
                    throw new Exception("La categoria que desea eliminar del evento no existe");
                }
                if (!_evnt.Categories.Contains(_cat))
                {
                    throw new Exception("El evento no contiene la categoria que desea eliminar");
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

        private EventRetrievalModel CreateRetrievalModel(DbContext context, Event e)
        {
            EventRetrievalModel eventResult = new EventRetrievalModel();
            eventResult.Id = e.Id;
            eventResult.Name = e.Name;
            eventResult.Date = e.Date;
            eventResult.Location = e.Location;
            eventResult.Description = e.Description;
            eventResult.CreatedBy = e.CreatedByNavigation.Name + ' ' + e.CreatedByNavigation.Surname;
            foreach (Category c in e.Categories)
            {
                eventResult.Categories.Add(c.Name);
            }
            if (e.Banner != null)
            {
                eventResult.HasBanner = true;
            }
            return eventResult;
        }

    }

}