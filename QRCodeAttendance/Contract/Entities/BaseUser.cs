using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using QRCodeAttendance.Models.Enums;

namespace QRCodeAttendance.Contract.Entities
{
    public class BaseUser : BaseEntity
    {
        public string FirstName {get; set;}
        public string LastName {get; set;}
        public string PhoneNumber {get; set;}
        public string Address {get; set;}
        public string Email {get; set;}
        public Gender Gender {get; set;}
        public DateTime DateOfBirth {get; set;}
        public string FullName ()
        {
            return $"{FirstName} {LastName}";
        }
    }
}