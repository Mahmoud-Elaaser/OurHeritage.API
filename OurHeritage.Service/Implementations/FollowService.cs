using AutoMapper;
using OurHeritage.Core.Entities;
using OurHeritage.Repo.Repositories.Interfaces;
using OurHeritage.Service.DTOs;
using OurHeritage.Service.Interfaces;

namespace OurHeritage.Service.Implementations
{
    public class FollowService : IFollowService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public FollowService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
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

            /// Check if the user is already following this user
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
            await _unitOfWork.Complete();



            return new ResponseDto
            {
                IsSucceeded = true,
                Message = "Followed successfully."
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
            await _unitOfWork.Complete();

            return new ResponseDto
            {
                IsSucceeded = true,
                Message = "Unfollowed successfully."
            };
        }


        public async Task<ResponseDto> GetFollowersAsync(int userId)
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

            var followers = await _unitOfWork.Repository<Follow>()
                .GetAllPredicated(f => f.FollowingId == userId, new[] { "Follower" });

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

            var following = await _unitOfWork.Repository<Follow>()
                .GetAllPredicated(f => f.FollowerId == userId, new[] { "Following" });

            var followingDtos = following.Select(f => new GetFollowerDto
            {
                Id = f.Following.Id,
                Username = f.Following.UserName
            });

            return new ResponseDto
            {
                IsSucceeded = true,
                Models = followingDtos
            };
        }
    }
}
