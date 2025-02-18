using System.Linq.Expressions;

namespace OurHeritage.Core.Specifications
{
    public class EntitySpecification<T> : Specification<T>
    {
        public EntitySpecification(SpecParams specParams, Expression<Func<T, bool>> criteria)
            : base(criteria)
        {
            ApplyPaging(specParams.PageSize * (specParams.PageIndex - 1), specParams.PageSize);
        }
    }

    public class SpecParams
    {
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string Search { get; set; }
        public int? FilterId { get; set; }
    }
}
