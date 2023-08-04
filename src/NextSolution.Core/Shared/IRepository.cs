using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace NextSolution.Core.Shared
{
    public interface IRepository<TEntity> where TEntity : class, IEntity
    {
        Task CreateAsync(TEntity entity);

        Task UpdateAsync(TEntity entity);

        Task DeleteAsync(TEntity entity);

        Task<TEntity?> FindByIdAsync(long id);

         Task<TEntity?> FindAsync(
                    Expression<Func<TEntity, bool>> predicate,
                    Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
                    params Expression<Func<TEntity, object>>[]? include);

        Task<TResult?> FindAsync<TResult>(
            Expression<Func<TEntity, TResult>> selector,
            Expression<Func<TEntity, bool>> predicate,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
            params Expression<Func<TEntity, object>>[]? include);

        Task<IEnumerable<TEntity>> FindManyAsync(
                    Expression<Func<TEntity, bool>> predicate,
                    Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
                    params Expression<Func<TEntity, object>>[]? include);

        Task<IEnumerable<TResult>> FindManyAsync<TResult>(
                    Expression<Func<TEntity, TResult>> selector,
                    Expression<Func<TEntity, bool>> predicate,
                    Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
                    params Expression<Func<TEntity, object>>[]? include);

        Task<IPageable<TEntity>> FindManyAsync(int pageNumber, int pageSize,
                    Expression<Func<TEntity, bool>>? predicate = null,
                    Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
                    params Expression<Func<TEntity, object>>[]? include);

        Task<IPageable<TResult>> FindManyAsync<TResult>(int pageNumber, int pageSize,
                    Expression<Func<TEntity, TResult>> selector,
                    Expression<Func<TEntity, bool>>? predicate = null,
                    Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
                    params Expression<Func<TEntity, object>>[]? include);

        Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate);

        Task<bool> AnyAsync();

        Task<long> CountAsync(Expression<Func<TEntity, bool>> predicate);

        Task<long> CountAsync();
    }
}
