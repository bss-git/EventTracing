using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EventTracing.CounterData;
using Microsoft.Diagnostics.NETCore.Client;
using Microsoft.Diagnostics.Tracing;
using Serilog;

namespace EventTracing
{
    /// <summary>
    /// Монитор счётчиков и событий EventSource
    /// </summary>
    public class EventsTracer : IDisposable
    {
        private readonly int _pid;
        private readonly string _pipeFile = string.Empty;
        private readonly ILogger _logger;
        private readonly TaskCompletionSource<object> _cancelTcs = new TaskCompletionSource<object>();
        private readonly IEnumerable<EventPipeProvider> _providers;

        /// <summary>
        /// Будет вызвано при поступлении данных счётчика EventSource
        /// </summary>
        public event EventHandler<CounterPayload> NewCounterData;
        
        /// <summary>
        /// Будет вызвано при поступлении данных события EventSource
        /// </summary>
        public event EventHandler<TraceEvent> NewEventData;

        private void OnNewCounterData(CounterPayload data)
        {
            var subscribers = NewCounterData;
            if (subscribers != null)
                subscribers(this, data);
        }        
        
        private void OnNewEventData(TraceEvent data)
        {
            var subscribers = NewEventData;
            if (subscribers != null)
                subscribers(this, data);
        }

        /// <summary>
        /// ctor, можно использовать и на Windows, и на Linux
        /// </summary>
        /// <param name="pid">Пид процесса для мониторинга</param>
        /// <param name="providers">Провайдеры EventSource, события которых нужно трэйсить</param>
        /// <param name="logger">Логгер для логирования ошибок и служебной информации</param>
        public EventsTracer(int pid, IEnumerable<EventPipeProvider> providers, ILogger logger)
        {
            _pid = pid;
            _logger = logger;
            _providers = providers;
        }

        /// <summary>
        /// Запуск мониторинга
        /// </summary>
        /// <returns>Задача, которая выполняется в течение всего процесса мониторинга</returns>
        public async Task Start()
        {
            var client = new DiagnosticsClient(_pid);

            using var session = client.StartEventPipeSession(_providers, false);
            
            var streamTask = Task.Run(() =>
            {
                var source = new EventPipeEventSource(session.EventStream);
                source.Dynamic.All += ProcessEvents;
                try
                {
                    source.Process();
                }
                
                catch (Exception ex)
                {
                    session.Stop();
                    _logger.Error($"Error encountered while processing events from {_pid}:{ex}");
                }
            });

            var cancelTask = Task.Run(async () =>
            {
                await _cancelTcs.Task;
                session.Stop();
                _logger.Information($"Session stopped {_pid}");
            });

            await Task.WhenAny(streamTask, cancelTask);
        }

        /// <summary>
        /// Остановка мониторинга
        /// </summary>
        public void Stop()
        {
            _cancelTcs.TrySetResult(null);
        }
        
        private void ProcessEvents(TraceEvent eventData)
        {
            if (eventData.EventName.Equals("EventCounters"))
            {
                try
                {
                    IDictionary<string, object> payloadVal = (IDictionary<string, object>) eventData.PayloadValue(0);
                    IDictionary<string, object> payloadFields = (IDictionary<string, object>) payloadVal["Payload"];

                    var payload = CounterPayload.GetFromKvPairs(payloadFields);

                    OnNewCounterData(payload);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex.ToString());
                }
            }
            else
            {
                try
                {
                    OnNewEventData(eventData);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex.ToString());
                }
            }
        }

        public void Dispose()
        {
            Stop();
            GC.SuppressFinalize(this);
        }

        ~EventsTracer()
        {
            Stop();
        }
    }
}