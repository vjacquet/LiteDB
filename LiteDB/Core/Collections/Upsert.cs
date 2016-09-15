using System;
using System.Collections.Generic;
using System.Linq;

namespace LiteDB
{
    public partial class LiteCollection<T>
    {
        /// <summary>
        /// Upserts a document in this collection. Returns false if the document could not be inserted or updated.
        /// </summary>
        internal bool Upsert(T document)
        {
            if (document == null) throw new ArgumentNullException("document");

            // get BsonDocument from object
            var doc = _mapper.ToDocument(document);

            return _engine.Upsert(_name, new BsonDocument[] { doc }) > 0;
        }

        /// <summary>
        /// Upserts a document in this collection. Returns false if the document could not be inserted or updated.
        /// </summary>
        internal bool Upsert(BsonValue id, T document)
        {
            if (document == null) throw new ArgumentNullException("document");
            if (id == null || id.IsNull) throw new ArgumentNullException("id");

            // get BsonDocument from object
            var doc = _mapper.ToDocument(document);

            // set document _id using id parameter
            doc["_id"] = id;

            return _engine.Upsert(_name, new BsonDocument[] { doc }) > 0;
        }

        /// <summary>
        /// Upserts all documents. Returns a count of documents inserted and updated.
        /// </summary>
        internal int Upsert(IEnumerable<T> documents)
        {
            if (documents == null) throw new ArgumentNullException("document");

            return _engine.Upsert(_name, documents.Select(x => _mapper.ToDocument(x)));
        }
    }
}