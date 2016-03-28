using System;

namespace Colsp.Logic
{
    public class FlagValueAttribute : System.Attribute
    {
        public string Value { get; protected set; }

        public FlagValueAttribute()
        {
            this.Value = string.Empty;
        }

        public FlagValueAttribute(string value)
        {
            this.Value = value;
        }

    }
}
