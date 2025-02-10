using Microsoft.EntityFrameworkCore;
using OurHeritage.Repo.Repositories.Interfaces;

namespace OurHeritage.Repo.Repositories.Implementations
{
    public static class SpecificationEvaluator<T> where T : class
    {
        public static IQueryable<T> GetQuery(IQueryable<T> EntryPoint, ISpecifications<T> Spec)
        {
            var Query = EntryPoint;
            if (Spec.Criteria != null)
            {
                Query = Query.Where(Spec.Criteria);
            }
            /// Ascending
            if (Spec.OrderBy != null)
            {
                Query = Query.OrderBy(Spec.OrderBy);
            }
            if (Spec.OrderByDesc != null)
            {
                Query = Query.OrderByDescending(Spec.OrderByDesc);
            }
            if (Spec.IsPaginationEnabled)
            {
                Query = Query.Skip(Spec.Skip).Take(Spec.Take);
            }

            Query = Spec.Includes.Aggregate(Query, (CurrentQuery, IncludeExpression)
                => CurrentQuery.Include(IncludeExpression));

            return Query;
        }
    }
}
