using System;
using System.Linq;
using AirService.Data.Contracts;
using AirService.Model;
using AirService.Services.Contracts;

namespace AirService.Services
{
    public abstract class SimpleService<T> : IService<T> where T : SimpleModel
    {
        protected IRepository<T> Repository;

        public virtual IQueryable<T> FindAllByUser(Guid userId)
        {
            return null;
        }

        #region IService<T> Members

        public virtual IQueryable<T> FindAll()
        {
            return FindAll(true);
        }

        public virtual IQueryable<T> FindAll(bool activeOnly)
        {
            var results = Repository.FindAll();
            if (activeOnly)
            {
                results = results.Where(e => e.Status == SimpleModel.StatusActive);
            }
            return results;
        }

        public virtual IQueryable<T> FindAllIncluding(params System.Linq.Expressions.Expression<Func<T, object>>[] includeProperties)
        {
            return this.FindAllIncluding(true, includeProperties);
        }

        public virtual IQueryable<T> FindAllIncluding(bool activeOnly, params System.Linq.Expressions.Expression<Func<T, object>>[] includeProperties)
        {
            var results = Repository.FindAllIncluding(includeProperties);
            if (activeOnly)
            {
                results = results.Where(e => e.Status == SimpleModel.StatusActive);
            }
            return results;
        }

        public virtual T Find(int id)
        {
            return Repository.Find(id);
        }

        public virtual void InsertOrUpdate(T entity)
        {
            entity.UpdateDate = DateTime.Now;
            if (entity.Id == default(int))
            {
                // fresh entity
                entity.CreateDate = DateTime.Now;
                entity.Status = SimpleModel.StatusActive;
                Repository.Insert(entity);
            }
            else
            {
                // previous entity
                Repository.Update(entity);
            }
        }

        public virtual void Delete(int id)
        {
            var entity = Repository.Find(id);
            entity.Status = SimpleModel.StatusDeleted;
            Repository.Update(entity);
        }

        public virtual void Save()
        {
            Repository.Save();
        }

        public IService<T> Clone(bool withNewContext)
        {
            var cloned = (SimpleService<T>) this.MemberwiseClone();
            if (withNewContext)
            {
                var newContext = this.Repository.Context.Clone();
                cloned.OnClone(newContext);
            }

            return cloned;
        }

        public IService<T> Clone(IAirServiceContext withContext)
        {
            var cloned = (SimpleService<T>)this.MemberwiseClone();
            cloned.OnClone(withContext);
            return cloned;
        }

        protected virtual void OnClone(IAirServiceContext context)
        {
            this.Repository = this.Repository.Clone(context);
        }

        #endregion

		}

}