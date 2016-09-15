using System;
using System.Collections.Generic;
using System.Linq;

namespace LiteDB
{
   public partial class DbEngine
    {
        /// <summary>
        /// Implement upsert command to documents in a collection. Calls update on all documents,
        /// then any documents not updated are then attempted to insert.
        /// This will have the side effect of throwing if duplicate items are attempted to be inserted.
        /// </summary>
        /// <param name="colName"></param>
        /// <param name="documents"></param>
        /// <returns></returns>
        internal int Upsert(String colName, IEnumerable<BsonDocument> documents)
        {
            using (var trans = BeginTrans())
            {
                foreach (var doc in documents)
                {
                    var arr = new BsonDocument[] { doc };
                    if (Update(colName, arr) == 0)
                    {
                        Insert(colName, arr);
                    }
                }

                return documents.Count();
            }
        }
    }
}