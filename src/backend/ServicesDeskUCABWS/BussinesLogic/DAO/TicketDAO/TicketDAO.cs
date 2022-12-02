﻿using AutoMapper;
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
using System.Net.Sockets;
using ServicesDeskUCABWS.BussinesLogic.Response;
using Microsoft.Data.SqlClient;
using ServicesDeskUCABWS.BussinesLogic.Exceptions;
using System.Threading.Tasks;

namespace ServicesDeskUCABWS.BussinesLogic.DAO.TicketDAO
{
    public class TicketDAO : ITicketDAO
    {
        private readonly IDataContext _dataContext;
        private readonly IMapper _mapper;
        private List<Ticket> listaTickets;

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

        public ApplicationResponse<TicketCreateDTO> RegistroTicket(TicketCreateDTO ticketDTO)
        {

            var response = new ApplicationResponse<TicketCreateDTO>();
            try
            {

                var ticket = new Ticket(ticketDTO.titulo, ticketDTO.descripcion);
                ticket.Prioridad = _dataContext.Prioridades.Find(Guid.Parse(ticketDTO.Prioridad));
                //List<Empleado> v =(List<Empleado>) contexto.Usuarios.ToList();
                //Prioridad t = contexto.Prioridades.Find();//Include(x => x.Cargo).ThenInclude(x => x.Departamento).ToList();
                //.Where(s => s.Id == Guid.Parse(ticketDTO.Emisor)).FirstOrDefault();
                ticket.Emisor = _dataContext.Empleados.Include(x => x.Cargo).ThenInclude(x => x.Departamento)
                    .Where(s => s.Id == Guid.Parse(ticketDTO.Emisor)).FirstOrDefault();
                ticket.Departamento_Destino = _dataContext.Departamentos.Find(Guid.Parse(ticketDTO.Departamento_Destino));
                ticket.Tipo_Ticket = _dataContext.Tipos_Tickets.Find(Guid.Parse(ticketDTO.Tipo_Ticket));
                ticket.Estado = _dataContext.Estados.Where(x => x.Estado_Padre.nombre == "Pendiente" &&
                x.Departamento.id == ticket.Emisor.Cargo.Departamento.id).FirstOrDefault();
                _dataContext.Tickets.Add(ticket);

                _dataContext.DbContext.SaveChanges();
                response.Data = ticketDTO;
                response.Exception = FlujoAprobacion(ticket);

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

        }

        public string FlujoAprobacion(Ticket ticket)
        {
            string result = null;
            switch (ticket.Tipo_Ticket.tipo)
            {
                case "Modelo_No_Aprobacion":
                    result = FlujoNoAprobacion(ticket);
                    break;
                case "Modelo_Paralelo":
                    result = FlujoParalelo(ticket);
                    break;
                case "Modelo_Jerarquico":
                    result = FlujoJerarquico(ticket);
                    break;

            }
            return result;
        }

        public string FlujoNoAprobacion(Ticket ticket)
        {
            string result = null;
            try
            {
                //Cambiar estado Ticket
                CambiarEstado(ticket, "Aprobado");

                //EnviarNotificacion(ticket.Emisor, ticket.Estado);
                List<Empleado> ListaEmpleado = _dataContext.Empleados.
                    Where(s => s.Cargo.Departamento.id == ticket.Departamento_Destino.id)
                    .ToList();
                //EnviarNotificacion(ListaEmpleado, ticket.Estado);


                _dataContext.DbContext.SaveChanges();
                return result;
            }
            catch (Exception ex)
            {
                result = ex.Message;
                return result;
            }
        }


        public string FlujoParalelo(Ticket ticket)
        {
            string result = null;
            try
            {

                var tipoCargos = _dataContext.Flujos_Aprobaciones
                    .Include(x => x.Tipo_Cargo)
                    .ThenInclude(x => x.Cargos_Asociados)
                    .Where(x => x.IdTicket == ticket.Tipo_Ticket.Id);

                var Cargos = new List<Cargo>();
                foreach (var tc in tipoCargos)
                {
                    Cargos.Add(tc.Tipo_Cargo.Cargos_Asociados.ToList()
                        .Where(x => x.Departamento.id == ticket.Emisor.Cargo.Departamento.id).First());
                }

                var ListaEmpleado = new List<Empleado>();
                foreach (var c in Cargos)
                {
                    ListaEmpleado.AddRange(_dataContext.Empleados.Where(x => x.Cargo.Id == c.Id));
                }

                var ListaVotos = ListaEmpleado.Select(x => new Votos_Ticket
                {
                    IdTicket = ticket.Id,
                    Ticket = ticket,
                    IdUsuario = x.Id,
                    Empleado = x,
                    voto = "Pendiente"
                });

                _dataContext.Votos_Tickets.AddRange(ListaVotos);

                //EnviarNotificacion(ListaEmpleado, ticket.Estado);

                _dataContext.DbContext.SaveChanges();

                return result;
            }
            catch (ExceptionsControl ex)
            {
                result = ex.Message;
                return result;
            }
        }

        public string FlujoJerarquico(Ticket ticket)
        {
            string result = null;
            try
            {
                ticket.nro_cargo_actual = 1;

                var tipoCargos = _dataContext.Flujos_Aprobaciones
                    .Include(x => x.Tipo_Cargo)
                    .ThenInclude(x => x.Cargos_Asociados)
                    .ThenInclude(x => x.Departamento)
                    .Where(x => x.IdTicket == ticket.Tipo_Ticket.Id)
                    .OrderBy(x => x.OrdenAprobacion).First();


                var Cargos = tipoCargos.Tipo_Cargo.Cargos_Asociados.ToList()
                    .Where(x => x.Departamento.id == ticket.Emisor.Cargo.Departamento.id).First();


                var ListaEmpleado = _dataContext.Empleados.Where(x => x.Cargo.Id == Cargos.Id).ToList();


                var ListaVotos = ListaEmpleado.Select(x => new Votos_Ticket
                {
                    IdTicket = ticket.Id,
                    Ticket = ticket,
                    IdUsuario = x.Id,
                    Empleado = x,
                    voto = "Pendiente",
                    Turno = ticket.nro_cargo_actual
                });

                _dataContext.Votos_Tickets.AddRange(ListaVotos);

                //EnviarNotificacion(ListaEmpleado, ticket.Estado);

                _dataContext.DbContext.SaveChanges();

                return result;
            }
            catch (ExceptionsControl ex)
            {
                result = ex.Message;
                return result;
            }
        }

        public bool CambiarEstado(Ticket ticket, string Estado)
        {
            try
            {
                ticket.Estado = _dataContext.Estados.Include(x => x.Estado_Padre).Include(x => x.Departamento).
                    Where(s => s.Estado_Padre.nombre == Estado &&
                    s.Departamento.id == ticket.Emisor.Cargo.Departamento.id)
                    .FirstOrDefault();
                var vticket = _dataContext.Tickets.Update(ticket);
                vticket.State = EntityState.Modified;

            }
            catch (ExceptionsControl ex)
            {
                return false;
            }
            return true;
        }

        public TicketDAO(IDataContext dataContext, IMapper mapper)
        {
            _dataContext = dataContext;
            _mapper = mapper;
        }

        public ApplicationResponse<string> crearTicket(TicketNuevoDTO solicitudTicket)
        {
            ApplicationResponse<string> respuesta = new ApplicationResponse<string>();
            try
            {
                TicketValidaciones validaciones = new TicketValidaciones(_dataContext);
                validaciones.nuevoTicketEsValido(solicitudTicket);
                crearNuevoTicket(solicitudTicket);
                respuesta.Data = "Ticket creado satisfactoriamente";
                respuesta.Message = "Ticket creado satisfactoriamente";
                respuesta.Success = true;
            } catch(TicketException e)
            {
                respuesta.Data = e.Message;
                respuesta.Message = e.Message;
                respuesta.Success = false;
            } catch(TicketDescripcionException e)
            {
                respuesta.Data = e.Message;
                respuesta.Message = e.Message;
                respuesta.Success = false;
            } catch(TicketEmisorException e)
            {
                respuesta.Data = e.Message;
                respuesta.Message = e.Message;
                respuesta.Success = false;
            } catch(TicketPrioridadException e)
            {
                respuesta.Data = e.Message;
                respuesta.Message = e.Message;
                respuesta.Success = false;
            } catch(TicketTipoException e)
            {
                respuesta.Data = e.Message;
                respuesta.Message = e.Message;
                respuesta.Success = false;
            } catch(TicketDepartamentoException e)
            {
                respuesta.Data = e.Message;
                respuesta.Message = e.Message;
                respuesta.Success = false;
            } catch(Exception e)
            {
                respuesta.Data = e.Message;
                respuesta.Message = e.Message;
                respuesta.Success = false;
            }
            return respuesta;
        }

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

        public ApplicationResponse<List<TicketInfoBasicaDTO>> obtenerTicketsPorEstadoYDepartamento(Guid idDepartamento, string estado)
        {
            ApplicationResponse<List<TicketInfoBasicaDTO>> respuesta = new ApplicationResponse<List<TicketInfoBasicaDTO>>();
            try
            {
                TicketValidaciones validaciones = new TicketValidaciones(_dataContext);
                validaciones.validarDepartamento(idDepartamento);
                respuesta.Data = rellenarTicketInfoBasicaHl(idDepartamento, estado);
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
                modificarEstadoTicket(ticketId, estadoId);
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
                respuesta.Message = $"Mano sendo error: {e.Message}";
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
            TicketNuevoDTO ticket = new TicketNuevoDTO();
            ticket.departamentoDestino_Id = solicitudTicket.departamentoDestino_Id;
            ticket.descripcion = solicitudTicket.descripcion;
            ticket.empleado_id = solicitudTicket.empleado_id;
            ticket.prioridad_id = solicitudTicket.prioridad_id;
            ticket.tipoTicket_id = solicitudTicket.tipoTicket_id;
            ticket.titulo = solicitudTicket.titulo;
            ticket.ticketPadre_Id = solicitudTicket.ticketPadre_Id;
            ticket.nro_cargo_actual = solicitudTicket.nro_cargo_actual;

            try
            {
                TicketValidaciones validaciones = new TicketValidaciones(_dataContext);
                validaciones.nuevoTicketEsValido(ticket);
                TicketDTO nuevoTicket = crearNuevoTicket(ticket);

                respuesta.Data = "Ticket Reenviado satisfactoriamente";
                respuesta.Message = "Ticket Reenviado satisfactoriamente";
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
                    throw new Exception("El ticket no tiene familia definida");
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
                respuesta.Data = "Ticket eliminado satisfactoriamente";
                respuesta.Message = "Ticket eliminado satisfactoriamente";
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
                                       .Where(t => t.Id == nuevoTicket.Emisor.Cargo.Id).FirstOrDefault();
            Guid prueba = cargo.Departamento.id;
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
            Guid? pruebaPadre = solicitudTicket.ticketPadre_Id;
            if(solicitudTicket.ticketPadre_Id != null)
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
            nuevoTicket.nro_cargo_actual = null;
            nuevoTicket.Votos_Ticket = null;
            try
            {
                nuevoTicket.Bitacora_Tickets = new HashSet<Bitacora_Ticket>
                {
                    crearNuevaBitacora(nuevoTicket)
                };
            } catch(Exception e)
            {
                throw new Exception("No se pudo crear la Bitacora para el ticket", e);
            }
            if (nuevoTicket.Bitacora_Tickets == null)
                throw new Exception("La Bitacora para el ticket no pudo ser creada");
            _dataContext.Tickets.Add(_mapper.Map<Ticket>(nuevoTicket));
            //_dataContext.Bitacora_Tickets.Add(nuevoTicket.Bitacora_Tickets.First());
            _dataContext.DbContext.SaveChanges();
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
        public void modificarEstadoTicket(Guid ticketId, Guid estadoId)
        {
            Ticket ticket = _dataContext.Tickets.Include(t => t.Estado).Include(t => t.Bitacora_Tickets).Where(t => t.Id == ticketId).Single();
            Estado nuevoEstado = _dataContext.Estados.Where(t => t.Id == estadoId).Single();
            ticket.Estado = nuevoEstado;
            ticket.Bitacora_Tickets.Last().Fecha_Fin = DateTime.UtcNow;
            Bitacora_Ticket nuevaBitacora = crearNuevaBitacora(_mapper.Map<TicketDTO>(ticket));
            ticket.Bitacora_Tickets.Add(nuevaBitacora);
            _dataContext.Tickets.Update(ticket);
            _dataContext.DbContext.SaveChanges();
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
        public List<TicketInfoBasicaDTO> rellenarTicketInfoBasicaHl(Guid idDepartamento, string opcion)
        {
            List<TicketDTO> tickets;
            if (opcion == "Todos")
                tickets = _mapper.Map<List<TicketDTO>>(_dataContext.Tickets
                                                                    .Include(t => t.Emisor)
                                                                    .Include(t => t.Prioridad)
                                                                    .Include(t => t.Tipo_Ticket)
                                                                    .Include(t => t.Estado)
                                                                    .Include(t=>t.Departamento_Destino)
                                                                    .Where(ticket => ticket.Departamento_Destino.id == idDepartamento && ticket.Estado.Estado_Padre.nombre != "Pendiente" && ticket.Estado.Estado_Padre.nombre != "Rechazado").ToList());
            else if (opcion == "Abiertos")
                tickets = _mapper.Map<List<TicketDTO>>(_dataContext.Tickets
                                                                    .Include(t => t.Emisor)
                                                                    .Include(t => t.Prioridad)
                                                                    .Include(t => t.Tipo_Ticket)
                                                                    .Include(t => t.Estado)
                                                                    .Include(t => t.Departamento_Destino)
                                                                    .Where(ticket => ticket.Departamento_Destino.id == idDepartamento && ticket.fecha_eliminacion == null && ticket.Estado.Estado_Padre.nombre != "Pendiente" && ticket.Estado.Estado_Padre.nombre != "Rechazado").ToList());
            else if (opcion == "Cerrados")
                tickets = _mapper.Map<List<TicketDTO>>(_dataContext.Tickets
                                                                    .Include(t => t.Emisor)
                                                                    .Include(t => t.Prioridad)
                                                                    .Include(t => t.Tipo_Ticket)
                                                                    .Include(t => t.Estado)
                                                                    .Include(t => t.Departamento_Destino)
                                                                    .Where(ticket => ticket.Departamento_Destino.id == idDepartamento && ticket.fecha_eliminacion != null && ticket.Estado.Estado_Padre.nombre != "Pendiente" && ticket.Estado.Estado_Padre.nombre != "Rechazado").ToList());
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
                    prioridad_nombre = ticket.Prioridad.nombre,
                    fecha_creacion = ticket.fecha_creacion,
                    fecha_eliminacion = ticket.fecha_eliminacion,
                    tipoTicket_nombre = ticket.Tipo_Ticket.nombre,
                    estado_nombre = ticket.Estado.nombre
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
    }
}
