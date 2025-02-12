using AutoMapper;
using OurHeritage.Core.Entities;
using OurHeritage.Repo.Repositories.Interfaces;
using OurHeritage.Service.DTOs;
using OurHeritage.Service.DTOs.CommentDto;
using OurHeritage.Service.Interfaces;

namespace OurHeritage.Service.Implementations
{
    public class CommentService : ICommentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CommentService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<ResponseDto> GetAllCommentsAsync()
        {
            var comment = await _unitOfWork.Repository<Comment>().GetAllAsync();
            var mappedcomment = _mapper.Map<IEnumerable<GetCommentDto>>(comment);
            return new ResponseDto
            {
                IsSucceeded = true,
                Status = 200,
                Models = mappedcomment
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
            await _unitOfWork.Complete();

            /// TODO: SgnalR 
            /// Notification: User with id: commented on your tweet


            var mappedComment = _mapper.Map<GetCommentDto>(comment);
            return new ResponseDto
            {
                IsSucceeded = true,
                Status = 200,
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
            await _unitOfWork.Complete();

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
                    Message = "comment not found."
                };
            }

            _unitOfWork.Repository<Comment>().Delete(comment);
            await _unitOfWork.Complete();

            return new ResponseDto
            {
                IsSucceeded = true,
                Status = 200,
                Message = "comment deleted successfully."
            };
        }

        public async Task<ResponseDto> GetAllCommentsOnCulturalArticleAsync(int culturalArticleId)
        {
            var comments = await _unitOfWork.Repository<Comment>().GetAllPredicated(t => t.CulturalArticleId == culturalArticleId);

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
