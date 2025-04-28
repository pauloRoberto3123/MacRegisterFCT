using System;

namespace MacRegister.Model.Jems4Api
{
    public class CompleteWipRequest
    {
        public int WipId { get; set; }
        public DateTime EndDateTime { get; set; }
        public bool IsSingleWipMode { get; set; }
    }
}
