using System;
namespace PROXY_MELI.ReverseProxy
{
    public class RequestMELI
    {
        public RequestMELI()
        {

        }

        public TimeSpan InitialTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public bool Success { get; set; }
    }
}
