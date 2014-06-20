﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CNISS.EnterpriseDomain.Domain.ValueObjects;

namespace CNISS.EnterpriseDomain.Domain.Entities
{
    public class Empresa:EnterpriseEntities
    {
        public virtual  string nombre { get; set; }
        public virtual IEnumerable<ActividadEconomica> ActividadesEconomicas { get; set; }
        public virtual DateTime fechaIngreso { get; set; }
        public virtual int empleadosTotales { get; set; }
        public virtual bool proyectoPiloto { get; set; }
        public virtual Gremio gremial { get; set; }
        public virtual ContentFile contrato { get; set; }

    }
}