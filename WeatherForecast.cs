using System;

namespace swagger_aad
{
    /// <summary>
    /// DataModel Object for the API.
    /// </summary>
    public class WeatherForecast
    {
        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public DateTime Date { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public int TemperatureC { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public string Summary { get; set; }
    }
}
