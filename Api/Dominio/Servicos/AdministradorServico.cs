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

        public Administrador? BuscaPorId(int id)
        {
           return _dbContexto.Administradores.Find(id);
        }

        public void Incluir(Administrador administrador)
        {
            _dbContexto.Administradores.Add(administrador);
            _dbContexto.SaveChanges();
        }

        public Administrador? Login(LoginDTO loginDTO)
        {
            var administrador = _dbContexto.Administradores.Where(
                adm => adm.Email == loginDTO.Email && adm.Senha == loginDTO.Senha
            ).FirstOrDefault();
            return administrador;
        }

        public List<Administrador> Todos(int? pagina = 1)
        {
            var query = _dbContexto.Administradores.AsQueryable();

            if (pagina != null)
            {
                int itensPorPagina = 10;
                query = query.Skip(((int)pagina - 1) * itensPorPagina).Take(itensPorPagina);
            }

            return query.ToList();
        }
    }
}