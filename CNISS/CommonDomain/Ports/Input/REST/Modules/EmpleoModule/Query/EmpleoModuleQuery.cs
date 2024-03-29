﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Web;
using CNISS.CommonDomain.Ports.Input.REST.Request.AuditoriaRequest;
using CNISS.CommonDomain.Ports.Input.REST.Request.BeneficiarioRequest;
using CNISS.CommonDomain.Ports.Input.REST.Request.EmpleoRequest;
using CNISS.CommonDomain.Ports.Input.REST.Request.EmpresaRequest;
using CNISS.CommonDomain.Ports.Input.REST.Request.GremioRequest;
using CNISS.CommonDomain.Ports.Input.REST.Request.MotivoDespidoRequest;
using CNISS.CommonDomain.Ports.Input.REST.Request.UserRequest;
using CNISS.CommonDomain.Ports.Input.REST.Request.VisitaRequest;
using CNISS.EnterpriseDomain.Domain;
using CNISS.EnterpriseDomain.Domain.Entities;
using CNISS.EnterpriseDomain.Domain.Repositories;
using CNISS.EnterpriseDomain.Domain.ValueObjects;
using Nancy;
using Nancy.ModelBinding;
using Nancy.Security;
using NUnit.Framework.Constraints;

namespace CNISS.CommonDomain.Ports.Input.REST.Modules.EmpleoModule.Query
{
    public class EmpleoModuleQuery:NancyModule
    {

        public EmpleoModuleQuery(IEmpleoRepositoryReadOnly repositoryRead)
        {
            Get["/enterprise/empleos"] = parameters =>
            {
                var empleos = repositoryRead.getAll();
                return Response.AsJson(getEmpleosRequests(empleos));

            };

            Get["/enterprise/empleos/id={id}"] = parameters =>
            {
                var id = parameters.id;

                Guid idRequest;
                if (Guid.TryParse(id, out idRequest))
                {
                    if (Guid.Empty != idRequest)
                    {
                        var empleo = repositoryRead.get(idRequest);

                        return Response.AsJson(getEmpleoRequest(empleo));
                    }
                 
                }

                return new Response()
                    .WithStatusCode(HttpStatusCode.BadRequest);
            };


            Get["/enterprise/empleos/empresa/id={rtn}"] = parameters =>
            {
                var rtnRequest = new RTNRequest() {RTN = parameters.rtn};
                if (rtnRequest.isValidPost())
                {
                    var rtn = new RTN(rtnRequest.RTN);
                    if (rtn.isRTNValid())
                    {
                        var empleos = repositoryRead.getEmpleosByEmpresa(rtn);
                        return Response.AsJson(getEmpleosRequests(empleos));
                    }
                  
                }
                return new Response()
                    .WithStatusCode(HttpStatusCode.BadRequest);
            };

            Get["/enterprise/empleos/beneficiario/id={identidad}"] = parameters =>
            {
                var identidadRequest = new IdentidadRequest() {identidad = parameters.identidad};
                if (identidadRequest.isValidPost())
                {
                    var identidad = new Identidad(identidadRequest.identidad);
                    var empleos = repositoryRead.getEmpleosByBeneficiario(identidad);
                    return Response.AsJson(getEmpleosRequests(empleos));

                }
                return new Response()
               .WithStatusCode(HttpStatusCode.BadRequest);
            };

           
        }
        private  IEnumerable<EmpleoRequest> getEmpleosRequests(IEnumerable<Empleo> empleos)
        {
            return empleos.Select(getEmpleoRequest);
        }

        private IEnumerable<DependienteRequest> getDependienteRequests(IEnumerable<Dependiente> dependientes)
        {
            var dependientesRequest = new List<DependienteRequest>();
            if (dependientes != null)
            {
                dependientesRequest = dependientes.Select(x => new DependienteRequest()
                {
                    IdGuid = x.idGuid,
                    identidadRequest = new IdentidadRequest() { identidad = x.Id.identidad},
                    fechaNacimiento = x.FechaNacimiento,
                    nombreRequest = new NombreRequest() { 
                        nombres = x.Nombre.Nombres,
                        primerApellido = x.Nombre.PrimerApellido,
                        segundoApellido = x.Nombre.SegundoApellido
                    },
                    parentescoRequest = new ParentescoRequest()
                    {
                        descripcion = x.Parentesco.Descripcion,
                        guid = x.Parentesco.Id
                    },
                    auditoriaRequest = new AuditoriaRequest()
                    {
                        fechaCreo = x.auditoria.FechaCreacion,
                        fechaModifico = x.auditoria.FechaActualizacion,
                        usuarioCreo = x.auditoria.CreadoPor,
                        usuarioModifico = x.auditoria.ActualizadoPor
                    }
                }).ToList();
            }

            return dependientesRequest;
        }

