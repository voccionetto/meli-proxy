using System;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;

namespace System.Linq
{
    public static class HttpRequestExtensions
    {
        private const string NullIpAddress = "::1";

        public static string GetToken(this HttpRequest request)
        {
            var index = "bearer ".Length;
            var token = request.Headers["Authorization"].ToString();
            var acessToken = token.Substring(index, token.Length - index);
            return acessToken;
        }

        public static string GetClientIpAddress(this HttpRequest request)
        {
            const string xForwardedForHeader = "X-Forwarded-For";

            var ipAddress = request.HttpContext.Connection.RemoteIpAddress.ToString();

            if (!request.Headers.TryGetValue(xForwardedForHeader, out var xForwardForList))
                return ipAddress;

            var xForwardedForIpAddress = xForwardForList.FirstOrDefault()?.Split(',')[0];

            return string.IsNullOrEmpty(xForwardedForIpAddress)
        ? ipAddress
        : xForwardedForIpAddress;
        }

        public static ClientSystemInfo GetClientSystemInfo(this HttpRequest request)
        {
            return new ClientSystemInfo(
        request.Headers[HeaderNames.UserAgent],
        request.GetClientIpAddress());
        }

        public static bool IpIsLocal(this HttpRequest request)
        {
            var connection = request.HttpContext.Connection;
            if (connection.RemoteIpAddress.IsSet())
            {
                return connection.LocalIpAddress.IsSet()
                    ? connection.RemoteIpAddress.Equals(connection.LocalIpAddress)
                    : IPAddress.IsLoopback(connection.RemoteIpAddress);
            }

            return true;
        }

        private static bool IsSet(this IPAddress address)
        {
            return address != null && address.ToString() != NullIpAddress;
        }

    }

    public class ClientSystemInfo
    {
        public string OperatingSystemInfo { get; }
        public string IpAddress { get; }

        public ClientSystemInfo(string operatingSystemInfo, string ipAddress)
        {
            OperatingSystemInfo = operatingSystemInfo;
            IpAddress = ipAddress;
        }

        public override string ToString() => $"O.S: {OperatingSystemInfo} IpAddress: {IpAddress}";
    }
}
