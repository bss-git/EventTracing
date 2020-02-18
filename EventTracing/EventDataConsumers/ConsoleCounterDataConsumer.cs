using System;
using EventTracing.CounterData;

namespace EventTracing.EventDataConsumers
{
    /// <summary>
    /// Выводит счётчики в консоль
    /// </summary>
    public class ConsoleCounterDataConsumer
    {
        /// <summary>
        /// Обработчик собьытия, выводит данные на конссоль
        /// </summary>
        /// <param name="sender">Кем сгенерировано событие</param>
        /// <param name="data">Данные счётчика</param>
        public void OnNewCounterData(object sender, CounterPayload data)
        {
            Console.WriteLine($"{data.DisplayName}: {data.Value} {data.DisplayUnits}");
        }
    }
}