        private DireccionRequest getDireccionRequest(Beneficiario beneficiario)
        {
            var direccion = beneficiario.Direccion;
            if (direccion == null)
            {
                return new DireccionRequest();
            }
            var departamentoRequest = new DepartamentoRequest()
            {
                idDepartamento = direccion.Departamento.Id,
                nombre = direccion.Departamento.Nombre
            };
            var municipioRequest = new MunicipioRequest()
            {
                idMunicipio = direccion.Municipio.Id,
                idDepartamento = direccion.Municipio.Id,
                nombre = direccion.Municipio.Nombre
            };
            return new DireccionRequest()
            {
                departamentoRequest = departamentoRequest,
                municipioRequest = municipioRequest,
                descripcion = direccion.ReferenciaDireccion,
                IdGuid = direccion.Id
            };
        }

        private IEnumerable<FichaSupervisionEmpleoRequest> getFichaSupervisionEmpleos(
            IEnumerable<FichaSupervisionEmpleo> fichasSupervision)
        {
           
            return fichasSupervision.Select(x => new FichaSupervisionEmpleoRequest()
            {
                telefonoCelular = x.TelefonoCelular,
                telefonoFijo = x.TelefonoFijo,
                fotografiaBeneficiario = x.FotografiaBeneficiario.Id,
                cargo = x.Cargo,
                funciones = x.Funciones,
                desempeñoEmpleado = x.DesempeñoEmpleado,
                posicionGPS = x.PosicionGps,
                supervisor = new SupervisorRequest()
                {
                    guid = x.Supervisor.Id,
                    userRequest = new UserRequest()
                    {
                        Id = x.Supervisor.Usuario.Id,
                        firstName = x.Supervisor.Usuario.FirstName,
                        secondName = x.Supervisor.Usuario.SecondName,
                        mail = x.Supervisor.Usuario.Mail
                    }
   
                },
                auditoriaRequest = new AuditoriaRequest()
                {
                    fechaCreo = x.Auditoria.FechaCreacion,
                    fechaModifico = x.Auditoria.FechaActualizacion,
                    usuarioCreo = x.Auditoria.CreadoPor,
                    usuarioModifico = x.Auditoria.ActualizadoPor
                },
                firma = new FirmaAutorizadaRequest()
                {
                    IdGuid = x.Firma.Id,
                    userRequest = new UserRequest()
                    {
                        Id = x.Firma.User.Id,
                        firstName = x.Firma.User.FirstName,
                        mail = x.Firma.User.Mail,
                        secondName = x.Firma.User.SecondName
                        
                    }


                }
            }).ToList();
        }

        private NotaDespidoRequest getNotaDespidoRequest(NotaDespido notaDespido)
        {
            if (notaDespido == null)
                return null;
            return new NotaDespidoRequest()
            {
                guid = notaDespido.Id,
                fechaDespido = notaDespido.FechaDespido,
                imagenNotaDespido = notaDespido.DocumentoDespido.Id,
                firmaAutorizadaRequest = new FirmaAutorizadaRequest()
                {
                    IdGuid = notaDespido.FirmaAutorizada.Id,
                    userRequest = new UserRequest()
                    {
                        Id = notaDespido.FirmaAutorizada.User.Id,
                        firstName = notaDespido.FirmaAutorizada.User.FirstName,
                        mail = notaDespido.FirmaAutorizada.User.Mail,
                        secondName = notaDespido.FirmaAutorizada.User.SecondName
                        
                    }


                },
                auditoriaRequest = new AuditoriaRequest()
                {
                    fechaCreo = notaDespido.Auditoria.FechaCreacion,
                    fechaModifico = notaDespido.Auditoria.FechaActualizacion,
                    usuarioCreo = notaDespido.Auditoria.CreadoPor,
                    usuarioModifico = notaDespido.Auditoria.ActualizadoPor
                },
                motivoDespidoRequest = new MotivoDespidoRequest()
                {
                    IdGuid = notaDespido.MotivoDespido.Id,
                    descripcion = notaDespido.MotivoDespido.Descripcion
                },
                posicionGPS = notaDespido.PosicionGps,
                supervisorRequest = new SupervisorRequest()
                {
                    guid = notaDespido.Supervisor.Id,
                    userRequest = new UserRequest()
                    {
                        Id = notaDespido.Supervisor.Usuario.Id,
                        firstName = notaDespido.Supervisor.Usuario.FirstName,
                        secondName = notaDespido.Supervisor.Usuario.SecondName,
                        mail = notaDespido.Supervisor.Usuario.Mail
                    }

                }
            };
        }

