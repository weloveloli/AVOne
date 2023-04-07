// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Impl.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Furion.LinqBuilder;

    public class RepositoryBase
    {

        /// <summary>
        /// The BuildPageList.
        /// </summary>
        /// <param name="totalCount">The totalCount<see cref="int"/>.</param>
        /// <param name="pageIndex">The pageIndex<see cref="int"/>.</param>
        /// <param name="pageSize">The pageSize<see cref="int"/>.</param>
        /// <param name="items">The items<see cref="IEnumerable{TEntity}"/>.</param>
        /// <returns>The <see cref="PagedList{TEntity}"/>.</returns>
        public PagedList<T> BuildPageList<T>(int totalCount, int pageIndex, int pageSize, IEnumerable<T> items) where T : new()
        {
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            return new PagedList<T>
            {
                TotalCount = totalCount,
                Items = items,
                TotalPages = totalPages,
                HasNextPages = pageIndex < totalPages,
                HasPrevPages = pageIndex > 1,
                PageIndex = 1,
                PageSize = pageSize
            };
        }

        /// <summary>
        /// The MergeExp.
        /// </summary>
        /// <param name="conditionPredicates">The conditionPredicates<see cref="(bool condition, Expression{Func{JobModel, bool}} expression)[]"/>.</param>
        /// <returns>The <see cref="Expression{Func{JobModel, bool}}"/>.</returns>
        public Expression<Func<T, bool>> MergeExp<T>(params (bool condition, Expression<Func<T, bool>> expression)[] conditionPredicates) where T : new()
        {
            Expression<Func<T, bool>> exp = (e) => true;
            if (conditionPredicates != null && conditionPredicates.Any())
            {
                foreach ((bool condition, Expression<Func<T, bool>> expression) c in conditionPredicates)
                {
                    if (c.condition)
                    {
                        exp = exp.And(c.expression);
                    }
                }
            }
            return exp;
        }
    }
}
