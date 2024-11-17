using Grpc.Core;
using Grpc.Core.Interceptors;
using Grpc.Net.ClientFactory;
using Microsoft.Extensions.DependencyInjection;

namespace GrpcService.Client.Wrapper;

public static class ServicesExtensions
{
    public static IServiceCollection AddGrpcClient<TClient>(this IServiceCollection services, string endpoint) where TClient : ClientBase
    {
        services.AddGrpcClient<TClient>(endpoint, false, null);
        return services;
    }

    public static IServiceCollection AddGrpcClient<TClient, TInterceptor>(this IServiceCollection services, string endpoint)
        where TClient : ClientBase
        where TInterceptor : Interceptor
    {
        services.AddGrpcClient<TClient, TInterceptor>(endpoint, false, null);
        return services;
    }

    public static IServiceCollection AddAuthorizedGrpcClient<TClient>(this IServiceCollection services, string endpoint, string authToken)
        where TClient : ClientBase
    {
        services.AddGrpcClient<TClient>(endpoint, true, authToken);
        return services;
    }

    public static IServiceCollection AddAuthorizedGrpcClient<TClient, TInterceptor>(this IServiceCollection services, string endpoint, string authToken)
        where TClient : ClientBase
        where TInterceptor : Interceptor
    {
        services.AddGrpcClient<TClient, TInterceptor>(endpoint, true, authToken);
        return services;
    }

    private static void AddGrpcClient<TClient>(this IServiceCollection services,
        string endpoint,
        bool useAuthentication,
        string? authToken = null)
        where TClient : ClientBase
    {
        var httpClientBuilder = services.AddGrpcClient<TClient>(o => { o.Address = new Uri(endpoint); })
            .ConfigurePrimaryHttpMessageHandler(() =>
            {
                var handler = new HttpClientHandler();
                return handler;
            });
        if (useAuthentication && authToken != null)
        {
            httpClientBuilder = httpClientBuilder.AddCallCredentials((context, metadata) =>
            {
                metadata.Add("Authorization", $"Bearer {authToken}");
                return Task.CompletedTask;
            });
        }

        httpClientBuilder.EnableCallContextPropagation();
    }

    private static void AddGrpcClient<TClient, TInterceptor>(this IServiceCollection services,
        string endpoint,
        bool useAuthentication,
        string? authToken = null)
        where TClient : ClientBase
        where TInterceptor : Interceptor
    {
        var httpClientBuilder = services.AddGrpcClient<TClient>(o => { o.Address = new Uri(endpoint); })
            .ConfigurePrimaryHttpMessageHandler(() =>
            {
                var handler = new HttpClientHandler();
                return handler;
            }).AddInterceptor<TInterceptor>(InterceptorScope.Client);

        if (useAuthentication && authToken != null)
        {
            httpClientBuilder = httpClientBuilder.AddCallCredentials((context, metadata) =>
            {
                metadata.Add("Authorization", $"Bearer {authToken}");
                return Task.CompletedTask;
            });
        }
        httpClientBuilder.EnableCallContextPropagation();
    }
}