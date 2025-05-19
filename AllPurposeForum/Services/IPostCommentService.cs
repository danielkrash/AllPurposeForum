using AllPurposeForum.Data.DTO;

namespace AllPurposeForum.Services;

public interface IPostCommentService
{
    public Task<List<PostCommentDTO>> GetAllPostCommentsAsync();
    public Task<PostCommentDTO> GetPostCommentByIdAsync(int id);
    public Task<CreatePostCommentDTO> CreatePostCommentAsync(CreatePostCommentDTO postComment);
    public Task<PostCommentDTO> UpdatePostCommentAsync(UpdatePostCommentDTO postComment, int id);
    public Task<bool> DeletePostCommentAsync(int id);
    public Task<List<PostCommentDTO>> GetPostCommentsByUserIdAsync(string userId);
    public Task<List<PostCommentDTO>> GetPostCommentsByPostIdAsync(int postId);
    public Task<List<UnapprovedCommentDTO>> GetUnapprovedCommentsAsync();
    public Task ApproveCommentAsync(int commentId);
    public Task RejectCommentAsync(int commentId);
}