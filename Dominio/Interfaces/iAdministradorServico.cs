using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MinimalApi.Dominio.DTOs;
using MinimalApi.Dominio.Entidades;

namespace MinimalApi.Dominio.Interfaces
{
    public interface iAdministradorServico
    {
        Administrador? Login(LoginDTO loginDTO);
        void Incluir(Administrador administrador);
        List<Administrador> Todos(int? pagina = 1);
    }
}