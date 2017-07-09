using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Linq.Dynamic.Core;
using Microsoft.AspNetCore.Http;
using RevStackCore.Serialization;

namespace RevStackCore.Extensions.DynamicLinq
{
    public static class Extensions
    {
		/// <summary>
		/// Applies Dynamic Linq Filtering to an IQueryable result using the Request $filter querystring value
		/// </summary>
		/// <returns>IQueryable</returns>
		/// <param name="request">Request.</param>
		/// <param name="query">Query.</param>
		/// <typeparam name="TEntity">The 1st type parameter.</typeparam>
		public static IQueryable<TEntity> ApplyTo<TEntity>(this HttpRequest request, IQueryable<TEntity> query)
        {
            var settings = getSettings(request);
            var queryCount= getItems(query, settings);
            return queryCount.Items;
        }

		/// <summary>
		/// Applies Dynamic Linq Filtering to an IQueryable overriding the  Request $filter querystring with  a passed QuerySettings instance
		/// </summary>
		/// <returns>IQueryable</returns>
		/// <param name="request">Request.</param>
		/// <param name="query">Query.</param>
		/// <param name="settings">Settings.</param>
		/// <typeparam name="TEntity">The 1st type parameter.</typeparam>
		public static IQueryable<TEntity> ApplyTo<TEntity>(this HttpRequest request, IQueryable<TEntity> query, QuerySettings settings)
		{
            if (settings.Filter != null && settings.Filter.Count()>0)
            {
                settings.Filter = getFilterOptions(request);
            }
            settings = checkSettings(request, settings);
			var queryCount = getItems(query, settings);
			return queryCount.Items;
		}

		/// <summary>
		/// Applies Dynamic Linq Filtering to an IQueryable using the Request $filter querystring value
		/// </summary>
		/// <returns>PageResult</returns>
		/// <param name="request">Request.</param>
		/// <param name="query">Query.</param>
		/// <typeparam name="TEntity">The 1st type parameter.</typeparam>
		public static QueryResult<TEntity> PageResult<TEntity>(this HttpRequest request, IQueryable<TEntity> query)
        {
            var result = new QueryResult<TEntity>();
            var settings = getSettings(request);
            var queryCount = getItems(query, settings);
            result.Items = queryCount.Items;
            result.Count = queryCount.Count;
            return result;
        }

		/// <summary>
		/// Applies Dynamic Linq Filtering to an IQueryable overriding the  Request $filter querystring with  a passed QuerySettings instance
		/// </summary>
		/// <returns>PageResult</returns>
		/// <param name="request">Request.</param>
		/// <param name="query">Query.</param>
		/// <param name="settings">Settings.</param>
		/// <typeparam name="TEntity">The 1st type parameter.</typeparam>
		public static QueryResult<TEntity> PageResult<TEntity>(this HttpRequest request, IQueryable<TEntity> query, QuerySettings settings)
		{
            var result = new QueryResult<TEntity>();
			settings = checkSettings(request, settings);
			if (settings.Filter != null && settings.Filter.Count() > 0)
			{
				settings.Filter = getFilterOptions(request);
			}
            var queryCount = getItems(query, settings);
            result.Items = queryCount.Items;
            result.Count = queryCount.Count;
            return result;
		}

        /// <summary>
        /// Gets the settings.
        /// </summary>
        /// <returns>The settings.</returns>
        /// <param name="request">Request.</param>
        private static QuerySettings getSettings(HttpRequest request)
        {
            var settings = new QuerySettings();
			var queryString = request.Query;
            string strSkip = queryString["$skip"];
            string strTop = queryString["$top"];
            string whereClause = queryString["$where"];
            string strOrderBy = queryString["$orderby"];
            int? skip = null;
            int? top = null;
            string orderBy = null;
            var filter = new List<QueryFilterOptions>();

            if(!string.IsNullOrEmpty(strSkip))
            {
;               skip = Convert.ToInt32(strSkip);
            }
			if (!string.IsNullOrEmpty(strTop))
			{
				top = Convert.ToInt32(strTop);
			}
            if (!string.IsNullOrEmpty(whereClause))
            {
                whereClause = WebUtility.UrlDecode(whereClause);
            }
            else 
            {
                filter = getFilterOptions(request).ToList();
            }
            if (!string.IsNullOrEmpty(strOrderBy))
			{
                whereClause = null;
                strOrderBy = WebUtility.UrlDecode(strOrderBy);
                string[] order = strOrderBy.Split(' ');
                if(order.Count()==2)
                {
                    orderBy = order[0] + " descending";
                }
                else
                {
                    orderBy = order[0];
                }
			}

            settings.Filter = filter;
            settings.OrderBy = orderBy;
            settings.Skip = skip;
            settings.Top = top;
            settings.Where = whereClause;
		
            return settings;
        }

        /// <summary>
        /// Checks the settings.
        /// </summary>
        /// <returns>The settings.</returns>
        /// <param name="request">Request.</param>
        /// <param name="settings">Settings.</param>
        private static QuerySettings checkSettings(HttpRequest request, QuerySettings settings)
        {

            return settings;
        }

        /// <summary>
        /// Gets the filter options.
        /// </summary>
        /// <returns>The filter options.</returns>
        /// <param name="request">Request.</param>
        private static IEnumerable<QueryFilterOptions> getFilterOptions(HttpRequest request)
        {
            var options = new List<QueryFilterOptions>();
            var queryString = request.Query;
            string filter = queryString["$filter"];
			if (!string.IsNullOrEmpty(filter))
            {
                filter = WebUtility.UrlDecode(filter);
                options = Json.DeserializeObject<List<QueryFilterOptions>>(filter);
            }

            return options;
        }

