using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EventTracing.EventDataConsumers;
using EventTracing.Providers;
using Microsoft.Diagnostics.NETCore.Client;
using Serilog;

namespace EventTracing
{
    /// <summary>
    /// Запуск мониторинга для одного процесса, можно использовать для отладки у на локальной машине
    /// </summary>
    public class SingleProcessTracing
    {
        private readonly ILogger _logger;

        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="logger">Логгер для логирования</param>
        public SingleProcessTracing(ILogger logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Запуск трэйсинга
        /// </summary>
        /// <param name="args">Аргументы командной строки</param>
        /// <returns>Задача, которая выполняется в течение всего процесса мониторинга</returns>
        public async Task Run(string[] args)
        {
            #region args_parsing

            var pid = 0;
            var pipeFile = string.Empty;
            var monitoredSystem = string.Empty;
            var intervalSec = 0;

            var parsedArgs = new HashSet<string>();
            for (int i = 0; i < args.Length; i++)
            {
                if (!parsedArgs.Add(args[i]))
                {
                    Console.WriteLine($"Duplicate argument: {args[i]}");
                    Console.WriteLine("Usage: dotnet ClrEventMonitoringDll (-p <targetPid> | -f <pipeFile>) -s <monitoredSystem> -i <intervalSeconds>");
                    return;
                }

                try
                {
                    switch (args[i])
                    {
                        case "-p":
                            pid = int.Parse(args[i + 1]);
                            break;
                    
                        case "-f":
                            pipeFile = args[i + 1];
                            break;
                    
                        case "-s":
                            monitoredSystem = args[i + 1];
                            break;
                        
                        case "-i":
                            intervalSec = int.Parse(args[i + 1]);
                            break;
                    }
                }
                catch (Exception)
                {
                    Console.WriteLine("Usage: dotnet ClrEventMonitoringDll (-p <targetPid> | -f <pipeFile>) -s <monitoredSystem> -i <intervalSeconds>");
                    return;
                }                    
            }

            if ((pid == 0) == string.IsNullOrWhiteSpace(pipeFile))
            {
                Console.WriteLine("Please, specify pid OR pipe file name");
                return;
            }

            if (string.IsNullOrWhiteSpace(monitoredSystem))
            {
                Console.WriteLine("Monitored system name is not specified");
                return;
            }
            
            if (intervalSec <= 0)
            {
                Console.WriteLine("Interval (seconds) is not specified");
                return;
            }

            #endregion
            
            var providers = new List<EventPipeProvider>();

            providers.Add(ProviderFactory.SystemRuntimeCounters(intervalSec));
            providers.Add(ProviderFactory.AspNetProvider(intervalSec));
            providers.Add(ProviderFactory.DnsCounters(intervalSec));
            
            var tracer = new EventsTracer(pid, providers, _logger);
            
            var consoleWriter = new ConsoleCounterDataConsumer();
            tracer.NewCounterData += consoleWriter.OnNewCounterData;

            await tracer.Start();
        }
    }
}