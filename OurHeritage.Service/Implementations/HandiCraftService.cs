using AutoMapper;
using OurHeritage.Core.Entities;
using OurHeritage.Repo.Repositories.Interfaces;
using OurHeritage.Service.DTOs;
using OurHeritage.Service.DTOs.HandiCraftDto;
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
            var HandiCraftDto = _mapper.Map<GetHandiCraftDto>(HandiCraft);
            return new ResponseDto
            {
                IsSucceeded = true,
                Status = 200,
                Model = HandiCraftDto
            };
        }

        public async Task<ResponseDto> GetAllHandiCraftsAsync()
        {
            var HandiCrafts = await _unitOfWork.Repository<HandiCraft>().ListAllAsync();

            var mappedHandiCrafts = _mapper.Map<IEnumerable<GetHandiCraftDto>>(HandiCrafts);

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
