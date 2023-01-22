﻿using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ServicesDeskUCAB.Models;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using ServiceDeskUCAB.Models.DTO.TicketDTO;
using ServiceDeskUCAB.Models;
using ServiceDeskUCAB.Models.ModelsVotos;
using ServiceDeskUCAB.Models.DTO.DepartamentoDTO;
using ServiceDeskUCAB.Models.DTO.Tipo_TicketDTO;
using ServiceDeskUCAB.Models.TipoTicketsModels;
using ServiceDeskUCAB.Models.Response;

namespace ServiceDeskUCAB.Servicios
{
	public class ServicioTicketAPI : IServicioTicketAPI
	{
        private static string _baseUrl;

        public ServicioTicketAPI()
        {
            var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json").Build();

            _baseUrl = builder.GetSection("ApiSettings:baseUrl").Value;
        }


        public async Task<TicketCompletoDTO> Obtener(string ticketId)
        {
            TicketCompletoDTO objeto = new TicketCompletoDTO();
            try
            {
                var cliente = new HttpClient();
                cliente.BaseAddress = new Uri(_baseUrl);
                var response = await cliente.GetAsync($"Ticket/Obtener/{ticketId}");

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Es success obtener");
                    var respuesta = await response.Content.ReadAsStringAsync();
                    JObject json_respuesta = JObject.Parse(respuesta);

                    //Obtengo la data del json respuesta
                    Console.WriteLine(json_respuesta["data"].ToString());
                    string stringDataRespuesta = json_respuesta["data"].ToString();
                    var resultado = JsonConvert.DeserializeObject<TicketCompletoDTO>(stringDataRespuesta);
                    objeto = resultado;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("no entra");
            }
            return objeto;
        }

        public async Task<List<TicketCompletoDTO>> FamiliaTicket(string ticketId)
        {
            List<TicketCompletoDTO> objeto = new List<TicketCompletoDTO>();
            try
            {
                var cliente = new HttpClient();
                cliente.BaseAddress = new Uri(_baseUrl);
                var response = await cliente.GetAsync($"Ticket/Familia/{ticketId}");
                if (response.IsSuccessStatusCode)
                {
                    var respuesta = await response.Content.ReadAsStringAsync();
                    JObject json_respuesta = JObject.Parse(respuesta);
                    string stringDataRespuesta = json_respuesta["data"].ToString();
                    var resultado = JsonConvert.DeserializeObject<List<TicketCompletoDTO>>(stringDataRespuesta);
                    objeto = resultado;
                    Console.WriteLine("Obtiene la familia del ticket");
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"ERROR de conexión con la API: '{ex.Message}'");

            }
            catch (Exception e)
            {
                Console.WriteLine("No obtiene la familia del ticket, algo a sucedido ", e.Message);
            }
            return objeto;
        }

        public async Task<List<BitacoraDTO>> BitacoraTicket(string ticketId)
        {
            List<BitacoraDTO> lista = new List<BitacoraDTO>();
            try
            {
                var cliente = new HttpClient();
                cliente.BaseAddress = new Uri(_baseUrl);
                var response = await cliente.GetAsync($"Ticket/Bitacora/{ticketId}");
                if (response.IsSuccessStatusCode)
                {
                    var respuesta = await response.Content.ReadAsStringAsync();
                    JObject json_respuesta = JObject.Parse(respuesta);
                    string stringDataRespuesta = json_respuesta["data"].ToString();
                    var resultado = JsonConvert.DeserializeObject<List<BitacoraDTO>>(stringDataRespuesta);
                    lista = resultado;
                    Console.WriteLine("Obtiene la bitacora del ticket");
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"ERROR de conexión con la API: '{ex.Message}'");

            }
            catch (Exception e)
            {
                Console.WriteLine("No obtiene la bitacora del ticket, algo a sucedido ", e.Message);
            }
            return lista;
        }

