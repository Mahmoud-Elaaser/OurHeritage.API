using OurHeritage.Service.DTOs;
using OurHeritage.Service.DTOs.CategoryDto;

namespace OurHeritage.Service.Interfaces
{
    public interface ICategoryService
    {
        Task<ResponseDto> CreateCategoryAsync(CreateOrUpdateCategoryDto dto);
        Task<ResponseDto> GetCategoryByIdAsync(int CategoryId);
        Task<ResponseDto> GetAllCategoriesAsync();
        Task<ResponseDto> UpdateCategoryAsync(int CategoryId, CreateOrUpdateCategoryDto dto);
        Task<ResponseDto> DeleteCategoryAsync(int CategoryId);
    }
}
