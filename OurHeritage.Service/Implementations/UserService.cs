using AutoMapper;
using OurHeritage.Core.Entities;
using OurHeritage.Repo.Repositories.Interfaces;
using OurHeritage.Service.DTOs;
using OurHeritage.Service.DTOs.UserDto;
using OurHeritage.Service.Interfaces;

namespace OurHeritage.Service.Implementations
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public UserService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<ResponseDto> CreateUserAsync(CreateOrUpdateUserDto dto)
        {
            if (dto == null)
            {
                return new ResponseDto
                {
                    IsSucceeded = false,
                    Status = 400,
                    Message = "Model doesn't exist"
                };
            }
            var user = _mapper.Map<User>(dto);
            await _unitOfWork.Repository<User>().AddAsync(user);
            await _unitOfWork.Complete();
            return new ResponseDto
            {
                IsSucceeded = true,
                Status = 201,
                Model = dto,
                Message = "User created successfully"
            };
        }

        public async Task<ResponseDto> GetUserByIdAsync(int userId)
        {
            var user = await _unitOfWork.Repository<User>().GetByIdAsync(userId);
            if (user == null)
            {
                return new ResponseDto
                {
                    IsSucceeded = false,
                    Status = 404,
                    Message = "User not found"
                };
            }
            var userDto = _mapper.Map<GetUserDto>(user);
            return new ResponseDto
            {
                IsSucceeded = true,
                Status = 200,
                Model = userDto
            };
        }

        public async Task<ResponseDto> GetAllUsersAsync()
        {
            var users = await _unitOfWork.Repository<User>().GetAllAsync();

            var mappedUsers = _mapper.Map<IEnumerable<GetUserDto>>(users);

            return new ResponseDto
            {
                IsSucceeded = true,
                Status = 200,
                Models = mappedUsers
            };
        }


        public async Task<ResponseDto> UpdateUserAsync(int userId, CreateOrUpdateUserDto dto)
        {
            var user = await _unitOfWork.Repository<User>().GetByIdAsync(userId);
            if (user == null)
            {
                return new ResponseDto
                {
                    IsSucceeded = false,
                    Status = 404,
                    Message = "User not found"
                };
            }
            _mapper.Map(dto, user);
            _unitOfWork.Repository<User>().Update(user);
            await _unitOfWork.Complete();
            return new ResponseDto
            {
                IsSucceeded = true,
                Status = 200,
                Message = "User updated successfully"
            };
        }

        public async Task<ResponseDto> DeleteUserAsync(int userId)
        {
            var user = await _unitOfWork.Repository<User>().GetByIdAsync(userId);
            if (user == null)
            {
                return new ResponseDto
                {
                    IsSucceeded = false,
                    Status = 404,
                    Message = "User not found"
                };
            }
            _unitOfWork.Repository<User>().Delete(user);
            await _unitOfWork.Complete();
            return new ResponseDto
            {
                IsSucceeded = true,
                Status = 200,
                Message = "User deleted successfully"
            };
        }

    }
}
