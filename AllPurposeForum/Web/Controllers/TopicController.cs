using AllPurposeForum.Data.DTO;
using AllPurposeForum.Data.Models;
using AllPurposeForum.Services;
using AllPurposeForum.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Linq;
using System.Threading.Tasks;
using AllPurposeForum.Helpers; // Add this line

namespace AllPurposeForum.Web.Controllers // Ensured namespace is correct
{
    public class TopicController : Controller
    {
        private readonly ITopicService _topicService;
        private readonly IPostService _postService;
        private readonly UserManager<ApplicationUser> _userManager; // Added UserManager

        // Updated constructor to include UserManager
        public TopicController(ITopicService topicService, IPostService postService, UserManager<ApplicationUser> userManager)
        {
            _topicService = topicService;
            _postService = postService;
            _userManager = userManager; // Initialize UserManager
        }

        [HttpGet("Topics/{topicId:int}", Name = "TopicDetails")]
        public async Task<IActionResult> Index(int topicId)
        {
            var topic = await _topicService.GetTopicByIdAsync(topicId);
            if (topic == null)
            {
                Response.StatusCode = 404;
                return View("NotFound"); // Return the custom NotFound view
            }

            var postsFromService = await _postService.GetPostsByTopicId(topicId);

            var postViewModels = postsFromService.Select(p => new PostViewModel
            {
                Id = p.Id,
                Title = p.Title,
                AuthorName = p.UserName,
                CreatedAtFormatted = Utils.TimeAgo(p.CreatedAt),
                CommentsCount = p.CommentsCount,
                TopicId = p.TopicId,
                ContentPreview = p.Content.Length > 100 ? p.Content.Substring(0, 100) + "..." : p.Content // Simple preview
            }).ToList();

            var viewModel = new TopicDetailViewModel
            {
                Topic = topic,
                Posts = postViewModels
            };

            return View(viewModel);
        }

        // GET: Topic/Create
        [Authorize]
        public IActionResult Create()
        {
            return View(new CreateTopicViewModel()); // Pass a new view model
        }

        // POST: Topic/Create
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateTopicViewModel model)
        {
            if (ModelState.IsValid)
            {
                var userId = _userManager.GetUserId(User); // More robust way to get UserId
                if (string.IsNullOrEmpty(userId))
                {
                    ModelState.AddModelError(string.Empty, "User not found. Please log in again.");
                    return View(model);
                }

                var createTopicDto = new CreateTopicDTO
                {
                    Title = model.Title,
                    Description = model.Description,
                    UserId = userId,
                    Nsfw = model.isNswf // Map the Nsfw property
                };

                try
                {
                    var createdTopic = await _topicService.CreateTopicAsync(createTopicDto);
                    // Redirect to the newly created topic's detail page
                    return RedirectToRoute("TopicDetails", new { topicId = createdTopic.Id }); 
                }
                catch (Exception ex)
                {
                    // Log the error (using ILogger or your preferred logging mechanism)
                    ModelState.AddModelError(string.Empty, "An error occurred while creating the topic. " + ex.Message);
                }
            }
            return View(model);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteTopic(int topicId)
        {
            var topic = await _topicService.GetTopicByIdAsync(topicId);
            if (topic == null)
            {
                TempData["ErrorMessage"] = "Topic not found.";
                return RedirectToAction("Index", "Home");
            }

            var currentUser = await _userManager.GetUserAsync(User);
            bool isOwner = currentUser != null && topic.UserId == currentUser.Id;
            bool isAdmin = User.IsInRole("Admin");
            bool isManager = User.IsInRole("Manager");

            if (!isOwner && !isAdmin && !isManager)
            {
                TempData["ErrorMessage"] = "You are not authorized to delete this topic.";
                return RedirectToAction("Index", "Home");
            }

            try
            {
                await _topicService.DeleteTopicAsync(topicId);
                TempData["SuccessMessage"] = "Topic deleted successfully.";
            }
            catch (Exception ex)
            {
                // Log the error
                TempData["ErrorMessage"] = "An error occurred while deleting the topic: " + ex.Message;
            }

            return RedirectToAction("Index", "Home");
        }

        // GET: Topic/EditTopic/{topicId}
        [Authorize]
        [HttpGet("Topics/Edit/{topicId:int}")]
        public async Task<IActionResult> EditTopic(int topicId)
        {
            var topicDto = await _topicService.GetTopicByIdAsync(topicId);
            if (topicDto == null)
            {
                Response.StatusCode = 404;
                return View("NotFound");
            }

            var currentUser = await _userManager.GetUserAsync(User);
            bool isOwner = currentUser != null && topicDto.UserId == currentUser.Id;
            bool isAdmin = User.IsInRole("Admin");
            bool isManager = User.IsInRole("Manager");

            // Only owner, admin, or manager can edit
            if (!isOwner && !isAdmin && !isManager)
            {
                TempData["ErrorMessage"] = "You are not authorized to edit this topic.";
                return RedirectToAction("Index", "Home");
            }

            var viewModel = new EditTopicViewModel
            {
                Id = topicDto.Id,
                Title = topicDto.Title,
                Description = topicDto.Description,
                IsNsfw = topicDto.Nsfw
            };

            return View(viewModel);
        }

        // POST: Topic/EditTopic
        [Authorize]
        [HttpPost("Topics/Edit/{topicId:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditTopic(int topicId, EditTopicViewModel model)
        {
            if (topicId != model.Id)
            {
                TempData["ErrorMessage"] = "Topic ID mismatch.";
                return RedirectToAction("Index", "Home");
            }

            var topicToUpdate = await _topicService.GetTopicByIdAsync(topicId);
            if (topicToUpdate == null)
            {
                Response.StatusCode = 404;
                return View("NotFound");
            }

            var currentUser = await _userManager.GetUserAsync(User);
            bool isOwner = currentUser != null && topicToUpdate.UserId == currentUser.Id;
            bool isAdmin = User.IsInRole("Admin");
            bool isManager = User.IsInRole("Manager");

            if (!isOwner && !isAdmin && !isManager)
            {
                TempData["ErrorMessage"] = "You are not authorized to edit this topic.";
                // Potentially redirect to topic details or home
                return RedirectToAction("Index", "Topic", new { topicId = topicId });
            }

            if (ModelState.IsValid)
            {
                var updateTopicDto = new UpdateTopicDTO
                {
                    Title = model.Title,
                    Description = model.Description,
                    Nsfw = model.IsNsfw
                };

                try
                {
                    await _topicService.UpdateTopicAsync(updateTopicDto, topicId); // Corrected parameter order
                    TempData["SuccessMessage"] = "Topic updated successfully.";
                    return RedirectToRoute("TopicDetails", new { topicId = topicId });
                }
                catch (Exception ex)
                {
                    // Log the error
                    ModelState.AddModelError(string.Empty, "An error occurred while updating the topic: " + ex.Message);
                }
            }
            // If model state is invalid, or an error occurred, return to the edit view with the current model
            return View(model);
        }
    }
}
