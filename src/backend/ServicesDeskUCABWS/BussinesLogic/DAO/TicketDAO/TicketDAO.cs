﻿using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NuGet.Packaging;
using NuGet.Protocol;
using ServicesDeskUCABWS.BussinesLogic.DAO.NotificacionDAO;
using ServicesDeskUCABWS.BussinesLogic.DAO.PlantillaNotificacionDAO;
using ServicesDeskUCABWS.BussinesLogic.DTO.TicketsDTO;
using ServicesDeskUCABWS.BussinesLogic.Exceptions;
using ServicesDeskUCABWS.BussinesLogic.Response;
using AutoMapper;
using ServicesDeskUCABWS.Data;
using ServicesDeskUCABWS.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using ServicesDeskUCABWS.BussinesLogic.DTO.TicketsDTO;
using ServicesDeskUCABWS.BussinesLogic.DTO.TicketDTO;
using ServicesDeskUCABWS.BussinesLogic.Excepciones;
using ServicesDeskUCABWS.BussinesLogic.Validaciones;
using Microsoft.EntityFrameworkCore;
using System.Net.Http;
using System.Net.Sockets;
using ServicesDeskUCABWS.BussinesLogic.Response;
using Microsoft.Data.SqlClient;
using ServicesDeskUCABWS.BussinesLogic.Exceptions;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Formatters;
using ServicesDeskUCABWS.BussinesLogic.DTO.DepartamentoDTO;
using ServicesDeskUCABWS.BussinesLogic.DAO.Tipo_TicketDAO;
using ServicesDeskUCABWS.BussinesLogic.DTO.Tipo_TicketDTO;
using ServicesDeskUCABWS.BussinesLogic.Mapper;
using ServicesDeskUCABWS.BussinesLogic.DTO.Plantilla;
using ServicesDeskUCABWS.BussinesLogic.Mapper.MapperTicket;
using System.Numerics;

namespace ServicesDeskUCABWS.BussinesLogic.DAO.TicketDAO
{
    public class TicketDAO : ITicketDAO
    {
        private IDataContext _dataContext;
        private readonly IMapper _mapper;
        private List<Ticket> listaTickets;
        private readonly INotificacion notificacion;
        private readonly IPlantillaNotificacion plantilla;

        public TicketDAO(IDataContext context, IPlantillaNotificacion plantilla, INotificacion notificacion, IMapper mapper)
        {
            _dataContext = context;
            this.notificacion = notificacion;
            this.plantilla = plantilla;
            _mapper = mapper;
        }

        /*public TicketDAO(IDataContext dataContext, IMapper mapper)
         {
             _dataContext = dataContext;
             _mapper = mapper;
         }*/

        public Task<bool> ActualizarTicket(Ticket ticket)
        {
            throw new NotImplementedException();
        }

        public List<Ticket> ConsultaListaTickets()
        {
            listaTickets = _dataContext.Tickets.ToList();
            return listaTickets;
        }

        public Ticket ConsultaTicket(Guid id)
        {
            var Ticket = (Ticket)_dataContext.Tickets.Where(s => s.Id == id);
            return Ticket;
        }

        /*public ApplicationResponse<TicketCreateDTO> RegistroTicket(TicketCreateDTO ticketDTO)
        {

            var response = new ApplicationResponse<TicketCreateDTO>();
            try
            {

                var ticket = new Ticket(ticketDTO.titulo, ticketDTO.descripcion);
                ticket.Prioridad = _dataContext.Prioridades.Find(Guid.Parse(ticketDTO.Prioridad));
                ticket.Emisor = _dataContext.Empleados.Include(x => x.Cargo).ThenInclude(x => x.Departamento)
                    .Where(s => s.Id == Guid.Parse(ticketDTO.Emisor)).FirstOrDefault();
                ticket.Departamento_Destino = _dataContext.Departamentos.Find(Guid.Parse(ticketDTO.Departamento_Destino));
                ticket.Tipo_Ticket = _dataContext.Tipos_Tickets.Find(Guid.Parse(ticketDTO.Tipo_Ticket));
                ticket.Estado = _dataContext.Estados.Where(x => x.Estado_Padre.nombre == "Pendiente" &&
                x.Departamento.id == ticket.Emisor.Cargo.Departamento.id).FirstOrDefault();=

                _dataContext.Tickets.Add(ticket);
                _dataContext.DbContext.SaveChanges();
                response.Data = ticketDTO;
                FlujoAprobacionCreacionTicket(ticket);

            }
            catch (ExceptionsControl ex)
            {

                response.Success = false;
                response.Message = ex.Message;
                response.Exception = ex.Excepcion.ToString();
            }
            catch (SqlException ex)
            {
                response.Success = false;
                response.Message = ex.Message;
                response.Exception = ex.ToString();
            }
            return response;

       }*/
        public ApplicationResponse<TicketNuevoDTO> RegistroTicket(TicketNuevoDTO ticketDTO)
        {
            var response = new ApplicationResponse<TicketNuevoDTO>();
            try
            {
                //CrearTicket
                var ticket = CrearTicket(ticketDTO);
                ticket.Bitacora_Tickets = new HashSet<Bitacora_Ticket>();
                Bitacora_Ticket nuevaBitacora = crearNuevaBitacora(ticket);
                ticket.Bitacora_Tickets.Add(nuevaBitacora);
                _dataContext.Bitacora_Tickets.Add(nuevaBitacora);
                FlujoAprobacionCreacionTicket(ticket);
                response.Data = TicketMapper.MapperTicketToTicketNuevoDTO(ticket);
                response.Message = "Ticket creado satisfactoriamente";
            }
            catch (ExceptionsControl ex)
            {

                response.Success = false;
                response.Message = ex.Message;
                response.Exception = ex.Excepcion.ToString();
            }
            catch (SqlException ex)
            {
                response.Success = false;
                response.Message = ex.Message;
                response.Exception = ex.ToString();
            }
            catch (Exception e)
            {
                response.Message = e.Message;
                response.Success = false;
                response.Exception = e.ToString();
            }
            return response;

        }

