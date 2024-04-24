using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProdigyPlanningAPI.Data;
using ProdigyPlanningAPI.Models;

namespace ProdigyPlanningAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventController : ControllerBase
    {
        private readonly ProdigyPlanningContext _context;
        public EventController(ProdigyPlanningContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IEnumerable<Category> GetCategories() 
        {
            return _context.Categories.ToList();
        }
    }
}