        private  EmpleoRequest getEmpleoRequest(Empleo empleo)
        {
            return new EmpleoRequest()
            {
                beneficiarioRequest = new BeneficiarioRequest()
                {
                    identidadRequest = new IdentidadRequest() { identidad = empleo.Beneficiario.Id.identidad },
                    nombreRequest = new NombreRequest()
                    {
                        nombres = empleo.Beneficiario.Nombre.Nombres,
                        primerApellido = empleo.Beneficiario.Nombre.PrimerApellido,
                        segundoApellido = empleo.Beneficiario.Nombre.SegundoApellido
                    },
                    fechaNacimiento = empleo.Beneficiario.FechaNacimiento,
                    dependienteRequests = getDependienteRequests(empleo.Beneficiario.Dependientes),
                    telefonoCelular = empleo.Beneficiario.TelefonoCelular ?? "",
                    telefonoFijo = empleo.Beneficiario.TelefonoFijo ?? "",
                    direccionRequest = getDireccionRequest(empleo.Beneficiario)




                },
                cargo = empleo.Cargo,
                supervisado = empleo.Supervisado,
                comprobantes = empleo.ComprobantesPago.Select(z => new ComprobantePagoRequest()
                {
                    deducciones = z.Deducciones,
                    fechaPago = z.FechaPago,
                    guid = z.Id,
                    sueldoNeto = z.SueldoNeto,
                    
                    bonificaciones = z.Bonificaciones,
                    archivoComprobante =  z.ImagenComprobante== null ?"": z.ImagenComprobante.Id.ToString()
                }),
                contrato = empleo.Contrato==null?"":empleo.Contrato.Id.ToString(),
                empresaRequest = new EmpresaRequest()
                {
                    nombre = empleo.Empresa.Nombre,
                    rtnRequest = new RTNRequest() { RTN = empleo.Empresa.Id.Rtn }
                },
                sucursalRequest = new SucursalRequest()
                {
                    guid = empleo.Sucursal.Id,
                    nombre = empleo.Sucursal.Nombre,
                    firmaAutorizadaRequest = new FirmaAutorizadaRequest()
                    {
                        IdGuid = empleo.Sucursal.Firma.Id,
                        fechaCreacion = empleo.Sucursal.Firma.fechaCreacion,
                        userRequest = new UserRequest()
                        {
                            Id = empleo.Sucursal.Firma.User.Id
                        }
                    }
                    
                    
                },
                fechaDeInicio = empleo.FechaDeInicio,
                horarioLaboralRequest = new HorarioLaboralRequest()
                {
                    diasLaborablesRequest = new DiasLaborablesRequest()
                    {
                        domingo = empleo.HorarioLaboral.DiasLaborables.Domingo,
                        lunes = empleo.HorarioLaboral.DiasLaborables.Lunes,
                        martes = empleo.HorarioLaboral.DiasLaborables.Martes,
                        miercoles = empleo.HorarioLaboral.DiasLaborables.Miercoles,
                        jueves = empleo.HorarioLaboral.DiasLaborables.Jueves,
                        viernes = empleo.HorarioLaboral.DiasLaborables.Viernes,
                        sabado = empleo.HorarioLaboral.DiasLaborables.Sabado
                    },
                    horaEntrada = new HoraRequest()
                    {
                        hora = empleo.HorarioLaboral.HoraEntrada.HoraEntera,
                        minutos = empleo.HorarioLaboral.HoraEntrada.Minutos,
                        parte = empleo.HorarioLaboral.HoraEntrada.Parte

                    },
                    horaSalida = new HoraRequest()
                    {
                        hora = empleo.HorarioLaboral.HoraSalida.HoraEntera,
                        minutos = empleo.HorarioLaboral.HoraSalida.Minutos,
                        parte = empleo.HorarioLaboral.HoraSalida.Parte

                    }
                },
                sueldo = empleo.Sueldo,
                tipoEmpleoRequest = new TipoEmpleoRequest()
                {
                    descripcion = empleo.TipoEmpleo.Descripcion,
                    IdGuid = empleo.TipoEmpleo.Id
                },
                IdGuid = empleo.Id,
                notaDespidoRequest = getNotaDespidoRequest(empleo.NotaDespido),
                fichaSupervisionEmpleoRequests = getFichaSupervisionEmpleos(empleo.FichasSupervisionEmpleos),
                auditoriaRequest = new AuditoriaRequest()
                {
                    fechaCreo = empleo.Auditoria.FechaCreacion,
                    fechaModifico = empleo.Auditoria.FechaActualizacion,
                    usuarioCreo = empleo.Auditoria.CreadoPor,
                    usuarioModifico = empleo.Auditoria.ActualizadoPor
                },
                
            };

        }
    }
      
    
}