        public Ticket CrearTicket(TicketNuevoDTO ticketDTO)
        {
            var ticket = new Ticket(ticketDTO.titulo, ticketDTO.descripcion);
            ticket.Prioridad = _dataContext.Prioridades.Find(ticketDTO.prioridad_id);
            ticket.Emisor = _dataContext.Empleados.Include(x => x.Cargo).ThenInclude(x => x.Departamento)
                .Where(s => s.Id == ticketDTO.empleado_id).FirstOrDefault();
            ticket.Departamento_Destino = _dataContext.Departamentos.Find(ticketDTO.departamentoDestino_Id);
            ticket.Tipo_Ticket = _dataContext.Tipos_Tickets.Find(ticketDTO.tipoTicket_id);
            ticket.Estado = _dataContext.Estados.Where(x => x.Estado_Padre.nombre == "Pendiente" &&
            x.Departamento.id == ticket.Emisor.Cargo.Departamento.id).FirstOrDefault();
            try
            {
                ticket.Bitacora_Tickets = new HashSet<Bitacora_Ticket>();
                Bitacora_Ticket nuevaBitacora = crearNuevaBitacora(ticket);
                ticket.Bitacora_Tickets.Add(nuevaBitacora);
                _dataContext.Bitacora_Tickets.Add(nuevaBitacora);
            }
            catch (Exception e)
            {
                throw new Exception("No se pudo crear la Bitacora para el ticket", e);
            }
            if (ticket.Bitacora_Tickets == null)
                throw new Exception("La Bitacora para el ticket no pudo ser creada");

            if (ticketDTO.ticketPadre_Id != null)
            {
                ticket.Ticket_Padre = _dataContext.Tickets.Where(padre => padre.Id == ticketDTO.ticketPadre_Id).FirstOrDefault();
                //Ticket ticketPadre = _dataContext.Tickets.Where(padre => padre.Id == ticketDTO.ticketPadre_Id).FirstOrDefault();
                if (ticket.Ticket_Padre != null)
                    ticket.Ticket_Padre.fecha_eliminacion = DateTime.Now;
            }
            else
            {
                ticket.Ticket_Padre = null;
            }

            _dataContext.Tickets.Add(ticket);
            _dataContext.DbContext.SaveChanges();
            return ticket;
        }

        public string FlujoAprobacionCreacionTicket(Ticket ticket)
        {
            //Flujo Aprobacion
            var ListaEmpleado = ticket.Tipo_Ticket.FlujoAprobacion(_dataContext, ticket);
            if (ListaEmpleado == null)
            {
                return null;
            }

            //Cambiar Estado
            ticket.Tipo_Ticket.CambiarEstadoCreacionTicket(ticket, ListaEmpleado,_dataContext,notificacion,plantilla);

            
            return "Exitoso";
        }



        public TicketDAO(IDataContext dataContext, IMapper mapper)
        {
            _dataContext = dataContext;
            _mapper = mapper;
        }

        /*public ApplicationResponse<string> crearTicket(TicketNuevoDTO solicitudTicket)
        {
            ApplicationResponse<string> respuesta = new ApplicationResponse<string>();
            try
            {
                TicketValidaciones validaciones = new TicketValidaciones(_dataContext);
                validaciones.nuevoTicketEsValido(solicitudTicket);
                var ticketnuevo=RegistroTicket(solicitudTicket);
                //TicketDTO ticketnuevo = crearNuevoTicket(solicitudTicket);
                //FlujoAprobacion(_mapper.Map<Ticket>(ticketnuevo));
                respuesta.Data = "Ticket creado satisfactoriamente";
                respuesta.Message = "Ticket creado satisfactoriamente";
                respuesta.Success = true;
            } catch (TicketException e)
            {
                respuesta.Data = e.Message;
                respuesta.Message = e.Message;
                respuesta.Success = false;
            } catch (TicketDescripcionException e)
            {
                respuesta.Data = e.Message;
                respuesta.Message = e.Message;
                respuesta.Success = false;
            } catch (TicketEmisorException e)
            {
                respuesta.Data = e.Message;
                respuesta.Message = e.Message;
                respuesta.Success = false;
            } catch (TicketPrioridadException e)
            {
                respuesta.Data = e.Message;
                respuesta.Message = e.Message;
                respuesta.Success = false;
            } catch (TicketTipoException e)
            {
                respuesta.Data = e.Message;
                respuesta.Message = e.Message;
                respuesta.Success = false;
            } catch (TicketDepartamentoException e)
            {
                respuesta.Data = e.Message;
                respuesta.Message = e.Message;
                respuesta.Success = false;
            } catch (Exception e)
            {
                respuesta.Data = e.Message;
                respuesta.Message = e.Message;
                respuesta.Success = false;
            }
            return respuesta;
        }*/

        public ApplicationResponse<TicketInfoCompletaDTO> obtenerTicketPorId(Guid id)
        {
            ApplicationResponse<TicketInfoCompletaDTO> respuesta = new ApplicationResponse<TicketInfoCompletaDTO>();
            try
            {
                TicketValidaciones validaciones = new TicketValidaciones(_dataContext);
                validaciones.validarTicket(id);
                respuesta.Data = rellenarTicketInfoCompletaHl(id);
                respuesta.Message = "Proceso de búsqueda exitoso";
                respuesta.Success = true;

            } catch (TicketException e)
            {
                respuesta.Data = null;
                respuesta.Message = e.Message;
                respuesta.Success = false;
            } catch (Exception e)
            {
                respuesta.Data = null;
                respuesta.Message = e.Message;
                respuesta.Success = false;
            }
            return respuesta;
        }

        public ApplicationResponse<List<TicketInfoBasicaDTO>> obtenerTicketsPorEstadoYDepartamento(Guid idDepartamento, string estado,Guid empleadoId)
        {
            ApplicationResponse<List<TicketInfoBasicaDTO>> respuesta = new ApplicationResponse<List<TicketInfoBasicaDTO>>();
            try
            {
                TicketValidaciones validaciones = new TicketValidaciones(_dataContext);
                validaciones.validarDepartamento(idDepartamento);
                respuesta.Data = rellenarTicketInfoBasicaHl(idDepartamento, estado, empleadoId);
                respuesta.Message = "Proceso de búsqueda exitoso";
                respuesta.Success = true;
            } catch (TicketDepartamentoException e)
            {
                respuesta.Data = null;
                respuesta.Message = e.Message;
                respuesta.Success = false;
            } catch (Exception e)
            {
                respuesta.Data = null;
                respuesta.Message = e.Message;
                respuesta.Success = false;
            }
            return respuesta;
        }

        public ApplicationResponse<string> cambiarEstadoTicket(Guid ticketId, Guid estadoId)
        {
            ApplicationResponse<string> respuesta = new ApplicationResponse<string>();
            try
            {
                Ticket ticket = _dataContext.Tickets.Where(t=>t.Id == ticketId).Single();
                Estado estado = _dataContext.Estados.Include(t=>t.Estado_Padre).Where(t=>t.Id == estadoId).Single();
                Console.WriteLine($"ESTADO 1: {estado.nombre}");
                if (!CambiarEstado(ticket, estado.Estado_Padre.nombre, null))
                    throw new Exception("No se pudo modificar el estado del ticket");
                Console.WriteLine("ÉXITOSO");
                respuesta.Data = "Estado del ticket modificado exitosamente";
                respuesta.Message = "Estado del ticket cambiado satisfactoriamente";
                respuesta.Success = true;
            }
            catch (TicketException e)
            {
                respuesta.Data = null;
                respuesta.Message = e.Message;
                respuesta.Success = false;
            }
            catch (Exception e)
            {
                respuesta.Data = null;
                respuesta.Message = e.Message;
                respuesta.Success = false;
            }
            return respuesta;
        }

