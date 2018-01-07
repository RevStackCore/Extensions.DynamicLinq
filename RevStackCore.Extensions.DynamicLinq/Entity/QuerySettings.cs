using System.Collections.Generic;

namespace RevStackCore.Extensions.DynamicLinq
{
    public class QuerySettings
    {
        public IEnumerable<QueryFilterOptions> Filter { get; set; }
        public string Where { get; set; }
        public string OrderBy { get; set; }
        public string[] SqlParams { get; set; }
        public int? Top { get; set; }
        public int? Skip { get; set; }
        public int? Page { get; set; }
        public int? PageSize { get; set; }

    }

   
}
