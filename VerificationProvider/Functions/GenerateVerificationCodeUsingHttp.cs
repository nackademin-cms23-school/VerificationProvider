using Azure.Messaging.ServiceBus;
using Google.Protobuf.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net;
using VerificationProvider.Models;
using VerificationProvider.Services;

namespace VerificationProvider.Functions;

public class GenerateVerificationCodeUsingHttp(ILogger<GenerateVerificationCodeUsingHttp> logger, IVerificationService verificationService, ServiceBusClient serviceBusClient)
{
    private readonly ILogger<GenerateVerificationCodeUsingHttp> _logger = logger;
    private readonly IVerificationService _verificationService = verificationService;
    private readonly ServiceBusClient _serviceBusClient = serviceBusClient;

    [Function("GenerateVerificationCodeUsingHttp")]
    [ServiceBusOutput("verification_request", Connection = "ServiceBus")]
    public async Task Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
    {
        try
        {
            var vr = await _verificationService.UnpackHttpVerificationRequest(req);
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
                                var sender = _serviceBusClient.CreateSender("email_request");
                                await sender.SendMessageAsync(new ServiceBusMessage(payload)
                                {
                                    ContentType = "application/json"
                                });
                            }
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error :: GenerateVerificationCode.Run :: {ex.Message} ");
        }
    }
}
