using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using QRCodeAttendance.Models.Enums;

namespace QRCodeAttendance.Models.DTOs.Student.Validation;
 public class CreateStudentValidation : AbstractValidator<CreateStudentRequestModel>
    {     
        public CreateStudentValidation()
        {
                RuleFor(x => x.MatricNumber).NotEmpty().WithMessage("MatricNumber is required");
                RuleFor(x => x.FirstName).Length(3, 50).NotEmpty().WithMessage("Firstname is required");
                RuleFor(x => x.LastName).Length(3, 50).NotEmpty().WithMessage("Lastname is required");
                RuleFor(x => x.Department).NotEmpty().WithMessage("Department is required");
                RuleFor(x => x.StudentLevel).NotEmpty().WithMessage("StudentLevel is required");
                RuleFor(x => x.Email).NotEmpty().WithMessage("Email is required");
                RuleFor(x => x.EmailConfirmed).NotEmpty().WithMessage("EmailConfirmed is required");
                RuleFor(x => x.PasswordHash).NotEmpty().WithMessage("Password is required");
                RuleFor(x => x.ConfirmPassword).Matches(x => x.PasswordHash).NotEmpty().WithMessage("Confirm password is required");
                RuleFor(x => x.Address).NotEmpty().WithMessage("Address is required");
                RuleFor(x => x.DateOfBirth).NotEmpty().WithMessage("Date of birth is required");
                RuleFor(x => x.Gender).NotEmpty().WithMessage("Gender is required");
                RuleFor(x => x.PhoneNumber).NotEmpty().WithMessage("Phone number is required");
        }
    }