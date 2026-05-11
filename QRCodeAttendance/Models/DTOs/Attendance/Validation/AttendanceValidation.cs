using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;

namespace QRCodeAttendance.Models.DTOs.Attendance.Validation
{
    public class CreateAttendanceValidation : AbstractValidator<CreateAttendanceRequestModel>
        {
        public CreateAttendanceValidation()
        {
            RuleFor(x => x.QrCodeData).NotEmpty().WithMessage("QR code data is required");
            RuleFor(x => x.QrCodeData)
                .Matches(@"^[A-Za-z0-9\-]+$") 
                .WithMessage("Invalid QR code format");
        }
    }
}