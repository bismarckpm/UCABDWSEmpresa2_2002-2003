﻿using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using NuGet.Packaging;
using ServicesDeskUCABWS.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ServicesDeskUCABWS.BussinesLogic.Exceptions;
using ServicesDeskUCABWS.Entities;
using ServicesDeskUCABWS.BussinesLogic.Response;
using ServicesDeskUCABWS.BussinesLogic.Recursos;
using ServicesDeskUCABWS.BussinesLogic.DTO.Tipo_TicketDTO;
using Microsoft.Data.SqlClient;
using ServicesDeskUCABWS.BussinesLogic.DTO.Flujo_AprobacionDTO;
using ServicesDeskUCABWS.BussinesLogic.DTO.DepartamentoDTO;
using System.Runtime.CompilerServices;
using ServicesDeskUCABWS.BussinesLogic.Validaciones;
using ServicesDeskUCABWS.BussinesLogic.Factory;
using ServicesDeskUCABWS.BussinesLogic.Mapper.MapperTipoTicket;

namespace ServicesDeskUCABWS.BussinesLogic.DAO.Tipo_TicketDAO
{
    public class Tipo_TicketService : ITipo_TicketDAO
    {
        // Inyeccion de dependencias DBcontext
        private IDataContext context;
        private readonly IMapper _mapper;

        // Constructor para el servicio de Tipo Ticket
        public Tipo_TicketService(IDataContext Context, IMapper mapper)
        {
            context = Context;
            _mapper = mapper;
        }
        //GET: Servicio para consultar la lista de tipo ticket
        public IEnumerable<Tipo_TicketDTOSearch> ConsultarTipoTicket()
        {
            try
            {
                var tipo = context.Tipos_Tickets
                .Include(dep => dep.Departamentos).ThenInclude(dep => dep.departamento)
                .Include(fa => fa.Flujo_Aprobacion)
                .ThenInclude(fb => fb.Cargo)
                .Where(fa => fa.fecha_elim == null)
                .ToList();

                return BuscaDepartamentosAsociadosATipoTickets(tipo);
            }

            catch (ExceptionsControl ex)
            {
                throw new ExceptionsControl("No existen Tipos de tickets registrados", ex);
            }
            catch (Exception ex)
            {
                throw new ExceptionsControl("Hubo un problema al consultar la lista de Tipos de Tickets", ex);
            }
        }
        public List<Tipo_TicketDTOSearch> BuscaDepartamentosAsociadosATipoTickets(List<Tipo_Ticket> data)
        {
            var tipo_ticketsDTO = new List<Tipo_TicketDTOSearch>();
            foreach (var r in data)
            {
                var listaDept = new List<DepartamentoSearchDTO>();
                foreach (var t in r.Departamentos)
                {
                    listaDept.Add(new DepartamentoSearchDTO()
                    {
                        Id = t.DepartamentoId.ToString(),
                        nombre = t.departamento.nombre
                    });
                }
                tipo_ticketsDTO.Add(new Tipo_TicketDTOSearch
                {
                    Id = r.Id,
                    nombre = r.nombre,
                    descripcion = r.descripcion,
                    Minimo_Aprobado = r.Minimo_Aprobado,
                    Maximo_Rechazado = r.Maximo_Rechazado,
                    tipo = r.ObtenerTipoAprobacion(),
                    Flujo_Aprobacion = _mapper.Map<List<Flujo_AprobacionDTOSearch>>(r.Flujo_Aprobacion),
                    Departamento = listaDept
                });
            }
            return tipo_ticketsDTO;
        }

