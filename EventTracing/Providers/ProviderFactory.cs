using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using Microsoft.Diagnostics.NETCore.Client;

namespace EventTracing.Providers
{
    /// <summary>
    /// Класс для получения известных EventSource провайдеров
    /// </summary>
    public static class ProviderFactory
    {
        /// <summary>
        /// Стандартные счётчики CLR
        /// </summary>
        /// <param name="intervalSec">Интервал опроса</param>
        public static EventPipeProvider SystemRuntimeCounters(int intervalSec)
        {
            return new EventPipeProvider("System.Runtime", EventLevel.Informational, arguments:new Dictionary<string, string>
            {
                { "EventCounterIntervalSec", intervalSec.ToString()}
            });
        }
        
        /// <summary>
        /// Счётчики и события AspNet
        /// </summary>
        /// <param name="intervalSec">Интервал опроса</param>
        /// <param name="eventLevel">Уровень логирования событий</param>
        public static EventPipeProvider AspNetProvider(int intervalSec, EventLevel eventLevel = EventLevel.Informational)
        {
            return new EventPipeProvider("Microsoft.AspNetCore.Hosting", eventLevel, arguments:new Dictionary<string, string>
            {
                { "EventCounterIntervalSec", intervalSec.ToString()}
            });
        }
        
        /// <summary>
        /// Наш кастомный провайдер - счётчики
        /// </summary>
        /// <param name="intervalSec">Интервал опроса</param>
        public static EventPipeProvider DnsCounters(int intervalSec)
        {
            return new EventPipeProvider("Dns-RequestStatistics-Counters", EventLevel.Informational, arguments:new Dictionary<string, string>
            {
                { "EventCounterIntervalSec", intervalSec.ToString()}
            });
        }
        
        /// <summary>
        /// Наш кастомный провайдер - события
        /// </summary>
        public static EventPipeProvider DnsEvents()
        {
            return new EventPipeProvider("Dns-RequestStatistics-Events", EventLevel.Informational);
        }

        /// <summary>
        /// Общий провайдер для разных служб, можно мониторить aspnet и http клиентов, например
        /// </summary>
        public static EventPipeProvider MicrosoftDiagnosticsDiagnosticSource()
        {
            var filterAndPayloadSpecs =
                @"HttpHandlerDiagnosticListener/System.Net.Http.Request@Activity2Start:Request.RequestUri" +
                @"\r\n" +
                @"HttpHandlerDiagnosticListener/System.Net.Http.Response@Activity2Stop:Response.StatusCode";
            
            return new EventPipeProvider("Microsoft-Diagnostics-DiagnosticSource", EventLevel.Informational, Convert.ToInt64("0xfffffffffffff7ff"),
                new Dictionary<string, string>
                {
                    { "FilterAndPayloadSpecs", filterAndPayloadSpecs}
                });
        }
        
        /// <summary>
        /// Провайдер для расставление идентификатора активити событиям + мониторинга TPL
        /// </summary>
        public static EventPipeProvider SystemTplEventSource()
        {
            return new EventPipeProvider("System.Threading.Tasks.TplEventSource", EventLevel.LogAlways, 0x1ff);
        }
    }
}