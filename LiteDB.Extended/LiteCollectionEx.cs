using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiteDB.Extended
{
    public static class LiteCollectionEx
    {
        /// <summary>
        /// Update a document in this collection. Returns false if not found document in collection
        /// </summary>
        public static bool Upsert<T>(this LiteCollection<T> coll, T document) where T : new()
        {
            return coll.Upsert(document);
        }

        /// <summary>
        /// Update a document in this collection. Returns false if not found document in collection
        /// </summary>
        public static bool Upsert<T>(this LiteCollection<T> coll, BsonValue id, T document) where T : new()
        {
            return coll.Upsert(id, document);
        }

        /// <summary>
        /// Update all documents
        /// </summary>
        public static int Upsert<T>(this LiteCollection<T> coll, IEnumerable<T> documents) where T : new()
        {
            return coll.Upsert(documents);
        }
    }
}