        private Tipo_Ticket AgregarRelacionesTipoTicket(Tipo_TicketDTOUpdate tipo_TicketDTO, Tipo_Ticket tipo_ticket)
        {
            var Cargos = context.Cargos.Where(x => tipo_TicketDTO.Flujo_Aprobacion
                .Select(x => x.IdCargo.ToUpper()).ToList().Contains(x.id.ToString().ToUpper()));
            tipo_ticket.Flujo_Aprobacion = new List<Flujo_Aprobacion>();

            foreach (var cargo in Cargos)
            {
                tipo_ticket.Flujo_Aprobacion.Add(new Flujo_Aprobacion()
                {
                    IdTicket = tipo_ticket.Id,
                    Tipo_Ticket = tipo_ticket,
                    IdCargo = cargo.id,
                    Cargo = cargo
                });
            }

            foreach (Flujo_Aprobacion flujo_aprob in tipo_ticket.Flujo_Aprobacion)
            {
                var flujo_aprob_filtrado = tipo_TicketDTO.Flujo_Aprobacion.Where(x => x.IdCargo.ToUpper() == flujo_aprob.IdCargo.ToString().ToUpper()).FirstOrDefault();

                flujo_aprob.OrdenAprobacion = flujo_aprob_filtrado.OrdenAprobacion;
                flujo_aprob.Minimo_aprobado_nivel = flujo_aprob_filtrado.Minimo_aprobado_nivel;
                flujo_aprob.Maximo_Rechazado_nivel = flujo_aprob_filtrado.Maximo_Rechazado_nivel;
            }

            tipo_ticket.Departamentos =
                context.Departamentos
               .Where(x => tipo_TicketDTO.Departamento.Select(y => y.ToString().ToUpper()).Contains(x.id.ToString().ToUpper()))
               .Select(s => new DepartamentoTipo_Ticket()
               {
                   departamento = s,
                   DepartamentoId = s.id,
                   tipo_Ticket = tipo_ticket,
                   Tipo_Ticekt_Id = tipo_ticket.Id

               }).ToList();

            return tipo_ticket;
        }

        public ApplicationResponse<Tipo_TicketDTOUpdate> ActualizarTipo_Ticket(Tipo_TicketDTOUpdate tipo_TicketDTO)
        {
            var response = new ApplicationResponse<Tipo_TicketDTOUpdate>();
            try
            {
                ValidarDatosEntradaTipo_Ticket_Update(tipo_TicketDTO);
                
                //Actualizando Datos 
                var tipo_ticket = context.Tipos_Tickets.Find(Guid.Parse(tipo_TicketDTO.Id));
                tipo_ticket = TipoTicketMapper.CambiarFlujoTipoTicket(tipo_ticket,tipo_TicketDTO.tipo,_mapper);
                tipo_ticket.nombre = tipo_TicketDTO.nombre;
                tipo_ticket.descripcion = tipo_TicketDTO.descripcion;
                //tipo_ticket.tipo = tipo_TicketDTO.tipo;
                tipo_ticket.fecha_ult_edic = DateTime.UtcNow;
                tipo_ticket.Minimo_Aprobado = tipo_TicketDTO.Minimo_Aprobado;
                tipo_ticket.Maximo_Rechazado = tipo_TicketDTO.Maximo_Rechazado;

                //Eliminando referencia en el tipo Ticket
                if (tipo_ticket.Departamentos != null) tipo_ticket.Departamentos.Clear();
                if (tipo_ticket.Flujo_Aprobacion != null) tipo_ticket.Flujo_Aprobacion.Clear();

                //Eliminando Entidades intermedias de flujo de aprobacion
                context.Flujos_Aprobaciones.RemoveRange(context.Flujos_Aprobaciones
                    .Where(x => x.IdTicket.ToString().ToUpper() == tipo_TicketDTO.Id.ToUpper()).ToList());

                context.DepartamentoTipo_Ticket.RemoveRange(context.DepartamentoTipo_Ticket
                    .Where(x => x.Tipo_Ticekt_Id.ToString().ToUpper() == tipo_TicketDTO.Id.ToUpper()).ToList());

                context.Tipos_Tickets.Remove(context.Tipos_Tickets.Find(tipo_ticket.Id));

                //Agregando las nuevas relaciones
                tipo_ticket = AgregarRelacionesTipoTicket(tipo_TicketDTO, tipo_ticket);

                //Actualizacion de la BD
                context.Tipos_Tickets.Add(tipo_ticket);
                context.DbContext.SaveChanges(); 

                //Paso a AR
                response.Data = TipoTicketMapper.MapperTipoTicketToTipoTicketDTOUpdate(tipo_ticket);
            }
            catch (ExceptionsControl ex)
            {
                response.Success = false;
                response.Message = ex.Mensaje;
            }

            return response;
        }

