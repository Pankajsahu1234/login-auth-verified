namespace login_auth5.Models
{
    public class Customer
    {
        public int Id { get; set; }

        public string? Email { get; set; }

        public string? Phone { get; set; }

        public string? Otp { get; set; }

        public DateTime? OtpGeneratedAt { get; set; }

        public string? RefreshToken { get; set; }

        public DateTime? RefreshTokenExpiryTime { get; set; }
    }
}