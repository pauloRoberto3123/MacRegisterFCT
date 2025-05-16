namespace MacRegister.Dtos
{
    public class MesApiRequest
    {
        public string Serial { get; set; }
        public string Customer { get; set; }
        public string Division { get; set; }
        public string Equipment { get; set; }
        public string Step { get; set; }
        //public string AssemblyNumber { get; set; }
        public string TestResult { get; set; }
        public string FailureLabel { get; set; }
    }
}