        public async Task<List<TicketBasicoDTO>> Lista(string departamentoId, string opcion, string empleadoId) 
        {
            List<TicketBasicoDTO> objeto = new List<TicketBasicoDTO>();
            try
            {
                var cliente = new HttpClient();
                cliente.BaseAddress = new Uri(_baseUrl);
                var response = await cliente.GetAsync($"Ticket/Lista/{departamentoId}/{opcion}/{empleadoId}");
                if (response.IsSuccessStatusCode)
                {
                    var respuesta = await response.Content.ReadAsStringAsync();
                    JObject json_respuesta = JObject.Parse(respuesta);
                    string stringDataRespuesta = json_respuesta["data"].ToString();
                    var resultado = JsonConvert.DeserializeObject<List<TicketBasicoDTO>>(stringDataRespuesta);
                    if (resultado == null) { resultado = new List<TicketBasicoDTO>(); }
                    objeto = resultado;
                    Console.WriteLine("Obtiene los tickets");
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"ERROR de conexión con la API: '{ex.Message}'");

            }
            catch (Exception e)
            {
                Console.WriteLine("No obtiene los tickets, algo a sucedido ", e.Message);
            }
            return objeto;
        }

        public async Task<List<TicketBasicoDTO>> TicketsEnviados(string idempleado)
        {
            List<TicketBasicoDTO> objeto = new List<TicketBasicoDTO>();
            Console.WriteLine("AQUÍ LLEGA");
            try
            {
                var cliente = new HttpClient();
                cliente.BaseAddress = new Uri(_baseUrl);
                var response = await cliente.GetAsync($"Ticket/ObtenerTicketsEnviados/{idempleado}");
                Console.WriteLine("AQUÍ LLEGA");
                if (response.IsSuccessStatusCode)
                {
                    var respuesta = await response.Content.ReadAsStringAsync();
                    JObject json_respuesta = JObject.Parse(respuesta);
                    string stringDataRespuesta = json_respuesta["data"].ToString();
                    var resultado = JsonConvert.DeserializeObject<List<TicketBasicoDTO>>(stringDataRespuesta);
                    if (resultado == null) { resultado = new List<TicketBasicoDTO>(); }
                    objeto = resultado;
                    Console.WriteLine($"AQUÍ: {resultado}");
                    Console.WriteLine("Obtiene los tickets");
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"ERROR de conexión con la API: '{ex.Message}'");

            }
            catch (Exception e)
            {
                Console.WriteLine("No obtiene los tickets, algo a sucedido ", e.Message);
            }
            return objeto;
        }

        public async Task<List<DepartamentoSearchDTO>> Departamentos(string empleadoId)
        {
            List<DepartamentoSearchDTO> objeto = new List<DepartamentoSearchDTO>();
            try
            {
                var cliente = new HttpClient();
                cliente.BaseAddress = new Uri(_baseUrl);
                var response = await cliente.GetAsync($"Ticket/ObtenerDepartamentos/{empleadoId}");
                if (response.IsSuccessStatusCode)
                {
                    var respuesta = await response.Content.ReadAsStringAsync();
                    JObject json_respuesta = JObject.Parse(respuesta);
                    string stringDataRespuesta = json_respuesta["data"].ToString();
                    var resultado = JsonConvert.DeserializeObject<List<DepartamentoSearchDTO>>(stringDataRespuesta);
                    if (resultado == null) { resultado = new List<DepartamentoSearchDTO>(); }
                    objeto = resultado;
                    Console.WriteLine("Obtiene los departamentos");
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"ERROR de conexión con la API: '{ex.Message}'");

            }
            catch (Exception e)
            {
                Console.WriteLine("No obtiene los departamentos, algo ha sucedido ", e.Message);
            }
            return objeto;
        }

        public async Task<List<Tipo_TicketDTOSearch>> TipoTickets(Guid idDepartamento)
        {
            List<Tipo_TicketDTOSearch> objeto = new List<Tipo_TicketDTOSearch>();
            try
            {
                var cliente = new HttpClient();
                cliente.BaseAddress = new Uri(_baseUrl);
                var response = await cliente.GetAsync($"Ticket/ObtenerTipoTickets/{idDepartamento}");
                if (response.IsSuccessStatusCode)
                {
                    var respuesta = await response.Content.ReadAsStringAsync();
                    JObject json_respuesta = JObject.Parse(respuesta);
                    string stringDataRespuesta = json_respuesta["data"].ToString();
                    var resultado = JsonConvert.DeserializeObject<List<Tipo_TicketDTOSearch>>(stringDataRespuesta);
                    if (resultado == null) { resultado = new List<Tipo_TicketDTOSearch>(); }
                    objeto = resultado;
                    Console.WriteLine("Obtiene los tipo tickets");
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"ERROR de conexión con la API: '{ex.Message}'");

            }
            catch (Exception e)
            {
                Console.WriteLine("No obtiene los tipo tickets, algo ha sucedido ", e.Message);
            }
            return objeto;
        }

