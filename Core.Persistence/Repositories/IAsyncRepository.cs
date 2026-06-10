using Core.Persistence.Dynamic;
using Core.Persistence.Paging;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace Core.Persistence.Repositories
{
    public interface IAsyncRepository<T> where T : class, new()
    {
        Task<List<T>> GetList(Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null, Expression<Func<T, bool>>? predicate = null, CancellationToken cancellationToken = default);
        Task<T> Get(Expression<Func<T, bool>> predicate, Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null, Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null, bool withDeleted = true, CancellationToken cancellationToken = default);
        Task Add(T entity, CancellationToken cancellationToken = default);
        Task Update(T entity, CancellationToken cancellationToken = default);
        Task Delete(T entity, CancellationToken cancellationToken = default);
        Task<Paginate<T>> GetListWithPaginate(Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null, Expression<Func<T, bool>>? predicate = null, int index = 0, int size = 10, bool withDeleted = false, CancellationToken cancellationToken = default);
        Task<Paginate<T>> GetListByDynamic(DynamicQuery? dynamic,
                                           Expression<Func<T, bool>>? predicate = null,
                                           Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null,
                                           int index = 0,
                                           int size = 10,
                                           bool withDeleted = false,
                                           CancellationToken cancellationToken = default);
        Task<bool> Any(Expression<Func<T, bool>>? predicate = null, CancellationToken cancellationToken = default);
        IQueryable<T> Query();
    }
}