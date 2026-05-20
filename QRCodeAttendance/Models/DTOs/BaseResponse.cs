using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QRCodeAttendance.Models.DTOs
{
    public class BaseResponse
    {

        public string Message { get; set; } = string.Empty;

        public bool Status { get; set; }

    }
    public class BaseResponse<T>
    {
        public string Message { get; set; } = string.Empty;

        public bool Status { get; set; }

        public T Data { get; set; } = default!;

    }
}

  
