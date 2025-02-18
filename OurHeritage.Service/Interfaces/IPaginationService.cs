using OurHeritage.Core.Specifications;

namespace OurHeritage.Service.Interfaces
{
    public interface IPaginationService
    {
        PaginationResponse<TDto> Paginate<TEntity, TDto>(IEnumerable<TEntity> entities, SpecParams specParams, Func<TEntity, TDto> selector);
    }
}
