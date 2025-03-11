using OurHeritage.Core.Entities;
using OurHeritage.Service.DTOs;
using OurHeritage.Service.DTOs.CommentDto;
using System.Linq.Expressions;
using System.Security.Claims;

namespace OurHeritage.Service.Interfaces
{
    public interface ICommentService
    {
        Task<ResponseDto> GetCommentByIdAsync(int id);
        Task<ResponseDto> GetAllCommentsAsync();
        Task<ResponseDto> AddCommentAsync(CreateOrUpdateCommentDto createCommentDto);
        Task<ResponseDto> UpdateCommentAsync(int id, CreateOrUpdateCommentDto updateCommentDto);
        Task<ResponseDto> DeleteCommentAsync(ClaimsPrincipal user, int commentId);
        Task<ResponseDto> GetAllCommentsOnCulturalArticleAsync(Expression<Func<Comment, bool>> predicate, string[] includes = null);
    }
}