        public ApplicationResponse<Tipo_TicketDTOCreate> RegistroTipo_Ticket(Tipo_TicketDTOCreate Tipo_TicketDTO)
        {
            var response = new ApplicationResponse<Tipo_TicketDTOCreate>();
            try
            {
                //Método para Validar los Datos de entrada 
                ValidarDatosEntradaTipo_Ticket(Tipo_TicketDTO);

                //Construccion de la instancia del objeto Tipo Ticket
                var tipo_ticket = TipoTicketFactory.ObtenerInstancia(Tipo_TicketDTO.tipo);
                tipo_ticket.LlenarDatos(
                    Tipo_TicketDTO.nombre,
                    Tipo_TicketDTO.descripcion,
                    Tipo_TicketDTO.tipo,
                    Tipo_TicketDTO.Minimo_Aprobado,
                    Tipo_TicketDTO.Maximo_Rechazado);
                try
                {
                    //Agregar los datos en la tabla intermedia entre Tipo Ticket y Departamento (DepartamentoTipo_Ticket)
                    RelacionConDepartamento(tipo_ticket, Tipo_TicketDTO);
                }
                catch (Exception) { }
                try
                {
                    //Agregar los datos en la tabla intermedia entre Tipo Ticket y Cargos (Flujo de Aprobacion)
                    RelacionConFLujoAprobacion(tipo_ticket, Tipo_TicketDTO);
                }
                catch (Exception ex) { }

                //Añadir y guardar cambios en la base de datos
                context.Tipos_Tickets.Add(tipo_ticket);
                context.DbContext.SaveChanges();

                //Paso a Application Response
                response.Data = TipoTicketMapper.MapperTipoTicketToTipoTicketDTOCreate(tipo_ticket);
            }
            catch (ExceptionsControl ex)
            {
                response.Success = false;
                response.Exception = ex.Mensaje;
            }

            return response;
        }

        public void RelacionConDepartamento(Tipo_Ticket tipo_ticket, Tipo_TicketDTOCreate Tipo_TicketDTO)
        {

            tipo_ticket.Departamentos = context.Departamentos
                    .Where(x => Tipo_TicketDTO.Departamento.Select(y => y).Contains(x.id.ToString().ToUpper()))
                    .Select(s => new DepartamentoTipo_Ticket()
                    {
                        departamento = s,
                        DepartamentoId = s.id,
                        tipo_Ticket = tipo_ticket,
                        Tipo_Ticekt_Id = tipo_ticket.Id
                    }).ToList();
        }

        public void RelacionConFLujoAprobacion(Tipo_Ticket tipo_ticket, Tipo_TicketDTOCreate Tipo_TicketDTO)
        {

            var Cargos = context.Cargos
            .Where(x => Tipo_TicketDTO.Flujo_Aprobacion.Select(x => x.IdCargo.ToUpper()).ToList().Contains(x.id.ToString().ToUpper()));
            tipo_ticket.Flujo_Aprobacion = new List<Flujo_Aprobacion>();
            foreach (var c in Cargos)
            {
                tipo_ticket.Flujo_Aprobacion.Add(new Flujo_Aprobacion()
                {
                    IdTicket = tipo_ticket.Id,
                    Tipo_Ticket = tipo_ticket,
                    IdCargo = c.id,
                    Cargo = c
                });
            }
            foreach (Flujo_Aprobacion fa in tipo_ticket.Flujo_Aprobacion)
            {
                var t = Tipo_TicketDTO.Flujo_Aprobacion.Where(x => x.IdCargo.ToUpper() == fa.IdCargo.ToString().ToUpper()).FirstOrDefault();

                fa.OrdenAprobacion = t.OrdenAprobacion;
                fa.Minimo_aprobado_nivel = t.Minimo_aprobado_nivel;
                fa.Maximo_Rechazado_nivel = t.Maximo_Rechazado_nivel;
            }
        }
        private void ValidarDatosEntradaTipo_Ticket_Update(Tipo_TicketDTOUpdate tipo_TicketDTOUpdate)
        {
            try
            {
                var tipo_Ticket = context.Tipos_Tickets.Find(Guid.Parse(tipo_TicketDTOUpdate.Id));
                if (tipo_Ticket == null)
                {
                    throw new ExceptionsControl(ErroresTipo_Tickets.TIPO_TICKET_DESC);
                }

                if (tipo_Ticket.ObtenerTipoAprobacion() != tipo_TicketDTOUpdate.tipo)
                {
                    var ticketsPendientes = context.Tickets.Include(x => x.Tipo_Ticket).Include(x => x.Estado).ThenInclude(x => x.Estado_Padre)
                        .Where(x => x.Tipo_Ticket.Id == tipo_Ticket.Id &&
                        x.Estado.Estado_Padre.nombre == "Pendiente").Count();
                    if (ticketsPendientes > 0)
                    {
                        throw new ExceptionsControl(ErroresTipo_Tickets.ERROR_UPDATE_MODELO_APROBACION);
                    }
                }

                var tipo_TicketDTOCreate = _mapper.Map<Tipo_TicketDTOCreate>(tipo_TicketDTOUpdate);
                if (tipo_TicketDTOCreate.Flujo_Aprobacion.Count() == 0)
                {
                    tipo_TicketDTOCreate.Flujo_Aprobacion = null;
                }
                ValidarDatosEntradaTipo_Ticket(tipo_TicketDTOCreate);
            }
            catch (FormatException ex)
            {
                throw new ExceptionsControl(ErroresTipo_Tickets.FORMATO_ID_TICKET, ex);
            }
            catch (ExceptionsControl ex)
            {
                throw new ExceptionsControl(ex.Mensaje);
            }
        }

