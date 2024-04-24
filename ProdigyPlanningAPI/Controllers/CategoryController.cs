using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProdigyPlanningAPI.Data;
using ProdigyPlanningAPI.Models;

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
        public IEnumerable<Category> GetCategories() 
        {
            return _context.Categories.ToList();
        }
    }
}
