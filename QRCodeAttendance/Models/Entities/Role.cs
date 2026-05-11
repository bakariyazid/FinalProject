using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using QRCodeAttendance.Contract.Entities;

namespace QRCodeAttendance.Models.Entities
{
    public class Role : BaseEntity
        {
            public required string Name {get; set;}
            public ICollection<User> Users { get; set; } = new List<User>();
        }
}