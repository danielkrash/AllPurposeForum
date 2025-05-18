using AllPurposeForum.Data;
using AllPurposeForum.Data.DTO;
using AllPurposeForum.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace AllPurposeForum.Services.Implementation;

[Authorize]
public class PostService : IPostService
{
    private readonly ApplicationDbContext _context;

    public PostService(ApplicationDbContext context)
    {
        _context = context;
    }

    [Authorize(Policy = "RequireAdministratorRole")]
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

    public async Task<PostDTO> GetPostById(int id)
    {
        var post = await _context.Posts
            .FirstOrDefaultAsync(p => p.Id == id);
        if (post == null)
        {
            throw new Exception("Post not found");
        }
        return await Task.FromResult(new PostDTO
        {
            Id = post.Id,
            Title = post.Title,
            Content = post.Content,
            Nsfw = post.Nsfw,
            UserId = post.ApplicationUserId,
            TopicId = post.TopicId
        });
    }

    public async Task<List<PostDTO>> GetPostsByTopicId(int topicId)
    {
        var posts = await _context.Posts
            .Where(p => p.TopicId == topicId)
            .ToListAsync();

        if (posts == null || posts.Count == 0)
        {
            throw new Exception("No posts found for this topic");
        }

        return await Task.FromResult(posts.Select(p => new PostDTO
        {
            Id = p.Id,
            Title = p.Title,
            Content = p.Content,
            Nsfw = p.Nsfw,
            UserId = p.ApplicationUserId,
            TopicId = p.TopicId
        }).ToList());
    }

    public async Task<List<PostDTO>> GetPostsByUserId(string userId)
    {
        var posts = await _context.Posts
            .Where(p => p.ApplicationUserId == userId)
            .ToListAsync();

        if (posts == null || posts.Count == 0)
        {
            throw new Exception("No posts found for this user");
        }

        return await Task.FromResult(posts.Select(p => new PostDTO
        {
            Id = p.Id,
            Title = p.Title,
            Content = p.Content,
            Nsfw = p.Nsfw,
            UserId = p.ApplicationUserId,
            TopicId = p.TopicId
        }).ToList());
    }

    public async Task<List<PostDTO>> GetPostsByUserIdAndTopicId(string userId, int topicId)
    {
        var posts = await _context.Posts
            .Where(p => p.ApplicationUserId == userId && p.TopicId == topicId)
            .ToListAsync();

        if (posts == null || posts.Count == 0)
        {
            throw new Exception("No posts found for this user in this topic");
        }

        return await Task.FromResult(posts.Select(p => new PostDTO
        {
            Id = p.Id,
            Title = p.Title,
            Content = p.Content,
            Nsfw = p.Nsfw,
            UserId = p.ApplicationUserId,
            TopicId = p.TopicId
        }).ToList());
    }

    public async Task<PostDTO> UpdatePost(PostDTO post)
    {
        var existingPost = await _context.Posts
            .FirstOrDefaultAsync(p => p.Id == post.Id);
        if (existingPost == null)
        {
            throw new Exception("Post not found");
        }

        existingPost.Title = post.Title;
        existingPost.Content = post.Content;
        existingPost.Nsfw = post.Nsfw;

        var result = await _context.SaveChangesAsync();
        if (result == 0)
        {
            throw new Exception("Failed to update post");
        }

        return await Task.FromResult(post);
    }

    public async Task<bool> DeletePost(int id)
    {
        var post = await _context.Posts
            .FirstOrDefaultAsync(p => p.Id == id);
        if (post == null)
        {
            throw new Exception("Post not found");
        }

        _context.Posts.Remove(post);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }
}