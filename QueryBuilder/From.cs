using System;
using System.Linq.Expressions;

namespace QueryBuilder
{
    public class From<T>
    {
        private string clause = "";
        private Select<T> select;

        public override string ToString()
        {
            return $"{select.ToString()} FROM {clause}";
        }

        public From(Select<T> select, string table, string prefix = "")
        {
            this.select = select;
            this.clause = $"{prefix}{table}";
        }

        public Where<T> Where<TWhere>(Expression<Func<T, object>> where) where TWhere : T
        {
            return new Where<T>(this, where);
        }
    }
}
