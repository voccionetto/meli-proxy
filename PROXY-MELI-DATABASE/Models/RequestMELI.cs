using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace PROXY_MELI_DATABASE.Models
{
    public class RequestMELI
    {
        public ObjectId _id { get; set; }

        public TimeSpan TotalTime { get; set; }
        public int StatusCode { get; set; }
        public string Ip { get; set; }
        public string Path { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime Date { get; set; }
    }
}
