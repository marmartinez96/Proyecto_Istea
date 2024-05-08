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

    public class EventController : Controller
    {
        private readonly ProdigyPlanningContext _context;
        public EventController(ProdigyPlanningContext context)
        {
            _context = context;
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("GetAll")]
        public dynamic GetEvents()
        {
            bool success = true;
            string message = "Success";
            List<Event> result = null;
            try
            {
                result = _context.Events.ToList();
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
        public dynamic AddEvent(Event evnt)
        {
            bool success = true;
            string message = "";
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var token = AuthorizationHelper.ValidateToken(identity, _context);
            if (!token.success) return token;

            User user = token.result;

            if (user.Roles != "[ROLE_ORGANIZER]")
            {
                return new
                {
                    success = false,
                    message = "Necesita permisos de organizador para utilizar este recurso",
                };
            }

            try
            {
                Event _event = new Event();
                _event.Name = evnt.Name;
                _event.Location = evnt.Location;
                _event.Date= evnt.Date;
                _event.Description = evnt.Description;
                _event.CreatedBy= user.Id;
                _event.CreatedByNavigation = user;
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

            if (user.Roles != "[ROLE_ORGANIZER]")
            {
                return new
                {
                    success = false,
                    message = "Necesita permisos de organizador para utilizar este recurso",
                };
            }

            try
            {
                Event _event = _context.Events.FirstOrDefault(c => c.Name == changeEventModel.OldName);
                if (_event == null)
                {
                    throw new Exception("El evento que desea modificar no existe");
                }
                if (_event.CreatedByNavigation != user)
                {
                    throw new Exception("El evento solo puede ser modificado por su creador");
                }
                if (changeEventModel.NewName != null && changeEventModel.NewName != _event.Name) { _event.Name = changeEventModel.NewName; }
                if(changeEventModel.NewDescription != null && changeEventModel.NewDescription != _event.Description) { _event.Description = changeEventModel.NewDescription;}
                if(changeEventModel.NewDate != null && changeEventModel.NewDate != _event.Date) { _event.Date = changeEventModel.NewDate;}
                if(changeEventModel.NewLocation != null && changeEventModel.NewLocation != _event.Location) { _event.Location = changeEventModel.NewLocation;}
                _context.SaveChanges();
                message = "Se ha actualizado el evento " + changeEventModel.OldName + " a: " + _event.Name;
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
        public dynamic DeleteEvento(Event evnt)
        {
            bool success = true;
            string message = "";
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var token = AuthorizationHelper.ValidateToken(identity, _context);
            if (!token.success) return token;

            User user = token.result;

            if (user.Roles != "[ROLE_ORGANIZER]")
            {
                return new
                {
                    success = false,
                    message = "Necesita permisos de organizador para utilizar este recurso",
                };
            }
            try
            {
                Event _event = _context.Events.FirstOrDefault(c => c.Name == evnt.Name);
                if (_event == null)
                {
                    throw new Exception("El evento que desea eliminar no existe");
                }
                if (_event.CreatedByNavigation != user)
                {
                    throw new Exception("El evento solo puede ser eliminado por su creador");
                }
                _context.Events.Remove(_event);
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
    }

}