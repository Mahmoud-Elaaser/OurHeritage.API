using AutoMapper;
using OurHeritage.Core.Entities;
using OurHeritage.Repo.Repositories.Interfaces;
using OurHeritage.Service.DTOs;
using OurHeritage.Service.DTOs.CategoryDto;
using OurHeritage.Service.Interfaces;

namespace OurHeritage.Service.Implementations
{
    public class CategoryService : ICategoryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CategoryService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<ResponseDto> CreateCategoryAsync(CreateOrUpdateCategoryDto dto)
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
            var Category = _mapper.Map<Category>(dto);
            await _unitOfWork.Repository<Category>().AddAsync(Category);
            await _unitOfWork.CompleteAsync();
            return new ResponseDto
            {
                IsSucceeded = true,
                Status = 201,
                Model = dto,
                Message = "Category created successfully"
            };
        }

        public async Task<ResponseDto> GetCategoryByIdAsync(int CategoryId)
        {
            var Category = await _unitOfWork.Repository<Category>().GetByIdAsync(CategoryId);
            if (Category == null)
            {
                return new ResponseDto
                {
                    IsSucceeded = false,
                    Status = 404,
                    Message = "Category not found"
                };
            }
            var CategoryDto = _mapper.Map<GetCategoryDto>(Category);
            return new ResponseDto
            {
                IsSucceeded = true,
                Status = 200,
                Model = CategoryDto
            };
        }

        public async Task<ResponseDto> GetAllCategoriesAsync()
        {
            var Categorys = await _unitOfWork.Repository<Category>().ListAllAsync();

            var mappedCategorys = _mapper.Map<IEnumerable<GetCategoryDto>>(Categorys);

            return new ResponseDto
            {
                IsSucceeded = true,
                Status = 200,
                Models = mappedCategorys
            };
        }


        public async Task<ResponseDto> UpdateCategoryAsync(int CategoryId, CreateOrUpdateCategoryDto dto)
        {
            var Category = await _unitOfWork.Repository<Category>().GetByIdAsync(CategoryId);
            if (Category == null)
            {
                return new ResponseDto
                {
                    IsSucceeded = false,
                    Status = 404,
                    Message = "Category not found"
                };
            }
            _mapper.Map(dto, Category);
            _unitOfWork.Repository<Category>().Update(Category);
            await _unitOfWork.CompleteAsync();
            return new ResponseDto
            {
                IsSucceeded = true,
                Status = 200,
                Message = "Category updated successfully"
            };
        }

        public async Task<ResponseDto> DeleteCategoryAsync(int CategoryId)
        {
            var Category = await _unitOfWork.Repository<Category>().GetByIdAsync(CategoryId);
            if (Category == null)
            {
                return new ResponseDto
                {
                    IsSucceeded = false,
                    Status = 404,
                    Message = "Category not found"
                };
            }
            _unitOfWork.Repository<Category>().Delete(Category);
            await _unitOfWork.CompleteAsync();
            return new ResponseDto
            {
                IsSucceeded = true,
                Status = 200,
                Message = "Category deleted successfully"
            };
        }

    }
}
