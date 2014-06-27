﻿using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CNISS.CommonDomain.Application;
using CNISS.CommonDomain.Domain;
using CNISS.EnterpriseDomain.Domain.Entities;
using CNISS.EnterpriseDomain.Domain.Repositories;
using NHibernate.Hql.Ast.ANTLR.Tree;

namespace CNISS.EnterpriseDomain.Application
{
    public class CommandInsertEmpleo:CommandInsertIdentity<Empleo>
    {
        private readonly IEmpleoRepositoryReadOnly _repositoryRead;
        private readonly IBeneficiarioRepositoryReadOnly _beneficiarioRepositoryRead;
        private readonly IProvideAllowedDaysForNewEmpleo _providerDays;
        private readonly IEmpresaRepositoryReadOnly _empresaRepositoryRead;
        private readonly ITipoDeEmpleoReadOnlyRepository _tipoDeEmpleoReadOnlyRepository;

        public CommandInsertEmpleo(IEmpleoRepositoryReadOnly repositoryRead, 
            IBeneficiarioRepositoryReadOnly beneficiarioRepositoryRead,
            IProvideAllowedDaysForNewEmpleo providerDays,  
            IEmpresaRepositoryReadOnly empresaRepositoryRead,       
            IRepositoryCommands<Empleo> repository,
            ITipoDeEmpleoReadOnlyRepository tipoDeEmpleoReadOnlyRepository,
            Func<IUnitOfWork> unitOfWork
            
            ) : base(repository, unitOfWork)
        {
            _repositoryRead = repositoryRead;
            _beneficiarioRepositoryRead = beneficiarioRepositoryRead;
            _providerDays = providerDays;
            _empresaRepositoryRead = empresaRepositoryRead;
            _tipoDeEmpleoReadOnlyRepository = tipoDeEmpleoReadOnlyRepository;
        }

      

        public bool isExecutable(Empleo identity)
        {
            var days = _providerDays.getDays();
            var empleoReciente = !_repositoryRead.existsEmpleoRecienteParaBeneficiario(identity.fechaDeInicio, days,
                identity.beneficiario.Id);
            var beneficiarioExiste = _beneficiarioRepositoryRead.exists(identity.beneficiario.Id);
            var empresaExiste = _empresaRepositoryRead.exists(identity.empresa.Id);
            var tipoEmpleoExiste = _tipoDeEmpleoReadOnlyRepository.exists(identity.tipoEmpleo.Id);


            return empleoReciente && beneficiarioExiste && empresaExiste && tipoEmpleoExiste;

        }
    }
}