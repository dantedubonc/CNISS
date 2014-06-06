﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CNISS.CommonDomain.Domain;

namespace CNISS.CommonDomain.Application
{
    public abstract class CommandUpdateIdentity<T>:ICommandUpdateIdentity<T>
    {
        protected IRepositoryCommands<T> _repository;
        protected Func<IUnitOfWork> _factory;

        protected CommandUpdateIdentity(IRepositoryCommands<T> repository, Func<IUnitOfWork> unitOfWork)
        {
            _repository = repository;
            _factory = unitOfWork;
        }
        public void execute(T identity)
        {
            var _uow = _factory();
            using (_uow)
            {
                _repository.update(identity);
                _uow.commit();
            }
        }
    }
}