using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker;

namespace VerificationProvider.Models;

public class OutputTypeRequest
{
    [ServiceBusOutput("email_request", Connection = "ServiceBus")]
    public string OutputEvent { get; set; } = null!;

    public HttpResponseData HttpResponse { get; set; } = null!;
}
