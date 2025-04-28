using System.Collections.Generic;

namespace MacRegister.Model.Jems4Api
{
    public class OkToStartResponse
    {
        public bool OkToStart { get; set; }
        public int RouteStepId { get; set; }
        public string RouteStepName { get; set; }
        public List<int> WipId { get; set; }

    }
}