        public void ValidarDatosEntradaTipo_Ticket(Tipo_TicketDTOCreate tipo_TicketDTOCreate)
        {
            try
            {
                var validaciones = new TipoTicketValidaciones(context);

                validaciones.LongitudNombre(tipo_TicketDTOCreate.nombre);
                validaciones.LongitudDescripcion(tipo_TicketDTOCreate.descripcion);
                validaciones.VerificarTipoFlujo(tipo_TicketDTOCreate.tipo);
                validaciones.VerificarDepartamento(tipo_TicketDTOCreate.Departamento);

                if (tipo_TicketDTOCreate.tipo == "Modelo_Paralelo")
                {
                    validaciones.HayCargos(tipo_TicketDTOCreate);
                    validaciones.VerificarCargos(tipo_TicketDTOCreate.Flujo_Aprobacion.Select(x => x.IdCargo));
                    validaciones.VerificarMinimoMaximoAprobadoFlujoParalelo(tipo_TicketDTOCreate);
                    validaciones.VerificarCargosFlujoParalelo(tipo_TicketDTOCreate);
                }

                if (tipo_TicketDTOCreate.tipo == "Modelo_Jerarquico")
                {
                    validaciones.HayCargos(tipo_TicketDTOCreate);
                    validaciones.VerificarCargos(tipo_TicketDTOCreate.Flujo_Aprobacion.Select(x => x.IdCargo));
                    validaciones.VerificarMinimoMaximoAprobadoFlujoJerarquico(tipo_TicketDTOCreate);
                    validaciones.VerificarCargosFlujoJerarquico(tipo_TicketDTOCreate);
                    validaciones.VerificarSecuenciaOrdenAprobacion(tipo_TicketDTOCreate);
                }

                if (tipo_TicketDTOCreate.tipo == "Modelo_No_Aprobacion")
                {
                    //Verificar si MA y MR son null
                    if (tipo_TicketDTOCreate.Minimo_Aprobado != null || tipo_TicketDTOCreate.Maximo_Rechazado != null)
                    {
                        throw new ExceptionsControl(ErroresTipo_Tickets.MODELO_NO_APROBACION_NO_VALIDO);
                    }

                    //Verificar si el flujo es null
                    if (tipo_TicketDTOCreate.Flujo_Aprobacion != null)
                    {
                        throw new ExceptionsControl(ErroresTipo_Tickets.MODELO_NO_APROBACION_CARGO);
                    }
                }
            }
            catch (FormatException ex)
            {
                throw new ExceptionsControl(ErroresTipo_Tickets.FORMATO_NO_VALIDO, ex);
            }


        }

        private static List<FlujoAprobacionDTOCreate> ObtenerCargos(Tipo_TicketDTOCreate tipo_TicketDTOCreate)
        {
            return tipo_TicketDTOCreate.Flujo_Aprobacion;
        }

