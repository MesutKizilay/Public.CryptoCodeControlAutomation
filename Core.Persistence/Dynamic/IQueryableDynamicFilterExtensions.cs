using System.Linq.Dynamic.Core;
using System.Text;

namespace Core.Persistence.Dynamic
{
    public static class IQueryableDynamicFilterExtensions
    {
        private static readonly string[] _orders = { "asc", "desc" };
        private static readonly string[] _logics = { "and", "or" };

        private static readonly IDictionary<string, string> _operators = new Dictionary<string, string>
        {
            { "eq", "=" },
            { "neq", "!=" },
            { "lt", "<" },
            { "lte", "<=" },
            { "gt", ">" },
            { "gte", ">=" },
            { "isnull", "== null" },
            { "isnotnull", "!= null" },
            { "startswith", "StartsWith" },
            { "endswith", "EndsWith" },
            { "contains", "Contains" },
            { "doesnotcontain", "Contains" }
        };

        public static IQueryable<T> ToDynamic<T>(this IQueryable<T> query, DynamicQuery dynamicQuery)
        {
            if (dynamicQuery.Filter is not null)
                query = Filter(query, dynamicQuery.Filter);
            if (dynamicQuery.Sort is not null && dynamicQuery.Sort.Any())
                query = Sort(query, dynamicQuery.Sort);
            return query;
        }

        private static IQueryable<T> Filter<T>(IQueryable<T> queryable, Filter filter)
        {
            IList<Filter> filters = GetAllFilters(filter);
            string?[] values = filters.Select(f => f.Value).ToArray();
            string where = Transform(filter, filters);
            //string where2 = "np(queueId) != null and ((np(WorkOrder.OrderNumber).Contains(@1)) and np(queueId) = @2 or (np(SourceType) = @3 or np(SourceType) = @4))";
            if (!string.IsNullOrEmpty(where) && values != null)
                queryable = queryable.Where(where, values);

            return queryable;
        }

        private static IQueryable<T> Sort<T>(IQueryable<T> queryable, IEnumerable<Sort> sort)
        {
            foreach (Sort item in sort)
            {
                if (string.IsNullOrEmpty(item.Field))
                    throw new ArgumentException("Invalid Field");
                if (string.IsNullOrEmpty(item.Direction) || !_orders.Contains(item.Direction))
                    throw new ArgumentException("Invalid Order Type");
            }

            if (sort.Any())
            {
                string ordering = string.Join(separator: ",", values: sort.Select(s => $"{s.Field} {s.Direction}"));
                return queryable.OrderBy(ordering);
            }

            return queryable;
        }

        public static IList<Filter> GetAllFilters(Filter filter)
        {
            List<Filter> filters = new();
            GetFilters(filter, filters);
            return filters;
        }

        private static void GetFilters(Filter filter, IList<Filter> filters)
        {
            filters.Add(filter);
            if (filter.Filters is not null && filter.Filters.Any())
                foreach (Filter item in filter.Filters)
                    GetFilters(item, filters);
        }

        private static string? tempFilter = null;
        public static string Transform(Filter filter, IList<Filter> filters)
        {
            if (string.IsNullOrEmpty(filter.Field))
                throw new ArgumentException("Invalid Field");
            if (string.IsNullOrEmpty(filter.Operator) || !_operators.ContainsKey(filter.Operator))
                throw new ArgumentException("Invalid Operator");

            int index = filters.IndexOf(filter);
            string comparison = _operators[filter.Operator];
            StringBuilder where = new();

            if (!string.IsNullOrEmpty(filter.Value))
            {
                if (filter.Operator == "doesnotcontain")
                    where.Append($"(!np({filter.Field}).{comparison}(@{index.ToString()}))");
                else if (comparison is "StartsWith" or "EndsWith" or "Contains")
                    where.Append($"(np({filter.Field}).{comparison}(@{index.ToString()}))");
                else
                    where.Append($"np({filter.Field}) {comparison} @{index.ToString()}");
            }
            else if (filter.Operator is "isnull" or "isnotnull")
            {
                where.Append($"np({filter.Field}) {comparison}");
            }


            var y = tempFilter ?? "abc";
            for (int i = index; i >= 0; i--)
            {
                tempFilter = string.IsNullOrEmpty(tempFilter) ? filters[i].Logic : tempFilter;

                //tempFilter = filters[i].Logic;
                if (!string.IsNullOrEmpty(tempFilter))
                {
                    break;
                }
            }

            if (filter.Logic is not null && filter.Filters is not null && filter.Filters.Any())
            {
                if (!_logics.Contains(filter.Logic))
                    throw new ArgumentException("Invalid Logic");
                var x = filter.Filters.Select(f => Transform(f, filters)).ToArray();
                return $"{where} {tempFilter ?? filter.Logic} ({string.Join(separator: $" {filter.Logic} ", value: x)})";
            }//{filter.Logic}

            return where.ToString();
        }
    }
}
