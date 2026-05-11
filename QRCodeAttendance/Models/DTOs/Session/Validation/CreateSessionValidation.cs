using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;

namespace QRCodeAttendance.Models.DTOs.Session.Validation
{
        public class CreateSessionValidation : AbstractValidator<CreateSessionRequestModel>
            {
                public CreateSessionValidation()
                {
                    RuleFor(x => x.CourseCode)
                        .NotEmpty().WithMessage("CourseCode is required")
                        .MaximumLength(20).WithMessage("CourseCode is too long");

                    RuleFor(x => x.CourseName)
                        .NotEmpty().WithMessage("CourseName is required")
                        .Length(3, 50).WithMessage("CourseName must be 3-50 characters");

                    RuleFor(x => x.Level)
                        .NotEmpty().WithMessage("Level is required");
                        
                    RuleFor(x => x.SessionStartTime)
                        .NotEmpty().WithMessage("SessionStartTime is required");

                    RuleFor(x => x.SessionEndTime)
                        .NotEmpty().WithMessage("SessionEndTime is required")
                        .GreaterThan(x => x.SessionStartTime).WithMessage("SessionEndTime must be after SessionStartTime");
                }
            }
}