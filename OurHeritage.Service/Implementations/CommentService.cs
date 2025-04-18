using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using OurHeritage.Core.Entities;
using OurHeritage.Repo.Repositories.Interfaces;
using OurHeritage.Service.DTOs;
using OurHeritage.Service.DTOs.CommentDto;
using OurHeritage.Service.Interfaces;
using OurHeritage.Service.SignalR;
using System.Linq.Expressions;
using System.Security.Claims;

namespace OurHeritage.Service.Implementations
{
    public class CommentService : ICommentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IHubContext<NotificationHub> _hubContext;

        public CommentService(IUnitOfWork unitOfWork, IMapper mapper, IHubContext<NotificationHub> hubContext)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _hubContext = hubContext;
        }

        public async Task<ResponseDto> GetAllCommentsAsync()
        {
            var comments = await _unitOfWork.Repository<Comment>()
                .GetAllPredicated(c => true, new[] { "User" });

            var mappedComments = _mapper.Map<IEnumerable<GetCommentDto>>(comments);

            // Set user information for each comment
            foreach (var dto in mappedComments)
            {
                var correspondingComment = comments.FirstOrDefault(c => c.Id == dto.Id);
                if (correspondingComment != null && correspondingComment.User != null)
                {
                    dto.NameOfUser = $"{correspondingComment.User.FirstName} {correspondingComment.User.LastName}";
                    dto.UserProfilePicture = correspondingComment.User?.ProfilePicture ?? "default.jpg";
                }
                else
                {
                    dto.NameOfUser = "Unknown User";
                    dto.UserProfilePicture = "default.jpg";
                }
            }

            return new ResponseDto
            {
                IsSucceeded = true,
                Status = 200,
                Models = mappedComments
            };
        }

        public async Task<ResponseDto> GetCommentByIdAsync(int id)
        {
            // Include the User in the query
            var comments = await _unitOfWork.Repository<Comment>()
                .GetAllPredicated(c => c.Id == id, new[] { "User" });

            if (comments == null || !comments.Any())
            {
                return new ResponseDto
                {
                    IsSucceeded = false,
                    Status = 404,
                    Message = "Comment not found."
                };
            }

            var comment = comments.First();
            var mappedComment = _mapper.Map<GetCommentDto>(comment);

            // Add user name and profile picture to the DTO
            mappedComment.NameOfUser = comment.User != null
                ? $"{comment.User.FirstName} {comment.User.LastName}"
                : "Unknown User";

            mappedComment.UserProfilePicture = comment.User?.ProfilePicture ?? "default.jpg";

            return new ResponseDto
            {
                IsSucceeded = true,
                Status = 200,
                Model = mappedComment
            };
        }

        public async Task<ResponseDto> AddCommentAsync(CreateOrUpdateCommentDto createCommentDto)
        {
            var comment = _mapper.Map<Comment>(createCommentDto);
            comment.DateCreated = DateTime.UtcNow;

            await _unitOfWork.Repository<Comment>().AddAsync(comment);
            await _unitOfWork.CompleteAsync();

            // SignalR Notification
            var article = await _unitOfWork.Repository<CulturalArticle>().GetByIdAsync(createCommentDto.CulturalArticleId);
            await _hubContext.Clients.User(article.UserId.ToString()).SendAsync("ReceiveNotification", $"User with id: {createCommentDto.UserId} commented on your article");

            // Get the user information to include in the response
            
              var user = await _unitOfWork.Repository<User>().GetByIdAsync(createCommentDto.UserId);
            
            var mappedComment = _mapper.Map<GetCommentDto>(comment);

            // Set user information
            if (user != null)
            {
                mappedComment.NameOfUser = $"{user.FirstName} {user.LastName}";
                mappedComment.UserProfilePicture = user.ProfilePicture ?? "default.jpg";
            }
            else
            {
                mappedComment.NameOfUser = "Unknown User";
                mappedComment.UserProfilePicture = "default.jpg";
            }

            return new ResponseDto
            {
                IsSucceeded = true,
                Status = 201,
                Model = mappedComment,
                Message = "Comment created successfully."
            };
        }

        public async Task<ResponseDto> UpdateCommentAsync(int id, CreateOrUpdateCommentDto updateCommentDto)
        {
            var existingComment = await _unitOfWork.Repository<Comment>().GetByIdAsync(id);
            if (existingComment == null)
            {
                return new ResponseDto
                {
                    IsSucceeded = false,
                    Status = 404,
                    Message = "Comment not found"
                };
            }

            _mapper.Map(updateCommentDto, existingComment);
            _unitOfWork.Repository<Comment>().Update(existingComment);
            await _unitOfWork.CompleteAsync();

            var mappedComment = _mapper.Map<GetCommentDto>(existingComment);
            return new ResponseDto
            {
                IsSucceeded = true,
                Status = 200,
                Message = "Comment updated successfully"
            };
        }

        public async Task<ResponseDto> DeleteCommentAsync(ClaimsPrincipal user, int commentId)
        {
            // Extract user ID from the token
            if (!int.TryParse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value, out int loggedInUserId))
            {
                return new ResponseDto
                {
                    IsSucceeded = false,
                    Status = 401,
                    Message = "User ID not found in token."
                };
            }

            // Extract user role from the token
            var loggedInUserRole = user.FindFirst(ClaimTypes.Role)?.Value;

            var comment = await _unitOfWork.Repository<Comment>().FindAsync(c => c.Id == commentId);
            if (comment == null)
            {
                return new ResponseDto
                {
                    IsSucceeded = false,
                    Status = 404,
                    Message = "Comment not found."
                };
            }

            // Check if the logged-in user is the comment owner or an admin
            if (loggedInUserId != comment.UserId && loggedInUserRole != "Admin")
            {
                return new ResponseDto
                {
                    IsSucceeded = false,
                    Status = 403,
                    Message = "You do not have permission to delete this comment."
                };
            }

            _unitOfWork.Repository<Comment>().Delete(comment);
            await _unitOfWork.CompleteAsync();

            return new ResponseDto
            {
                IsSucceeded = true,
                Status = 200,
                Message = "Comment deleted successfully."
            };
        }


        public async Task<ResponseDto> GetAllCommentsOnCulturalArticleAsync(Expression<Func<Comment, bool>> predicate, string[] includes = null)
        {
            var comments = await _unitOfWork.Repository<Comment>().GetAllPredicated(predicate, new[] { "User" });

            var mappedComments = _mapper.Map<IEnumerable<GetCommentDto>>(comments);

            // Set user information for each comment
            foreach (var dto in mappedComments)
            {
                var correspondingComment = comments.FirstOrDefault(c => c.Id == dto.Id);
                if (correspondingComment != null && correspondingComment.User != null)
                {
                    dto.NameOfUser = $"{correspondingComment.User.FirstName} {correspondingComment.User.LastName}";
                    dto.UserProfilePicture = !string.IsNullOrEmpty(correspondingComment.User.ProfilePicture)
                        ? correspondingComment.User.ProfilePicture
                        : "default.jpg";
                }
                else
                {
                    dto.NameOfUser = "Unknown User";
                    dto.UserProfilePicture = "default.jpg";
                }
            }

            return new ResponseDto
            {
                IsSucceeded = true,
                Status = 200,
                Message = "Comments retrieved successfully",
                Models = mappedComments
            };
        }
    }
}