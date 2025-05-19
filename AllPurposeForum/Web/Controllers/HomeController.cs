using System.Diagnostics;
using AllPurposeForum.Web.Models;
using AllPurposeForum.Services; // Add this using directive
using Microsoft.AspNetCore.Mvc;

namespace AllPurposeForum.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ITopicService _topicService; // Add ITopicService field

        // Modify constructor to inject ITopicService
        public HomeController(ILogger<HomeController> logger, ITopicService topicService)
        {
            _logger = logger;
            _topicService = topicService;
        }

        // Modify Index action to fetch and pass topics to the view
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
