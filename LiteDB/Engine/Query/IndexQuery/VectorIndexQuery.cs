using System;
using System.Collections.Generic;
using System.Linq;

namespace LiteDB.Engine
{
    internal sealed class VectorIndexQuery : Index, IDocumentLookup
    {
        private readonly Snapshot _snapshot;
        private readonly CollectionIndex _index;
        private readonly VectorIndexMetadata _metadata;
        private readonly float[] _target;
        private readonly double _maxDistance;
        private readonly int? _limit;
        private readonly bool _applyLimit;
        private readonly Collation _collation;

        private readonly Dictionary<PageAddress, (BsonDocument Document, double Score)> _cache = new Dictionary<PageAddress, (BsonDocument, double)>();

        public string Expression => _index.Expression;

        public VectorIndexQuery(
            string name,
            Snapshot snapshot,
            CollectionIndex index,
            VectorIndexMetadata metadata,
            float[] target,
            double maxDistance,
            int? limit,
            Collation collation,
            bool applyLimit = true)
            : base(name, Query.Ascending)
        {
            _snapshot = snapshot;
            _index = index;
            _metadata = metadata;
            _target = target;
            _maxDistance = maxDistance;
            _limit = limit;
            _applyLimit = applyLimit;
            _collation = collation;
        }

        public override uint GetCost(CollectionIndex index)
        {
            return 1;
        }

        public override IEnumerable<IndexNode> Execute(IndexService indexer, CollectionIndex index)
        {
            throw new NotSupportedException();
        }

        public override IEnumerable<IndexNode> Run(CollectionPage col, IndexService indexer)
        {
            _cache.Clear();

            var service = new VectorIndexService(_snapshot, _collation);
            var results = service.Search(_metadata, _target, _maxDistance, _limit, _applyLimit).ToArray();

            foreach (var result in results)
            {
                var rawId = result.Document.RawId;

                if (rawId.IsEmpty)
                {
                    continue;
                }

                _cache[rawId] = (result.Document, result.Distance);
                yield return new IndexNode(result.Document);
            }
        }

        public BsonDocument Load(IndexNode node)
        {
            return node.Key as BsonDocument;
        }

        public BsonDocument Load(PageAddress rawId)
        {
            return _cache.TryGetValue(rawId, out var result) ? result.Document : null;
        }

        internal bool Matches(VectorScoreProjection projection)
        {
            return string.Equals(Expression, projection.Field, StringComparison.OrdinalIgnoreCase) &&
                _target.SequenceEqual(projection.Target);
        }

        internal bool TryGetScore(PageAddress rawId, out double score)
        {
            score = default;
            if (!_cache.TryGetValue(rawId, out var result))
            {
                return false;
            }

            score = result.Score;
            return true;
        }

        internal LiteDB.Vector.VectorDistanceMetric Metric => _metadata.Metric;

        public override string ToString()
        {
            return "VECTOR INDEX SEARCH";
        }
    }
}
