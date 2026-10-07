using QRCodeAttendance.Interface.Repositories;
using QRCodeAttendance.Interface.Services;
using QRCodeAttendance.Models.DTOs;
using QRCodeAttendance.Models.DTOs.Admin;
using QRCodeAttendance.Models.Entities;

namespace QRCodeAttendance.Implementation.Services
{
    public class AdminProfileService : IAdminProfileService
    {
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;

        public AdminProfileService(IUserRepository userRepository, IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<BaseResponse<AdminProfileDto>> GetProfile(Guid userId)
        {
            var admin = await _userRepository.Get<Admin>(a => a.UserId == userId);
            if (admin == null) return new BaseResponse<AdminProfileDto> { Status = false, Message = "Admin profile not found." };

            return new BaseResponse<AdminProfileDto>
            {
                Status = true,
                Data = new AdminProfileDto
                {
                    FirstName = admin.FirstName, LastName = admin.LastName, Email = admin.Email,
                    PhoneNumber = admin.PhoneNumber, Address = admin.Address, Gender = admin.Gender, DateOfBirth = admin.DateOfBirth
                }
            };
        }

        public async Task<BaseResponse> UpdateProfile(Guid userId, UpdateAdminProfileRequestModel request)
        {
            var admin = await _userRepository.Get<Admin>(a => a.UserId == userId);
            if (admin == null) return new BaseResponse { Status = false, Message = "Admin profile not found." };

            var email = request.Email.Trim().ToLowerInvariant();
            if (await _userRepository.Any(u => u.Id != userId && u.Email.ToLower() == email))
                return new BaseResponse { Status = false, Message = "That email address is already in use." };

            admin.FirstName = request.FirstName.Trim();
            admin.LastName = request.LastName.Trim();
            admin.Email = email;
            admin.PhoneNumber = request.PhoneNumber?.Trim() ?? string.Empty;
            admin.Address = request.Address?.Trim() ?? string.Empty;
            admin.Gender = request.Gender;
            admin.DateOfBirth = request.DateOfBirth;
            admin.UpdatedDate = DateTime.UtcNow;
            _userRepository.Update(admin);

            var user = await _userRepository.Get<User>(u => u.Id == userId);
            if (user != null)
            {
                user.Email = email;
                user.UserName = email;
                user.UpdatedDate = DateTime.UtcNow;
                _userRepository.Update(user);
            }

            await _unitOfWork.SaveChangesAsync();
            return new BaseResponse { Status = true, Message = "Admin profile updated successfully." };
        }
    }
}
