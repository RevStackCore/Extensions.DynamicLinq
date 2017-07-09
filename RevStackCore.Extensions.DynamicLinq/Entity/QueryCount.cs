using System.Linq;

namespace RevStackCore.Extensions.DynamicLinq
{
    internal class QueryCount<TEntity>
    {
        public IQueryable<TEntity> Items { get; set; }
        public int Count { get; set; }
    }
}
