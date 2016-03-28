using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colsp.Logic
{
    public class DisplayValueAttribute : System.Attribute
    {
        public DisplayValueAttribute()
        {
            this.Value = string.Empty;
        }

        public DisplayValueAttribute(string value)
        {
            this.Value = value;
        }

        public string Value { get; protected set; }
    }
}
