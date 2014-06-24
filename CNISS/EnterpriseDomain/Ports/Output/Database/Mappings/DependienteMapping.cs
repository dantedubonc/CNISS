﻿using CNISS.EnterpriseDomain.Domain.Entities;
using FluentNHibernate.Mapping;

namespace CNISS.EnterpriseDomain.Ports.Output.Database.Mappings
{
    public class DependienteMapping:ClassMap<Dependiente>
    {
        public DependienteMapping()
        {
            CompositeId()
                .ComponentCompositeIdentifier(x => x.Id)
                .KeyProperty(x => x.Id.identidad);

            Component(x => x.nombre, z =>
            {
                z.Map(x => x.primerNombre);
                z.Map(x => x.primerApellido);
                z.Map(x => x.segundoApellido);
                z.Map(x => x.segundoNombre);
            }
                );

            Map(x => x.edad);

            References(x => x.parentesco);

        }
    }
}