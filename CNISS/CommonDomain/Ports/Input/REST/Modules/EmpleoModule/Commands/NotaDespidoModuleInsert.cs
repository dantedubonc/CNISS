﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CNISS.AutenticationDomain.Domain.Entities;
using CNISS.AutenticationDomain.Domain.ValueObjects;
using CNISS.CommonDomain.Domain;
using CNISS.CommonDomain.Ports.Input.REST.Request.EmpleoRequest;
using CNISS.CommonDomain.Ports.Input.REST.Request.EmpresaRequest;
using CNISS.CommonDomain.Ports.Input.REST.Request.MotivoDespidoRequest;
using CNISS.CommonDomain.Ports.Input.REST.Request.VisitaRequest;
using CNISS.EnterpriseDomain.Application;
using CNISS.EnterpriseDomain.Domain.Entities;
using CNISS.EnterpriseDomain.Domain.ValueObjects;
using Nancy;
using Nancy.ModelBinding;
using Nancy.Security;

namespace CNISS.CommonDomain.Ports.Input.REST.Modules.EmpleoModule.Commands
{
    public class NotaDespidoModuleInsert:NancyModule
    {
        private const string directorioImagenes = @"/ImagenesMoviles";
        private const string extensionImagenes = ".jpeg";

        public NotaDespidoModuleInsert(ICommandInsertNotaDespido command, IFileGetter fileGetter)
        {
            Post["/movil/notaDespido"] = parameters =>
            {
                this.RequiresClaims(new[] { "movil" });
                var notaDespidoRequest = this.Bind<NotaDespidoRequest>();
                if (notaDespidoRequest.isValidPost())
                {
                    var archivoNotaDespido = notaDespidoRequest.imagenNotaDespido.ToString();
                    if (fileGetter.existsFile(directorioImagenes, archivoNotaDespido, extensionImagenes))
                    {
                        var notaDespido = getNotaDespido(notaDespidoRequest);
                        if (command.isExecutable(notaDespidoRequest.empleoId, notaDespido))
                        {
                            var dataImage = fileGetter.getFile(directorioImagenes, archivoNotaDespido, extensionImagenes);
                            var imageFile = new ContentFile(dataImage);
                            notaDespido.documentoDespido = imageFile;
                            command.execute(notaDespidoRequest.empleoId,notaDespido);

                            return new Response()
     .WithStatusCode(HttpStatusCode.OK);
                        }
               
                    }

                    
                }
                return new Response()
                    .WithStatusCode(HttpStatusCode.BadRequest);
            };
        }

        private NotaDespido getNotaDespido(NotaDespidoRequest notaDespidoRequest)
        {
            var motivoDespido = getMotivoDespido(notaDespidoRequest.motivoDespidoRequest);
            var supervisor = getSupervisor(notaDespidoRequest.supervisorRequest);
            var firma = getFirmaAutorizada(notaDespidoRequest.firmaAutorizadaRequest);

            var notaDespido = new NotaDespido(motivoDespido, notaDespidoRequest.fechaDespido,
                notaDespidoRequest.posicionGPS, supervisor, firma);

            var auditoriaRequest = notaDespidoRequest.auditoriaRequest;
            notaDespido.auditoria = new Auditoria(auditoriaRequest.usuarioCreo, auditoriaRequest.fechaCreo,
                auditoriaRequest.usuarioModifico, auditoriaRequest.fechaModifico); ;
            return notaDespido;
        }

        private MotivoDespido getMotivoDespido(MotivoDespidoRequest motivoDespidoRequest)
        {
            return new MotivoDespido(motivoDespidoRequest.descripcion){Id = motivoDespidoRequest.IdGuid};
        }

        private FirmaAutorizada getFirmaAutorizada(FirmaAutorizadaRequest firmaAutorizadaRequest)
        {
            var userRequest = firmaAutorizadaRequest.userRequest;
            var user = new User(userRequest.Id, "", "", userRequest.password, "", new RolNull());
            var firma = new FirmaAutorizada(user, firmaAutorizadaRequest.fechaCreacion);
            firma.Id = firmaAutorizadaRequest.IdGuid;

            return firma;
        }

        private Supervisor getSupervisor(SupervisorRequest supervisorRequest)
        {
            var userRequest = supervisorRequest.userRequest;
            var user = new User(userRequest.Id, "", "", userRequest.password, "", new RolNull());
            var supervisor = new Supervisor(user);
            supervisor.Id = supervisorRequest.guid;
            return supervisor;
        }
    }
}