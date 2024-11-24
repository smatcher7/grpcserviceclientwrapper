using System.Security.Authentication;

namespace GrpcService.Client.Wrapper;

public class WrapperOptions
{
    public WrapperOptions(string endpoint)
    {
        ServiceEndpoint = endpoint;
    }

    public string ServiceEndpoint { get; set; }

    public SslProtocols SslProtocol { get; set; } = SslProtocols.Tls12;

    public bool IgnoreCertificates { get; set; }

    public bool UseGrpcWeb { get; set; }

    public bool UseAuthentication { get; set; }

    public string? AuthToken { get; set; }
}