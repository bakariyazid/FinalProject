using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QRCodeAttendance.Interface.Services
{
    public interface ICurrentUserService
    {

        public Guid UserId { get; }
        public string Email { get; }
        public string Role { get; }

    }
}