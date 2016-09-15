using System;
using System.Collections.Generic;
using System.Linq;

namespace LiteDB
{
    /// <summary>
    /// Do a full scan over the index, executing a function on each value.
    /// Should be much faster than a full table scan.
    /// 
    /// Will be exposed in LiteDB.Extended package only.
    /// </summary>
    internal class QueryIndexWithFunction : Query
    {
        private Func<BsonValue, bool> func;

        public QueryIndexWithFunction(string field, Func<BsonValue, bool> func)
            : base(field)
        {
            this.func = func;
        }

        internal override IEnumerable<IndexNode> ExecuteIndex(IndexService indexer, CollectionIndex index)
        {
            return indexer
                .FindAll(index, Query.Ascending)
                .Where(i => func(i.Key));
        }
    }
}