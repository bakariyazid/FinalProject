using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QRCodeAttendance.Contract.Entities
{
    public class BaseEntity
    {
        public Guid Id {get; set;} = Guid.NewGuid();
        public DateTime CreatedDate {get; set;} = DateTime.UtcNow;  
        public DateTime UpdatedDate {get; set;}
    }
}