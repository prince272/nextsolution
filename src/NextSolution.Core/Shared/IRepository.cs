using NextSolution.Core.Utilities;
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
        Task<TEntity> CreateAsync(TEntity entity, CancellationToken cancellationToken = default);

        Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);

        Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default);

        Task DeleteManyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

        Task DeleteAllAsync(CancellationToken cancellationToken = default);

        Task<TEntity?> GetByIdAsync(long id, CancellationToken cancellationToken = default);

        Task<TEntity?> GetAsync(
            Expression<Func<TEntity, bool>> predicate,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
            Expression<Func<TEntity, object>>[]? include = null,
            CancellationToken cancellationToken = default);

        Task<TResult?> GetAsync<TResult>(
            Expression<Func<TEntity, TResult>> selector,
            Expression<Func<TEntity, bool>> predicate,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
            Expression<Func<TEntity, object>>[]? include = null,
            CancellationToken cancellationToken = default);

        Task<IEnumerable<TEntity>> GetManyAsync(
            Expression<Func<TEntity, bool>> predicate,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
            Expression<Func<TEntity, object>>[]? include = null,
            CancellationToken cancellationToken = default);

        Task<IEnumerable<TResult>> GetManyAsync<TResult>(
            Expression<Func<TEntity, TResult>> selector,
            Expression<Func<TEntity, bool>> predicate,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
            Expression<Func<TEntity, object>>[]? include = null,
            CancellationToken cancellationToken = default);

        Task<IPageable<TEntity>> GetManyAsync(long offset, int limit,
            Expression<Func<TEntity, bool>>? predicate = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
            Expression<Func<TEntity, object>>[]? include = null,
            CancellationToken cancellationToken = default);

        Task<IPageable<TResult>> GetManyAsync<TResult>(long offset, int limit,
            Expression<Func<TEntity, TResult>> selector,
            Expression<Func<TEntity, bool>>? predicate = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
            Expression<Func<TEntity, object>>[]? include = null,
            CancellationToken cancellationToken = default);

        Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

        Task<bool> AnyAsync(CancellationToken cancellationToken = default);

        Task<long> CountAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

        Task<long> CountAsync(CancellationToken cancellationToken = default);
    }
}
