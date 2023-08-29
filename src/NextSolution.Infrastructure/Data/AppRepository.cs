using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using NextSolution.Core.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace NextSolution.Infrastructure.Data
{
    public abstract class AppRepository<TEntity> : IRepository<TEntity> where TEntity : class, IEntity
    {
        private readonly AppDbContext _dbContext;

        public AppRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public virtual async Task CreateAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            await _dbContext.AddAsync(entity, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public virtual async Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            _dbContext.Update(entity);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public virtual async Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
         
            _dbContext.Remove(entity);
            await _dbContext.SaveChangesAsync(cancellationToken);
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
            return GetQueryable(predicate, orderBy, include, enableTracking: true, enableFilters: true).FirstOrDefaultAsync(cancellationToken);
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
            return GetQueryable(predicate, orderBy, include, enableTracking: true, enableFilters: true).Select(selector).FirstOrDefaultAsync(cancellationToken);
        }

        public virtual async Task<IEnumerable<TEntity>> GetManyAsync(
            Expression<Func<TEntity, bool>> predicate,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
            Expression<Func<TEntity, object>>[]? include = null,
            CancellationToken cancellationToken = default)
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));
            return await GetQueryable(predicate, orderBy, include, enableTracking: true, enableFilters: true).ToArrayAsync(cancellationToken);
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
            return await GetQueryable(predicate, orderBy, include, enableTracking: true, enableFilters: true).Select(selector).ToArrayAsync(cancellationToken);
        }

        public virtual async Task<IEnumerable<TEntity>> GetAllAsync(
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
            Expression<Func<TEntity, object>>[]? include = null,
            CancellationToken cancellationToken = default)
        {
            return await GetQueryable(null, orderBy, include, enableTracking: true, enableFilters: true).ToArrayAsync(cancellationToken);
        }

        public virtual async Task<IEnumerable<TResult>> GetAllAsync<TResult>(
            Expression<Func<TEntity, TResult>> selector,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
            Expression<Func<TEntity, object>>[]? include = null,
            CancellationToken cancellationToken = default)
        {
            if (selector == null) throw new ArgumentNullException(nameof(selector));
            return await GetQueryable(null, orderBy, include, enableTracking: true, enableFilters: true).Select(selector).ToArrayAsync(cancellationToken);
        }

        public virtual async Task<IPageable<TEntity>> GetManyAsync(int pageNumber, int pageSize,
            Expression<Func<TEntity, bool>>? predicate = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
            Expression<Func<TEntity, object>>[]? include = null,
            CancellationToken cancellationToken = default)
        {
            return await GetQueryable(predicate, orderBy, include, enableTracking: false, enableFilters: true).PaginateAsync(pageNumber, pageSize, cancellationToken);
        }

        public virtual async Task<IPageable<TResult>> GetManyAsync<TResult>(int pageNumber, int pageSize,
            Expression<Func<TEntity, TResult>> selector,
            Expression<Func<TEntity, bool>>? predicate = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
            Expression<Func<TEntity, object>>[]? include = null,
            CancellationToken cancellationToken = default)
        {
            if (selector == null) throw new ArgumentNullException(nameof(selector));
            return await GetQueryable(predicate, orderBy, include, enableTracking: false, enableFilters: true).Select(selector).PaginateAsync(pageNumber, pageSize, cancellationToken);
        }

        protected IQueryable<TEntity> GetQueryable(
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