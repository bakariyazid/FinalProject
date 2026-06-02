using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QRCodeAttendance.Contract.Entities;
using QRCodeAttendance.Models.Enums;

namespace QRCodeAttendance.Models.Entities
{
    public class Admin : BaseUser
    {

        public Guid UserId { get; set; }

        public User User { get; set; } = null!;

       

    }
}