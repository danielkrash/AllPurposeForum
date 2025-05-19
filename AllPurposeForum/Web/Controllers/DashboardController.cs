using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AllPurposeForum.Web.Models; // Added for DashboardViewModel
using AllPurposeForum.Data.Models; // Added for ApplicationUser
using Microsoft.AspNetCore.Identity; // Added for UserManager
using AllPurposeForum.Data; // Added for ApplicationDbContext
using System.Linq; // Added for CountAsync and other LINQ methods
using System.Threading.Tasks; // Added for Task
using Microsoft.EntityFrameworkCore; // Added for ToListAsync, CountAsync

namespace AllPurposeForum.Web.Controllers
{
    [Authorize(Roles = "Admin,Manager")]
    public class DashboardController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context; // Using DbContext directly for simplicity here

        public DashboardController(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var totalUsers = await _userManager.Users.CountAsync();
            var totalTopics = await _context.Topics.CountAsync();
            var totalPosts = await _context.Posts.CountAsync();
            var totalComments = await _context.PostComments.CountAsync();

            var viewModel = new DashboardViewModel
            {
                TotalUsers = totalUsers,
                TotalTopics = totalTopics,
                TotalPosts = totalPosts,
                TotalComments = totalComments,
                ChartLabels = new List<string> { "Users", "Topics", "Posts", "Comments" },
                ChartData = new List<int> { totalUsers, totalTopics, totalPosts, totalComments }
            };

            return View(viewModel);
        }
    }
}
