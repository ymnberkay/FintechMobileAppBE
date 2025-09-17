namespace TechMobileBE.DTOs
{
    public class HandleMoneyRequestDto
    {
        public string RequestId { get; set; } = null!;
        public bool Approve { get; set; }
    }
}