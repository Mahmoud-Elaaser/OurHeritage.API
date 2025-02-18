using OurHeritage.Core.Specifications;
using OurHeritage.Service.Interfaces;

namespace OurHeritage.Service.Implementations
{
    public class PaginationService : IPaginationService
    {
        public PaginationResponse<TDto> Paginate<TEntity, TDto>(IEnumerable<TEntity> entities, SpecParams specParams, Func<TEntity, TDto> selector)
        {
            var totalEntities = entities.Count();
            var items = entities.Skip((specParams.PageIndex - 1) * specParams.PageSize)
                                .Take(specParams.PageSize)
                                .Select(selector)
                                .ToList();

            return new PaginationResponse<TDto>
            {
                Items = items,
                PageIndex = specParams.PageIndex,
                PageSize = specParams.PageSize,
                TotalItems = totalEntities,
                TotalPages = (int)Math.Ceiling((double)totalEntities / specParams.PageSize)
            };
        }
    }
}