        public ApplicationResponse<List<TicketBitacorasDTO>> obtenerBitacoras(Guid ticketId)
        {
            ApplicationResponse<List<TicketBitacorasDTO>> respuesta = new ApplicationResponse<List<TicketBitacorasDTO>>();
            try
            {
                respuesta.Data = obtenerBitacorasHl(ticketId);
                respuesta.Message = "Búsqueda de bitácora exitosa";
                respuesta.Success = true;
            }
            catch (TicketException e)
            {
                respuesta.Data = null;
                respuesta.Message = e.Message;
                respuesta.Success = false;
            }
            return respuesta;
        }

        public ApplicationResponse<string> mergeTickets(Guid ticketPrincipalId, List<Guid> ticketsSecundariosId)
        {
            //CREAR LA FAMILIA TICKET, Y PONER FECHA FIN A LOS QUE ESTÁN EN LA LISTA!
            ApplicationResponse<string> respuesta = new ApplicationResponse<string>();
            try
            {
                Familia_Ticket nuevaFamilia = new Familia_Ticket
                {
                    Id = Guid.NewGuid(),
                    Lista_Ticket = new List<Ticket>()
                };
                ticketsSecundariosId.ForEach(delegate (Guid e)
                {
                    anadirFamiliaHl(e, nuevaFamilia, false);
                });
                anadirFamiliaHl(ticketPrincipalId, nuevaFamilia, true);
                respuesta.Data = "Proceso de Merge realizado exitosamente";
                respuesta.Message = "Proceso de Merge realizado exitosamente";
                respuesta.Success = true;
            }
            catch (TicketException e)
            {
                respuesta.Data = null;
                respuesta.Message = e.Message;
                respuesta.Success = false;
            }
            catch (Exception e)
            {
                respuesta.Data = "Proceso de Merge no se procesó exitosamente";
                respuesta.Message = e.Message;
                respuesta.Success = true;
            }
            return respuesta;
        }

        public ApplicationResponse<string> reenviarTicket(TicketReenviarDTO solicitudTicket)
        {
            ApplicationResponse<string> respuesta = new ApplicationResponse<string>();
            try
            {

                //Ticket ticket = new Ticket();
                //ticket = _dataContext.Tickets.Find(solicitudTicket.ticketPadre_Id);
                Ticket ticket = _dataContext.Tickets
                                                    .Include(x => x.Departamento_Destino).ThenInclude(x => x.grupo)
                                                    .Include(x => x.Prioridad)
                                                    .Include(x => x.Emisor).ThenInclude(x => x.Cargo).ThenInclude(x => x.Departamento)
                                                    .Include(x => x.Tipo_Ticket)
                                                    .Include(x=> x.Estado)
                                                    .Include(x=>x.Familia_Ticket)
                                                    .Include(x=>x.Prioridad)
                                                    .Include(x=>x.Votos_Ticket)
                                                    .Include(x=>x.Responsable)
                                                    .Where(x => x.Id == solicitudTicket.ticketPadre_Id).FirstOrDefault();
                Ticket nuevoTicket = new Ticket()
                {
                    Id = Guid.NewGuid(),
                    titulo = ticket.titulo,
                    descripcion = ticket.descripcion,
                    fecha_creacion = DateTime.UtcNow,
                    fecha_eliminacion = null,
                    Departamento_Destino = _dataContext.Departamentos.Find(solicitudTicket.departamentoDestino_Id),
                    Estado = ticket.Estado,
                    Prioridad = ticket.Prioridad,
                    Tipo_Ticket = ticket.Tipo_Ticket,
                    Votos_Ticket = ticket.Votos_Ticket,
                    Familia_Ticket = ticket.Familia_Ticket,
                    Ticket_Padre = ticket,
                    Emisor = ticket.Emisor,
                    Responsable = null,
                    ResponsableId = null,
                    nro_cargo_actual = ticket.nro_cargo_actual
                };
                ticket.fecha_creacion = DateTime.UtcNow;
                //ticket.Id = Guid.NewGuid();
                //ticket.Departamento_Destino = _dataContext.Departamentos.Find(solicitudTicket.departamentoDestino_Id);
                var bitacoras = new HashSet<Bitacora_Ticket>();
                bitacoras.AddRange(_dataContext.Bitacora_Tickets.Include(x => x.Ticket).Where(x => x.Ticket.Id == solicitudTicket.ticketPadre_Id));

                //ticket.Ticket_Padre = _dataContext.Tickets.Where(x => x.Id == solicitudTicket.ticketPadre_Id).FirstOrDefault();
                //Console.WriteLine($"Ticket Padre: {ticket.Ticket_Padre.Id}");
                foreach (var bitacora in bitacoras)
                {
                    var nuevabitacora = new Bitacora_Ticket();
                    nuevabitacora.Id = Guid.NewGuid();
                    nuevabitacora.Estado = bitacora.Estado;
                    nuevabitacora.Ticket = bitacora.Ticket;
                    nuevabitacora.Fecha_Inicio = bitacora.Fecha_Inicio;
                    nuevabitacora.Fecha_Fin= bitacora.Fecha_Fin;
                    
                }
                nuevoTicket.Bitacora_Tickets = bitacoras;
                _dataContext.Tickets.Add(nuevoTicket);
                //_dataContext.DbContext.SaveChanges();
                var ticketviejo = _dataContext.Tickets.Include(x => x.Departamento_Destino)
                    .Where(x => x.Id == solicitudTicket.ticketPadre_Id).FirstOrDefault();
                ticketviejo.fecha_eliminacion = DateTime.UtcNow;
                _dataContext.Tickets.Update(ticketviejo);
                _dataContext.DbContext.SaveChanges();
                respuesta.Data = "Exitoso";
                respuesta.Message = "Reenvío exitoso";
                respuesta.Success = true;
            }
            catch (TicketException e)
            {
                respuesta.Data = e.Message;
                respuesta.Message = e.Message;
                respuesta.Success = false;
            }
            catch (TicketDescripcionException e)
            {
                respuesta.Data = e.Message;
                respuesta.Message = e.Message;
                respuesta.Success = false;
            }
            catch (TicketEmisorException e)
            {
                respuesta.Data = e.Message;
                respuesta.Message = e.Message;
                respuesta.Success = false;
            }
            catch (TicketPrioridadException e)
            {
                respuesta.Data = e.Message;
                respuesta.Message = e.Message;
                respuesta.Success = false;
            }
            catch (TicketTipoException e)
            {
                respuesta.Data = e.Message;
                respuesta.Message = e.Message;
                respuesta.Success = false;
            }
            catch (TicketDepartamentoException e)
            {
                respuesta.Data = e.Message;
                respuesta.Message = e.Message;
                respuesta.Success = false;
            }
            catch (TicketPadreException e)
            {
                respuesta.Data = e.Message;
                respuesta.Message = e.Message;
                respuesta.Success = false;
            }
            catch (Exception e)
            {
                respuesta.Data = e.Message;
                respuesta.Message = e.Message;
                respuesta.Success = false;
            }
            return respuesta;
        }

