using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MinimalApi.Dominio.DTOs
{
    public record VeiculoDTO
    {
        public String? Nome { get; set; }
        public String? Marca { get; set; }
        public int Ano { get; set; }
    }
}