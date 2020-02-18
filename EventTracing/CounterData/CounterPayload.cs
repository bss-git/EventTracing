using System;
using System.Collections.Generic;

namespace EventTracing.CounterData
{
    /// <summary>
    /// Данные внутри события счётчика
    /// </summary>
    public class CounterPayload
    {
        /// <summary>
        /// Имя счётчика
        /// </summary>
        public string Name { get; private set; }
        
        /// <summary>
        /// Отображаемое имя
        /// </summary>
        public string DisplayName { get; private set; }
        
        /// <summary>
        /// Тип счётчика
        /// </summary>
        public CounterType Type { get; private set; }
        
        /// <summary>
        /// Значение счётчика
        /// </summary>
        public double Value { get; private set; }
        
        /// <summary>
        /// Единицы измерения
        /// </summary>
        public string DisplayUnits { get; private set; }
        
        /// <summary>
        /// Интервал, за который сняты данные
        /// </summary>
        public int IntervalSec { get; private set; }
        
        /// <summary>
        /// Получить данные из словаря
        /// </summary>
        /// <exception cref="Exception"></exception>
        public static CounterPayload GetFromKvPairs(IDictionary<string, object> pairs)
        {
            var payload = new CounterPayload
            {
                Name = pairs["Name"].ToString(),
                DisplayName = pairs["DisplayName"].ToString(),
                DisplayUnits = pairs["DisplayUnits"].ToString(),
                IntervalSec = (int) Math.Round(double.Parse(pairs["IntervalSec"].ToString()), MidpointRounding.ToEven)
            };
            
            var type = pairs["CounterType"].ToString();
            if (type == "Sum")
            {
                payload.Type = CounterType.Sum;
                payload.Value = double.Parse(pairs["Increment"].ToString());
            }
            else if (type == "Mean")
            {
                payload.Type = CounterType.Mean;
                payload.Value = double.Parse(pairs["Mean"].ToString());
            }
            else
            {
                throw new Exception("Incorrect counter type");
            }

            return payload;
        }
    }
}