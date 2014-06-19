﻿using System;
using CNISS.CommonDomain.Domain;
using CNISS.CommonDomain.Ports.Output.Database;
using CNISS.EnterpriseDomain.Domain;
using CNISS.EnterpriseDomain.Domain.Entities;
using CNISS.EnterpriseDomain.Domain.Repositories;
using CNISS.EnterpriseDomain.Domain.ValueObjects;
using FluentNHibernate.Conventions.AcceptanceCriteria;
using NHibernate;

namespace CNISS.EnterpriseDomain.Ports.Output
{
    public class GremioRepositoryCommands : NHibernateCommandRepository<Gremio, RTN>, IGremioRepositoryCommands
    {
        private readonly IRepresentanteLegalRepositoryReadOnly _representanteLegalRepositoryRead;
        private readonly IDireccionRepositoryReadOnly _direccionRepositoryRead;

        public GremioRepositoryCommands(ISession session, IRepresentanteLegalRepositoryReadOnly representanteLegalRepositoryRead, 
            IDireccionRepositoryReadOnly direccionRepositoryRead) : base(session)
        {
            _representanteLegalRepositoryRead = representanteLegalRepositoryRead;
            _direccionRepositoryRead = direccionRepositoryRead;
        }

        public void update(Gremio entity)
        {
            
            _session.Update(entity);

        }

        public void updateRepresentante(Gremio entity)
        {
            var representante = entity.representanteLegal;
            if (!isRepresentantExisting(representante.Id))
                _session.Save(representante);
            update(entity);
        }

        public void updateDireccion(Gremio entity)
        {
            var direccion = entity.direccion;
            if (!isDireccionExisting(direccion.Id))
            {
                _session.Save(direccion);
            }
            update(entity);

        }

        public void save(Gremio entity)
        {
            var direccion = entity.direccion;

            var representante = entity.representanteLegal;
            if (!isRepresentantExisting(representante.Id))
                _session.Save(representante);
            _session.Save(direccion);
            _session.Save(entity);
        }

        private bool isDireccionExisting(Guid id)
        {
            return _direccionRepositoryRead.exists(id);
        }

        private bool isRepresentantExisting(Identidad id)
        {
            return _representanteLegalRepositoryRead.exists(id);
        }


    }
}