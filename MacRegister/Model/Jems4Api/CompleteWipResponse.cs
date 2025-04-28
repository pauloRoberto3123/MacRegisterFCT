using System.Collections.Generic;

namespace MacRegister.Model.Jems4Api
{
    public class CompleteWipResponse
    {
        public List<WipInQueueRouteStep> WipInQueueRouteSteps { get; set; }
        public List<string> ResponseMessages { get; set; }
        public Document Document { get; set; }
    }

    public class WipInQueueRouteStep
    {
        public List<object> InQueueRouteStep { get; set; } // Adjust the type based on actual data if known
        public string SerialNumber { get; set; }
    }

    public class Document
    {
        public List<object> Model { get; set; } // Adjust the type based on actual data if known
        public string ErrorMessage { get; set; }
    }
}
