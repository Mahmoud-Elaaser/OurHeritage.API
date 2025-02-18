using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using OurHeritage.Core.Entities;
using OurHeritage.Repo.Repositories.Interfaces;
using OurHeritage.Service.DTOs;
using OurHeritage.Service.DTOs.CommentDto;
using OurHeritage.Service.Interfaces;
using OurHeritage.Service.SignalR;
using System.Linq.Expressions;

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
            var comments = await _unitOfWork.Repository<Comment>().ListAllAsync();
            var mappedComments = _mapper.Map<IEnumerable<GetCommentDto>>(comments);
            return new ResponseDto
            {
                IsSucceeded = true,
                Status = 200,
                Models = mappedComments
            };
        }

        public async Task<ResponseDto> GetCommentByIdAsync(int id)
        {
            var comment = await _unitOfWork.Repository<Comment>().FindAsync(c => c.Id == id);
            if (comment == null)
            {
                return new ResponseDto
                {
                    IsSucceeded = false,
                    Status = 404,
                    Message = "Comment not found."
                };
            }
            var mappedComment = _mapper.Map<GetCommentDto>(comment);

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

            var mappedComment = _mapper.Map<GetCommentDto>(comment);
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

        public async Task<ResponseDto> DeleteCommentAsync(int id)
        {
            var comment = await _unitOfWork.Repository<Comment>().FindAsync(c => c.Id == id);
            if (comment == null)
            {
                return new ResponseDto
                {
                    IsSucceeded = false,
                    Status = 404,
                    Message = "Comment not found."
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
            var comments = await _unitOfWork.Repository<Comment>().GetAllPredicated(predicate, includes);

            if (comments == null)
            {
                return new ResponseDto
                {
                    IsSucceeded = false,
                    Status = 404,
                    Message = "Comments not found"
                };
            }

            var mappedComments = _mapper.Map<IEnumerable<GetCommentDto>>(comments);
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