        public ApplicationResponse<List<TicketInfoCompletaDTO>> obtenerFamiliaTicket(Guid ticketPrincipalId)
        {
            ApplicationResponse<List<TicketInfoCompletaDTO>> respuesta = new ApplicationResponse<List<TicketInfoCompletaDTO>>();
            try
            {
                TicketValidaciones validaciones = new TicketValidaciones(_dataContext);
                validaciones.validarTicket(ticketPrincipalId);
                List<TicketInfoCompletaDTO> lista = new List<TicketInfoCompletaDTO>();
                Ticket ticket = _dataContext.Tickets.Include(t => t.Familia_Ticket).Include(t => t.Familia_Ticket.Lista_Ticket).Where(t => t.Id == ticketPrincipalId).Single();
                if (ticket.Familia_Ticket == null)
                {
                    throw new Exception("El ticket no tiene familia definida");

                }
                ticket.Familia_Ticket.Lista_Ticket.ForEach(delegate (Ticket e)
                {
                    lista.Add(rellenarTicketInfoCompletaHl(e.Id));
                });
                if (lista.Count == 0)
                    throw new Exception("El ticket no tiene integrantes en su familia");
                respuesta.Data = lista;
                respuesta.Message = "Familia de tickets";
                respuesta.Success = true;
            }
            catch (TicketException e)
            {
                respuesta.Data = null;
                respuesta.Message = e.Message;
                respuesta.Success = true;
            }
            catch (Exception e)
            {
                respuesta.Data = null;
                respuesta.Message = e.Message;
                respuesta.Success = true;
            }
            return respuesta;
        }

        public ApplicationResponse<string> mergeTicketsHl(Guid ticketPrincipalId, List<Guid> ticketsSecundariosId)
        {
            ApplicationResponse<string> respuesta = new ApplicationResponse<string>();
            try
            {
                if (ticketsSecundariosId == null || ticketsSecundariosId.Count == 0)
                    throw new Exception("Lista de tickets secundarios no puede estar vacía");
                Familia_Ticket nuevaFamilia = new Familia_Ticket
                {
                    Id = Guid.NewGuid(),
                    Lista_Ticket = new List<Ticket>()
                };
                ticketsSecundariosId.ForEach(delegate (Guid e)
                {
                    anadirFamiliaHl(e, nuevaFamilia, false);
                });
                anadirFamiliaHl(ticketPrincipalId, nuevaFamilia, true);
                respuesta.Data = "Proceso de Merge realizado exitosamente";
                respuesta.Message = "Proceso de Merge realizado exitosamente";
                respuesta.Success = true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                respuesta.Data = "Proceso de Merge no se procesó correctamente";
                respuesta.Message = e.Message;
                respuesta.Success = false;
            }
            return respuesta;
        }

        public ApplicationResponse<string> eliminarTicket(Guid id)
        {
            ApplicationResponse<string> respuesta = new ApplicationResponse<string>();
            try
            {
                eliminarTicketHl(id);
                respuesta.Data = "Ticket finalizado satisfactoriamente";
                respuesta.Message = "Ticket finalizado satisfactoriamente";
                respuesta.Success = true;
            }
            catch (TicketException e)
            {
                respuesta.Data = "Proceso de eliminado no se procesó correctamente";
                respuesta.Message = e.Message;
                respuesta.Success = true;
            }
            catch (Exception e)
            {
                respuesta.Data = "Proceso de eliminado no se procesó correctamente";
                respuesta.Message = e.Message;
                respuesta.Success = true;
            }
            return respuesta;
        }
        public ApplicationResponse<List<DepartamentoSearchDTO>> buscarDepartamentos(Guid id)
        {
            ApplicationResponse<List<DepartamentoSearchDTO>> respuesta = new ApplicationResponse<List<DepartamentoSearchDTO>>();
            try
            {
                respuesta.Data = buscarDepartamentoTicketSolcitud(id);
                respuesta.Message = "Lista de departamentos obtenida exitosamente";
                respuesta.Success = true;
            }
            catch (Exception e)
            {
                respuesta.Data = null;
                respuesta.Message = $"Ha ocurrido un error, {e.Message}";
                respuesta.Success = true;
            }
            return respuesta;
        }

        public ApplicationResponse<DepartamentoSearchDTO> buscarDepartamentoEmpleado(Guid id)
        {
            ApplicationResponse<DepartamentoSearchDTO> respuesta = new ApplicationResponse<DepartamentoSearchDTO>();
            try
            {
                respuesta.Data = buscarDepartamentoUsuarioHl(id);
                respuesta.Message = "Lista de departamentos obtenida exitosamente";
                respuesta.Success = true;
            }
            catch (Exception e)
            {
                respuesta.Data = null;
                respuesta.Message = $"Ha ocurrido un error, {e.Message}";
                respuesta.Success = false;
            }
            return respuesta;
        }
        public ApplicationResponse<List<Tipo_TicketDTOSearch>> buscarTipoTickets(Guid id)
        {
            ApplicationResponse<List<Tipo_TicketDTOSearch>> respuesta = new ApplicationResponse<List<Tipo_TicketDTOSearch>>();
            try
            {
                respuesta.Data = obtenerTipoTickets(id);
                respuesta.Message = "Lista de Tipo tickets obtenida exitosamente";
                respuesta.Success = true;
            }
            catch (Exception e)
            {
                respuesta.Data = null;
                respuesta.Message = $"Ha ocurrido un error, {e.Message}";
                respuesta.Success = true;
            }
            return respuesta;
        }

        public ApplicationResponse<List<Tipo_Ticket>> buscarTiposTickets()
        {
            ApplicationResponse<List<Tipo_Ticket>> respuesta = new ApplicationResponse<List<Tipo_Ticket>>();
            try
            {
                respuesta.Data = obtenerTiposTicketsHl();
                respuesta.Message = "Lista de Tipo tickets obtenida exitosamente";
                respuesta.Success = true;
            }
            catch (Exception e)
            {
                respuesta.Data = null;
                respuesta.Message = $"Ha ocurrido un error, {e.Message}";
                respuesta.Success = true;
            }
            return respuesta;
        }

