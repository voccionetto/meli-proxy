using System;
namespace PROXY_MELI.ReverseProxy
{
    public class RequestMELI
    {
        public TimeSpan TotalTime { get; set; }
        public int StatusCode { get; set; }
        public string Ip { get; set; }
        public string Path { get; set; }
    }
}
