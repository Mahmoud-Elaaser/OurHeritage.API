using AutoMapper;
using OurHeritage.Core.Entities;
using OurHeritage.Core.Specifications;
using OurHeritage.Repo.Repositories.Interfaces;
using OurHeritage.Service.DTOs;
using OurHeritage.Service.DTOs.StoryDto;
using OurHeritage.Service.Interfaces;
using System.Security.Claims;

namespace OurHeritage.Service.Implementations
{
    public class StoryService : IStoryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IPaginationService _paginationService;

        public StoryService(IUnitOfWork unitOfWork, IMapper mapper, IPaginationService paginationService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _paginationService = paginationService;
        }

        public async Task<ResponseDto> CreateStoryAsync(CreateOrUpdateStoryDto dto)
        {
            if (dto == null)
            {
                return new ResponseDto
                {
                    IsSucceeded = false,
                    Status = 400,
                    Message = "Model doesn't exist"
                };
            }

            var story = _mapper.Map<Story>(dto);
            await _unitOfWork.Repository<Story>().AddAsync(story);
            await _unitOfWork.CompleteAsync();

            return new ResponseDto
            {
                IsSucceeded = true,
                Status = 201,
                Model = dto,
                Message = "Story created successfully"
            };
        }

        public async Task<ResponseDto> GetStoryByIdAsync(int storyId)
        {
            var story = await _unitOfWork.Repository<Story>().GetByIdAsync(storyId);
            if (story == null)
            {
                return new ResponseDto
                {
                    IsSucceeded = false,
                    Status = 404,
                    Message = "Story not found"
                };
            }

            var storyDto = _mapper.Map<GetStoryDto>(story);
            return new ResponseDto
            {
                IsSucceeded = true,
                Status = 200,
                Model = storyDto
            };
        }

        public async Task<PaginationResponse<GetStoryDto>> GetAllStoriesAsync(int pageIndex = 1, int pageSize = 10)
        {
            var stories = await _unitOfWork.Repository<Story>().ListAllAsync();

            var specParams = new SpecParams
            {
                PageIndex = pageIndex,
                PageSize = pageSize
            };

            // Use PaginationService to paginate and map the stories
            var paginatedStories = _paginationService.Paginate(
                stories,
                specParams,
                story => _mapper.Map<GetStoryDto>(story)
            );

            return paginatedStories;
        }

        public async Task<ResponseDto> UpdateStoryAsync(int storyId, CreateOrUpdateStoryDto dto)
        {
            var story = await _unitOfWork.Repository<Story>().GetByIdAsync(storyId);
            if (story == null)
            {
                return new ResponseDto
                {
                    IsSucceeded = false,
                    Status = 404,
                    Message = "Story not found"
                };
            }

            _mapper.Map(dto, story);
            _unitOfWork.Repository<Story>().Update(story);
            await _unitOfWork.CompleteAsync();

            return new ResponseDto
            {
                IsSucceeded = true,
                Status = 200,
                Message = "Story updated successfully"
            };
        }

        public async Task<ResponseDto> DeleteStoryAsync(ClaimsPrincipal user, int storyId)
        {
            if (!int.TryParse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value, out int loggedInUserId))
            {
                return new ResponseDto
                {
                    IsSucceeded = false,
                    Status = 401,
                    Message = "User ID not found in token."
                };
            }

            var loggedInUserRole = user.FindFirst(ClaimTypes.Role)?.Value;

            var storyToDelete = await _unitOfWork.Repository<Story>().GetByIdAsync(storyId);
            if (storyToDelete == null)
            {
                return new ResponseDto
                {
                    IsSucceeded = false,
                    Status = 404,
                    Message = "Story not found."
                };
            }

            // Check if the logged-in user is an admin
            if (loggedInUserRole != "Admin")
            {
                return new ResponseDto
                {
                    IsSucceeded = false,
                    Status = 403,
                    Message = "You do not have permission to delete this story."
                };
            }

            _unitOfWork.Repository<Story>().Delete(storyToDelete);
            await _unitOfWork.CompleteAsync();

            return new ResponseDto
            {
                IsSucceeded = true,
                Status = 200,
                Message = "Story deleted successfully."
            };
        }
    }
}