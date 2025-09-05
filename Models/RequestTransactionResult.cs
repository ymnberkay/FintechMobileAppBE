namespace TechMobileBE.Models
{
    public class RequestTransactionResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public MoneyRequest? Transaction { get; set; }
    }
}