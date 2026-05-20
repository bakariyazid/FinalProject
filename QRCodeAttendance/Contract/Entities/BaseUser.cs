using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using QRCodeAttendance.Models.Enums;

namespace QRCodeAttendance.Contract.Entities
{
    public class BaseUser : BaseEntity
    {
        public string FirstName {get; set;} = string.Empty;
        public string LastName {get; set;} = string.Empty;
        public string PhoneNumber {get; set;} = string.Empty;
        public string Address {get; set;} = string.Empty;
        public string Email {get; set;} = string.Empty;
        public Gender Gender {get; set;}
        public DateTime DateOfBirth {get; set;}
        public string FullName ()
        {
            return $"{FirstName} {LastName}";
        }
    }
}
