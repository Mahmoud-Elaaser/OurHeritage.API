namespace OurHeritage.Core.Specifications
{
    public class PaginationResponse<T>
    {
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
        public IEnumerable<T> Items { get; set; }
    }
}
