﻿using System.ComponentModel.DataAnnotations;
using System;

namespace ServicesDeskUCABWS.BussinesLogic.Grupo_I.Gestion_de_Usuario.Dto
{
    public class UserDto_Update
    {
        public Guid id { get; set; }

        public int cedula { get; set; }

        [MaxLength(50)]
        [MinLength(3)]
        public string primer_nombre { get; set; } = string.Empty;

        [MaxLength(50)]
        [MinLength(3)]
        public string segundo_nombre { get; set; } = string.Empty;

        [MaxLength(50)]
        [MinLength(3)]
        public string primer_apellido { get; set; } = string.Empty;
        [MaxLength(50)]
        [MinLength(3)]
        public string segundo_apellido { get; set; } = string.Empty;

        public DateTime fecha_nacimiento { get; set; }
    }
}
