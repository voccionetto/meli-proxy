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
        public int RateLimit { get; set; }
        public string Name { get; set; }
        public int BlockedTime { get; set; }

        public string KeyRuleRedis
        {
            get
            {
                if (!string.IsNullOrEmpty(Ip) && !string.IsNullOrEmpty(Path))
                    return PrefixNameRedis + Ip + Path;

                if (!string.IsNullOrEmpty(Ip))
                    return PrefixNameRedis + Ip;

                return PrefixNameRedis + Path;
            }
        }

        public string KeyRateLimitRedis
        {
            get
            {
                return PrefixRateLimitRedis + KeyRuleRedis;
            }
        }

        public static string PrefixNameRedis
        {
            get
            {
                return "#rule_";
            }
        }

        public static string PrefixRateLimitRedis
        {
            get
            {
                return "#rateLimit_";
            }
        }
    }
}
