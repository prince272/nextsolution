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
        Task CreateAsync(TEntity entity, CancellationToken cancellationToken = default);

        Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);

        Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default);

        Task<TEntity?> FindByIdAsync(long id, CancellationToken cancellationToken = default);

         Task<TEntity?> FindAsync(
                    Expression<Func<TEntity, bool>> predicate,
                    Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
                    Expression<Func<TEntity, object>>[]? include = null, CancellationToken cancellationToken = default);

        Task<TResult?> FindAsync<TResult>(
            Expression<Func<TEntity, TResult>> selector,
            Expression<Func<TEntity, bool>> predicate,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
            Expression<Func<TEntity, object>>[]? include = null, CancellationToken cancellationToken = default);

        Task<IEnumerable<TEntity>> FindManyAsync(
                    Expression<Func<TEntity, bool>> predicate,
                    Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
                    Expression<Func<TEntity, object>>[]? include = null, CancellationToken cancellationToken = default);

        Task<IEnumerable<TResult>> FindManyAsync<TResult>(
                    Expression<Func<TEntity, TResult>> selector,
                    Expression<Func<TEntity, bool>> predicate,
                    Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
                    Expression<Func<TEntity, object>>[]? include = null, CancellationToken cancellationToken = default);

        Task<IPageable<TEntity>> FindManyAsync(int pageNumber, int pageSize,
                    Expression<Func<TEntity, bool>>? predicate = null,
                    Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
                    Expression<Func<TEntity, object>>[]? include = null, CancellationToken cancellationToken = default);

        Task<IPageable<TResult>> FindManyAsync<TResult>(int pageNumber, int pageSize,
                    Expression<Func<TEntity, TResult>> selector,
                    Expression<Func<TEntity, bool>>? predicate = null,
                    Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
                    Expression<Func<TEntity, object>>[]? include = null, CancellationToken cancellationToken = default);

        Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

        Task<bool> AnyAsync(CancellationToken cancellationToken = default);

        Task<long> CountAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

        Task<long> CountAsync(CancellationToken cancellationToken = default);
    }
}
