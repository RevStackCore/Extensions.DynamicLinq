using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace RevStackCore.Extensions.DynamicLinq

{
    public class QueryCollection : IQueryCollection
    {
        public static readonly QueryCollection Empty;

        private Dictionary<string, StringValues> queryCollection = new Dictionary<string, StringValues>();

        public QueryCollection()
        {
        }

        public QueryCollection(Dictionary<string, StringValues> store)
        {
            this.queryCollection = store;
        }

        public StringValues this[string key] => queryCollection.ContainsKey(key) ? queryCollection[key] : new StringValues(string.Empty);

        public int Count => queryCollection.Count;

        public ICollection<string> Keys => queryCollection.Keys;

        public bool ContainsKey(string key)
        {
            if (queryCollection.ContainsKey(key))
                return true;
            return false;
        }

        public IEnumerator<KeyValuePair<string, StringValues>> GetEnumerator()
        {
            return queryCollection.GetEnumerator();
        }

        public bool TryGetValue(string key, out StringValues value)
        {
            return queryCollection.TryGetValue(key, out value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return queryCollection.GetEnumerator();
        }
    }
}
