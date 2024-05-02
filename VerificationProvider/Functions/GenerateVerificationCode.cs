using System;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VerificationProvider.Data.Contexts;
using VerificationProvider.Models;

namespace VerificationProvider.Functions;

public class GenerateVerificationCode(ILogger<GenerateVerificationCode> logger, IServiceProvider serviceProvider)
{
    private readonly ILogger<GenerateVerificationCode> _logger = logger;
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    [Function(nameof(GenerateVerificationCode))]
    [ServiceBusOutput("email_request", Connection = "ServiceBus")]
    public async Task<string> Run([ServiceBusTrigger("verification_request", Connection = "ServiceBus")] ServiceBusReceivedMessage message, ServiceBusMessageActions messageActions)
    {
        try
        {
            var vr = UnpackVerificationRequest(message);
            if (vr != null)
            {
                var code = GeneratedCode();

                if(await SaveVerificationRequest(vr.Email, code))
                {
                    var emailRequest = GenerateEmailRequsetEmail(vr.Email, code);

                    if (emailRequest != null)
                    {
                        var payload = GenerateServiceBusMessage(emailRequest);

                        if(!string.IsNullOrEmpty(payload))
                        {
                            await messageActions.CompleteMessageAsync(message);
                            return payload;
                        }
                    }
                }

            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error :: Run :: {ex.Message} ");
        }
        return null!;
    }

    public VerificationRequest UnpackVerificationRequest(ServiceBusReceivedMessage message)
    {
        try
        {
            var request = JsonConvert.DeserializeObject<VerificationRequest>(message.Body.ToString());
            if (request != null)
            {
                return request;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error :: UnpackVerificationRequest :: {ex.Message} ");
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
            _logger.LogError($"Error :: GeneratedCode :: {ex.Message} ");
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
            _logger.LogError($"Error :: SaveVerificationRequest :: {ex.Message} ");
        }
        return false;
    }

    public EmailRequest GenerateEmailRequsetEmail(string email, string code)
    {
        try
        {
            if(!string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(code))
            {
                var request = new EmailRequest
                {
                    To = email,
                    Subject = $"Verification code {code}",
                    Body = $@"
                        <html>
                            <head>
                                <meta charset='utf-8' />
                                <meta name='viewport' content='width=device-width, initial-scale=1.0' />
                                <title>Verification Code</title>
                            </head>
                            <body>
                                <div style='color: #191919;'>
                                    <p>{code}</p>
                                <div>
                            </body>
                        </html>
                    ",
                    PlainText = $"Please verify your account using this code: {code}. If you "
                };
                return request;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error :: SaveVerificationRequest :: {ex.Message} ");
        }
        return null!;
    }


    public string GenerateServiceBusMessage(EmailRequest emailRequest)
    {
        try
        {
            var payload = JsonConvert.SerializeObject(emailRequest);
            if(!string.IsNullOrEmpty (payload))
            {
                return payload;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error :: GenerateServiceBusMessage :: {ex.Message} ");
        }
        return null!;
    }
}
