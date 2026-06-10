using Core.Persistence.Dynamic;
using Core.Persistence.Paging;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace Core.Persistence.Repositories
{
    public class EfRepositoryBase<TEntity, TContext> : IAsyncRepository<TEntity>
    where TEntity : class, IEntity, new()
    where TContext : DbContext
    {
        protected readonly TContext Context;

        public EfRepositoryBase(TContext context)
        {
            Context = context;
        }

        public async Task Add(TEntity entity, CancellationToken cancellationToken = default)
        {
            await Context.AddAsync(entity);
            await Context.SaveChangesAsync(cancellationToken);
        }

        public async Task Delete(TEntity entity, CancellationToken cancellationToken = default)
        {
            Context.Remove(entity);
            await Context.SaveChangesAsync(cancellationToken);
        }

        public async Task<TEntity> Get(Expression<Func<TEntity, bool>> predicate, Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null, bool withDeleted = true, CancellationToken cancellationToken = default)
        {
            IQueryable<TEntity> queryable = Query();

            if (include != null)
                queryable = include(queryable);
            if (withDeleted)
                queryable = queryable.IgnoreQueryFilters();
            if (orderBy != null)
                queryable = orderBy(queryable);

            return await queryable.FirstOrDefaultAsync(predicate, cancellationToken);
        }

        public async Task<List<TEntity>> GetList(Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null, Expression<Func<TEntity, bool>>? predicate = null, CancellationToken cancellationToken = default)
        {
            IQueryable<TEntity> queryable = Query();

            if (include != null)
                queryable = include(queryable);

            return predicate == null
                ? await queryable.ToListAsync(cancellationToken: cancellationToken)
                : await queryable.Where(predicate).ToListAsync(cancellationToken: cancellationToken);
        }

        public async Task Update(TEntity entity, CancellationToken cancellationToken = default)
        {
            //Context.ChangeTracker.Clear();
            Context.Update(entity);
            await Context.SaveChangesAsync(cancellationToken);
        }

        public IQueryable<TEntity> Query() => Context.Set<TEntity>();

        public async Task<Paginate<TEntity>> GetListWithPaginate(Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null, Expression<Func<TEntity, bool>>? predicate = null, int index = 0, int size = 10, bool withDeleted = false, CancellationToken cancellationToken = default)
        {
            IQueryable<TEntity> queryable = Query();

            if (include != null)
                queryable = include(queryable);
            if (withDeleted)
                queryable = queryable.IgnoreQueryFilters();
            if (predicate != null)
                queryable = queryable.Where(predicate);

            return await queryable.ToPaginateAsync(index, size, cancellationToken);
        }

        public async Task<Paginate<TEntity>> GetListByDynamic(DynamicQuery? dynamic = null, Expression<Func<TEntity, bool>>? predicate = null, Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null, int index = 0, int size = 10, bool withDeleted = false, CancellationToken cancellationToken = default)
        {
            IQueryable<TEntity> queryable = Query();
            if (dynamic != null)
                queryable = queryable.ToDynamic(dynamic);
            if (include != null)
                queryable = include(queryable);
            if (withDeleted)
                queryable = queryable.IgnoreQueryFilters();
            if (predicate != null)
                queryable = queryable.Where(predicate);
            return await queryable.ToPaginateAsync(index, size, cancellationToken);
        }

        public async Task<bool> Any(Expression<Func<TEntity, bool>>? predicate = null, CancellationToken cancellationToken = default)
        {
            IQueryable<TEntity> queryable = Query();

            if (predicate != null)
            {
                queryable = queryable.Where(predicate);
            }

            return await queryable.AnyAsync(cancellationToken);
        }
    }
}