        public ApplicationResponse<List<Estado>> buscarEstadosPorDepartamento(Guid idDepartamento)
        {
            ApplicationResponse<List<Estado>> respuesta = new ApplicationResponse<List<Estado>>();
            try
            {
                respuesta.Data = buscarEstadosPorDepartamentoHl(idDepartamento);
                respuesta.Message = "Lista de Tipo tickets obtenida exitosamente";
                respuesta.Success = true;
            }
            catch (Exception e)
            {
                respuesta.Data = null;
                respuesta.Message = $"Ha ocurrido un error, {e.Message}";
                respuesta.Success = true;
            }
            return respuesta;
        }
        public ApplicationResponse<string> adquirirTicket(TicketTomarDTO ticketPropio)
        {
            ApplicationResponse<string> respuesta = new ApplicationResponse<string>();
            try
            {
                TicketValidaciones validaciones = new TicketValidaciones(_dataContext);
                validaciones.validarEmpleado(new Guid(ticketPropio.empleadoId));
                validaciones.validarTicket(new Guid(ticketPropio.ticketId));
                adquirirTicketHl(ticketPropio);
                respuesta.Data = "Ticket adquirido exitosamente";
                respuesta.Message = "Ticket adquirido exitosamente";
                respuesta.Success = true;
            }
            catch (Exception e)
            {
                respuesta.Data = null;
                respuesta.Message = $"Ha ocurrido un error, {e.Message}";
                respuesta.Success = true;
            }
            return respuesta;
        }
        public ApplicationResponse<List<TicketInfoBasicaDTO>> obtenerTicketsPropios(Guid idEmpleado)
        {
            ApplicationResponse<List<TicketInfoBasicaDTO>> respuesta = new ApplicationResponse<List<TicketInfoBasicaDTO>>();
            try
            {
                respuesta.Data = obtenerTicketsPropiosHl(idEmpleado);
                respuesta.Message = "Lista de tickets obtenida exitosamente";
                respuesta.Success = true;
            }
            catch (Exception e)
            {
                respuesta.Data = null;
                respuesta.Message = $"Ha ocurrido un error, {e.Message}";
                respuesta.Success = true;
            }
            return respuesta;
        }
        public ApplicationResponse<List<TicketInfoBasicaDTO>> obtenerTicketsEnviados(Guid idEmpleado)
        {
            ApplicationResponse<List<TicketInfoBasicaDTO>> respuesta = new ApplicationResponse<List<TicketInfoBasicaDTO>>();
            try
            {
                respuesta.Data = obtenerTicketsEnviadosHl(idEmpleado);
                respuesta.Message = "Lista de tickets obtenida exitosamente";
                respuesta.Success = true;
            }
            catch (Exception e)
            {
                respuesta.Data = null;
                respuesta.Message = e.Message;
                respuesta.Success = true;
            }
            return respuesta;
        }
        
        //HELPERS
        public TicketDTO crearNuevoTicket(TicketNuevoDTO solicitudTicket)
        {
            TicketDTO nuevoTicket = _mapper.Map<TicketDTO>(solicitudTicket);
            nuevoTicket.Id = Guid.NewGuid();
            nuevoTicket.fecha_creacion = DateTime.UtcNow;
            nuevoTicket.fecha_eliminacion = null;
            nuevoTicket.Emisor = _dataContext.Empleados
                                                .Include(t => t.Cargo)
                                                .Where(empleado => empleado.Id == solicitudTicket.empleado_id).FirstOrDefault();
            Cargo cargo = _dataContext.Cargos
                                       .Include(t => t.Departamento)
                                       .Where(t => t.id == nuevoTicket.Emisor.Cargo.id).FirstOrDefault();
            nuevoTicket.Departamento_Destino = _dataContext.Departamentos.Where(departamento => departamento.id == solicitudTicket.departamentoDestino_Id).FirstOrDefault();
            Estado estado = _dataContext.Estados
                                                .Include(t => t.Estado_Padre)
                                                .Include(t => t.Departamento)
                                                .Where(x => x.Estado_Padre.nombre == "Pendiente" && x.Departamento.id == cargo.Departamento.id).FirstOrDefault();
            if (estado == null)
                throw new Exception("No se halló el estado para el ticket");
            else
                nuevoTicket.Estado = estado;

            nuevoTicket.Prioridad = _dataContext.Prioridades.Where(prioridad => prioridad.Id == solicitudTicket.prioridad_id).FirstOrDefault();
            nuevoTicket.Tipo_Ticket = _dataContext.Tipos_Tickets.Where(tipoTicket => tipoTicket.Id == solicitudTicket.tipoTicket_id).FirstOrDefault();
            if (nuevoTicket.Tipo_Ticket.GetType()  == typeof(TipoTicket_FlujoAprobacionJerarquico))
                nuevoTicket.nro_cargo_actual = 1;
            else
                nuevoTicket.nro_cargo_actual = null;
            Guid? pruebaPadre = solicitudTicket.ticketPadre_Id;
            if (solicitudTicket.ticketPadre_Id != null)
            {
                nuevoTicket.Ticket_Padre = _dataContext.Tickets.Where(padre => padre.Id == solicitudTicket.ticketPadre_Id).FirstOrDefault();
                Ticket ticketPadre = _dataContext.Tickets.Where(t => t.Id == solicitudTicket.ticketPadre_Id).FirstOrDefault();
                if (ticketPadre != null)
                    ticketPadre.fecha_eliminacion = DateTime.Now;
            }
            else
            {
                nuevoTicket.Ticket_Padre = null;
            }

            nuevoTicket.Votos_Ticket = null;
            try
            {
                nuevoTicket.Bitacora_Tickets = new HashSet<Bitacora_Ticket>
                {
                    crearNuevaBitacora(nuevoTicket)
                };
            } catch (Exception e)
            {
                throw new Exception("No se pudo crear la Bitacora para el ticket", e);
            }
            if (nuevoTicket.Bitacora_Tickets == null)
                throw new Exception("La Bitacora para el ticket no pudo ser creada");
            //_dataContext.Bitacora_Tickets.Add(nuevoTicket.Bitacora_Tickets.First());
            _dataContext.Tickets.Add(_mapper.Map<Ticket>(nuevoTicket));
            _dataContext.DbContext.SaveChanges();
            //FlujoAprobacion(_mapper.Map<Ticket>(nuevoTicket));
            return nuevoTicket;
        }
        public Bitacora_Ticket crearNuevaBitacora(TicketDTO ticket)
        {
            Bitacora_Ticket nuevaBitacora = new Bitacora_Ticket()
            {
                Id = Guid.NewGuid(),
                Estado = ticket.Estado,
                Ticket = _mapper.Map<Ticket>(ticket),
                Fecha_Inicio = DateTime.Today,
                Fecha_Fin = null
            };
            return nuevaBitacora;
        }

