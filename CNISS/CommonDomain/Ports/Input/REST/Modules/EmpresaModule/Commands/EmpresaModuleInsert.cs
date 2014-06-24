﻿using CNISS.CommonDomain.Application;
using CNISS.CommonDomain.Ports.Input.REST.Request.EmpresaRequest;
using CNISS.EnterpriseDomain.Domain.Entities;
using Nancy;
using Nancy.ModelBinding;

namespace CNISS.CommonDomain.Ports.Input.REST.Modules.EmpresaModule.Commands
{
    public class EmpresaModuleInsert:NancyModule
    {
     
        public EmpresaModuleInsert(ICommandInsertIdentity<Empresa> _commandInsert, IFileGetter fileGetter)
        {
            Post["enterprise/"] = parameters =>
            {
                var request = this.Bind<EmpresaRequest>();
                if (request.isValidPost())
                {
                    var directory = @"/EmpresasContratos";
                    var extension = ".pdf";
                    var nameFile = request.contentFile;
                    if (fileGetter.existsFile(directory, nameFile, extension))
                    {
                        var dataFile = fileGetter.getFile(directory, nameFile, extension);
                        var empresaMap = new EmpresaMap();
                        var empresa = empresaMap.getEmpresa(request, dataFile);
                        if (_commandInsert.isExecutable(empresa))
                        {
                            _commandInsert.execute(empresa);
                            return new Response()
                      .WithStatusCode(HttpStatusCode.OK);
                        }
                        
                       
                    }

                }
                   

                return new Response()
                    .WithStatusCode(HttpStatusCode.BadRequest);
            };
            
        }
    }
}