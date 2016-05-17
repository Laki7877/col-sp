using System;

namespace Colsp.Logic
{
    public class StringValueAttribute : System.Attribute
    {
        public StringValueAttribute()
        {
            this.Value = string.Empty;
        }

        public StringValueAttribute(string value)
        {
            this.Value = value;
        }

        public string Value { get; protected set; }
    }
}
