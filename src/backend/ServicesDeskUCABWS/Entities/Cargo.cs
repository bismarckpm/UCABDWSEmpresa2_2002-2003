﻿using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using System;

namespace ServicesDeskUCABWS.Entities
{
    public class Cargo
    {

        [Key]
        public Guid id { get; set; }
        [Required]
        [StringLength(50)]
        public string nombre_departamental { get; set; } = string.Empty;

        [Required]
        [StringLength(150)]
        public string descripcion { get; set; } = string.Empty;

        [Required]
        public DateTime fecha_creacion { get; set; }

        [JsonIgnore]
        public DateTime? fecha_ultima_edicion { get; set; }
        public DateTime? fecha_eliminacion { get; set; }
    }
}
