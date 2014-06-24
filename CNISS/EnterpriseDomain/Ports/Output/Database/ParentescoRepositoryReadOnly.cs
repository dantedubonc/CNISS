﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CNISS.CommonDomain.Ports.Output.Database;
using CNISS.EnterpriseDomain.Domain.Repositories;
using CNISS.EnterpriseDomain.Domain.ValueObjects;
using NHibernate;
using NHibernate.Linq;

namespace CNISS.EnterpriseDomain.Ports.Output.Database
{
    public class ParentescoRepositoryReadOnly:NHibernateReadOnlyRepository<Parentesco,Guid>,IParentescoReadOnlyRepository
    {
        public ParentescoRepositoryReadOnly(ISession session) : base(session)
        {
        }

        public bool exists(Guid id)
        {
            return (from parentesco in Session.Query<Parentesco>()
                    where parentesco.Id == id
                    select parentesco.Id).Any();
                
        }
    }
}