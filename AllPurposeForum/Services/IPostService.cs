using AllPurposeForum.Data.DTO;

namespace AllPurposeForum.Services;

public interface IPostService
{
    Task<PostDTO> CreatePost(CreatePostDTO post); // Changed to CreatePostAsync and return PostDTO
    Task<PostDTO?> GetPostById(int id); // Changed to PostDTO?
    Task<List<PostDTO>> GetPostsByTopicId(int topicId);
    Task<List<PostDTO>> GetPostsByUserId(string userId);
    Task<List<PostDTO>> GetAllPosts();
    Task<List<PostDTO>> GetPostsByUserIdAndTopicId(string userId, int topicId);
    Task<UpdatePostDTO> UpdatePost(UpdatePostDTO post);
    Task<bool> DeletePost(int id);
}