using AllPurposeForum.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace AllPurposeForum.Web.Controllers
{
    [Authorize(Roles = "Manager")]
    public class ModerationController : Controller
    {
        private readonly IPostCommentService _postCommentService;

        public ModerationController(IPostCommentService postCommentService)
        {
            _postCommentService = postCommentService;
        }

        public async Task<IActionResult> Index()
        {
            var unapprovedComments = await _postCommentService.GetUnapprovedCommentsAsync();
            return View(unapprovedComments);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int commentId)
        {
            await _postCommentService.ApproveCommentAsync(commentId);
            TempData["SuccessMessage"] = "Comment approved successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int commentId)
        {
            await _postCommentService.RejectCommentAsync(commentId);
            TempData["SuccessMessage"] = "Comment rejected successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}
