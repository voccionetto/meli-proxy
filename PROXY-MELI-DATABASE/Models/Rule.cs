using System;
namespace PROXY_MELI_DATABASE.Models
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
                    return PrefixKeyNameRedis + Ip + Path;

                if (!string.IsNullOrEmpty(Ip))
                    return PrefixKeyNameRedis + Ip;

                return PrefixKeyNameRedis + Path;
            }
        }

        public string KeyRateLimitRedis
        {
            get
            {
                return PrefixKeyRateLimitRedis + KeyRuleRedis;
            }
        }

        public static string PrefixKeyNameRedis
        {
            get
            {
                return "#rule_";
            }
        }

        public static string PrefixKeyRateLimitRedis
        {
            get
            {
                return "#rateLimit_";
            }
        }
    }
}
