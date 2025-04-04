using Microsoft.AspNetCore.Mvc;

namespace AllPurposeForum.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : Controller
    {
        [HttpGet]   // GET /api/test2
        public IActionResult ListProducts()
        {
            return Ok("5");
        }

        [HttpGet("{id}")]   // GET /api/test2/xyz
        public IActionResult GetProduct(string id)
        {
            return Ok("5");
        }

        [HttpGet("int/{id:int}")] // GET /api/test2/int/3
        public IActionResult GetIntProduct(int id)
        {
            return Ok("5");
        }

        [HttpGet("int2/{id}")]  // GET /api/test2/int2/3
        public IActionResult GetInt2Product(int id)
        {
            return Ok("5");
        }
    }
}
