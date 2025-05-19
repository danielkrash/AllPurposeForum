using AllPurposeForum.Data.Models;
using AllPurposeForum.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AllPurposeForum.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : Controller
    {
        private readonly IUserService _userService;
        private readonly UserManager<ApplicationUser> _userManager;

        public TestController(IUserService userService , UserManager<ApplicationUser> userManager)
        {
            _userService = userService;
            _userManager = userManager;
        }

        [HttpGet]   // GET /api/test2
        public IActionResult ListProducts()
        {
            var products = new List<string> { "Product1", "Product2", "Product3" };
            return Ok(products);
        }

        [HttpGet("{id}")]   // GET /api/test2/xyz
        public async Task<IActionResult> GetProduct(string? id)
        {
            var product = $"Product {id}";
            var user = _userService.GetUserByIdAsync("55c5b4b2-aa19-4467-ba5c-db88b268316b");
            var result = await _userManager.GetLockoutEndDateAsync(user);
            var setLockut = await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddDays(1));
            //var clearLockout = await _userManager.SetLockoutEndDateAsync(user, null);
            return Ok(DateTimeOffset.UtcNow.AddDays(1));
        }

        [HttpGet("int/{id:int}")] // GET /api/test2/int/3
        public IActionResult GetIntProduct(int id)
        {
            var product = $"Product {id}";
            return Ok(product);
        }

        [HttpGet("int2/{id}")]  // GET /api/test2/int2/3
        public IActionResult GetInt2Product(int id)
        {
            var product = $"Product {id}";
            return Ok(product);
        }
    }
}
