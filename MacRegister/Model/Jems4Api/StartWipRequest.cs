using System;

namespace MacRegister.Model.Jems4Api
{
    public class StartWipRequest
    {
        public int WipId { get; set; }
        public string SerialNumber { get; set; }
        public string ResourceName { get; set; }
        public DateTime StartDateTime { get; set; }
        public bool IsSingleWipMode { get; set; }
        public bool ShouldSkipValidation { get; set; }
    }
}
