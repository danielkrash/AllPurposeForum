using AllPurposeForum.Data;
using AllPurposeForum.Data.DTO;
using Microsoft.EntityFrameworkCore;

namespace AllPurposeForum.Services.Implementation;

public class PostCommentService : IPostCommentService
{
    private readonly ApplicationDbContext _context;

    public PostCommentService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<PostCommentDTO>> GetAllPostCommentsAsync()
    {
        var posts = await _context.PostComments.Include(p => p.User).ToListAsync();
        if (posts == null)
        {
            throw new Exception("Post not found");
        }

        return await Task.FromResult(posts.Select(p => new PostCommentDTO
        {
            Id = p.Id,
            PostId = p.PostId,
            UserId = p.UserId,
            Content = p.Content,
            UserName = p.User.UserName,
            CreatedAt = p.CreatedAt
        }).ToList());
    }

    public async Task<Data.DTO.PostCommentDTO> GetPostCommentByIdAsync(int id)
    {
        var post = await _context.PostComments
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.Id == id);
        if (post == null)
        {
            throw new Exception("Post not found");
        }

        return await Task.FromResult(new PostCommentDTO
        {
            Id = post.Id,
            PostId = post.PostId,
            UserId = post.UserId,
            Content = post.Content,
            UserName = post.User.UserName,
            CreatedAt = post.CreatedAt
        });
    }

    public async Task<CreatePostCommentDTO> CreatePostCommentAsync(CreatePostCommentDTO postComment)
    {
        if (!_context.Users.Any(u => u.Id == postComment.UserId) ||
            !_context.Posts.Any(p => p.Id == postComment.PostId))
        {
            throw new Exception("User or Post not found");
        }

        var a = _context.PostComments.Add(new Data.Models.PostComment
        {
            UserId = postComment.UserId,
            PostId = postComment.PostId,
            Content = postComment.Content,
        });
        var result = await _context.SaveChangesAsync();
        if (result == 0)
        {
            throw new Exception("Failed to create post");
        }

        return postComment;
    }

    public async Task<Data.DTO.PostCommentDTO> UpdatePostCommentAsync(UpdatePostCommentDTO postComment, int id)
    {
        var post = await _context.PostComments
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.Id == id);
        if (post == null)
        {
            throw new Exception("Post not found");
        }

        post.Content = postComment.Content;
        post.Acceptence = postComment.IsApproved;
        var result = await _context.SaveChangesAsync();
        if (result == 0)
        {
            throw new Exception("Failed to update post");
        }

        return await Task.FromResult(new PostCommentDTO
        {
            Id = post.Id,
            PostId = post.PostId,
            UserId = post.UserId,
            Content = post.Content,
            UserName = post.User.UserName
        });
    }

    public async Task<bool> DeletePostCommentAsync(int id)
    {
        var post = await _context.PostComments
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.Id == id);
        if (post == null)
        {
            throw new Exception("Post not found");
        }

        _context.PostComments.Remove(post);
        var result = await _context.SaveChangesAsync();
        if (result == 0)
        {
            throw new Exception("Failed to delete post");
        }

        return true;
    }

    public async Task<List<Data.DTO.PostCommentDTO>> GetPostCommentsByUserIdAsync(string userId)
    {
        var posts = await _context.PostComments
            .Include(p => p.User)
            .Where(p => p.UserId == userId)
            .ToListAsync();
        if (posts == null)
        {
            throw new Exception("Post not found");
        }

        return await Task.FromResult(posts.Select(p => new PostCommentDTO
        {
            Id = p.Id,
            PostId = p.PostId,
            UserId = p.UserId,
            Content = p.Content,
            UserName = p.User.UserName,
            CreatedAt = p.CreatedAt
        }).ToList());
    }

    public async Task<List<Data.DTO.PostCommentDTO>> GetPostCommentsByPostIdAsync(int postId)
    {
        var posts = await _context.PostComments
            .Include(p => p.User)
            .Where(p => p.PostId == postId)
            .ToListAsync();
        if (posts == null)
        {
            throw new Exception("Post not found");
        }

        return await Task.FromResult(posts.Select(p => new PostCommentDTO
        {
            Id = p.Id,
            PostId = p.PostId,
            UserId = p.UserId,
            Content = p.Content,
            UserName = p.User.UserName,
            CreatedAt = p.CreatedAt
        }).ToList());
    }
}