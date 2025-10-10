using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MinimalApi.Dominio.DTOs;
using MinimalApi.Dominio.Entidades;
using MinimalApi.Dominio.Interfaces;
using MinimalApi.Infraestrutura.Db;

namespace MinimalApi.Dominio.Servicos
{
    public class AdministradorServico : iAdministradorServico
    {
        private readonly DbContexto _dbContexto;
        public AdministradorServico(DbContexto dbContexto)
        {
            _dbContexto = dbContexto;
        }
        public Administrador? Login(LoginDTO loginDTO)
        {
            var administrador = _dbContexto.Administradores.Where(
                adm => adm.Email == loginDTO.Email && adm.Senha == loginDTO.Senha
            ).FirstOrDefault();
            return administrador;
        }
    }
}