﻿using System;
using System.Collections.Generic;
using ServicesDeskUCAB.Models;

namespace ServicesDeskUCAB.ViewModel
{
	public class TicketReenviarDTOViewModel
    {
		public TicketCompletoDTO ticketPadre;
		public TicketReenviarDTO ticket;
		public List<Departamento> departamentos;
		public List<Prioridad> prioridades;
		public List<Tipo_Ticket> tipo_tickets;
	}
}

