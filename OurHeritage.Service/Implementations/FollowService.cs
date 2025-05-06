using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using OurHeritage.Core.Entities;
using OurHeritage.Repo.Repositories.Interfaces;
using OurHeritage.Service.DTOs;
using OurHeritage.Service.DTOs.FollowDto;
using OurHeritage.Service.Interfaces;
using OurHeritage.Service.SignalR;
using System.Linq.Expressions;

namespace OurHeritage.Service.Implementations
{
    public class FollowService : IFollowService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly INotificationService _notificationService;

        public FollowService(IUnitOfWork unitOfWork, IMapper mapper, IHubContext<NotificationHub> hubContext, INotificationService notificationService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _hubContext = hubContext;
            _notificationService = notificationService;
        }


        public async Task<ResponseDto> FollowUserAsync(FollowDto createFollowDto)
        {
            var follower = await _unitOfWork.Repository<User>().GetByIdAsync(createFollowDto.FollowerId);
            var following = await _unitOfWork.Repository<User>().GetByIdAsync(createFollowDto.FollowingId);

            if (follower == null || following == null)
            {
                return new ResponseDto
                {
                    IsSucceeded = false,
                    Message = "Follower or following user not found."
                };
            }

            // Check if the user is already following this user
            var existingFollow = await _unitOfWork.Repository<Follow>()
                .FindAsync(f => f.FollowerId == createFollowDto.FollowerId
                && f.FollowingId == createFollowDto.FollowingId);

            if (existingFollow != null)
            {
                return new ResponseDto
                {
                    IsSucceeded = false,
                    Message = "You are already following this user."
                };
            }

            var follow = _mapper.Map<Follow>(createFollowDto);
            await _unitOfWork.Repository<Follow>().AddAsync(follow);
            await _unitOfWork.CompleteAsync();

            // Create notification using NotificationService
            string notificationMessage = $"{follower.FirstName} {follower.LastName} started following you";
            var notificationResult = await _notificationService.CreateFollowNotificationAsync(
                createFollowDto.FollowerId,
                createFollowDto.FollowingId,
                notificationMessage);

            // Send real-time notification via SignalR
            //await _hubContext.Clients
            //    .User(createFollowDto.FollowingId.ToString())
            //    .SendAsync("ReceiveNotification", notificationMessage);

            await _hubContext.Clients.User(createFollowDto.FollowerId.ToString())
                .SendAsync("NotifyUserFollowed", createFollowDto.FollowingId, notificationMessage);

            return new ResponseDto
            {
                IsSucceeded = true,
                Message = "Followed successfully.",
                Model = new
                {
                    Follow = follow,
                    Notification = notificationResult.Success ? notificationResult.Data : null
                }
            };
        }

        public async Task<ResponseDto> UnfollowUserAsync(FollowDto followDto)
        {
            var followerId = followDto.FollowerId;
            var followingId = followDto.FollowingId;

            var follow = await _unitOfWork.Repository<Follow>()
                .FindAsync(f => f.FollowerId == followerId && f.FollowingId == followingId);

            if (follow == null)
            {
                return new ResponseDto
                {
                    IsSucceeded = false,
                    Message = "You are not following this user."
                };
            }

            _unitOfWork.Repository<Follow>().Delete(follow);
            await _unitOfWork.CompleteAsync();

            return new ResponseDto
            {
                IsSucceeded = true,
                Message = "Unfollowed successfully."
            };
        }

        public async Task<ResponseDto> GetFollowersAsync(int userId)
        {
            // Check if the user exists
            var user = await _unitOfWork.Repository<User>().GetByIdAsync(userId);

            if (user == null)
            {
                return new ResponseDto
                {
                    IsSucceeded = false,
                    Message = "User not found."
                };
            }

            // Define the predicate to filter followers
            Expression<Func<Follow, bool>> predicate = f => f.FollowingId == userId;

            // Include the Follower navigation property to ensure it's loaded
            string[] includes = { nameof(Follow.Follower) };

            var followers = await _unitOfWork.Repository<Follow>().GetAllPredicated(predicate, includes);



            var followerDtos = _mapper.Map<IEnumerable<GetFollowerDto>>(followers);

            return new ResponseDto
            {
                IsSucceeded = true,
                Models = followerDtos
            };
        }

        public async Task<ResponseDto> GetFollowingAsync(int userId)
        {
            var user = await _unitOfWork.Repository<User>().GetByIdAsync(userId);
            if (user == null)
            {
                return new ResponseDto
                {
                    IsSucceeded = false,
                    Message = "User not found."
                };
            }

            Expression<Func<Follow, bool>> predicate = f => f.FollowerId == userId;

            string[] includes = { nameof(Follow.Following) };

            var following = await _unitOfWork.Repository<Follow>().GetAllPredicated(predicate, includes);

            var followingDtos = _mapper.Map<IEnumerable<GetFollowingDto>>(following);

            return new ResponseDto
            {
                IsSucceeded = true,
                Models = followingDtos
            };
        }


    }
}