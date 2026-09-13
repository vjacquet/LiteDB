using System.Collections.Generic;
using System.Linq;
using LiteDB.Vector;

namespace LiteDB.Engine
{
    internal sealed class VectorScoreProjection
    {
        private readonly BsonExpression _fallback;

        internal string Field { get; }
        internal float[] Target { get; }

        internal VectorScoreProjection(string field, float[] target)
        {
            Field = field;
            Target = target.ToArray();
            _fallback = BsonExpression.Create($"VECTOR_SIM({field}, @0)",
                new BsonArray(Target.Select(x => new BsonValue(x))));
        }

        internal IEnumerable<BsonDocument> Project(IEnumerable<BsonDocument> source,
            BsonExpression select, VectorIndexQuery index, Collation collation)
        {
            var useIndexScore = index != null && index.Matches(this);
            foreach (var document in source)
            {
                var metric = VectorDistanceMetric.Cosine;
                BsonValue score;
                if (useIndexScore && index.TryGetScore(document.RawId, out var cachedScore))
                {
                    score = cachedScore;
                    metric = index.Metric;
                }
                else
                {
                    score = _fallback.ExecuteScalar(document, collation);
                }

                // Keep metadata outside the user's document, including documents with Score fields.
                yield return new BsonDocument
                {
                    ["Document"] = select.ExecuteScalar(document, collation),
                    ["Score"] = score,
                    ["Metric"] = (int)metric
                };
            }
        }
    }
}
