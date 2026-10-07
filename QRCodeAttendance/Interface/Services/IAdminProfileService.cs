using QRCodeAttendance.Models.DTOs;
using QRCodeAttendance.Models.DTOs.Admin;

namespace QRCodeAttendance.Interface.Services
{
    public interface IAdminProfileService
    {
        Task<BaseResponse<AdminProfileDto>> GetProfile(Guid userId);
        Task<BaseResponse> UpdateProfile(Guid userId, UpdateAdminProfileRequestModel request);
    }
}
