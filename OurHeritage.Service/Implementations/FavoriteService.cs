using AutoMapper;
using OurHeritage.Core.Entities;
using OurHeritage.Repo.Repositories.Interfaces;
using OurHeritage.Service.DTOs;
using OurHeritage.Service.DTOs.FavoriteDto;
using OurHeritage.Service.Interfaces;

namespace OurHeritage.Service.Implementations
{
    public class FavoriteService : IFavoriteService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public FavoriteService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<ResponseDto> AddToFavoriteAsync(AddToFavoriteDto dto)
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
            var favorite = _mapper.Map<Favorite>(dto);
            await _unitOfWork.Repository<Favorite>().AddAsync(favorite);
            await _unitOfWork.CompleteAsync();
            return new ResponseDto
            {
                IsSucceeded = true,
                Status = 201,
                Model = dto,
                Message = "Favorite created successfully"
            };
        }

        public async Task<ResponseDto> GetFavoriteByIdAsync(int favoriteId)
        {
            var favorite = await _unitOfWork.Repository<Favorite>().GetByIdAsync(favoriteId);
            if (favorite == null)
            {
                return new ResponseDto
                {
                    IsSucceeded = false,
                    Status = 404,
                    Message = "Favorite not found"
                };
            }
            var favoriteDto = _mapper.Map<GetFavoriteDto>(favorite);
            return new ResponseDto
            {
                IsSucceeded = true,
                Status = 200,
                Model = favoriteDto
            };
        }

        public async Task<ResponseDto> GetAllFavoritesAsync()
        {
            var favorites = await _unitOfWork.Repository<Favorite>().ListAllAsync();

            var mappedFavorites = _mapper.Map<IEnumerable<GetFavoriteDto>>(favorites);

            return new ResponseDto
            {
                IsSucceeded = true,
                Status = 200,
                Models = mappedFavorites
            };
        }

        public async Task<ResponseDto> DeleteFavoriteAsync(int favoriteId)
        {
            var favorite = await _unitOfWork.Repository<Favorite>().GetByIdAsync(favoriteId);
            if (favorite == null)
            {
                return new ResponseDto
                {
                    IsSucceeded = false,
                    Status = 404,
                    Message = "Favorite not found"
                };
            }
            _unitOfWork.Repository<Favorite>().Delete(favorite);
            await _unitOfWork.CompleteAsync();
            return new ResponseDto
            {
                IsSucceeded = true,
                Status = 200,
                Message = "Favorite deleted successfully"
            };
        }
    }
}