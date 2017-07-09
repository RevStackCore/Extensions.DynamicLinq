namespace RevStackCore.Extensions.DynamicLinq
{
    public class QueryFilterOptions
    {
        public FilterOperation Operation { get; set; }
        public string Property { get; set; }
        public object Value { get; set; }
        public string Sql { get; set; }
        public FilterTransform? Transform { get; set; }
    }
}
