using Common.Hosting.Configuration;
using Common.Hosting.Service;
using Common.Hosting.ServiceProvider;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SshAgent;
using SshAgent.Proxy;
using SshAgent.Transport.Pipe;
using SshAgent.Transport.TcpSocket;

namespace SshAgentProxyService
{
    internal class ServiceBootstrap
    {
        static async Task Main(string[] args)
        {
            try
            {
                var hostBuilder = new HostBuilder();

                ConfigureCommonHost(hostBuilder);
                ConfigureSshAgentProxyHost(hostBuilder);

                using var host = hostBuilder.Build();

                // Start generic host
                await host.StartAsync();
                await host.WaitForShutdownAsync();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                Console.Error.WriteLine(ex.StackTrace);
            }
        }

        static void ConfigureCommonHost(HostBuilder hostBuilder)
        {
            hostBuilder.ConfigureHostConfiguration(builder =>
            {
                // File configuration
                builder.AddJsonFile("config.json", true);
            });

            hostBuilder.ConfigureServices((context, services) =>
            {
                services.AddLogging(builder =>
                {
                    // Load configuration from logging section
                    builder.AddConfiguration(context.Configuration.GetSection("Logging"));

                    // Register loggers
                    builder.AddConsole();
                });

                // Configure common services
                ConfigureCommonServices(services);
            });
        }

        static void ConfigureCommonServices(IServiceCollection services)
        {
            services.AddServiceMapByName();
        }

        static void ConfigureSshAgentProxyHost(HostBuilder hostBuilder)
        {
            hostBuilder.ConfigureServices((context, services) =>
            {
                #region [SshAgent]

                services.ConfigureByName<PipeSshAgentClientOptions>();
                services.ConfigureByName<SocketSshAgentClientOptions>();

                services.AddSingleton(p =>
                {
                    var providers = new Dictionary<string, ISshAgent>
                    {
                        { "PipeSshAgentClient", p.CreateService<PipeSshAgentClient>() },
                        { "SocketSshAgentClient", p.CreateService<SocketSshAgentClient>() },
                    };

                    return ServiceMap.Create(providers);
                });

                services.ConfigureByName<SshAgentProxyOptions>();
                services.AddSingleton<ISshAgent, SshAgentProxy>();

                #endregion

                #region [SshAgentService]

                services.ConfigureByName<PipeSshAgentHostConnectionFactoryOptions>();
                services.ConfigureByName<SocketSshAgentHostConnectionFactoryOptions>();

                services.AddSingleton(p => {

                    var factories = new Dictionary<string, ISshAgentHostConnectionFactory>
                    {
                        { "PipeSshAgentHostConnectionFactory", p.CreateService<PipeSshAgentHostConnectionFactory>() },
                        { "SocketSshAgentHostConnectionFactory", p.CreateService<SocketSshAgentHostConnectionFactory>() },
                    };

                    return ServiceMap.Create(factories);
                });

                services.AddSingleton<ISshAgentHostConnectionFactory, SshAgentConnectionFactoryProxy>();
                services.AddSingleton<SshAgentService>();

                #endregion

                #region [SshAgentProxyBackgroundService]

                services.AddHostedService<SshAgentProxyBackgroundService>();

                #endregion
            });

            hostBuilder.UseWindowsService();
        }
    }
}
