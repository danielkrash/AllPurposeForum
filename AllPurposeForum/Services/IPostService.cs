using AllPurposeForum.Data.DTO;

namespace AllPurposeForum.Services;

public interface IPostService
{
    Task<CreatePostDTO> CreatePost(CreatePostDTO post);
    Task<PostDTO> GetPostById(int id);
    Task<List<PostDTO>> GetPostsByTopicId(int topicId);
    Task<List<PostDTO>> GetPostsByUserId(string userId);
    Task<List<PostDTO>> GetPostsByUserIdAndTopicId(string userId, int topicId);
    Task<PostDTO> UpdatePost(PostDTO post);
    Task<bool> DeletePost(int id);
}