        public async Task<List<Tipo>> TiposTickets()
        {
            List<Tipo> objeto = new List<Tipo>();
            try
            {
                var cliente = new HttpClient();
                cliente.BaseAddress = new Uri(_baseUrl);
                var response = await cliente.GetAsync($"Ticket/ObtenerTiposTickets/");
                if (response.IsSuccessStatusCode)
                {
                    var respuesta = await response.Content.ReadAsStringAsync();
                    JObject json_respuesta = JObject.Parse(respuesta);
                    string stringDataRespuesta = json_respuesta["data"].ToString();
                    var resultado = JsonConvert.DeserializeObject<List<Tipo>>(stringDataRespuesta);
                    if (resultado == null) { resultado = new List<Tipo>(); }
                    objeto = resultado;
                    Console.WriteLine("Obtiene los tipo tickets");
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"ERROR de conexión con la API: '{ex.Message}'");

            }
            catch (Exception e)
            {
                Console.WriteLine("No obtiene los tipo tickets, algo ha sucedido ", e.Message);
            }
            return objeto;
        }

        public async Task<List<Estado>> DepartamentoEstados(string departamentoId)
        {
            List<Estado> objeto = new List<Estado>();
            try
            {
                var cliente = new HttpClient();
                cliente.BaseAddress = new Uri(_baseUrl);
                var response = await cliente.GetAsync($"Ticket/ObtenerEstadosPorDepartamento/{departamentoId}");
                if (response.IsSuccessStatusCode)
                {
                    var respuesta = await response.Content.ReadAsStringAsync();
                    JObject json_respuesta = JObject.Parse(respuesta);
                    string stringDataRespuesta = json_respuesta["data"].ToString();
                    var resultado = JsonConvert.DeserializeObject<List<Estado>>(stringDataRespuesta);
                    if (resultado == null) { resultado = new List<Estado>(); }
                    objeto = resultado;
                    Console.WriteLine("Obtiene los tipo tickets");
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"ERROR de conexión con la API: '{ex.Message}'");

            }
            catch (Exception e)
            {
                Console.WriteLine("No obtiene los tipo tickets, algo ha sucedido ", e.Message);
            }
            return objeto;
        }

      
        public async Task<ApplicationResponse<DepartamentoSearchDTO>> departamentoEmpleado(string empleadoId)
        {
            var objeto = new ApplicationResponse<DepartamentoSearchDTO>();
            try
            {
                var cliente = new HttpClient();
                cliente.BaseAddress = new Uri(_baseUrl);
                var response = await cliente.GetAsync($"Ticket/ObtenerDepartamentoEmpleado/{empleadoId}");
                if (response.IsSuccessStatusCode)
                {
                    var respuesta = await response.Content.ReadAsStringAsync();
                    var json_respuesta = JsonConvert.DeserializeObject<ApplicationResponse<DepartamentoSearchDTO>>(respuesta);
                    /*string stringDataRespuesta = json_respuesta["data"].ToString();

                    var resultado = JsonConvert.DeserializeObject<DepartamentoSearchDTO>(stringDataRespuesta);
                    if (resultado == null) { resultado = new DepartamentoSearchDTO(); }*/
                    objeto = json_respuesta;
                    Console.WriteLine("Obtiene el departamento del empleado");
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"ERROR de conexión con la API: '{ex.Message}'");

            }
            catch (Exception e)
            {
                Console.WriteLine("No obtiene los departamentos, algo ha sucedido ", e.Message);
            }
            
            return objeto;
        }

        public async Task<JObject> TomarTicket(TicketTomarDTO objeto)
        {

            var cliente = new HttpClient();
            cliente.BaseAddress = new Uri(_baseUrl);
            var content = new StringContent(JsonConvert.SerializeObject(objeto), Encoding.UTF8, "application/json");
            try
            {
                var response = await cliente.PostAsync($"Ticket/Tomar/", content);
                var respuesta = await response.Content.ReadAsStringAsync();
                JObject _json_respuesta = JObject.Parse(respuesta);
                return _json_respuesta;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"ERROR de conexión con la API: '{ex.Message}'");
                return null;

            }
            catch (Exception e)
            {
                Console.WriteLine("No obtiene los tickets, algo a sucedido ", e.Message);
                return null;
            }
        }

