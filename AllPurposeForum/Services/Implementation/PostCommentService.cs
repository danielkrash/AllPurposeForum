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
        if (!posts.Any()) // Should be !posts.Any() or check count if expecting multiple
        {
            // Consider returning an empty list instead of throwing an exception
            // if no comments for a post is a valid scenario.
            // For now, keeping existing logic but noting it.
            throw new Exception("Post comments not found for the given post ID.");
        }

        return posts.Select(p => new PostCommentDTO
        {
            Id = p.Id,
            PostId = p.PostId,
            UserId = p.UserId,
            Content = p.Content,
            UserName = p.User?.UserName ?? "Unknown User",
            CreatedAt = p.CreatedAt,
            isApproved = p.Acceptence ?? false
        }).ToList();
    }

    public async Task<List<UnapprovedCommentDTO>> GetUnapprovedCommentsAsync()
    {
        var unapprovedComments = await _context.PostComments
            .Where(pc => pc.Acceptence == false || pc.Acceptence == null)
            .Include(pc => pc.User)
            .Include(pc => pc.Post)
                .ThenInclude(p => p.Topic)
            .Select(pc => new UnapprovedCommentDTO
            {
                CommentId = pc.Id,
                Text = pc.Content ?? string.Empty,
                DateCreated = pc.CreatedAt ?? System.DateTime.MinValue,
                AuthorUserName = pc.User == null ? "Unknown User" : (pc.User.UserName ?? "Unnamed User"),
                PostId = pc.PostId,
                PostTitle = pc.Post == null ? "Unknown Post" : (pc.Post.Title ?? "Untitled Post"),
                TopicId = pc.Post == null || pc.Post.Topic == null ? 0 : pc.Post.Topic.Id,
                TopicTitle = pc.Post == null || pc.Post.Topic == null ? "Unknown Topic" : (pc.Post.Topic.Title ?? "Untitled Topic"),
                OriginalPostContentPreview = pc.Post == null || string.IsNullOrEmpty(pc.Post.Content)
                                             ? "No content preview available"
                                             : (pc.Post.Content.Length > 100 ? pc.Post.Content.Substring(0, 100) + "..." : pc.Post.Content)
            })
            .ToListAsync();

        return unapprovedComments;
    }

    public async Task ApproveCommentAsync(int commentId)
    {
        var comment = await _context.PostComments.FindAsync(commentId);
        if (comment == null)
        {
            throw new Exception("Comment not found.");
        }
        comment.Acceptence = true;
        await _context.SaveChangesAsync();
    }

    public async Task RejectCommentAsync(int commentId)
    {
        var comment = await _context.PostComments.FindAsync(commentId);
        if (comment == null)
        {
            throw new Exception("Comment not found.");
        }
        _context.PostComments.Remove(comment);
        await _context.SaveChangesAsync();
    }
}