        public Bitacora_Ticket crearNuevaBitacora(Ticket ticket)
        {
            Bitacora_Ticket nuevaBitacora = new Bitacora_Ticket()
            {
                Id = Guid.NewGuid(),
                Estado = ticket.Estado,
                Ticket = ticket,
                Fecha_Inicio = DateTime.UtcNow,
                Fecha_Fin = null
            };
            return nuevaBitacora;
        }
        public void modificarEstadoTicket(Guid ticketId, Guid estadoId)
        {
            Ticket ticket = _dataContext.Tickets
                                                .Include(t => t.Estado)
                                                .Include(t => t.Bitacora_Tickets)
                                                .Where(t => t.Id == ticketId).Single();
            Estado nuevoEstado = _dataContext.Estados.Where(t => t.Id == estadoId).Single();
            ticket.Estado = nuevoEstado;
            //ticket.Bitacora_Tickets.Last().Fecha_Fin = DateTime.UtcNow;
            Bitacora_Ticket nuevaBitacora = crearNuevaBitacora(ticket);
            ticket.Bitacora_Tickets.Add(nuevaBitacora);
            _dataContext.Bitacora_Tickets.Add(nuevaBitacora);
            _dataContext.Tickets.Update(ticket);
            _dataContext.DbContext.SaveChanges();
            Console.WriteLine("Aquí");
        }
        public List<TicketBitacorasDTO> obtenerBitacorasHl(Guid ticketId)
        {
            TicketValidaciones ticketValidaciones = new TicketValidaciones(_dataContext);
            ticketValidaciones.validarTicket(ticketId);
            List<Bitacora_Ticket> listaBitacoras = _dataContext.Bitacora_Tickets
                                                                    .AsNoTracking()
                                                                    .Include(x => x.Ticket)
                                                                    .Include(x => x.Estado)
                                                                    .Where(x => x.Ticket.Id == ticketId)
                                                                    .ToList();
            if (listaBitacoras.Count == 0)
                throw new Exception("Lista de Bitacoras vacías");
            List<TicketBitacorasDTO> bitacoras = new List<TicketBitacorasDTO>();
            listaBitacoras.ForEach(delegate (Bitacora_Ticket bitacora)
            {
                bitacoras.Add(new TicketBitacorasDTO
                {
                    Id = bitacora.Id,
                    estado_nombre = bitacora.Estado.nombre,
                    Fecha_Inicio = bitacora.Fecha_Inicio,
                    Fecha_Fin = bitacora.Fecha_Fin
                });
            });
            return bitacoras;
        }
        public void inicializarFamiliaTicketHl(TicketDTO nuevoTicket)
        {   //PARA LOS TICKETS HERMANOS
            nuevoTicket.Familia_Ticket = new Familia_Ticket()
            {
                Id = Guid.NewGuid(),
                Lista_Ticket = new List<Ticket>()
            };
        }
        public TicketInfoCompletaDTO rellenarTicketInfoCompletaHl(Guid id)
        {
            Ticket ticket = _dataContext.Tickets
                                .Include(t => t.Estado)
                                .Include(t => t.Tipo_Ticket)
                                .Include(t => t.Departamento_Destino)
                                .Include(t => t.Prioridad)
                                .Include(t => t.Emisor)
                                .Include(t => t.Responsable)
                                .Include(t => t.Ticket_Padre)
                                .Where(ticket => ticket.Id == id).Single();
            Guid? idPadre;
            if (ticket.Ticket_Padre == null || ticket.Ticket_Padre.Id.Equals(Guid.Empty))
                idPadre = null;
            else
                idPadre = ticket.Ticket_Padre.Id;
            Guid prueba = ticket.Estado.Id;
            return new TicketInfoCompletaDTO
            {
                ticket_id = id,
                ticketPadre_id = idPadre,
                encargado_correo = ticket.Responsable != null ? ticket.Responsable.correo : null,
                responsable = ticket.ResponsableId,
                fecha_creacion = ticket.fecha_creacion,
                fecha_eliminacion = ticket.fecha_eliminacion,
                titulo = ticket.titulo,
                descripcion = ticket.descripcion,
                estado_nombre = ticket.Estado.nombre,
                tipoTicket_nombre = ticket.Tipo_Ticket.nombre,
                departamentoDestino_nombre = ticket.Departamento_Destino.nombre,
                prioridad_nombre = ticket.Prioridad.nombre,
                empleado_correo = ticket.Emisor.correo,
            };
        }

        public List<TicketInfoBasicaDTO> rellenarTicketInfoBasicaHl(Guid idDepartamento, string opcion, Guid empleadoId)
        {
            List<TicketDTO> tickets;
            if (opcion == "Todos")
                tickets = _mapper.Map<List<TicketDTO>>(_dataContext.Tickets
                                                                    .Include(t => t.Emisor)
                                                                    .Include(t => t.Prioridad)
                                                                    .Include(t => t.Tipo_Ticket)
                                                                    .Include(t => t.Estado)
                                                                    .Include(t => t.Departamento_Destino)
                                                                    .Include(t => t.Ticket_Padre)
                                                                    .Where(ticket => ticket.Departamento_Destino.id == idDepartamento && ticket.Estado.Estado_Padre.nombre != "Pendiente" && ticket.Estado.Estado_Padre.nombre != "Rechazado").ToList());
            else if (opcion == "Abiertos")
                tickets = _mapper.Map<List<TicketDTO>>(_dataContext.Tickets
                                                                    .Include(t => t.Emisor)
                                                                    .Include(t => t.Prioridad)
                                                                    .Include(t => t.Tipo_Ticket)
                                                                    .Include(t => t.Estado)
                                                                    .Include(t => t.Departamento_Destino)
                                                                    .Include(t => t.Ticket_Padre)
                                                                    .Where(ticket => ticket.Departamento_Destino.id == idDepartamento && ticket.ResponsableId == null && ticket.fecha_eliminacion == null && ticket.Estado.Estado_Padre.nombre != "Pendiente" && ticket.Estado.Estado_Padre.nombre != "Rechazado").ToList());
            else if (opcion == "Cerrados")
                tickets = _mapper.Map<List<TicketDTO>>(_dataContext.Tickets
                                                                    .Include(t => t.Emisor)
                                                                    .Include(t => t.Prioridad)
                                                                    .Include(t => t.Tipo_Ticket)
                                                                    .Include(t => t.Estado)
                                                                    .Include(t => t.Departamento_Destino)
                                                                    .Include(t => t.Ticket_Padre)
                                                                    .Where(ticket => ticket.Departamento_Destino.id == idDepartamento && ticket.fecha_eliminacion != null && ticket.Estado.Estado_Padre.nombre != "Pendiente" && ticket.Estado.Estado_Padre.nombre != "Rechazado").ToList());
            else if (opcion == "Mis-Tickets")
                tickets = _mapper.Map<List<TicketDTO>>(_dataContext.Tickets
                                                                    .Include(t => t.Emisor)
                                                                    .Include(t => t.Prioridad)
                                                                    .Include(t => t.Tipo_Ticket)
                                                                    .Include(t => t.Estado)
                                                                    .Include(t => t.Departamento_Destino)
                                                                    .Include(t => t.Ticket_Padre)
                                                                    .Where(ticket => ticket.Departamento_Destino.id == idDepartamento && ticket.ResponsableId == empleadoId && ticket.fecha_eliminacion == null && ticket.Estado.Estado_Padre.nombre != "Pendiente" && ticket.Estado.Estado_Padre.nombre != "Rechazado").ToList());
            else
                throw new TicketException("Lista de tickets no encontrada debido a que la opción de búsqueda no es válido");
            if (tickets.Count() == 0)
                throw new TicketException("No existen tickets que satisfagan el tipo de búsqueda");

            List<TicketInfoBasicaDTO> respuesta = new List<TicketInfoBasicaDTO>();
            tickets.ForEach(delegate (TicketDTO ticket)
            {
                respuesta.Add(new TicketInfoBasicaDTO
                {
                    Id = ticket.Id,
                    titulo = ticket.titulo,
                    empleado_correo = ticket.Emisor.correo,
                    encargado_correo = ticket.Responsable != null ? ticket.Responsable.correo : null,
                    prioridad_nombre = ticket.Prioridad.nombre,
                    fecha_creacion = ticket.fecha_creacion,
                    fecha_eliminacion = ticket.fecha_eliminacion,
                    tipoTicket_nombre = ticket.Tipo_Ticket.nombre,
                    estado_nombre = ticket.Estado.nombre,
                    ticket_padre = ticket.Ticket_Padre != null ? ticket.Ticket_Padre.Id : null
                });
            });
            return respuesta;
        }
        public void anadirFamiliaHl(Guid id, Familia_Ticket nuevaFamilia, bool ticketPrincipal)
        {
            Ticket ticket = _dataContext.Tickets.Where(t => t.Id == id).Single();
            nuevaFamilia.Lista_Ticket.Add(ticket);
            if (!ticketPrincipal)
            {
                ticket.fecha_eliminacion = DateTime.UtcNow;
                nuevaFamilia.Lista_Ticket.Add(ticket);
            }
            else
            {
                _dataContext.Familia_Tickets.Add(nuevaFamilia);
                ticket.Familia_Ticket = nuevaFamilia;
            }
            _dataContext.DbContext.Update(ticket);
            _dataContext.DbContext.SaveChanges();
        }
        public void eliminarTicketHl(Guid id)
        {
            TicketValidaciones validaciones = new TicketValidaciones(_dataContext);
            validaciones.validarTicket(id);
            Ticket ticket = _dataContext.Tickets.Where(t => t.Id == id).Single();
            ticket.fecha_eliminacion = DateTime.UtcNow;
            _dataContext.DbContext.Update(ticket);
            _dataContext.DbContext.SaveChanges();
        }
        public List<DepartamentoSearchDTO> buscarDepartamentoTicketSolcitud(Guid usuarioId)
        {
            if (!_dataContext.Empleados.Include(t => t.Cargo).Where(t => t.Id == usuarioId).Any())
                throw new Exception("Usuario no es un empleado, por lo tanto no puede crear un ticket");
            Empleado empleado = _dataContext.Empleados.Include(t => t.Cargo).Where(t => t.Id == usuarioId).Single();
            if (empleado.Cargo == null)
                throw new Exception("Empleado no tiene cargo asignado");
            Cargo cargo = _dataContext.Cargos.Include(t => t.Departamento).Where(t => t.id == empleado.Cargo.id).Single();
            if (!_dataContext.Departamentos.Where(t => t.id == cargo.Departamento.id).Any())
                throw new Exception("No existen departamentos acorde al cargo del empleado");
            List<DepartamentoSearchDTO> lista = _mapper.Map<List<DepartamentoSearchDTO>>(_dataContext.Departamentos.Where(t => t.id != cargo.Departamento.id).ToList());
            Console.WriteLine($"Lista: {lista.Count()}");
            return lista;
        }

