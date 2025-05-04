using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace login_auth5.Services
{
    public class SmsService
    {
        private readonly IConfiguration _config;

        public SmsService(IConfiguration config)
        {
            _config = config;

            // Initialize Twilio client
            var accountSid = _config["Twilio:AccountSid"];
            var authToken = _config["Twilio:AuthToken"];

            if (string.IsNullOrWhiteSpace(accountSid) || string.IsNullOrWhiteSpace(authToken))
            {
                throw new InvalidOperationException("Twilio AccountSid or AuthToken is not configured.");
            }

            TwilioClient.Init(accountSid, authToken);
        }

        public async Task SendOtpSms(string phoneNumber, string otp)
        {
            var fromPhone = _config["Twilio:FromNumber"];

            if (string.IsNullOrWhiteSpace(fromPhone))
            {
                throw new InvalidOperationException("Twilio 'FromNumber' is not configured.");
            }

            try
            {
                var message = await MessageResource.CreateAsync(
                    body: $"Your OTP code is: {otp}",
                    from: new PhoneNumber(fromPhone),
                    to: new PhoneNumber(phoneNumber)  // should be in +91xxxxxxxxxx format
                );

                Console.WriteLine($"SMS sent to {phoneNumber}. SID: {message.Sid}, Status: {message.Status}");

                if (message.ErrorCode != null)
                {
                    Console.WriteLine($"Twilio error: {message.ErrorCode} - {message.ErrorMessage}");
                    throw new Exception($"Failed to send SMS: {message.ErrorMessage}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in SendOtpSms: {ex.Message}");
                throw;
            }
        }
    }
}
