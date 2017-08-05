using System;

namespace ArLehm.QueryBuilder
{
    [AttributeUsage(AttributeTargets.Class)]
    public class DataSourceNameAttribute : Attribute
    {
        public string Name { get; }

        public DataSourceNameAttribute(string name)
        {
            Name = name;
        }
    }
}
