using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using NextSolution.Core.Shared;
using NextSolution.Core.Utilities;
using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace NextSolution.Infrastructure.Data
{
    public abstract class AppRepository<TEntity> : IRepository<TEntity> where TEntity : class, IEntity
    {
        protected readonly AppDbContext _dbContext;

        public AppRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public virtual async Task<TEntity> CreateAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            await _dbContext.AddAsync(entity, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return entity;
        }

        public virtual async Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            _dbContext.Update(entity);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return entity;
        }

        public virtual async Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            _dbContext.Remove(entity);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public virtual async Task DeleteManyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));

            await _dbContext.Set<TEntity>().Where(predicate).ExecuteDeleteAsync(cancellationToken);
        }

        public virtual async Task DeleteAllAsync(CancellationToken cancellationToken = default)
        {
            await _dbContext.Set<TEntity>().ExecuteDeleteAsync(cancellationToken);
        }

        public virtual async Task<TEntity?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
        {
            return await _dbContext.FindAsync<TEntity>(keyValues: new object[] { id }, cancellationToken);
        }

        public virtual Task<TEntity?> GetAsync(
            Expression<Func<TEntity, bool>> predicate,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
            Expression<Func<TEntity, object>>[]? include = null,
            CancellationToken cancellationToken = default)
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));
            return GetQuery(predicate, orderBy, include, enableTracking: true, enableFilters: true).FirstOrDefaultAsync(cancellationToken);
        }

        public virtual Task<TResult?> GetAsync<TResult>(
            Expression<Func<TEntity, TResult>> selector,
            Expression<Func<TEntity, bool>> predicate,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
            Expression<Func<TEntity, object>>[]? include = null,
            CancellationToken cancellationToken = default)
        {
            if (selector == null) throw new ArgumentNullException(nameof(selector));
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));
            return GetQuery(predicate, orderBy, include, enableTracking: true, enableFilters: true).Select(selector).FirstOrDefaultAsync(cancellationToken);
        }

        public virtual async Task<IEnumerable<TEntity>> GetManyAsync(
            Expression<Func<TEntity, bool>> predicate,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
            Expression<Func<TEntity, object>>[]? include = null,
            CancellationToken cancellationToken = default)
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));
            return await GetQuery(predicate, orderBy, include, enableTracking: true, enableFilters: true).ToArrayAsync(cancellationToken);
        }

        public virtual async Task<IEnumerable<TResult>> GetManyAsync<TResult>(
            Expression<Func<TEntity, TResult>> selector,
            Expression<Func<TEntity, bool>> predicate,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
            Expression<Func<TEntity, object>>[]? include = null,
            CancellationToken cancellationToken = default)
        {
            if (selector == null) throw new ArgumentNullException(nameof(selector));
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));
            return await GetQuery(predicate, orderBy, include, enableTracking: true, enableFilters: true).Select(selector).ToArrayAsync(cancellationToken);
        }

        public virtual async Task<IPageable<TEntity>> GetManyAsync(long offset, int limit,
            Expression<Func<TEntity, bool>>? predicate = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
            Expression<Func<TEntity, object>>[]? include = null,
            CancellationToken cancellationToken = default)
        {
            if (offset < 0)
                throw new ArgumentException("Offset must be greater than or equal to 0.");

            if (limit < 1)
                throw new ArgumentException("Limit must be greater than or equal to 1.");

            var query = GetQuery(predicate, orderBy, include, enableTracking: false, enableFilters: true);

            var totalItems = await query.LongCountAsync(cancellationToken);
            var items = await query.LongSkip(offset).Take(limit).ToListAsync(cancellationToken);
            var result = new Pageable<TEntity>(offset, limit, totalItems, items);
            return result;
        }

        public virtual async Task<IPageable<TResult>> GetManyAsync<TResult>(long offset, int limit,
            Expression<Func<TEntity, TResult>> selector,
            Expression<Func<TEntity, bool>>? predicate = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
            Expression<Func<TEntity, object>>[]? include = null,
            CancellationToken cancellationToken = default)
        {
            if (offset < 0)
                throw new ArgumentException("Offset must be greater than or equal to 0.");

            if (limit < 1)
                throw new ArgumentException("Limit must be greater than or equal to 1.");

            if (selector == null) throw new ArgumentNullException(nameof(selector));
            var query = GetQuery(predicate, orderBy, include, enableTracking: false, enableFilters: true);

            var totalItems = await query.LongCountAsync(cancellationToken);
            var items = await query.LongSkip(offset).Take(limit).Select(selector).ToListAsync(cancellationToken);
            var result = new Pageable<TResult>(offset, limit, totalItems, items);
            return result;
        }

        protected virtual IQueryable<TEntity> GetQuery(
            Expression<Func<TEntity, bool>>? predicate = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
            Expression<Func<TEntity, object>>[]? include = null,
            bool enableTracking = true,
            bool enableFilters = false)
        {
            IQueryable<TEntity> query = _dbContext.Set<TEntity>();

            if (!enableTracking)
                query = query.AsNoTracking();

            if (!enableFilters)
                query = query.IgnoreQueryFilters();

            if (include != null)
                query = include.Aggregate(query, EvaluateInclude);

            if (predicate != null)
                query = query.Where(predicate);

            if (orderBy != null)
                query = orderBy(query);

            return query;
        }

        protected IQueryable<TEntity> EvaluateInclude(IQueryable<TEntity> current, Expression<Func<TEntity, object>> item)
        {
            if (item.Body is MethodCallExpression)
            {
                var arguments = ((MethodCallExpression)item.Body).Arguments;
                if (arguments.Count > 1)
                {
                    var navigationPath = string.Empty;
                    for (var i = 0; i < arguments.Count; i++)
                    {
                        var arg = arguments[i];
                        var path = arg.ToString().Substring(arg.ToString().IndexOf('.') + 1);

                        navigationPath += (i > 0 ? "." : string.Empty) + path;
                    }
                    return current.Include(navigationPath);
                }
            }

            return current.Include(item);
        }

        public virtual Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
        {
            IQueryable<TEntity> query = _dbContext.Set<TEntity>();

            if (predicate != null)
                query = query.Where(predicate);

            return query.AnyAsync(cancellationToken);
        }

        public virtual Task<bool> AnyAsync(CancellationToken cancellationToken = default)
        {
            IQueryable<TEntity> query = _dbContext.Set<TEntity>();
            return query.AnyAsync(cancellationToken);
        }

        public virtual Task<long> CountAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
        {
            IQueryable<TEntity> query = _dbContext.Set<TEntity>();

            if (predicate != null)
                query = query.Where(predicate);

            return query.LongCountAsync(cancellationToken);
        }

        public virtual Task<long> CountAsync(CancellationToken cancellationToken = default)
        {
            IQueryable<TEntity> query = _dbContext.Set<TEntity>();
            return query.LongCountAsync(cancellationToken);
        }
    }
}