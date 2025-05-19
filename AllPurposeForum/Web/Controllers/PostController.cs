// Filepath: c:\Users\danik\source\repos\AllPurposeForum\AllPurposeForum\Controllers\PostController.cs
using AllPurposeForum.Services;
using AllPurposeForum.Web.Models;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using AllPurposeForum.Helpers;
using AllPurposeForum.Data.DTO;
using Microsoft.AspNetCore.Identity;
using AllPurposeForum.Data.Models;
using System.Security.Claims; // Added for User.FindFirstValue

namespace AllPurposeForum.Web.Controllers
{
    public class PostController : Controller
    {
        private readonly IPostService _postService;
        private readonly IPostCommentService _postCommentService;
        private readonly UserManager<ApplicationUser> _userManager;

        public PostController(IPostService postService, IPostCommentService postCommentService, UserManager<ApplicationUser> userManager)
        {
            _postService = postService;
            _postCommentService = postCommentService;
            _userManager = userManager;
        }

        [HttpGet("Posts/{postId:int}", Name = "PostDetails")]
        public async Task<IActionResult> Index(int postId)
        {
            var post = await _postService.GetPostById(postId);
            if (post == null)
            {
                return NotFound();
            }

            var allComments = await _postCommentService.GetPostCommentsByPostIdAsync(postId);
            // Filter comments to include only those that are approved
            var approvedComments = allComments.Where(c => c.isApproved).ToList();

            var commentViewModels = approvedComments.Select(c => new PostCommentViewModel
            {
                UserName = c.UserName,
                Content = c.Content,
                CreatedAtFormatted = Utils.TimeAgo(c.CreatedAt)
            }).ToList();

            var viewModel = new PostDetailViewModel
            {
                Post = post,
                Comments = commentViewModels,
                NewComment = new CreateCommentViewModel { PostId = postId },
                PostCreatedAtFormatted = Utils.TimeAgo(post.CreatedAt)
            };

            return View(viewModel);
        }

        [HttpPost("Posts/{postId:int}/Comment")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment(int postId, [Bind(Prefix = "NewComment")] CreateCommentViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge(); 
            }

            if (ModelState.IsValid)
            {
                var createCommentDto = new CreatePostCommentDTO
                {
                    PostId = postId,
                    UserId = user.Id, 
                    Content = model.Content
                };

                try
                {
                    await _postCommentService.CreatePostCommentAsync(createCommentDto);
                    return RedirectToRoute("PostDetails", new { postId = postId });
                }
                catch (Exception ex) // Added ex to potentially log it
                {
                    // TODO: Log the exception ex
                    ModelState.AddModelError(string.Empty, "An unexpected error occurred while trying to post your comment. Please try again.");
                }
            }

            // If ModelState is invalid or an exception occurred, re-display the page with current data and errors
            var post = await _postService.GetPostById(postId);
            if (post == null)
            {
                // This case should ideally not be reached if the user is on a valid post page
                return NotFound("The post you are trying to comment on was not found.");
            }

            var comments = await _postCommentService.GetPostCommentsByPostIdAsync(postId);
            var commentViewModels = comments.Select(c => new PostCommentViewModel
            {
                UserName = c.UserName,
                Content = c.Content,
                CreatedAtFormatted = Utils.TimeAgo(c.CreatedAt)
            }).ToList();

            var viewModel = new PostDetailViewModel
            {
                Post = post,
                Comments = commentViewModels,
                NewComment = model, // Pass back the model with its (potentially invalid) data and validation errors
                PostCreatedAtFormatted = Utils.TimeAgo(post.CreatedAt)
            };

            return View("Index", viewModel); // Explicitly return the Index view
        }
    }
}
