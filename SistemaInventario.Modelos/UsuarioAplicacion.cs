using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaInventario.Modelos
{
    public class UsuarioAplicacion:IdentityUser
    {
        [Required(ErrorMessage ="Los Nombres son Requeridos")]
        [MaxLength(80,ErrorMessage ="Máximo 80 Caracteres")]
        public string Nombres { get; set; }

        [Required(ErrorMessage = "Los Apellidos son Requeridos")]
        [MaxLength(100, ErrorMessage = "Máximo 100 Caracteres")]
        public string Apellidos { get; set; }

        [Required(ErrorMessage = "La Dirección es Requerida")]
        [MaxLength(200, ErrorMessage = "Máximo 200 Caracteres")]
        public string Direccion { get; set; }

        [Required(ErrorMessage = "La Ciudad es Requerida")]
        [MaxLength(60, ErrorMessage = "Máximo 60 Caracteres")]
        public string Ciudad { get; set; }

        [Required(ErrorMessage = "El Pais es Requerido")]
        [MaxLength(60, ErrorMessage = "Máximo 60 Caracteres")]
        public string Pais { get; set; }

        [NotMapped]
        public string Role { get; set; }
    }
}
