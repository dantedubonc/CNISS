using System.Linq;
using CNISS.CommonDomain.Ports.Output.Database;
using CNISS.EnterpriseDomain.Domain;
using CNISS.EnterpriseDomain.Domain.Entities;
using CNISS.EnterpriseDomain.Domain.Repositories;
using NHibernate;

namespace CNISS.EnterpriseDomain.Ports.Output
{
    public class EmpresaRepositoryCommands : NHibernateCommandRepository<Empresa, RTN>, IEmpresaRepositoryCommands
    {
        private readonly IGremioRepositoryReadOnly _repositoryReadGremio;

        public EmpresaRepositoryCommands(ISession session, IGremioRepositoryReadOnly repositoryReadGremio
            ) : base(session)
        {
            this._repositoryReadGremio = repositoryReadGremio;
        }

        public void save(Empresa entity)
        {
            var sucursales = entity.sucursales;

          //  entity.gremial = getGremio(entity.gremial.Id);
            base.save(entity);
            sucursales.ToList().ForEach(saveSucursal);

            


        }

        private Gremio getGremio(RTN rtnGremio)
        {
            return _repositoryReadGremio.get(rtnGremio);
        }

        private void saveSucursal(Sucursal sucursal)
        {
            var direccion = sucursal.direccion;
            var firma = sucursal.firma;
            _session.Save(direccion);
            _session.Save(firma);
            _session.Save(sucursal);
        }



       


    }
}