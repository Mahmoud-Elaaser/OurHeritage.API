using AutoMapper;
using OurHeritage.Core.Entities;
using OurHeritage.Repo.Repositories.Interfaces;
using OurHeritage.Service.DTOs;
using OurHeritage.Service.DTOs.HandiCraftDto;
using OurHeritage.Service.Helper;
using OurHeritage.Service.Interfaces;

namespace OurHeritage.Service.Implementations
{
    public class HandiCraftService : IHandiCraftService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public HandiCraftService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<ResponseDto> CreateHandiCraftAsync(CreateOrUpdateHandiCraftDto dto)
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
            foreach (var image in dto.Images)
            {
                var uploadedFiles = FilesSetting.UploadFile(image, "HandiCraft");
                if (uploadedFiles != null && uploadedFiles.Any())
                {
                    dto.ImageOrVideo.Add(uploadedFiles);
                }
                else
                {
                    return new ResponseDto
                    {
                        IsSucceeded = false,
                        Status = 401,
                        Message = "Please upload a valid file."
                    };

                }
            }
            var HandiCraft = _mapper.Map<HandiCraft>(dto);
            await _unitOfWork.Repository<HandiCraft>().AddAsync(HandiCraft);
            await _unitOfWork.CompleteAsync();
            return new ResponseDto
            {
                IsSucceeded = true,
                Status = 201,
                Model = dto,
                Message = "HandiCraft created successfully"
            };
        }

        public async Task<ResponseDto> GetHandiCraftByIdAsync(int HandiCraftId)
        {
            var HandiCraft = await _unitOfWork.Repository<HandiCraft>()
                .GetAllPredicated(e => e.Id == HandiCraftId, new[] { "User" });

            if (HandiCraft == null || !HandiCraft.Any())
            {
                return new ResponseDto
                {
                    IsSucceeded = false,
                    Status = 404,
                    Message = "HandiCraft not found"
                };
            }

            var handiCraftEntity = HandiCraft.First();

            var HandiCraftDto = _mapper.Map<GetHandiCraftDto>(handiCraftEntity);

            // Convert List<string> to a List<string> in DTO
            HandiCraftDto.ImageOrVideo = handiCraftEntity.ImageOrVideo ?? new List<string>();

            // Assign user details
            HandiCraftDto.NameOfUser = handiCraftEntity.User != null
                ? $"{handiCraftEntity.User.FirstName} {handiCraftEntity.User.LastName}"
                : "Unknown User";

            // Assign profile picture
            HandiCraftDto.UserProfilePicture = handiCraftEntity.User?.ProfilePicture ?? "default.jpg";

            return new ResponseDto
            {
                IsSucceeded = true,
                Status = 200,
                Model = HandiCraftDto
            };
        }





        public async Task<ResponseDto> GetAllHandiCraftsAsync()
        {
            var HandiCrafts = await _unitOfWork.Repository<HandiCraft>()
                .GetAllPredicated(h => true, new[] { "User" });

            var mappedHandiCrafts = _mapper.Map<IEnumerable<GetHandiCraftDto>>(HandiCrafts);

            // Set NameOfUser for each item
            foreach (var dto in mappedHandiCrafts)
            {
                var correspondingHandiCraft = HandiCrafts.FirstOrDefault(h => h.Id == dto.Id);
                if (correspondingHandiCraft != null && correspondingHandiCraft.User != null)
                {
                    dto.NameOfUser = $"{correspondingHandiCraft.User.FirstName} {correspondingHandiCraft.User.LastName}";
                }
                else
                {
                    dto.NameOfUser = "Unknown User";
                }
            }

            return new ResponseDto
            {
                IsSucceeded = true,
                Status = 200,
                Models = mappedHandiCrafts
            };
        }



        public async Task<ResponseDto> UpdateHandiCraftAsync(int HandiCraftId, CreateOrUpdateHandiCraftDto dto)
        {

            var HandiCraft = await _unitOfWork.Repository<HandiCraft>().GetByIdAsync(HandiCraftId);
            if (HandiCraft == null)
            {
                return new ResponseDto
                {
                    IsSucceeded = false,
                    Status = 404,
                    Message = "HandiCraft not found"
                };
            }
            if (dto.Images != null)
            {
                foreach (var imageUrl in dto.ImageOrVideo)
                {
                    FilesSetting.DeleteFile(imageUrl, "CulturalArticle");

                }
            }
            foreach (var image in dto.Images)
            {
                var uploadedFiles = FilesSetting.UploadFile(image, "CulturalArticle");
                if (uploadedFiles != null && uploadedFiles.Any())
                {
                    dto.ImageOrVideo.Add(uploadedFiles);
                }
                else
                {
                    return new ResponseDto
                    {
                        IsSucceeded = false,
                        Status = 401,
                        Message = "Please upload a valid file."
                    };

                }
            }
            _mapper.Map(dto, HandiCraft);
            _unitOfWork.Repository<HandiCraft>().Update(HandiCraft);
            await _unitOfWork.CompleteAsync();
            return new ResponseDto
            {
                IsSucceeded = true,
                Status = 200,
                Message = "HandiCraft updated successfully"
            };
        }

        public async Task<ResponseDto> DeleteHandiCraftAsync(int HandiCraftId)
        {
            var HandiCraft = await _unitOfWork.Repository<HandiCraft>().GetByIdAsync(HandiCraftId);
            if (HandiCraft == null)
            {
                return new ResponseDto
                {
                    IsSucceeded = false,
                    Status = 404,
                    Message = "HandiCraft not found"
                };
            }
            _unitOfWork.Repository<HandiCraft>().Delete(HandiCraft);
            await _unitOfWork.CompleteAsync();
            return new ResponseDto
            {
                IsSucceeded = true,
                Status = 200,
                Message = "HandiCraft deleted successfully"
            };
        }

    }
}
