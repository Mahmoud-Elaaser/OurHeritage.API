using AutoMapper;
using OurHeritage.Core.Entities;
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

        public StoryService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
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

        public async Task<ResponseDto> GetAllStoriesAsync()
        {
            var stories = await _unitOfWork.Repository<Story>().ListAllAsync();

            var mappedStories = _mapper.Map<IEnumerable<GetStoryDto>>(stories);

            return new ResponseDto
            {
                IsSucceeded = true,
                Status = 200,
                Models = mappedStories
            };
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