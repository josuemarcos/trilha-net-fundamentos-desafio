using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MinimalApi.Dominio.Enums;

namespace MinimalApi.Dominio.DTOs
{
    public record SalvarAdministradorDTO
    {
        public string? Email { get; set; }
        public string? Senha { get; set; }
        public Perfil? Perfil { get; set; }
    }
}