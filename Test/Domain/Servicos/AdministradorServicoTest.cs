using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MinimalApi.Dominio.Entidades;
using MinimalApi.Dominio.Servicos;
using MinimalApi.Infraestrutura.Db;

namespace Test;

[TestClass]
public class AdministradorServicoTest
{
    private DbContexto CriarContextoDeTestes()
    {
        var assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        var path = Path.GetFullPath(Path.Combine(assemblyPath ?? "", "..", "..", ".."));
        var builder = new ConfigurationBuilder()
            .SetBasePath(path ?? Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddEnvironmentVariables();
        var configuration = builder.Build();

        return new DbContexto(configuration);
    }
    [TestMethod]
    public void TestarSalvarAdministrador()
    {
        //Arrange
        var contexto = CriarContextoDeTestes();
        contexto.Database.ExecuteSqlRaw("TRUNCATE TABLE Administradores");

        var adm = new Administrador
        {
            Email = "teste@teste.com",
            Senha = "teste",
            Perfil = "adm"
        };

        var administradorServico = new AdministradorServico(contexto);
        //Act
        administradorServico.Incluir(adm);


        //Assert
        Assert.AreEqual(1, administradorServico.Todos(1).Count);

    }

    [TestMethod]
    public void TestarBuscaPorId()
    {
        //Arrange
        var contexto = CriarContextoDeTestes();
        contexto.Database.ExecuteSqlRaw("TRUNCATE TABLE Administradores");

        var adm = new Administrador();
        adm.Email = "teste@teste.com";
        adm.Senha = "teste";
        adm.Perfil = "adm";

        var administradorServico = new AdministradorServico(contexto);
        //Act
        administradorServico.Incluir(adm);
        var admBusca = administradorServico.BuscaPorId(adm.Id);


        //Assert
        Assert.AreEqual(1, adm.Id);

    }
    
    [TestMethod]
    public void TestarTodos()
    {
        //Arrange
        var contexto = CriarContextoDeTestes();
        contexto.Database.ExecuteSqlRaw("TRUNCATE TABLE Administradores");

        var adm = new Administrador
        {
            Email = "teste@teste.com",
            Senha = "teste",
            Perfil = "adm"
        };

        var administradorServico = new AdministradorServico(contexto);
        //Act
        administradorServico.Incluir(adm);
        var admsBusca = administradorServico.Todos(1);


        //Assert
        Assert.AreEqual(1, admsBusca.Count);

    }
}