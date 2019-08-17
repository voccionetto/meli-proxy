using System;
namespace PROXY_MELI.ReverseProxy
{
    public class Rule
    {
        public Rule()
        {
        }

        public string Ip { get; set; }
        public string Path { get; set; }
        public int QtdMax { get; set; }
    }
}
