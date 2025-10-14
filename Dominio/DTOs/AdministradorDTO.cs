using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MinimalApi.Dominio.DTOs
{
    public record AdministradorDTO
    {
        public string? Email { get; set; }
        public string? Senha { get; set; }
        public string? Perfil { get; set; }
    }
}