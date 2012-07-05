using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Data.EntityClient;
using System.Data.Objects;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using AirService.Data.Contracts;

namespace AirService.Data
{
    public class SimpleRepository<T> : IRepository<T> where T: class
    { 
        public SimpleRepository(IAirServiceContext context)
        {
            Context = context;
        }

        public IAirServiceContext Context
        {
            get; 
            private set; 
        }
        
        #region IRepository<T> Members
        
        public IQueryable<T> FindAll()
        {
            return Context.Set<T>();
        }

        public IQueryable<T> FindAllIncluding(params System.Linq.Expressions.Expression<Func<T, object>>[] includeProperties)
        {
            var query = Context.Set<T>().AsQueryable();
            foreach (var includeProperty in includeProperties)
            {
                query = query.Include(includeProperty);
            }
            return query;
        }

        public T Find(int id)
        {
            return Context.Set<T>().Find(id);
        }

        public void Insert<TOrTAssociate>(TOrTAssociate entity) where TOrTAssociate : class
        {
            this.Context.Set<TOrTAssociate>().Add(entity);
        }

        public void Update<TOrTAssociate>(TOrTAssociate entity) where TOrTAssociate : class
        {
            this.Context.Set<TOrTAssociate>().Attach(entity);
            this.Context.SetState(entity, EntityState.Modified);
        }

        public void Delete(int id)
        {
            var entity = Context.Set<T>().Find(id);
            Context.Set<T>().Remove(entity);
        }

        public virtual void Save()
        {
            try
            {
                this.Context.SaveChanges();
            }
            catch (DbEntityValidationException exception)
            {
                foreach (var validationErrors in exception.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        Trace.TraceInformation("Property: {0} Error: {1}",
                                               validationError.PropertyName,
                                               validationError.ErrorMessage);
                    }
                }

                throw;
            }
            catch (Exception exception)
            {
                while (exception != null)
                {
                    Trace.TraceInformation(exception.Message);
                    exception = exception.InnerException;
                }

                throw;
            }
        }

        void IRepository<T>.SetContextOption(ContextOptions option, bool enabled)
        { 
            Context.SetOption(option, enabled);
        }

        public IDbSet<TAssociate> Set<TAssociate>() where TAssociate: class
        {
            return Context.Set<TAssociate>();
        }

        int IRepository<T>.ExecuteCommand(string commandText,
                                          params object[] parameters)
        {
            return this.Context.ExecuteQuery(commandText, parameters);
        }

        DataSet IRepository<T>.ExecuteDataSet(string commandText, CommandType commandType, Dictionary<string, object> parameters = null)
        {
            var objContext = this.Context.ObjectContext;
            var connection = objContext.Connection as EntityConnection;
            if(connection.State != ConnectionState.Open)
            {
                connection.Open();
            }

            var command = connection.StoreConnection.CreateCommand();
            command.CommandText = commandText;
            command.CommandType = commandType;

            if (parameters != null)
            {
                foreach (var keyPair in parameters)
                {
                    var param = command.CreateParameter();
                    param.ParameterName = keyPair.Key;
                    param.Value = keyPair.Value;
                    command.Parameters.Add(param);
                }
            }

            var dataSet = new DataSet();
            var table = new DataTable("Table0");
            dataSet.Tables.Add(table);
            dataSet.EnforceConstraints = false;
            using (var reader = command.ExecuteReader())
            {
                var fieldCount = reader.FieldCount;
                for (int i = 0; i < fieldCount; i++)
                {
                    table.Columns.Add(reader.GetName(i), reader.GetFieldType(i));
                }

                while (reader.Read())
                {
                    var row = table.NewRow();
                    for (int i = 0; i < fieldCount; i++)
                    {
                        if (!reader.IsDBNull(i))
                        {
                            row[i] = reader[i];
                        }
                    }

                    table.Rows.Add(row);
                }
            }

            return dataSet;
        }

        public virtual IRepository<T> Clone(IAirServiceContext withContext)
        {
            var cloned = (SimpleRepository<T>) this.MemberwiseClone();
            cloned.Context = withContext;
            return cloned;
        } 

        #endregion
    }
}