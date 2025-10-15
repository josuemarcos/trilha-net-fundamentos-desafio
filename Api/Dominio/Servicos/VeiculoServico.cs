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
    public class VeiculoServico : iVeiculoServico
    {
        private readonly DbContexto _dbContexto;
        public VeiculoServico(DbContexto dbContexto)
        {
            _dbContexto = dbContexto;
        }

        public void Apagar(Veiculo veiculo)
        {
            _dbContexto.Veiculos.Remove(veiculo);
            _dbContexto.SaveChanges();
        }

        public void Atualizar(Veiculo veiculo)
        {
            _dbContexto.Veiculos.Update(veiculo);
            _dbContexto.SaveChanges();
        }

        public Veiculo? BuscaPorId(int id)
        {
           return _dbContexto.Veiculos.Find(id);
        }

        public void Incluir(Veiculo veiculo)
        {
            _dbContexto.Veiculos.Add(veiculo);
            _dbContexto.SaveChanges();
        }

        public List<Veiculo> Todos(int? pagina = 1, string? nome = null, string? marca = null)
        {
            var query = _dbContexto.Veiculos.AsQueryable();
            if (!string.IsNullOrEmpty(nome))
            {
                query = query.Where(veiculo => veiculo.Nome.ToLower().Contains(nome));
            }
            else if (!string.IsNullOrEmpty(marca))
            {
                query = query.Where(veiculo => veiculo.Marca.ToLower().Contains(marca));
            }

            if (pagina != null)
            {
                int itensPorPagina = 10;
                query = query.Skip(((int)pagina - 1) * itensPorPagina).Take(itensPorPagina);
            }

            return query.ToList();
        }
        
    }
}