        /// <summary>
        /// Gets the items.
        /// </summary>
        /// <returns>The items.</returns>
        /// <param name="query">Query.</param>
        /// <param name="settings">Settings.</param>
        /// <typeparam name="TEntity">The 1st type parameter.</typeparam>
        private static QueryCount<TEntity> getItems<TEntity>(IQueryable<TEntity> query,QuerySettings settings)
        {
            var queryCount = new QueryCount<TEntity>();
            var filters = settings.Filter;
            int skip;
            int top;
            //store orignal qeryable for any union operations
            var origQuery = query;
            /////////////// FILTERING ////////////////////////////////////
            //filter query by list of QueryFilterOptions
            if(filters.Any())
            {
                foreach (var filter in filters)
                {
                    var val = filter.Value;
                    //insert direct sql expression
                    if (filter.Operation==FilterOperation.Sql)
                    {
                        string strSql = filter.Sql;
                        query = query.Where(strSql,val);
                      
                    }
                    //insert direct sql expression w/ union 
                    else if (filter.Operation==FilterOperation.SqlUnion)
                    {
						string strSql = filter.Sql;
						query = query.Union(origQuery.Where(strSql, val));	
                     
                    }
                    //insert a dynamically generated paramterized sql statement from QueryFilterOptions
                    else
                    {
						string sql = filter.toSqlExpression();
						if (!string.IsNullOrEmpty(sql))
						{
							if (filter.Transform == FilterTransform.Lower)
							{
								val = val.ToString().ToLower();
							}
							else if (filter.Transform == FilterTransform.Upper)
							{
								val = val.ToString().ToUpper();
							}
							else if (filter.Transform == FilterTransform.Trim)
							{
								val = val.ToString().Trim();
							}

							query = query.Where(sql, val);
						}
                    }
                }
            }
            //else directly insert a passed sql where clause
            else if(!string.IsNullOrEmpty(settings.Where))
            {
                query = query.Where(settings.Where);
            }

            //////// ASSIGN TOTAL COUNT ///////////////////////////////////////
            queryCount.Count = query.Count();

            ///////////// 
            if(settings.Top!=null && settings.Skip!=null)
            {
                skip = Convert.ToInt32(settings.Skip);
                top = Convert.ToInt32(settings.Top);
                query = query.Skip(skip).Take(top);
            }
            else if(settings.Top!=null)
            {
				top = Convert.ToInt32(settings.Top);
				query = query.Take(top);
            }

            ////////////// ORDERING /////////////////////////////////////////
			if (!string.IsNullOrEmpty(settings.OrderBy))
			{
				query = query.OrderBy(settings.OrderBy);
			}

            queryCount.Items = query;
            return queryCount;
        }

        /// <summary>
        /// Converts a QueryFilterOptions instance into a paramtrized sql where clause expression.
        /// </summary>
        /// <returns>The sql expression.</returns>
        /// <param name="filter">Filter.</param>
        private static string toSqlExpression(this QueryFilterOptions filter)
        {
            var operation = filter.Operation;
            string sql = "";
            string property = filter.Property.ToProperCase();
            var transform = filter.Transform;
            switch(operation)
            {
                case FilterOperation.Eq:
                    if(transform==FilterTransform.Lower)
                    {
                        sql = property + ".ToLower()=@0";
                    }else if(transform==FilterTransform.Upper)
                    {
                        sql = property + ".ToUpper()=@0";
                    }else if(transform==FilterTransform.Trim)
                    {
                        sql = property + ".Trim()=@0";
                    }
                    else
                    { 
                       sql = property + "=@0"; 
                    }
                    break;
				case FilterOperation.Ge:
                    sql = property + ">=@0";   
					break;
                case FilterOperation.Gt:
					sql = property + ">@0";
					break;
                case FilterOperation.Le:
					sql = property + "<=@0";
					break;
                case FilterOperation.Ne:
					if (transform == FilterTransform.Lower)
					{
						sql = property + ".ToLower()<>@0";
					}
					else if (transform == FilterTransform.Upper)
					{
						sql = property + ".ToUpper()<>@0";
					}
					else if (transform == FilterTransform.Trim)
					{
						sql = property + ".Trim()<>@0";
					}
					else
					{
						sql = property + "<>@0";
					}
					break;
                case FilterOperation.Contains:
					if (transform == FilterTransform.Lower)
					{
                        sql = property + ".ToLower().Contains(@0)";
					}
					else if (transform == FilterTransform.Upper)
					{
                        sql = property + ".ToUpper().Contains(@0)";
					}
					else if (transform == FilterTransform.Trim)
					{
                        sql = property + ".Trim().Contains(@0)";
					}
					else
					{
                        sql = property + ".Contains(@0)";
					}
					break;
                case FilterOperation.StartsWith:
					if (transform == FilterTransform.Lower)
					{
						sql = property + ".ToLower().StartsWith(@0)";
					}
					else if (transform == FilterTransform.Upper)
					{
						sql = property + ".ToUpper().StartsWith(@0)";
					}
					else if (transform == FilterTransform.Trim)
					{
						sql = property + ".Trim().StartsWith(@0)";
					}
					else
					{
						sql = property + ".StartsWith(@0)";
					}
					break;
                case FilterOperation.EndsWith:
					if (transform == FilterTransform.Lower)
					{
						sql = property + ".ToLower().EndsWith(@0)";
					}
					else if (transform == FilterTransform.Upper)
					{
						sql = property + ".ToUpper().EndsWith(@0)";
					}
					else if (transform == FilterTransform.Trim)
					{
						sql = property + ".Trim().EndsWith(@0)";
					}
					else
					{
						sql = property + ".EndsWith(@0)";
					}
					break;

			}

            return sql;
        }

    }
}
