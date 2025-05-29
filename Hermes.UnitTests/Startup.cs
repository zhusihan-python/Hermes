using Hermes.Common;
using Hermes.Models;
using Hermes.Repositories;
using Hermes.Services;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace HermesTests;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddTransient<AesEncryptor>();
        services.AddTransient<FileService>();
        services.AddTransient<Settings>();
    }
}