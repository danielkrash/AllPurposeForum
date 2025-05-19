using AllPurposeForum.Data.DTO;
using AllPurposeForum.Data.Models;
using AllPurposeForum.Helpers;
using AllPurposeForum.Services;
using AllPurposeForum.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace AllPurposeForum.Web.Controllers
{
    [Authorize]
    public class PostCommentController : Controller
    {
        private readonly IPostCommentService _postCommentService;
        private readonly IPostService _postService;
        private readonly UserManager<ApplicationUser> _userManager;

        public PostCommentController(
            IPostCommentService postCommentService,
            IPostService postService,
            UserManager<ApplicationUser> userManager)
        {
            _postCommentService = postCommentService;
            _postService = postService;
            _userManager = userManager;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> MarkCommentAsNotApproved(int commentId)
        {
            var comment = await _postCommentService.GetPostCommentByIdAsync(commentId);
            if (comment == null)
            {
                return NotFound();
            }

            comment.isApproved = false;
            await _postCommentService.UpdatePostCommentAsync(new UpdatePostCommentDTO
            {
                Content = comment.Content,
                IsApproved = false
            }, commentId);

            return Redirect($"/posts/{comment.PostId}");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> MarkCommentAsApproved(int commentId)
        {
            var comment = await _postCommentService.GetPostCommentByIdAsync(commentId);
            if (comment == null)
            {
                return NotFound();
            }

            comment.isApproved = true;
            await _postCommentService.UpdatePostCommentAsync(new UpdatePostCommentDTO
            {
                Content = comment.Content,
                IsApproved = true
            }, commentId);

            return Redirect($"/posts/{comment.PostId}");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteComment(int commentId, int postId)
        {
            var comment = await _postCommentService.GetPostCommentByIdAsync(commentId);
            if (comment == null)
            {
                return NotFound();
            }

            var currentUser = await _userManager.GetUserAsync(User);
            bool isAuthor = comment.UserId == currentUser.Id;
            bool isManagerOrAdmin = User.IsInRole("Manager") || User.IsInRole("Admin");

            if (!isAuthor && !isManagerOrAdmin)
            {
                return Forbid();
            }

            await _postCommentService.DeletePostCommentAsync(commentId);
            TempData["SuccessMessage"] = "Comment deleted successfully.";
            return Redirect($"/posts/{comment.PostId}");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int commentId)
        {
            var comment = await _postCommentService.GetPostCommentByIdAsync(commentId);
            if (comment == null)
            {
                return NotFound();
            }

            var post = await _postService.GetPostById(comment.PostId);
            if (post == null)
            {
                return NotFound(); // Or handle as appropriate
            }

            var currentUser = await _userManager.GetUserAsync(User);
            bool isAuthor = comment.UserId == currentUser.Id;
            bool isManager = User.IsInRole("Manager");
            bool isAdmin = User.IsInRole("Admin");

            if (!isAuthor && !isManager && !isAdmin)
            {
                return Forbid();
            }

            var model = new EditCommentViewModel
            {
                Id = comment.Id,
                PostId = comment.PostId,
                Content = comment.Content,
                IsApproved = comment.isApproved, // Keep passing this to the view for the hidden field
                PostTitle = post.Title,
                OriginalCommentContentPreview = comment.Content.Length > 100 ? comment.Content.Substring(0, 100) + "..." : comment.Content
                // CanChangeApprovalStatus is removed
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditCommentViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var originalCommentForPost = await _postCommentService.GetPostCommentByIdAsync(model.Id);
                if (originalCommentForPost != null) {
                    var post = await _postService.GetPostById(originalCommentForPost.PostId);
                    if (post != null) model.PostTitle = post.Title;
                    model.OriginalCommentContentPreview = originalCommentForPost.Content.Length > 100 ? originalCommentForPost.Content.Substring(0, 100) + "..." : originalCommentForPost.Content;
                    // model.IsApproved will be retained from the hidden field if needed for re-display
                }
                // CanChangeApprovalStatus logic removed
                return View(model);
            }

            var commentToUpdate = await _postCommentService.GetPostCommentByIdAsync(model.Id);
            if (commentToUpdate == null)
            {
                return NotFound();
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) // Added null check for currentUser
            {
                return Challenge(); // Or Forbid(), or redirect to login
            }
            bool isAuthor = commentToUpdate.UserId == currentUser.Id;
            bool isManager = User.IsInRole("Manager");
            bool isAdmin = User.IsInRole("Admin");

            if (!isAuthor && !isManager && !isAdmin)
            {
                return Forbid();
            }

            // ML Model Prediction for approval status
            var predictionResult = MLModel.Predict(new MLModel.ModelInput { Sentiment = model.Content ?? string.Empty });
            bool newIsApprovedStatus = Utils.IsCommentAcceptable(predictionResult.PredictedLabel);

            var updateDto = new UpdatePostCommentDTO
            {
                Content = model.Content ?? string.Empty, // Added null check for model.Content
                IsApproved = newIsApprovedStatus
            };

            await _postCommentService.UpdatePostCommentAsync(updateDto, model.Id);
            TempData["SuccessMessage"] = "Comment updated successfully.";
            return Redirect($"/posts/{model.PostId}");
        }
    }
}
