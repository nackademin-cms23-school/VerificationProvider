using Azure.Messaging.ServiceBus;
using Microsoft.AspNetCore.Http;
using VerificationProvider.Models;

namespace VerificationProvider.Services
{
    public interface IVerificationService
    {
        string GeneratedCode();
        EmailRequest GenerateEmailRequsetEmail(string email, string code);
        string GenerateServiceBusMessage(EmailRequest emailRequest);
        Task<bool> SaveVerificationRequest(string email, string code);
        Task<VerificationRequest> UnpackHttpVerificationRequest(HttpRequest req);
        VerificationRequest UnpackVerificationRequest(ServiceBusReceivedMessage message);
    }
}