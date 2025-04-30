using Microsoft.EntityFrameworkCore;
using OurHeritage.Core.Context;
using OurHeritage.Core.Entities;
using OurHeritage.Repo.Repositories.Interfaces;
using OurHeritage.Service.DTOs;
using OurHeritage.Service.DTOs.OrderDto;
using OurHeritage.Service.Interfaces;

namespace OurHeritage.Service.Implementations
{
    public class BasketService : IBasketService
    {
        private readonly ApplicationDbContext _context;
        private readonly IUnitOfWork _unitOfWork;

        public BasketService(IUnitOfWork unitOfWork, ApplicationDbContext context)
        {
            _context = context;
            _unitOfWork = unitOfWork;
        }

        public async Task<ResponseDto> AddToBasketAsync(int userId, int handiCraftId, int quantity)
        {
            var handiCraft = await _context.HandiCrafts.FindAsync(handiCraftId);
            if (handiCraft == null)
            {
                return new ResponseDto { IsSucceeded = false, Message = "HandiCraft not found." };
            }

            var repo = _unitOfWork.Repository<BasketItem>();
            var existingItem = await repo.FindAsync(b => b.UserId == userId && b.HandiCraftId == handiCraftId);

            BasketItem item;

            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
                item = existingItem;
            }
            else
            {
                item = new BasketItem
                {
                    UserId = userId,
                    HandiCraftId = handiCraftId,
                    Quantity = quantity
                };
                await repo.AddAsync(item);
            }

            await _context.SaveChangesAsync();

            // Refetch with HandiCraft data included
            var basketItemWithHandiCraft = await _context.BasketItems
                .Include(b => b.HandiCraft)
                .FirstOrDefaultAsync(b => b.UserId == userId && b.HandiCraftId == handiCraftId);

            var responseDto = new BasketItemResponseDto
            {
                Id = basketItemWithHandiCraft.Id,
                Quantity = basketItemWithHandiCraft.Quantity,
                HandiCraftId = basketItemWithHandiCraft.HandiCraftId,
                HandiCraft = new HandiCraftResponseDto
                {
                    Id = basketItemWithHandiCraft.HandiCraft.Id,
                    Title = basketItemWithHandiCraft.HandiCraft.Title,
                    Price = basketItemWithHandiCraft.HandiCraft.Price,
                    ImageOrVideo = basketItemWithHandiCraft.HandiCraft.ImageOrVideo?.ToList()
                }
            };

            return new ResponseDto
            {
                IsSucceeded = true,
                Message = "Item added to basket successfully.",
                Model = responseDto
            };
        }


        public async Task<ResponseDto> GetItemByIdAsync(int id)
        {
            var includes = new string[] { "HandiCraft" };
            var item = (await _unitOfWork.Repository<BasketItem>()
                .GetAllPredicated(b => b.Id == id, includes)).FirstOrDefault();

            if (item == null)
            {
                return new ResponseDto { IsSucceeded = false, Message = "Item not found." };
            }

            var itemDto = new BasketItemResponseDto
            {
                Id = item.Id,
                Quantity = item.Quantity,
                HandiCraftId = item.HandiCraftId,
                HandiCraft = new HandiCraftResponseDto
                {
                    Id = item.HandiCraft.Id,
                    Title = item.HandiCraft.Title,
                    Price = item.HandiCraft.Price,
                    ImageOrVideo = item.HandiCraft.ImageOrVideo?.ToList()
                }
            };

            return new ResponseDto
            {
                IsSucceeded = true,
                Model = itemDto
            };
        }


        public async Task<ResponseDto> GetAllItemsAsync()
        {
            var items = await _unitOfWork.Repository<BasketItem>().ListAllAsync();
            return new ResponseDto { IsSucceeded = true, Models = items };
        }

        public async Task<ResponseDto> GetItemsForUserAsync(int userId)
        {
            var includes = new string[] { "HandiCraft" };

            var items = await _unitOfWork.Repository<BasketItem>()
                .GetAllPredicated(b => b.UserId == userId, includes);

            if (items == null || !items.Any())
            {
                return new ResponseDto
                {
                    IsSucceeded = false,
                    Message = "No basket items found for this user."
                };
            }

            var basketDtos = items.Select(item => new BasketItemResponseDto
            {
                Id = item.Id,
                Quantity = item.Quantity,
                HandiCraftId = item.HandiCraftId,
                HandiCraft = new HandiCraftResponseDto
                {
                    Id = item.HandiCraft.Id,
                    Title = item.HandiCraft.Title,
                    Price = item.HandiCraft.Price,
                    ImageOrVideo = item.HandiCraft.ImageOrVideo?.ToList()
                }
            }).ToList();

            return new ResponseDto
            {
                IsSucceeded = true,
                Models = basketDtos
            };
        }




        public async Task<ResponseDto> UpdateBasketItemAsync(int basketItemId, int quantity)
        {
            var item = await _unitOfWork.Repository<BasketItem>().GetByIdAsync(basketItemId);
            if (item == null)
            {
                return new ResponseDto { IsSucceeded = false, Message = "Item not found." };
            }

            item.Quantity = quantity;
            await _context.SaveChangesAsync();

            return new ResponseDto { IsSucceeded = true, Message = "Basket item updated successfully." };
        }

        public async Task<ResponseDto> DeleteItemAsync(int id)
        {
            var item = await _unitOfWork.Repository<BasketItem>().GetByIdAsync(id);
            if (item == null)
            {
                return new ResponseDto { IsSucceeded = false, Message = "Item not found." };
            }

            _unitOfWork.Repository<BasketItem>().Delete(item);
            await _context.SaveChangesAsync();

            return new ResponseDto { IsSucceeded = true, Message = "Item deleted successfully." };
        }
    }

}
