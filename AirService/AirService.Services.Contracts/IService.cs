using System;
using System.Linq;
using System.Linq.Expressions;
using AirService.Data.Contracts;

namespace AirService.Services.Contracts
{
    public interface IService<T> where T: class 
    {
        IQueryable<T> FindAll();
        IQueryable<T> FindAllIncluding(params Expression<Func<T, object>>[] includeProperties);
        IQueryable<T> FindAllByUser(Guid userId);
        T Find(int id);
        void InsertOrUpdate(T entity);
        void Delete(int id);
        void Save();
        IService<T> Clone(bool withNewContext);
        IService<T> Clone(IAirServiceContext withContext);
    }
}