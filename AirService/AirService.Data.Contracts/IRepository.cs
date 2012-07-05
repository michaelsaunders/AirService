using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;

namespace AirService.Data.Contracts
{
    public interface IRepository<T> where T : class
    {
        IAirServiceContext Context { get; }

        IQueryable<T> FindAll();

        IQueryable<T> FindAllIncluding(params Expression<Func<T, object>>[] includeProperties);

        T Find(int id);

        void Insert<TOrTAssociate>(TOrTAssociate entity) where TOrTAssociate : class;

        void Update<TOrTAssociate>(TOrTAssociate entity) where TOrTAssociate : class;

        void Delete(int id);

        void Save();

        void SetContextOption(ContextOptions option, bool enabled);

        IDbSet<TAssociate> Set<TAssociate>() where TAssociate : class;

        int ExecuteCommand(string commandText, params object[] parameters);
        
        DataSet ExecuteDataSet(string commandText, CommandType commandType, Dictionary<string, object> parameters = null);

        IRepository<T> Clone(IAirServiceContext withContext);

        
    }
}
