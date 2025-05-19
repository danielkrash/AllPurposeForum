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
    public async Task<PostDTO> CreatePost(CreatePostDTO post)
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

        return new PostDTO
        {
            Id = a.Entity.Id,
            Title = a.Entity.Title,
            Content = a.Entity.Content,
            Nsfw = a.Entity.Nsfw,
            UserId = a.Entity.ApplicationUserId,
            TopicId = a.Entity.TopicId,
        };
    }

    public async Task<PostDTO> GetPostById(int id)
    {
        var post = await _context.Posts
            .Include(p => p.ApplicationUser)
            .Include(p => p.PostComments)
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
            TopicId = post.TopicId,
            UserName = post.ApplicationUser.UserName,
            CommentsCount = post.PostComments.Count,
            CreatedAt = post.CreatedAt
        });
    }

    public async Task<List<PostDTO>> GetPostsByTopicId(int topicId)
    {
        var posts = await _context.Posts
            .Include(p => p.ApplicationUser)
            .Include(p => p.PostComments)
            .Where(p => p.TopicId == topicId)
            .ToListAsync();

        return await Task.FromResult(posts.Select(p => new PostDTO
        {
            Id = p.Id,
            Title = p.Title,
            Content = p.Content,
            Nsfw = p.Nsfw,
            UserId = p.ApplicationUserId,
            TopicId = p.TopicId,
            UserName = p.ApplicationUser.UserName,
            CommentsCount = p.PostComments.Count,
            CreatedAt = p.CreatedAt
        }).ToList());
    }

    public async Task<List<PostDTO>> GetPostsByUserId(string userId)
    {
        var posts = await _context.Posts
            .Include(p => p.ApplicationUser)
            .Include(p => p.PostComments)
            .Where(p => p.ApplicationUserId == userId)
            .ToListAsync();

        return await Task.FromResult(posts.Select(p => new PostDTO
        {
            Id = p.Id,
            Title = p.Title,
            Content = p.Content,
            Nsfw = p.Nsfw,
            UserId = p.ApplicationUserId,
            TopicId = p.TopicId,
            UserName = p.ApplicationUser.UserName,
            CommentsCount = p.PostComments.Count,
            CreatedAt = p.CreatedAt
        }).ToList());
    }

    public async Task<List<PostDTO>> GetPostsByUserIdAndTopicId(string userId, int topicId)
    {
        var posts = await _context.Posts
            .Include(e => e.ApplicationUser)
            .Include(e => e.PostComments)
            .Where(p => p.ApplicationUserId == userId && p.TopicId == topicId)
            .ToListAsync();

        return await Task.FromResult(posts.Select(p => new PostDTO
        {
            Id = p.Id,
            Title = p.Title,
            Content = p.Content,
            Nsfw = p.Nsfw,
            UserId = p.ApplicationUserId,
            TopicId = p.TopicId,
            UserName = p.ApplicationUser.UserName,
            CommentsCount = p.PostComments.Count,
            CreatedAt = p.CreatedAt
        }).ToList());
    }

    public async Task<List<PostDTO>> GetAllPosts()
    {
        var posts = await _context.Posts
            .Include(p => p.ApplicationUser)
            .Include(p => p.PostComments)
            .ToListAsync();


        return await Task.FromResult(posts.Select(p => new PostDTO
        {
            Id = p.Id,
            Title = p.Title,
            Content = p.Content,
            Nsfw = p.Nsfw,
            UserId = p.ApplicationUserId,
            TopicId = p.TopicId,
            UserName = p.ApplicationUser.UserName,
            CommentsCount = p.PostComments.Count,
            CreatedAt = p.CreatedAt
        }).ToList());
    }

    public async Task<UpdatePostDTO> UpdatePost(UpdatePostDTO post)
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