using System;

namespace MacRegister.Model.MacApi
{
    public class Mac_Record_Logs
    {
        public Guid Id { get; set; }
        public string Serial { get; set; }
        public string Mac { get; set; }
        public DateTime Date_Creation { get; set; }

        public Mac_Record_Logs()
        {
            Serial = string.Empty;
            Mac = string.Empty;
        }
    }
}
