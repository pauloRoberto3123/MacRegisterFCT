namespace MacRegister.Service
{
    public class FctLog
    {
        public string Serial { get; set; }
        public string Mac { get; set; }

        public string PgmVersion { get; set; }

        public string Jig { get; set; }

        public string DateStart { get; set; }
        public string DateEnd { get; set; }



        public string TestResult { get; set; }
        public string FailureMessage { get; set; }
    }
}
