using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiteDB.Extended
{
    public class QueryEx
    {
        /// <summary>
        /// Returns documents where the specified function returns true for the specified index field
        /// </summary>
        public static Query QueryIndexWithFunc(string field, Func<BsonValue, bool> func)
        {
            return new QueryIndexWithFunction(field, func);
        }
    }
}
