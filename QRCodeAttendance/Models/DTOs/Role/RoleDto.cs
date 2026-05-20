using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QRCodeAttendance.Models.DTOs.Role
{
    public class RoleDto
    {
            public Guid RoleId {get; set;}
            public string Name {get; set;} = string.Empty;
            public DateTime CreatedDate {get; set;}
            public DateTime UpdatedDate {get; set;}
           
    }
}
