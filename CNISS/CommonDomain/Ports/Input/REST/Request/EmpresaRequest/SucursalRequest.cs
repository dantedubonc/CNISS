﻿using CNISS.CommonDomain.Ports.Input.REST.Request.GremioRequest;

namespace CNISS.CommonDomain.Ports.Input.REST.Request.EmpresaRequest
{
    public class SucursalRequest:IValidPost
    {
        public string nombre { get; set; }
        public UserRequest.UserRequest firmaRequest { get; set; }
        public DireccionRequest direccionRequest { get; set; }


        public bool isValidPost()
        {
            return firmaRequest!=null && firmaRequest.isValidPost() 
                && direccionRequest!=null && direccionRequest.isValidPost()
                && nombre!=null &&!string.IsNullOrEmpty(nombre);
        }
    }
}