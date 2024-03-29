﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using CNISS.AutenticationDomain.Domain.Entities;
using CNISS.AutenticationDomain.Domain.ValueObjects;
using CNISS.CommonDomain.Ports.Output.Database;
using CNISS.EnterpriseDomain.Domain;
using CNISS.EnterpriseDomain.Domain.Entities;
using CNISS.EnterpriseDomain.Domain.Repositories;
using CNISS.EnterpriseDomain.Domain.ValueObjects;
using CNISS.EnterpriseDomain.Ports.Output;
using CNISS.EnterpriseDomain.Ports.Output.Database;
using CNISS_Integration_Test.Unit_Of_Work;
using FluentAssertions;
using Machine.Specifications;
using NHibernate;

namespace CNISS_Integration_Test.Repositories.EmpresasRepository.Command
{
    [Subject(typeof(EmpresaRepositoryCommands))]
    public class when_UserUpdatesValidEmpresaNuevaSucursal_Should_UpdateEmpresa
    {
        static InFileDataBaseTest _dataBaseTest;
        static ISessionFactory _sessionFactory;
        static ISession _session;
        private static IEmpresaRepositoryCommands _repositoryCommands;
        private static Empresa _expectedEmpresa;
        private static Empresa _responseEmpresa;
        private static Sucursal _nuevaSucursal;

        private Establish context = () =>
        {
            _dataBaseTest = new InFileDataBaseTest();
            _sessionFactory = _dataBaseTest.sessionFactory;
            _expectedEmpresa = getEmpresa();

            prepareDependenciesInDataBase(_expectedEmpresa);

            _nuevaSucursal = getNuevaSucursal();

            prepareUser(_nuevaSucursal.Firma.User);

         
        

            using (var uow = new NHibernateUnitOfWork(_sessionFactory.OpenSession()))
            {
                var gremioRepository = new GremioRepositoryReadOnly(uow.Session);
                _repositoryCommands = new EmpresaRepositoryCommands(uow.Session, gremioRepository);
                _repositoryCommands.save(_expectedEmpresa);
                uow.commit();
            }


        };

        private Because of = () =>
        {
            _expectedEmpresa.AddSucursal(_nuevaSucursal);

            using (var uow = new NHibernateUnitOfWork(_sessionFactory.OpenSession()))
            {

                var us = uow.Session.Get<User>(_nuevaSucursal.Firma.User.Id);


                var gremioRepository = new GremioRepositoryReadOnly(uow.Session);
                _repositoryCommands = new EmpresaRepositoryCommands(uow.Session, gremioRepository);
                _repositoryCommands.update(_expectedEmpresa);
                uow.commit();
            }


        };

        It should_update_empresa = () =>
        {
            _session = _sessionFactory.OpenSession();

            using (var tx = _session.BeginTransaction())
            {
                _responseEmpresa = _session.Get<Empresa>(_expectedEmpresa.Id);

                _responseEmpresa.Id.ShouldBeEquivalentTo(_expectedEmpresa.Id);
            }
        };

        #region Helper Methods

        private static void prepareDependenciesInDataBase(Empresa empresa)
        {
            var gremio = empresa.Gremial;
            var sucursales = empresa.Sucursales;
            var actividades = empresa.ActividadesEconomicas;
            prepareGremio(gremio);
            sucursales.ToList().ForEach(x => prepareUser(x.Firma.User));
            actividades.ToList().ForEach(prepareActividades);

        }

        private static void prepareActividades(ActividadEconomica actividadEconomica)
        {
            _session = _sessionFactory.OpenSession();
            using (var tx = _session.BeginTransaction())
            {
                _session.Save(actividadEconomica);
                tx.Commit();
            }
            _session.Close();
        }

        private static void prepareGremio(Gremio gremio)
        {
            var direccion = gremio.Direccion;
            saveDepartamentoMunicipio(direccion.Departamento, direccion.Municipio);

            using (var uow = new NHibernateUnitOfWork(_sessionFactory.OpenSession()))
            {
                var representanteRepository = new RepresentanteLegalRepositoryReadOnly(uow.Session);
                var direccionRepository = new DireccionRepositoryReadOnly(uow.Session);


                var gremioRepository = new GremioRepositoryCommands(uow.Session, representanteRepository,
                    direccionRepository);
                gremioRepository.save(gremio);
                uow.commit();
            }

        }


