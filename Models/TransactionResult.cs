namespace TechMobileBE.Models
{
    public class TransactionResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public Transaction Transaction { get; set; }
    }
}