        public DepartamentoSearchDTO buscarDepartamentoUsuarioHl(Guid idEmpleado)
        {
            if (!_dataContext.Empleados.Include(t => t.Cargo).Where(t => t.Id == idEmpleado).Any())
                throw new Exception("Usuario no es un empleado, por lo tanto no puede crear un ticket");
            Empleado empleado = _dataContext.Empleados.Include(t => t.Cargo).Where(t => t.Id == idEmpleado).Single();
            if (empleado.Cargo == null)
                throw new Exception("Empleado no tiene cargo asignado");
            Cargo cargo = _dataContext.Cargos.Include(t => t.Departamento).Where(t => t.id == empleado.Cargo.id).Single();
            if (!_dataContext.Departamentos.Where(t => t.id == cargo.Departamento.id).Any())
                throw new Exception("No existen departamentos acorde al cargo del empleado");
            return _mapper.Map<DepartamentoSearchDTO>(_dataContext.Departamentos.Where(t => t.id == cargo.Departamento.id).Single());
        }

        public List<Tipo_TicketDTOSearch> obtenerTipoTickets(Guid departamentoId)
        {
            Tipo_TicketService tipoTicket = new Tipo_TicketService(_dataContext, _mapper);
            return tipoTicket.ConsultaTipoTicketAgregarTicket(departamentoId);
        }

