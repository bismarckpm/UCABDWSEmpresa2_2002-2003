﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using ServicesDeskUCABWS.BussinesLogic.DAO.DepartamentoDAO;
using ServicesDeskUCABWS.BussinesLogic.DAO.EtiquetaDAO;
using ServicesDeskUCABWS.BussinesLogic.DAO.TipoEstadoDAO;
using ServicesDeskUCABWS.BussinesLogic.DTO.DepartamentoDTO;
using ServicesDeskUCABWS.BussinesLogic.DTO.Plantilla;
using ServicesDeskUCABWS.BussinesLogic.Exceptions;
using ServicesDeskUCABWS.BussinesLogic.Response;
using ServicesDeskUCABWS.Controllers;
using ServicesDeskUCABWS.Controllers.ControllerDepartamento;
using ServicesDeskUCABWS.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTestServicesDeskUCABWS.UnitTest_GrupoH.DepartamentoTest
{
    [TestClass]
    public class DepartamentoControllerTest
    {
        private readonly DepartamentoController _controller;
        private readonly Mock<IDepartamentoDAO> _serviceMock;
        public Departamento dept = It.IsAny<Departamento>();
        public DepartamentoDto deptDto = It.IsAny<DepartamentoDto>();


        public DepartamentoControllerTest()
        {
            _serviceMock = new Mock<IDepartamentoDAO>();
            _controller = new DepartamentoController(_serviceMock.Object);
        }

        [TestMethod(displayName: "Prueba Unitaria Controlador para crear Departamento exitoso")]
        public void Crear()
        {
            var dept = new DepartamentoDto()
            {

                Id = new Guid("38f401c9-12aa-46bf-82a2-05ff65bb2c86"),

                Nombre = "Seguridad Ambiental",

                Descripcion = "Cuida el ambiente",

                Fecha_creacion = DateTime.Now.Date,

                Fecha_ultima_edicion = null,

                Fecha_eliminacion = null,
            };

            //arrange
            _serviceMock.Setup(p => p.AgregarDepartamentoDAO(It.IsAny<Departamento>())).Returns(new DepartamentoDto());
            var application = new ApplicationResponse<DepartamentoDto>();

            //act
            var result = _controller.CrearDepartamento(dept);

            //assert
            Assert.AreEqual(application.GetType(), result.GetType());
        }

        [TestMethod(displayName: "Prueba Unitaria Controlador para crear Departamento excepcion")]
        public void CrearDepartamentoExcepcion()
        {

            var dept = new DepartamentoDto()
            {

                Id = new Guid("38f401c9-12aa-46bf-82a2-05ff65bb2c86"),

                Nombre = "Seguridad Ambiental",

                Descripcion = "Cuida el ambiente",

                Fecha_creacion = DateTime.Now.Date,

                Fecha_ultima_edicion = null,

                Fecha_eliminacion = null,
            };

            //arrange
            _serviceMock.Setup(p => p.AgregarDepartamentoDAO(It.IsAny<Departamento>())).Throws(new ExceptionsControl("", new Exception()));

            //act
            var ex = _controller.CrearDepartamento(dept);

            //assert
            Assert.IsNotNull(ex);
            Assert.IsFalse(ex.Success);
        }

        [TestMethod(displayName: "Prueba Unitaria Controlador para consultar los departamentos")]
        public void ConsultarDepartamentos()
        {
            var dept = new DepartamentoDto()
            {

                Id = new Guid("38f401c9-12aa-46bf-82a2-05ff65bb2c86"),

                Nombre = "Seguridad Ambiental",

                Descripcion = "Cuida el ambiente",

                Fecha_creacion = DateTime.Now.Date,

                Fecha_ultima_edicion = null,

                Fecha_eliminacion = null,
            };  

            //arrange
            _serviceMock.Setup(p => p.ConsultarDepartamentos()).Returns(new List<DepartamentoDto>());
            var application = new ApplicationResponse<List<DepartamentoDto>>();

            //act
            var result = _controller.ConsultarDepartamentos();

            //assert
            Assert.AreEqual(application.GetType(), result.GetType());
        }

        [TestMethod(displayName: "Prueba Unitaria Controlador para crear Departamento excepcion")]
        public void ConsultarDepartamentosExcepcion()
        {

            var dept = new DepartamentoDto()
            {

                Id = new Guid("38f401c9-12aa-46bf-82a2-05ff65bb2c86"),

                Nombre = "Seguridad Ambiental",

                Descripcion = "Cuida el ambiente",

                Fecha_creacion = DateTime.Now.Date,

                Fecha_ultima_edicion = null,

                Fecha_eliminacion = null,
            };

            //arrange
            _serviceMock.Setup(p => p.ConsultarDepartamentos()).Throws(new ExceptionsControl("", new Exception()));

            //act
            var ex = _controller.ConsultarDepartamentos();

            //assert
            Assert.IsNotNull(ex);
            Assert.IsFalse(ex.Success);
        }

        [TestMethod(displayName: "Prueba Unitaria Controlador para eliminar departamento exitoso")]
        public void EliminarDepartamento()
        {
            var dept = new DepartamentoDto()
            {

                Id = new Guid("38f401c9-12aa-46bf-82a2-05ff65bb2c86"),

                Nombre = "Seguridad Ambiental",

                Descripcion = "Cuida el ambiente",

                Fecha_creacion = DateTime.Now.Date,

                Fecha_ultima_edicion = null,

                Fecha_eliminacion = null,
            };


            //arrange
            _serviceMock.Setup(p => p.eliminarDepartamento(It.IsAny<Guid>())).Returns(new DepartamentoDto());
            var application = new ApplicationResponse<DepartamentoDto>();

            //act
            var result = _controller.EliminarDepartamento(dept.Id);

            //assert
            Assert.AreEqual(application.GetType(), result.GetType());
        }

        [TestMethod(displayName: "Prueba Unitaria Controlador para eliminar Departamento excepcion")]
        public void EliminarDepartamentosExcepcion()
        {

            var dept = new DepartamentoDto()
            {

                Id = new Guid("38f401c9-12aa-46bf-82a2-05ff65bb2c86"),

                Nombre = "Seguridad Ambiental",

                Descripcion = "Cuida el ambiente",

                Fecha_creacion = DateTime.Now.Date,

                Fecha_ultima_edicion = null,

                Fecha_eliminacion = null
            };

            //arrange
            _serviceMock.Setup(p => p.eliminarDepartamento(It.IsAny<Guid>())).Throws(new ExceptionsControl("", new Exception()));

            //act
            var ex = _controller.EliminarDepartamento(dept.Id);

            //assert
            Assert.IsNotNull(ex);
            Assert.IsFalse(ex.Success);
        }

        [TestMethod(displayName: "Prueba Unitaria Controlador para modificar departamento exitoso")]
        public void ActualizarDepartamento()
        {
            var dept = new DepartamentoDto_Update()
            {

                Id = new Guid("38f401c9-12aa-46bf-82a2-05ff65bb2c86"),

                Nombre = "Seguridad Ambiental",

                Descripcion = "Cuida el ambiente",

                Fecha_creacion = DateTime.Now.Date,

                Fecha_ultima_edicion = DateTime.Now.Date,

                Fecha_eliminacion = null
            };

            //arrange
            _serviceMock.Setup(p => p.ActualizarDepartamento(It.IsAny<Departamento>())).Returns(new DepartamentoDto_Update());
            var application = new ApplicationResponse<DepartamentoDto_Update>();

            //act
            var result = _controller.ActualizarDepartamento(dept);

            //assert
            Assert.AreEqual(application.GetType(), result.GetType());
        }

        [TestMethod(displayName: "Prueba Unitaria Controlador para actualizar Departamento excepcion")]
        public void ActualizarDepartamentosExcepcion()
        {

            var dept = new DepartamentoDto_Update()
            {

                Id = new Guid("38f401c9-12aa-46bf-82a2-05ff65bb2c86"),

                Nombre = "Seguridad Ambiental",

                Descripcion = "Cuida el ambiente",

                Fecha_creacion = DateTime.Now.Date,

                Fecha_ultima_edicion = null,

                Fecha_eliminacion = null
            };

            //arrange
            _serviceMock.Setup(p => p.ActualizarDepartamento(It.IsAny<Departamento>())).Throws(new ExceptionsControl("", new Exception()));

            //act
            var ex = _controller.ActualizarDepartamento(dept);

            //assert
            Assert.IsNotNull(ex);
            Assert.IsFalse(ex.Success);
        }

        [TestMethod(displayName: "Prueba Unitaria Controlador para modificar departamento por id de departamento exitoso")]
        public void ConsultarDepartamentoPorID()
        {
           
            var dept = new DepartamentoDto()
            {

                Id = new Guid("38f401c9-12aa-46bf-82a2-05ff65bb2c86"),

                Nombre = "Seguridad Ambiental",

                Descripcion = "Cuida el ambiente",

                Fecha_creacion = DateTime.Now.Date,

                Fecha_ultima_edicion = null,

                Fecha_eliminacion = null

            };

            //arrange
            _serviceMock.Setup(p => p.ConsultarPorID(It.IsAny<Guid>())).Returns(new DepartamentoDto());
            var application = new ApplicationResponse<DepartamentoDto>();

            //act
            var result = _controller.ConsultarPorID(dept.Id);

            //assert
            Assert.AreEqual(application.GetType(), result.GetType());
        }

        [TestMethod(displayName: "Prueba Unitaria Controlador para consultar departamento por id de departamento excepcion")]
        public void ExcepcionConsultarDepartamentoPorID()
        {
            var dept = new DepartamentoDto()
            {

                Id = new Guid("38f401c9-12aa-46bf-82a2-05ff65bb2c87"),

                Nombre = "Nuevo Grupo",

                Descripcion = "Grupo nuevo",

                Fecha_creacion = DateTime.Now.Date,

                Fecha_ultima_edicion = null,

                Fecha_eliminacion = null
            };

            //arrange
            _serviceMock.Setup(p => p.ConsultarPorID(It.IsAny<Guid>())).Throws(new ExceptionsControl("", new Exception()));

            //act
            var ex = _controller.ConsultarPorID(dept.Id);

            //assert
            Assert.IsNotNull(ex);
            Assert.IsFalse(ex.Success);
        }





       

        

      

        [TestMethod(displayName: "Prueba Unitaria Controlador para consultar los departamentos no asociados exitoso")]
        public void ListaDepartamentoNoAsociado()
        {
            var grupo = new Grupo()
            {

                id = new Guid("38f401c9-12aa-46bf-82a2-05ff65bb2c87"),

                nombre = "Nuevo Grupo",

                descripcion = "Grupo nuevo",

                fecha_creacion = DateTime.Now.Date,

                fecha_ultima_edicion = null,

                fecha_eliminacion = null
            };

            var dept = new DepartamentoDto()
            {

                Id = new Guid("38f401c9-12aa-46bf-82a2-05ff65bb2c86"),

                Nombre = "Seguridad Ambiental",

                Descripcion = "Cuida el ambiente",

                Fecha_creacion = DateTime.Now.Date,

                Fecha_ultima_edicion = null,

                Fecha_eliminacion = null

            };

            //arrange
            _serviceMock.Setup(p => p.NoAsociado()).Returns(new List<DepartamentoDto>());
            var application = new ApplicationResponse<List<DepartamentoDto>>();

            //act
            var result = _controller.ListaDepartamentoNoAsociado();

            //assert
            Assert.AreEqual(application.GetType(), result.GetType());
        }

        [TestMethod(displayName: "Prueba Unitaria Controlador para consultar los departamentos no asociados excepcion")]
        public void ExcepcionListaDepartamentoNoAsociado()
        {
            var grupo = new Grupo()
            {

                id = new Guid("38f401c9-12aa-46bf-82a2-05ff65bb2c87"),

                nombre = "Nuevo Grupo",

                descripcion = "Grupo nuevo",

                fecha_creacion = DateTime.Now.Date,

                fecha_ultima_edicion = null,

                fecha_eliminacion = null
            };

            var dept = new DepartamentoDto()
            {

                Id = new Guid("38f401c9-12aa-46bf-82a2-05ff65bb2c86"),

                Nombre = "Seguridad Ambiental",

                Descripcion = "Cuida el ambiente",

                Fecha_creacion = DateTime.Now.Date,

                Fecha_ultima_edicion = null,

                Fecha_eliminacion = null

            };

            //arrange
            _serviceMock.Setup(p => p.NoAsociado()).Throws(new ExceptionsControl("", new Exception()));

            //act
            var ex = _controller.ListaDepartamentoNoAsociado();

            //assert
            Assert.IsNotNull(ex);
            Assert.IsFalse(ex.Success);
        }

        [TestMethod(displayName: "Prueba Unitaria Controlador para consultar los departamentos no eliminados exitoso")]
        public void ListaDepartamentonoEliminado()
        {
        
            var dept = new DepartamentoDto()
            {

                Id = new Guid("38f401c9-12aa-46bf-82a2-05ff65bb2c86"),

                Nombre = "Seguridad Ambiental",

                Descripcion = "Cuida el ambiente",

                Fecha_creacion = DateTime.Now.Date,

                Fecha_ultima_edicion = null,

                Fecha_eliminacion = null

            };

            //arrange
            _serviceMock.Setup(p => p.DepartamentosNoEliminados()).Returns(new List<DepartamentoDto>());
            var application = new ApplicationResponse<List<DepartamentoDto>>();

            //act
            var result = _controller.ListaDepartamentonoEliminado();

            //assert
            Assert.AreEqual(application.GetType(), result.GetType());
        }

        [TestMethod(displayName: "Prueba Unitaria Controlador para consultar los departamentos no eliminados excepcion")]
        public void ExcepcionListaDepartamentoNoEliminados()
        {
            var grupo = new Grupo()
            {

                id = new Guid("38f401c9-12aa-46bf-82a2-05ff65bb2c87"),

                nombre = "Nuevo Grupo",

                descripcion = "Grupo nuevo",

                fecha_creacion = DateTime.Now.Date,

                fecha_ultima_edicion = null,

                fecha_eliminacion = null
            };

            var dept = new DepartamentoDto()
            {

                Id = new Guid("38f401c9-12aa-46bf-82a2-05ff65bb2c86"),

                Nombre = "Seguridad Ambiental",

                Descripcion = "Cuida el ambiente",

                Fecha_creacion = DateTime.Now.Date,

                Fecha_ultima_edicion = null,

                Fecha_eliminacion = null

            };

            //arrange
            _serviceMock.Setup(p => p.DepartamentosNoEliminados()).Throws(new ExceptionsControl("", new Exception()));

            //act
            var ex = _controller.ListaDepartamentonoEliminado();

            //assert
            Assert.IsNotNull(ex);
            Assert.IsFalse(ex.Success);
        }

        [TestMethod]
        //Consultar Departamento por Cargo
        public void ListaDepartamentoCargo()
        {

            //arrange
            _serviceMock.Setup(p => p.ConsultarDepartamentoCargo()).Returns(new List<DepartamentoCargoDTO>());
            //ar application = new ApplicationResponse<List<DepartamentoDto>>();

            //act
            var result = _controller.ListaDepartamentonoCargo();

            //assert
            Assert.IsTrue(result.Success);
        }

        [TestMethod(displayName: "Prueba Unitaria Controlador para consultar los departamentos no eliminados excepcion")]
        public void ExcepcionListaDepartamentoCargo()
        {
            

            //arrange
            _serviceMock.Setup(p => p.ConsultarDepartamentoCargo()).Throws(new ExceptionsControl("", new Exception()));

            //act
            var ex = _controller.ListaDepartamentonoCargo();

            //assert
            Assert.IsNotNull(ex);
            Assert.IsFalse(ex.Success);
        }




    }
}