        private static void saveDepartamentoMunicipio(Departamento departamento, Municipio municipio)
        {
            _session = _sessionFactory.OpenSession();
            using (var tx = _session.BeginTransaction())
            {
                _session.SaveOrUpdate(departamento);
                _session.SaveOrUpdate(municipio);
                tx.Commit();
            }
            _session.Close();
        }

        private static void prepareUser(User user)
        {
            var rol = user.UserRol;
            _session = _sessionFactory.OpenSession();
            using (var tx = _session.BeginTransaction())
            {
                _session.SaveOrUpdate(rol);
                _session.SaveOrUpdate(user);
                tx.Commit();
            }
            _session.Close();
        }

        private static Empresa getEmpresa()
        {
            var actividades = getActividadEconomicas();
            var sucursales = getSucursales();
            var gremio = getGremio();
            var fechaIngreso = DateTime.ParseExact(DateTime.Now.ToString("g"), "g", null);
            var rtn = new RTN("08011985123960");
            var empresa = new Empresa(rtn, "La Holgazana", fechaIngreso, gremio);
            empresa.ActividadesEconomicas = actividades;
            empresa.Sucursales = sucursales;
            empresa.Contrato = getContrato();
            return empresa;
        }

        private static IEnumerable<ActividadEconomica> getActividadEconomicas()
        {
            return new List<ActividadEconomica>()
            {
                new ActividadEconomica("Camaronera"),
                new ActividadEconomica("Arrocera")
            };
        }

        private static Gremio getGremio()
        {
            var municipio = new Municipio("01", "01", "Municipio");
            var departamento = new Departamento() { Id = "01", Municipios = new List<Municipio>() { municipio }, Nombre = "Departamento" };
            var direccion = new Direccion(departamento, municipio, "direccion gremio");

            var RTN = new RTN("08011985123960");
            var representante = new RepresentanteLegal(new Identidad("0801198512396"), "Dante");

            var gremio = new Gremio(RTN, representante, direccion, "Camara");
            return gremio;

        }

        private static IList<Sucursal> getSucursales()
        {
            var municipio = new Municipio("01", "01", "Municipio");
            var departamento = new Departamento() { Id = "01", Municipios = new List<Municipio>() { municipio }, Nombre = "Departamento" };
            var direccion = new Direccion(departamento, municipio, "direccion");
            var fechaDeCreacionFirma = DateTime.ParseExact(DateTime.Now.ToString("g"), "g", null);

            var firma1 = new FirmaAutorizada(new User("DRCD", "Dante", "Ruben", "SDSD", "as", new Rol("rol", "rol")), fechaDeCreacionFirma);
            var firma2 = new FirmaAutorizada(new User("Angela", "Angela", "Castillo", "SSS", "SS", new Rol("rol", "rol")), fechaDeCreacionFirma);

            var sucursal1 = new Sucursal("El Centro", direccion, firma1);
            var sucursal2 = new Sucursal("Barrio Abajo", direccion, firma2);


            return new List<Sucursal>() { sucursal1, sucursal2 };

        }

        private static Sucursal getNuevaSucursal()
        {
            var municipio = new Municipio("01", "01", "Municipio");
            var departamento = new Departamento() { Id = "01", Municipios = new List<Municipio>() { municipio }, Nombre = "Departamento" };
            var direccion = new Direccion(departamento, municipio, "direccion nueva");
            var fechaDeCreacionFirma = DateTime.ParseExact(DateTime.Now.ToString("g"), "g", null);
            var firma = new FirmaAutorizada(new User("ARVP", "Angela", "Castillo", "SDSD", "as", new Rol("rol esposa", "rol esposa")), fechaDeCreacionFirma);
            return new Sucursal("El Coyolito",direccion,firma);
        }

        private static ContentFile getContrato()
        {
            var data = new byte[5];
            return new ContentFile(data);
        }
        #endregion
    }
}