        //GET: Servicio para consultar un tipo de ticket por un ID en específico
        public Tipo_TicketDTOSearch ConsultarTipoTicketGUID(Guid id)
        {
            try
            {
                var data = context.Tipos_Tickets
                   .AsNoTracking()
                   .Include(p => p.Departamentos)
                   .ThenInclude(dep => dep.departamento)
                   .Include(fa => fa.Flujo_Aprobacion)
                   .ThenInclude(_fa => _fa.Cargo)
                   .Where(p => p.Id == id)
                   .Single();

                var dep = BuscaDepartamentosAsociados(data);
                return dep;
            }
            catch (Exception ex)
            {
                throw new ExceptionsControl("No existe el tipo de ticket con ese ID", ex);
            }
        }
        //GET: Servicio para Consultar un tipo de ticket por un nombre en específico
        public Tipo_TicketDTOSearch ConsultarNombreTipoTicket(string nombre)
        {
            try
            {
                var data = context.Tipos_Tickets.AsNoTracking()
                   .Include(p => p.Departamentos)
                   .ThenInclude(dep => dep.departamento)
                   .Include(fa => fa.Flujo_Aprobacion)
                   .ThenInclude(_fa => _fa.Cargo)
                   .Where(t => t.nombre == nombre)
                   .Single();

                var dep = BuscaDepartamentosAsociados(data);
                return dep;

            }
            catch (Exception ex)
            {
                throw new ExceptionsControl("No existe el tipo de ticket con ese nombre", ex);
            }
        }
        public Tipo_TicketDTOSearch BuscaDepartamentosAsociados(Tipo_Ticket data)
        {
            var tipo_ticketsDTO = new Tipo_TicketDTOSearch();
            var listaDept = new List<DepartamentoSearchDTO>();
            foreach (var t in data.Departamentos)
            {
                listaDept.Add(new DepartamentoSearchDTO()
                {
                    Id = t.DepartamentoId.ToString(),
                    nombre = t.departamento.nombre
                });
            }
            tipo_ticketsDTO = new Tipo_TicketDTOSearch()
            {
                Id = data.Id,
                nombre = data.nombre,
                descripcion = data.descripcion,
                Minimo_Aprobado = data.Minimo_Aprobado,
                Maximo_Rechazado = data.Maximo_Rechazado,
                tipo = data.ObtenerTipoAprobacion(),
                Flujo_Aprobacion = _mapper.Map<List<Flujo_AprobacionDTOSearch>>(data.Flujo_Aprobacion),
                Departamento = listaDept
            };

            return tipo_ticketsDTO;

        }


        //DELETE: Servicio para Desactivar un tipo de ticket por un id en especifico
        public Boolean EliminarTipoTicket(Guid id)
        {
            try
            {
                //Validar los datos de entrada (id)
                ValidarDatosEntradaTipo_Ticket_Delete(id);

                var tipo_ticket = context.Tipos_Tickets.Find(id);

                //Cambia la fecha de eliminacion por la fecha actual 
                tipo_ticket.fecha_elim = DateTime.UtcNow;

                //Se guardan los cambios
                context.DbContext.SaveChanges();

                return true;
            }
            catch (Exception ex)
            {
                throw new ExceptionsControl("No se pudo desactivar el tipo de ticket", ex);
            }
        }

        public void ValidarDatosEntradaTipo_Ticket_Delete(Guid Id)
        {
            
            var tipo_Ticket = context.Tipos_Tickets.Find(Id);
            if (tipo_Ticket == null)
            {
                throw new ExceptionsControl(ErroresTipo_Tickets.TIPO_TICKET_DESC);
            }
            var ticketsPendientes = context.Tickets.Include(x => x.Tipo_Ticket).Include(x => x.Estado).ThenInclude(x => x.Estado_Padre)
                    .Where(x => x.Tipo_Ticket.Id == tipo_Ticket.Id &&
                    x.Estado.Estado_Padre.nombre == "Pendiente").Count();

            if (ticketsPendientes > 0)
            {
                throw new ExceptionsControl(ErroresTipo_Tickets.ERROR_UPDATE_MODELO_APROBACION);
            }
        }

        public List<Tipo_TicketDTOSearch> ConsultaTipoTicketAgregarTicket(Guid Id)
        {
            var ListaTipoTicket = context.Tipos_Tickets.Include(x => x.Departamentos).ThenInclude(x=>x.departamento).Where(x => x.Departamentos.Select(x => x.departamento.id).Contains(Id)).ToList();
            ListaTipoTicket.AddRange(context.Tipos_Tickets.Include(x => x.Departamentos).Where(x => x.Departamentos.Count==0).ToList());
            var ListaTipoTicketDTO = _mapper.Map<List<Tipo_TicketDTOSearch>>(ListaTipoTicket);
            return ListaTipoTicketDTO;
        }

       
    }
}
