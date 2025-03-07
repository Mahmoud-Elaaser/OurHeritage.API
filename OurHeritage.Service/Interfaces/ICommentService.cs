﻿using OurHeritage.Core.Entities;
using OurHeritage.Service.DTOs;
using OurHeritage.Service.DTOs.CommentDto;
using System.Linq.Expressions;

namespace OurHeritage.Service.Interfaces
{
    public interface ICommentService
    {
        Task<ResponseDto> GetCommentByIdAsync(int id);
        Task<ResponseDto> GetAllCommentsAsync();
        Task<ResponseDto> AddCommentAsync(CreateOrUpdateCommentDto createCommentDto);
        Task<ResponseDto> UpdateCommentAsync(int id, CreateOrUpdateCommentDto updateCommentDto);
        Task<ResponseDto> DeleteCommentAsync(int id);
        Task<ResponseDto> GetAllCommentsOnCulturalArticleAsync(Expression<Func<Comment, bool>> predicate, string[] includes = null);
    }
}
