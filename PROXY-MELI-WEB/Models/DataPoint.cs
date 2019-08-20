using System;
using System.Runtime.Serialization;

namespace PROXY_MELI_WEB.Models
{
    [DataContract]
    public class DataPoint
    {
        public DataPoint(double y, string label)
        {
            this.Y = y;
            this.Label = label;
        }

        public DataPoint(double x, double y)
        {
            this.X = x;
            this.Y = y;
        }


        public DataPoint(double x, double y, string label)
        {
            this.X = x;
            this.Y = y;
            this.Label = label;
        }

        public DataPoint(double x, double y, double z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }

        public DataPoint(double x, double y, double z, string label)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
            this.Label = label;
        }


        [DataMember(Name = "label")]
        public string Label;

        [DataMember(Name = "y")]
        public double? Y;

        [DataMember(Name = "x")]
        public double? X;

        [DataMember(Name = "z")]
        public double? Z;
    }
}
