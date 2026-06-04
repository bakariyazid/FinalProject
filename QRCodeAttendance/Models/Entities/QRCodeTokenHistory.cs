using QRCodeAttendance.Contract.Entities;

namespace QRCodeAttendance.Models.Entities
{
    public class QRCodeTokenHistory : BaseEntity
    {
        public Guid SessionId { get; set; }
        public required string Token { get; set; }
        public DateTime ValidFrom { get; set; }
        public DateTime ValidUntil { get; set; }
        public Session? Session { get; set; }
    }
}
