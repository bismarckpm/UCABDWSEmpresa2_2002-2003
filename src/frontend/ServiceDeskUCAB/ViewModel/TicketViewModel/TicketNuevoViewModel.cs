﻿using System;
using System.Collections.Generic;
using ServiceDeskUCAB.Models;
using ServiceDeskUCAB.Models.TipoTicketsModels;
using ServiceDeskUCAB.Models.ModelsVotos;
using ServiceDeskUCAB.Models.DTO.PrioridadDTO;
using ServiceDeskUCAB.Models.DTO.DepartamentoDTO;
using ServiceDeskUCAB.Models.DTO.Tipo_TicketDTO;

namespace ServiceDeskUCAB.ViewModel
{
	public class TicketNuevoViewModel
	{
		public TicketDTO ticket;
		public List<DepartamentoSearchDTO> departamentos;
		public List<PrioridadDTO> prioridades;
		public List<Tipo_TicketDTOSearch> tipo_tickets;

	}
}

