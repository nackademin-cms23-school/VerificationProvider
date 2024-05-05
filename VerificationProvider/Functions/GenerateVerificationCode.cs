using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using VerificationProvider.Services;

namespace VerificationProvider.Functions;

public class GenerateVerificationCode(ILogger<GenerateVerificationCode> logger, IVerificationService verificationService)
{
    private readonly ILogger<GenerateVerificationCode> _logger = logger;
    private readonly IVerificationService _verificationService = verificationService;


    [Function(nameof(GenerateVerificationCode))]
    [ServiceBusOutput("email_request", Connection = "ServiceBus")]
    public async Task<string> Run([ServiceBusTrigger("verification_request", Connection = "ServiceBus")] ServiceBusReceivedMessage message, ServiceBusMessageActions messageActions)
    {
        try
        {
            var vr = _verificationService.UnpackVerificationRequest(message);
            if (vr != null)
            {
                var code = _verificationService.GeneratedCode();

                if (!string.IsNullOrEmpty(code))
                {
                    var result = await _verificationService.SaveVerificationRequest(vr.Email, code);
                    if (result)
                    {
                        var emailRequest = _verificationService.GenerateEmailRequsetEmail(vr.Email, code);

                        if (emailRequest != null)
                        {
                            var payload = _verificationService.GenerateServiceBusMessage(emailRequest);

                            if (!string.IsNullOrEmpty(payload))
                            {
                                await messageActions.CompleteMessageAsync(message);
                                return payload;
                            }
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
}
