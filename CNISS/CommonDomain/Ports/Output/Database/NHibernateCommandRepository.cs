﻿using CNISS.CommonDomain.Domain;
using NHibernate;


namespace CNISS.CommonDomain.Ports.Output.Database
{
    public class NHibernateCommandRepository<T,TKey>:IRepositoryCommands<T>
    {
        protected readonly ISession _session;
        public NHibernateCommandRepository(ISession session )
        {
            _session = session;
        }
        public void save(T entity)
        {
            _session.Save(entity);
        }

        public void delete(T entity)
        {
            _session.Delete(entity);
        }

        public void update(T entity)
        {
            _session.Update(entity);
        }
    }
}