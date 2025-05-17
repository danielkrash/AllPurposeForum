using AllPurposeForum.Data;
using AllPurposeForum.Data.DTO;
using AllPurposeForum.Data.Models;

namespace AllPurposeForum.Services.Implementation;

public class PostService : IPostService
{
    private readonly ApplicationDbContext _context;
    
    public async Task<CreatePostDTO> CreatePost(CreatePostDTO post)
    {
        var a = _context.Posts.Add(new Post
        {
            ApplicationUserId = post.UserId,
            TopicId = post.TopicId,
            Content = post.Content,
            Title = post.Title,
            Nsfw = post.Nsfw
        });
        var result = await _context.SaveChangesAsync();
        if (result == 0)
        {
            throw new Exception("Failed to create post");
        }
        return post;
    }

    public Task<PostDTO> GetPostById(int id)
    {
        throw new NotImplementedException();
    }

    public Task<List<PostDTO>> GetPostsByTopicId(int topicId)
    {
        throw new NotImplementedException();
    }

    public Task<List<PostDTO>> GetPostsByUserId(string userId)
    {
        throw new NotImplementedException();
    }

    public Task<List<PostDTO>> GetPostsByUserIdAndTopicId(string userId, int topicId)
    {
        throw new NotImplementedException();
    }

    public Task<PostDTO> UpdatePost(PostDTO post)
    {
        throw new NotImplementedException();
    }

    public Task<bool> DeletePost(int id)
    {
        throw new NotImplementedException();
    }
}