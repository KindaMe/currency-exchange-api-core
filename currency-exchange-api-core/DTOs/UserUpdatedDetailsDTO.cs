namespace currency_exchange_api_core.DTOs
{
    public class UserUpdatedDetailsDTO
    {
        public string? NewEmail { get; set; }

        public string? NewPassword { get; set; }

        public string ConfirmPassword { get; set; } = null!;
    }
}