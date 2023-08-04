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

        public virtual async Task CreateAsync(TEntity entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            await _dbContext.AddAsync(entity);
            await _dbContext.SaveChangesAsync();
        }

        public virtual async Task UpdateAsync(TEntity entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            _dbContext.Update(entity);
            await _dbContext.SaveChangesAsync();
        }

        public virtual async Task DeleteAsync(TEntity entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
         
            _dbContext.Remove(entity);
            await _dbContext.SaveChangesAsync();
        }

        public virtual async Task<TEntity?> FindByIdAsync(long id)
        {
            return await _dbContext.FindAsync<TEntity>(id);
        }

        public Task<TEntity?> FindAsync(
            Expression<Func<TEntity, bool>> predicate,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
            params Expression<Func<TEntity, object>>[]? include)
        {
            return GetQueryable(predicate, orderBy, include, enableTracking: true, enableFilters: true).FirstOrDefaultAsync();
        }

        public Task<TResult?> FindAsync<TResult>(
            Expression<Func<TEntity, TResult>> selector,
            Expression<Func<TEntity, bool>> predicate,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
            params Expression<Func<TEntity, object>>[]? include)
        {
            return GetQueryable(predicate, orderBy, include, enableTracking: true, enableFilters: true).Select(selector).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<TEntity>> FindManyAsync(
            Expression<Func<TEntity, bool>> predicate,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
            params Expression<Func<TEntity, object>>[]? include)
        {
            return await GetQueryable(predicate, orderBy, include, enableTracking: true, enableFilters: true).ToArrayAsync();
        }

        public async Task<IEnumerable<TResult>> FindManyAsync<TResult>(
            Expression<Func<TEntity, TResult>> selector,
            Expression<Func<TEntity, bool>> predicate,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
            params Expression<Func<TEntity, object>>[]? include)
        {
            return await GetQueryable(predicate, orderBy, include, enableTracking: true, enableFilters: true).Select(selector).ToArrayAsync();
        }

        public async Task<IPageable<TEntity>> FindManyAsync(int pageNumber, int pageSize,
            Expression<Func<TEntity, bool>>? predicate = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
            params Expression<Func<TEntity, object>>[]? include)
        {
            return await GetQueryable(predicate, orderBy, include, enableTracking: false, enableFilters: true).PaginateAsync(pageNumber, pageSize);
        }

        public async Task<IPageable<TResult>> FindManyAsync<TResult>(int pageNumber, int pageSize,
            Expression<Func<TEntity, TResult>> selector,
            Expression<Func<TEntity, bool>>? predicate = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
            params Expression<Func<TEntity, object>>[]? include)
        {
            return await GetQueryable(predicate, orderBy, include, enableTracking: false, enableFilters: true).Select(selector).PaginateAsync(pageNumber, pageSize);
        }

        public IQueryable<TEntity> GetQueryable(
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



        private IQueryable<TEntity> EvaluateInclude(IQueryable<TEntity> current, Expression<Func<TEntity, object>> item)
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

        public Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate)
        {
            IQueryable<TEntity> query = _dbContext.Set<TEntity>();

            if (predicate != null)
                query = query.Where(predicate);

            return query.AnyAsync();
        }

        public Task<bool> AnyAsync()
        {
            IQueryable<TEntity> query = _dbContext.Set<TEntity>();
            return query.AnyAsync();
        }

        public Task<long> CountAsync(Expression<Func<TEntity, bool>> predicate)
        {
            IQueryable<TEntity> query = _dbContext.Set<TEntity>();

            if (predicate != null)
                query = query.Where(predicate);

            return query.LongCountAsync();
        }


        public Task<long> CountAsync()
        {
            IQueryable<TEntity> query = _dbContext.Set<TEntity>();
            return query.LongCountAsync();
        }
    }
}