using Azure.Messaging.ServiceBus;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VerificationProvider.Data.Contexts;
using VerificationProvider.Functions;
using VerificationProvider.Models;

namespace VerificationProvider.Services;

public class VerificationService(ILogger<VerificationService> logger, IServiceProvider serviceProvider) : IVerificationService
{
    private readonly ILogger<VerificationService> _logger = logger;
    private readonly IServiceProvider _serviceProvider = serviceProvider;


    public VerificationRequest UnpackVerificationRequest(ServiceBusReceivedMessage message)
    {
        try
        {
            var request = JsonConvert.DeserializeObject<VerificationRequest>(message.Body.ToString());
            if (request != null && !string.IsNullOrEmpty(request.Email))
            {
                return request;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error :: VerificationService.UnpackVerificationRequest :: {ex.Message} ");
        }
        return null!;
    }

    public async Task<VerificationRequest> UnpackHttpVerificationRequest(HttpRequest req)
    {
        try
        {
            var body = await new StreamReader(req.Body).ReadToEndAsync();
            if (!string.IsNullOrEmpty(body))
            {
                var verificationRequest = JsonConvert.DeserializeObject<VerificationRequest>(body);
                if (verificationRequest != null)
                {
                    return verificationRequest;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error :: VerificationService.UnpackHttpVerificationRequest :: {ex.Message} ");
        }
        return null!;
    }

    public string GeneratedCode()
    {
        try
        {
            var rnd = new Random();
            var code = rnd.Next(100000, 999999);

            return code.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error :: VerificationService.GeneratedCode :: {ex.Message} ");
        }
        return null!;
    }

    public async Task<bool> SaveVerificationRequest(string email, string code)
    {
        try
        {
            using var context = _serviceProvider.GetRequiredService<DataContext>();
            var existingRequest = await context.VerificationRequests.FirstOrDefaultAsync(x => x.Email == email);

            if (existingRequest != null)
            {
                existingRequest.Code = code;
                existingRequest.ExpirationDate = DateTime.Now.AddMinutes(5);
                context.Entry(existingRequest).State = EntityState.Modified;
            }
            else
            {
                context.VerificationRequests.Add(new() { Email = email, Code = code });
            }

            await context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error :: VerificationService.SaveVerificationRequest :: {ex.Message} ");
        }
        return false;
    }

    public EmailRequest GenerateEmailRequsetEmail(string email, string code)
    {
        try
        {
            if (!string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(code))
            {
                var emailRequest = new EmailRequest
                {
                    To = email,
                    Subject = $"Verification code {code}",
                    Body = $@"
                     <html lang='en'>
                        <head>
                            <meta charset='UTF-8'>
                            <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                            <title>Verification Code</title>
                        </head>
                        <body>
                            <div style='color: #191919; max-width: 500px'>
                                <div style='background-color: #4F85F6; color: white; text-align: center; padding: 20px 0;'>
                                    <h1 style='font-weight: 400;'>Verification Code</h1>
                                </div>
                                <div style='background-color: #f4f4f4; padding: 1rem 2rem;'>
                                    <p>Dear user,</p>
                                    <p>We received a request to sign in to your account using e-mail {email}. Please verify your account using this verification code:</p>
                                    <p class='code' style='font-weight: 700; text-align:center; font-size: 48px; letter-spacing: 8px;'>
                                        {code}
                                    </p>
                                    <div class='noreply' style='color: #191919; font-size: 11px;'>
                                        <p>If you did not request this code, it is possible that someone else is trying to access the Silicon Account <span style='color: #0041cd;'>{email}</span>. This email can't receive replies. For more information, visit the Silicons Help Center.</p>
                                    </div>
                                </div>
                                <div style='color: #191919; text-align:center; font-size: 11px;'>
                                    <p>© Silicon, Sveavägen 1, SE-123 45 Stockholm, Sweden</p>
                                </div> 
                            </div>
                        </body>
                    </html>   
                    ",
                    PlainText = $"Please verify your account using this verification code: {code}. If you did not request this code, it is possible that someone else is trying to access the Silicon Account {email}. This email can't receive replies. For more information, visit the Silicons Help Center. © Silicon, Sveavägen 1, SE-123 45 Stockholm, Sweden"
                };
                return emailRequest;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error :: VerificationService.SaveVerificationRequest :: {ex.Message} ");
        }
        return null!;
    }


    public string GenerateServiceBusMessage(EmailRequest emailRequest)
    {
        try
        {
            var payload = JsonConvert.SerializeObject(emailRequest);
            if (!string.IsNullOrEmpty(payload))
            {
                return payload;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error :: VerificationService.GenerateServiceBusMessage :: {ex.Message} ");
        }
        return null!;
    }
}
