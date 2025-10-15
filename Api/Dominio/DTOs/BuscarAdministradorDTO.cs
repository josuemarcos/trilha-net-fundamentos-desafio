using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MinimalApi.Dominio.Enums;

namespace MinimalApi.Dominio.DTOs
{
    public record BuscarAdministradorDTO
    {
        public string? Email { get; set; }
        public Perfil? Perfil { get; set; }
    }
}