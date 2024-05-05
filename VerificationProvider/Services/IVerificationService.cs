using Azure.Messaging.ServiceBus;
using VerificationProvider.Models;

namespace VerificationProvider.Services
{
    public interface IVerificationService
    {
        string GeneratedCode();
        EmailRequest GenerateEmailRequsetEmail(string email, string code);
        string GenerateServiceBusMessage(EmailRequest emailRequest);
        Task<bool> SaveVerificationRequest(string email, string code);
        VerificationRequest UnpackVerificationRequest(ServiceBusReceivedMessage message);
    }
}