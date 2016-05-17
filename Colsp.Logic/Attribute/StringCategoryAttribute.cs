using System;

namespace Colsp.Logic
{
    public class StringCategoryAttribute : System.Attribute
    {
        public StringCategoryAttribute(string value)
        {
            this.Value = value;
        }
        public string Value { get; private set; }
    }
}
