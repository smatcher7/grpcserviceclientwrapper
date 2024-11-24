using System.Globalization;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Grpc.Net.Client.Web;
using Grpc.Net.ClientFactory;
using Microsoft.Extensions.DependencyInjection;

namespace GrpcService.Client.Wrapper;

public static class ServicesExtensions
{
    public static IServiceCollection AddGrpcClient<TClient>(this IServiceCollection services, Action<WrapperOptions> optionsSetup) where TClient : ClientBase
    {
        WrapperOptions options = new WrapperOptions("https://localhost:443");

        optionsSetup?.Invoke(options);

        services.AddGrpcClient<TClient>(options);
        return services;
    }

    public static IServiceCollection AddGrpcClient<TClient, TInterceptor>(this IServiceCollection services, Action<WrapperOptions> optionsSetup)
        where TClient : ClientBase
        where TInterceptor : Interceptor
    {
        WrapperOptions options = new WrapperOptions("https://localhost:443");

        services.AddGrpcClient<TClient, TInterceptor>(options);
        return services;
    }

    private static void AddGrpcClient<TClient>(this IServiceCollection services, WrapperOptions options) where TClient : ClientBase
    {
        GenerateHttpClientBuilder<TClient>(services, options);
    }

    private static void AddGrpcClient<TClient, TInterceptor>(this IServiceCollection services, WrapperOptions options) where TClient : ClientBase
        where TInterceptor : Interceptor
    {
        GenerateHttpClientBuilder<TClient>(services, options).AddInterceptor<TInterceptor>(InterceptorScope.Client);
    }

    private static IHttpClientBuilder GenerateHttpClientBuilder<TClient>(IServiceCollection services, WrapperOptions options) where TClient : ClientBase
    {
        var httpClientBuilder = services.AddGrpcClient<TClient>(o => { o.Address = new Uri(options.ServiceEndpoint); })
            .ConfigurePrimaryHttpMessageHandler(() =>
            {
                HttpMessageHandler? handler = null;

                var httpHandler = new HttpClientHandler
                {
                    SslProtocols = options.SslProtocol
                };
                if (options.IgnoreCertificates)
                {
                    httpHandler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
                }

                if (options.UseGrpcWeb)
                {
                    handler = new GrpcWebHandler(GrpcWebMode.GrpcWeb, httpHandler);
                }
                else
                {
                    handler = httpHandler;
                }

                return handler;
            })
            .EnableCallContextPropagation(c => c.SuppressContextNotFoundErrors = true)
            .AddCallCredentials((context, metadata) =>
            {
                metadata.Add("accept-language", CultureInfo.CurrentCulture.Name);

                if (options.UseAuthentication && options.AuthToken != null)
                {
                    metadata.Add("Authorization", $"Bearer {options.AuthToken}");
                }

                return Task.CompletedTask;
            });

        return httpClientBuilder;
    }
}