        public List<Tipo_Ticket> obtenerTiposTicketsHl()
        {
            return _dataContext.Tipos_Tickets.Include(t => t.Departamentos).ToList();
        }
        public List<Estado> buscarEstadosPorDepartamentoHl(Guid idDepartamento)
        {
            return _dataContext.Estados.Include(t => t.Departamento).Include(t => t.Estado_Padre).Where(t => t.Departamento.id == idDepartamento && t.Estado_Padre.nombre != "Aprobado" && t.Estado_Padre.nombre != "Rechazado" && t.Estado_Padre.nombre != "Pendiente" && t.Estado_Padre.nombre != "Siendo Procesado").ToList();
        }
        public void adquirirTicketHl(TicketTomarDTO ticketPropio)
        {
            Empleado empleado = _dataContext.Empleados.Include(t=>t.Tickets_Propios).Where(t => t.Id == new Guid(ticketPropio.empleadoId)).Single();
            Ticket ticket = _dataContext.Tickets.Where(t => t.Id == new Guid(ticketPropio.ticketId)).Single();
            ticket.Responsable = empleado;
            if (empleado.Tickets_Propios == null)
                empleado.Tickets_Propios = new List<Ticket>();
            empleado.Tickets_Propios.Add(ticket);
            _dataContext.DbContext.Update(ticket);
            _dataContext.DbContext.Update(empleado);
            _dataContext.DbContext.SaveChanges();
        }
        public List<TicketInfoBasicaDTO> obtenerTicketsPropiosHl(Guid idEmpleado)
        {
            List<Ticket> tickets = _dataContext.Tickets
                                                .Include(t => t.Responsable)
                                                .Include(t =>t.Emisor)
                                                .Include(t => t.Prioridad)
                                                .Include(t=>t.Tipo_Ticket)
                                                .Include(t=>t.Estado)
                                                .Where(t => t.Responsable.Id == idEmpleado)
                                                .ToList();
            if (tickets.Count() > 0)
            {
                List<TicketInfoBasicaDTO> respuesta = new List<TicketInfoBasicaDTO>();
                tickets.ForEach(delegate (Ticket ticket)
                {
                    respuesta.Add(new TicketInfoBasicaDTO
                    {
                        Id = ticket.Id,
                        titulo = ticket.titulo,
                        empleado_correo = ticket.Emisor.correo,
                        prioridad_nombre = ticket.Prioridad.nombre,
                        fecha_creacion = ticket.fecha_creacion,
                        fecha_eliminacion = ticket.fecha_eliminacion,
                        tipoTicket_nombre = ticket.Tipo_Ticket.nombre,
                        estado_nombre = ticket.Estado.nombre
                    });
                });
                return respuesta;
            }
            else
                throw new Exception("Empleado no tiene tickets propios");
        }
        public bool CambiarEstado(Ticket ticketLlegada, string Estado, List<Empleado> ListaEmpleados)
        {
            try
            {
                var ticket = _dataContext.Tickets.Include(x => x.Departamento_Destino).ThenInclude(x => x.grupo).Include(x => x.Prioridad)
                    .Include(x => x.Emisor).ThenInclude(x => x.Cargo).ThenInclude(x => x.Departamento)
                    .Include(x => x.Tipo_Ticket).Include(x => x.Votos_Ticket)
                    .Include(x => x.Bitacora_Tickets)
                    .Include(x => x.Ticket_Padre)
                    .Include(x => x.Familia_Ticket)
                    .Where(x => x.Id == ticketLlegada.Id).FirstOrDefault();
                Console.WriteLine("CONSIGUE EL TICKET");
                Estado nuevoEstado = _dataContext.Estados
                    .Include(x => x.Estado_Padre)
                    .Include(x => x.Departamento).
                    Where(s => s.Estado_Padre.nombre == Estado &&
                    s.Departamento.id == ticket.Departamento_Destino.id)
                    .FirstOrDefault();
                Console.WriteLine($"EL ESTADO: {nuevoEstado.nombre}");

                // if (ticket.Ticket_Padre != null)
                //     CambiarEstado(ticket.Ticket_Padre, Estado, ListaEmpleados);
                Ticket ticketPtro = ticket.Ticket_Padre;
                while(ticketPtro != null)
                {
                    Console.WriteLine("ENTRA EN EL WHILE");
                    Ticket t = _dataContext.Tickets.Include(x=>x.Bitacora_Tickets).Include(x=> x.Estado).Include(x => x.Ticket_Padre).Where(x => x.Id == ticketPtro.Id).Single();
                    t.Estado = nuevoEstado;
                    Bitacora_Ticket nuevaBitacoraPadre = crearNuevaBitacora(ticketPtro);
                    if (t.Bitacora_Tickets.Count != 0)
                    {
                        t.Bitacora_Tickets.Last().Fecha_Fin = DateTime.UtcNow;
                    }
                    t.Bitacora_Tickets.Add(nuevaBitacoraPadre);
                    _dataContext.Bitacora_Tickets.Add(nuevaBitacoraPadre);
                    _dataContext.Tickets.Update(t);
                    _dataContext.DbContext.SaveChanges();
                    Console.WriteLine("AQUÍ");
                    ticketPtro = t.Ticket_Padre;
                }
                Console.WriteLine("SALE DEL WHILE");
                if (ticket.Familia_Ticket != null && ticket.Familia_Ticket.Lista_Ticket != null)
                {
                    foreach(var t in ticket.Familia_Ticket.Lista_Ticket)
                    {
                        Console.WriteLine("ENTRA EN EL FOREACH");
                        t.Estado = nuevoEstado;
                        _dataContext.Tickets.Update(t);
                        _dataContext.DbContext.SaveChanges();
                    }
                }
                Console.WriteLine("SALE DEL FOREACH");
                ticket.Estado = nuevoEstado;
                _dataContext.Tickets.Update(ticket);
                _dataContext.DbContext.SaveChanges();

                Bitacora_Ticket nuevaBitacora = crearNuevaBitacora(ticket);
                if (ticket.Bitacora_Tickets.Count != 0)
                {
                    ticket.Bitacora_Tickets.Last().Fecha_Fin = DateTime.UtcNow;
                }
                ticket.Bitacora_Tickets.Add(nuevaBitacora);
                _dataContext.Bitacora_Tickets.Add(nuevaBitacora);
                _dataContext.Tickets.Update(ticket);
                _dataContext.DbContext.SaveChanges();
                //vticket.State = EntityState.Modified;
                if (Estado == "Aprobado")
                {
                    try
                    {
                        var plant = plantilla.ConsultarPlantillaTipoEstadoID(ticket.Estado.Estado_Padre.Id);
                        plant.Descripcion = notificacion.ReemplazoEtiqueta(ticket, plant);
                        notificacion.EnviarCorreo(plant, ticket.Emisor.correo);

                    }
                    catch (ExceptionsControl) { }
                    CambiarEstado(ticket, "Siendo Procesado", null);
                    return true;
                }

                if (Estado == "Siendo Procesado")
                {
                    var empleados = _dataContext.Empleados.Include(x => x.Cargo).ThenInclude(x => x.Departamento).Where(x => x.Cargo.Departamento.id == ticket.Departamento_Destino.id).ToList();
                    var plant2 = plantilla.ConsultarPlantillaTipoEstadoID(ticket.Estado.Estado_Padre.Id);
                    plant2.Descripcion = notificacion.ReemplazoEtiqueta(ticket, plant2);
                    foreach (var emp in empleados)
                    {
                        try
                        {
                            notificacion.EnviarCorreo(plant2, emp.correo);
                        }
                        catch (ExceptionsControl) { }
                    }
                    Console.WriteLine("Cambiar Estado 2");
                    return true;
                }

                if (Estado == "Pendiente")
                {
                    var plant2 = plantilla.ConsultarPlantillaTipoEstadoID(ticket.Estado.Estado_Padre.Id);
                    plant2.Descripcion = notificacion.ReemplazoEtiqueta(ticket, plant2);
                    foreach (var emp in ListaEmpleados)
                    {
                        try
                        {
                            notificacion.EnviarCorreo(plant2, emp.correo);
                        }
                        catch (ExceptionsControl) { }
                    }
                    return true;
                }

                try
                {
                    Console.WriteLine("ULTIMO TRY");
                    var plant = plantilla.ConsultarPlantillaTipoEstadoID(ticket.Estado.Estado_Padre.Id);
                    plant.Descripcion= notificacion.ReemplazoEtiqueta(ticket, plant);
                    notificacion.EnviarCorreo(plant, ticket.Emisor.correo);
                }
                catch (ExceptionsControl) { }
            }
            catch (ExceptionsControl ex)
            {
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine("ENTRA EN MÍ EXCEPCIÓN " + ex.Message);
                return false;
            }
            return true;
        }
        public List<TicketInfoBasicaDTO> obtenerTicketsEnviadosHl(Guid idEmpleado)
        {
            List<TicketInfoBasicaDTO> nuevaLista = new List<TicketInfoBasicaDTO>();
            List<Ticket> tickets = _dataContext.Tickets
                                                        .Include(t=>t.Emisor)
                                                        .Include(t=>t.Responsable)
                                                        .Include(t=>t.Prioridad)
                                                        .Include(t=>t.Tipo_Ticket)
                                                        .Include(t=>t.Estado)
                                                        .Where(t=>t.Emisor.Id == idEmpleado)
                                                        .ToList();
                                                        
            if (tickets.Count() == 0)
                throw new Exception("Empleado no tiene tickets creados");
            tickets.ForEach(delegate (Ticket ticket)
            {
                nuevaLista.Add(new TicketInfoBasicaDTO
                {
                    Id = ticket.Id,
                    titulo = ticket.titulo,
                    empleado_correo = ticket.Emisor.correo,
                    encargado_correo = ticket.Responsable != null ? ticket.Responsable.correo : null,
                    prioridad_nombre = ticket.Prioridad.nombre,
                    fecha_creacion = ticket.fecha_creacion,
                    fecha_eliminacion = ticket.fecha_eliminacion,
                    tipoTicket_nombre = ticket.Tipo_Ticket.nombre,
                    estado_nombre = ticket.Estado.nombre
                });
            });
            return nuevaLista;
        }
    }
}
