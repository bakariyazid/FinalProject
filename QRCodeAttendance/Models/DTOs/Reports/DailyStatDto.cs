using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QRCodeAttendance.Models.DTOs.Reports
{
    public class DailyStatDto
        {
            public DateTime Date { get; set; }
            public int Count { get; set; }
        }
}