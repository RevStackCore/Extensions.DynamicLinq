using System;
using System.Linq;

namespace RevStackCore.Extensions.DynamicLinq
{
	public class QueryResult<TEntity>
	{
		public IQueryable<TEntity> Items { get; set; }
		public Uri NextPageLink { get; set; }
		public long? Count { get; set; }
		public QueryResult() { }
        public QueryResult(IQueryable<TEntity> items, long? count)
		{
			Items = items;
			Count = count;
		}
        public QueryResult(IQueryable<TEntity> items, Uri nextPageLink, long? count)
		{
			Items = items;
			NextPageLink = nextPageLink;
			Count = count;
		}
	}
}
