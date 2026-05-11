using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;

namespace QRCodeAttendance.Models.DTOs.User.Validation
{
    public class LoginValidation : AbstractValidator<LoginRequestModel>
    {
           public LoginValidation()
        {
            RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.");
            RuleFor(x => x.Password).NotEmpty().WithMessage("Password is required.");
        }
    
    }
}