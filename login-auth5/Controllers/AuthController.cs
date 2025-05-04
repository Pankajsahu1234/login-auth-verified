using login_auth5.Data;
using login_auth5.Dtos;
using login_auth5.Models;
using login_auth5.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace login_auth5.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context; private readonly JwtTokenService _jwtService; private readonly EmailService _emailService; private readonly SmsService _smsService;


        public AuthController(
    ApplicationDbContext context,
    JwtTokenService jwtService,
    EmailService emailService,
    SmsService smsService)
        {
            _context = context;
            _jwtService = jwtService;
            _emailService = emailService;
            _smsService = smsService;
        }

        [HttpPost("request-otp")]
        public async Task<IActionResult> RequestOtp([FromBody] LoginDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Email) && string.IsNullOrWhiteSpace(dto.Phone))
                return BadRequest("Email or phone is required.");

            var user = _context.Customers.FirstOrDefault(c =>
                (!string.IsNullOrEmpty(dto.Email) && c.Email == dto.Email) ||
                (!string.IsNullOrEmpty(dto.Phone) && c.Phone == dto.Phone));

            if (user == null)
                return NotFound("User not found.");

            var otp = new Random().Next(100000, 999999).ToString();
            user.Otp = otp;
            user.OtpGeneratedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            try
            {
                if (!string.IsNullOrEmpty(dto.Email))
                {
                    await _emailService.SendOtpEmailAsync(dto.Email, otp);
                }
                else if (!string.IsNullOrEmpty(dto.Phone))
                {
                    await _smsService.SendOtpSms(dto.Phone, otp);
                }

                return Ok("OTP sent successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending OTP: {ex.Message}");
                return StatusCode(500, "Failed to send OTP.");
            }
        }

        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Otp))
                return BadRequest("OTP is required.");

            var user = _context.Customers.FirstOrDefault(c =>
                (!string.IsNullOrEmpty(dto.Email) && c.Email == dto.Email) ||
                (!string.IsNullOrEmpty(dto.Phone) && c.Phone == dto.Phone));

            if (user == null || user.Otp != dto.Otp || user.OtpGeneratedAt == null ||
                (DateTime.UtcNow - user.OtpGeneratedAt.Value).TotalMinutes > 5)
            {
                return Unauthorized("Invalid or expired OTP.");
            }

            var identifier = user.Email ?? user.Phone;
            var accessToken = _jwtService.GenerateAccessToken(identifier!);
            var (refreshToken, expiry) = _jwtService.GenerateRefreshToken();

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = expiry;
            user.Otp = null;
            user.OtpGeneratedAt = null;
            await _context.SaveChangesAsync();

            return Ok(new TokenResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            });
        }

        [HttpPost("refresh-token")]
        public IActionResult RefreshToken([FromBody] RefreshTokenRequestDto dto)
        {
            var user = _context.Customers.FirstOrDefault(c => c.RefreshToken == dto.RefreshToken);

            if (user == null || user.RefreshTokenExpiryTime < DateTime.UtcNow)
                return Unauthorized("Invalid or expired refresh token.");

            var identifier = user.Email ?? user.Phone;
            var accessToken = _jwtService.GenerateAccessToken(identifier!);
            var (newRefreshToken, expiry) = _jwtService.GenerateRefreshToken();

            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiryTime = expiry;
            _context.SaveChanges();

            return Ok(new TokenResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = newRefreshToken
            });
        }

        [HttpGet("secure")]
        [Authorize]
        public IActionResult Secure()
        {
            return Ok("Accessed a secure endpoint.");
        }
    }
}