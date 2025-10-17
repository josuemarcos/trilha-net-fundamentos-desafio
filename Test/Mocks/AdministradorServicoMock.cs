using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using MinimalApi.Dominio.DTOs;
using MinimalApi.Dominio.Entidades;
using MinimalApi.Dominio.Interfaces;
using MinimalApi.Infraestrutura.Db;

namespace Test.Mocks
{
    public class AdministradorServicoMock : iAdministradorServico
    {
        
        private static List<Administrador> administradores = [
            new Administrador {
                Id = 1,
                Email = "adm@teste.com",
                Senha = "12345",
                Perfil = "adm"
            },
            new Administrador {
                Id = 2,
                Email = "editor@teste.com",
                Senha = "12345",
                Perfil = "editor"
            }

        ];
        public Administrador? BuscaPorId(int id)
        {
            return administradores.Find(administrador => administrador.Id == id);
        }

        public void Incluir(Administrador administrador)
        {
            administrador.Id = administradores.Count + 1;
            administradores.Add(administrador);
        }

        public Administrador? Login(LoginDTO loginDTO)
        {
            var administrador = administradores.Find(administrador =>
            administrador.Email == loginDTO.Email && administrador.Senha == loginDTO.Senha);
            return administrador;
        }

        public List<Administrador> Todos(int? pagina = 1)
        {
            return administradores;
        }
    }
}