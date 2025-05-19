using AllPurposeForum.Data;
using AllPurposeForum.Data.DTO;
using Microsoft.EntityFrameworkCore;
// Add using for the ML model namespace if it's different or to shorten type names
// Assuming MLModelNewAttempt is in the AllPurposeForum namespace as per MLModelNewAttempt.consumption.cs
using AllPurposeForum.Helpers; // Added for Utils class

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
            UserName = p.User?.UserName ?? "Unknown User", // Added null check
            CreatedAt = p.CreatedAt,
            isApproved = p.Acceptence ?? false // Map Acceptence to isApproved
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
            UserName = post.User?.UserName ?? "Unknown User", // Added null check
            CreatedAt = post.CreatedAt,
            isApproved = post.Acceptence ?? false // Map Acceptence to isApproved
        });
    }

    public async Task<CreatePostCommentDTO> CreatePostCommentAsync(CreatePostCommentDTO postCommentDto)
    {
        if (!_context.Users.Any(u => u.Id == postCommentDto.UserId) ||
            !_context.Posts.Any(p => p.Id == postCommentDto.PostId))
        {
            throw new Exception("User or Post not found");
        }

        // Predict sentiment using the ML model
        var modelInput = new MLModel.ModelInput
        {
            Sentiment = postCommentDto.Content // Assuming 'Content' is the text to analyze
        };
        var prediction = MLModel.Predict(modelInput);
        
        // Use the utility function to determine if the comment is acceptable
        bool isAcceptable = Utils.IsCommentAcceptable(prediction.PredictedLabel);

        var newCommentEntity = new Data.Models.PostComment
        {
            UserId = postCommentDto.UserId,
            PostId = postCommentDto.PostId,
            Content = postCommentDto.Content,
            Acceptence = isAcceptable // Set based on ML model prediction
        };

        _context.PostComments.Add(newCommentEntity);
        var result = await _context.SaveChangesAsync();
        if (result == 0)
        {
            throw new Exception("Failed to create post comment."); // Corrected message
        }

        // Return the original DTO, or map the created entity back to a DTO if needed
        return postCommentDto;
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
            UserName = post.User?.UserName ?? "Unknown User", // Added null check
            isApproved = post.Acceptence ?? false // Map Acceptence to isApproved after update
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
            UserName = p.User?.UserName ?? "Unknown User", // Added null check
            CreatedAt = p.CreatedAt,
            isApproved = p.Acceptence ?? false // Map Acceptence to isApproved
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
            UserName = p.User?.UserName ?? "Unknown User", // Added null check
            CreatedAt = p.CreatedAt,
            isApproved = p.Acceptence ?? false // Map Acceptence to isApproved
        }).ToList());
    }
}