        [HttpPost]
        public async Task<JObject> Guardar(TicketDTO Objeto)
        {
            var cliente = new HttpClient();
            cliente.BaseAddress = new Uri(_baseUrl);
            var content = new StringContent(JsonConvert.SerializeObject(Objeto), Encoding.UTF8, "application/json");
            try
            {
                var response = await cliente.PostAsync($"Ticket/Guardar/", content);
                var respuesta = await response.Content.ReadAsStringAsync();
                JObject _json_respuesta = JObject.Parse(respuesta);
                return _json_respuesta;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"ERROR de conexión con la API: '{ex.Message}'");

            }
            catch (Exception e)
            {
                Console.WriteLine("No obtiene los tickets, algo a sucedido ", e.Message);
            }
            return null;
        }

        [HttpPost]
        public async Task<JObject> GuardarReenviar(TicketReenviarDTO Objeto)
        {
            var cliente = new HttpClient();
            cliente.BaseAddress = new Uri(_baseUrl);
            var content = new StringContent(JsonConvert.SerializeObject(Objeto), Encoding.UTF8, "application/json");
            try
            {
                var response = await cliente.PostAsync($"Ticket/Reenviar/", content);
                var respuesta = await response.Content.ReadAsStringAsync();
                JObject _json_respuesta = JObject.Parse(respuesta);
                return _json_respuesta;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"ERROR de conexión con la API: '{ex.Message}'");

            }
            catch (Exception e)
            {
                Console.WriteLine("No obtiene los tickets, algo a sucedido ", e.Message);
            }
            return null;
        }

        [HttpPost]
        public async Task<JObject> GuardarMerge(FamiliaMergeDTO Objeto)
        {
            var cliente = new HttpClient();
            cliente.BaseAddress = new Uri(_baseUrl);
            var content = new StringContent(JsonConvert.SerializeObject(Objeto), Encoding.UTF8, "application/json");
            try
            {
                var response = await cliente.PostAsync($"Ticket/Merge/", content);
                var respuesta = await response.Content.ReadAsStringAsync();
                JObject _json_respuesta = JObject.Parse(respuesta);
                return _json_respuesta;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"ERROR de conexión con la API: '{ex.Message}'");

            }
            catch (Exception e)
            {
                Console.WriteLine("No obtiene los tickets, algo a sucedido ", e.Message);
            }
            return null;
        }

        public async Task<JObject> Finalizar(string Objeto)
        {
            var cliente = new HttpClient();
            cliente.BaseAddress = new Uri(_baseUrl);
            var content = new StringContent(JsonConvert.SerializeObject(Objeto), Encoding.UTF8, "application/json");
            try
            {
                var response = await cliente.PostAsync($"Ticket/Finalizar/", content);
                var respuesta = await response.Content.ReadAsStringAsync();
                JObject _json_respuesta = JObject.Parse(respuesta);
                return _json_respuesta;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"ERROR de conexión con la API: '{ex.Message}'");
                return null;

            }
            catch (Exception e)
            {
                Console.WriteLine("No obtiene los tickets, algo a sucedido ", e.Message);
                return null;
            }
        }

        [HttpPost]
        public async Task<JObject> CambiarEstado(ActualizarDTO Objeto)
        {
            var cliente = new HttpClient();
            cliente.BaseAddress = new Uri(_baseUrl);
            var content = new StringContent(JsonConvert.SerializeObject(Objeto), Encoding.UTF8, "application/json");
            try
            {
                var response = await cliente.PostAsync("Ticket/CambiarEstado/", content);
                var respuesta = await response.Content.ReadAsStringAsync();
                JObject _json_respuesta = JObject.Parse(respuesta);
                return _json_respuesta;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"ERROR de conexión con la API: '{ex.Message}'");

            }
            catch (Exception e)
            {
                Console.WriteLine("No cambia el estado algo sucedio ", e.Message);
            }
            return null;
        }

        

    }
}


