using System.Diagnostics;
using AllPurposeForum.Web.Models;
using AllPurposeForum.Services;
using Microsoft.AspNetCore.Mvc;

namespace AllPurposeForum.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ITopicService _topicService;
        
        public HomeController(ILogger<HomeController> logger, ITopicService topicService)
        {
            _logger = logger;
            _topicService = topicService;
        }
        
        public async Task<IActionResult> Index()
        {
            var topics = await _topicService.GetAllTopicsAsync();
            return View(topics);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
