
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MinimalApi.Dominio.DTOs;
using MinimalApi.Dominio.Entidades;
using MinimalApi.Dominio.Enums;
using MinimalApi.Dominio.Interfaces;
using MinimalApi.Dominio.ModelViews;
using MinimalApi.Dominio.Servicos;
using MinimalApi.Infraestrutura.Db;
#region Builder
var builder = WebApplication.CreateBuilder(args);

var key = builder.Configuration.GetSection("Jwt").ToString();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(option =>
{
    option.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateLifetime = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
    };
});

builder.Services.AddAuthorization();

builder.Services.AddScoped<iAdministradorServico, AdministradorServico>();
builder.Services.AddScoped<iVeiculoServico, VeiculoServico>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<DbContexto>(options =>
{
    options.UseMySql(builder.Configuration.GetConnectionString("mysql"),
    ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("mysql")));
});
var app = builder.Build();
#endregion

#region Home
app.MapGet("/", () => Results.Json(new Home())).WithTags("Home");
#endregion

#region Administradores
app.MapPost("administradores/login", ([FromBody] LoginDTO loginDTO, iAdministradorServico administradorServico) =>
{
    if (administradorServico.Login(loginDTO) != null)
        return Results.Ok("Login com sucesso");
    else
        return Results.Unauthorized();
}).WithTags("Administrador");

app.MapPost("administradores", ([FromBody] SalvarAdministradorDTO administradorDTO, iAdministradorServico administradorServico) =>
{
    var validacao = ValidaAdministrador(administradorDTO);
    if (validacao.Mensagens.Count > 0)
    {
        return Results.UnprocessableEntity(validacao);
    }
    Administrador administrador = new Administrador
    {
        Email = administradorDTO.Email,
        Perfil = administradorDTO.Perfil.ToString(),
        Senha = administradorDTO.Senha
    };

    administradorServico.Incluir(administrador);
    return Results.Created($"/administradores/{administrador.Id}", administrador);
}).WithTags("Administrador");

app.MapGet("administradores", ([FromQuery] int? pagina, iAdministradorServico administradorServico) =>
{
    var administradoresEntidade = administradorServico.Todos(pagina);
    var administradoresDTO = new List<BuscarAdministradorDTO>();

    foreach(var administrador in administradoresEntidade)
    {
        administradoresDTO.Add(new BuscarAdministradorDTO
        {
            Email = administrador.Email,
            Perfil = (Perfil)Enum.Parse(typeof(Perfil), administrador.Perfil)
        });
    }



    return Results.Ok(administradoresDTO);
}).WithTags("Administrador");

app.MapGet("administradores/{id}", ([FromRoute] int id, iAdministradorServico administradorServico) =>
{
    var administrador = administradorServico.BuscaPorId(id);
    if (administrador == null) return Results.NotFound();
    var administradorDTO = new BuscarAdministradorDTO
    {
        Email = administrador.Email,
        Perfil = (Perfil)Enum.Parse(typeof(Perfil), administrador.Perfil)
    };
    return Results.Ok(administradorDTO);
}).WithTags("Administrador");
#endregion

#region Veiculos
app.MapPost("veiculos/login", ([FromBody] VeiculoDTO veiculoDTO, iVeiculoServico veiculoServico) =>
{
    var validacao = ValidaVeiculo(veiculoDTO);
    if (validacao.Mensagens.Count > 0)
    {
        return Results.UnprocessableEntity(validacao);
    }

    var veiculo = new Veiculo
    {
        Nome = veiculoDTO.Nome,
        Marca = veiculoDTO.Marca,
        Ano = veiculoDTO.Ano,
    };
    veiculoServico.Incluir(veiculo);

    return Results.Created($"/veiculo/{veiculo.Id}", veiculo);

}).WithTags("Veiculo");

app.MapGet("veiculos", ([FromQuery] int? pagina, iVeiculoServico veiculoServico) =>
{
    var veiculos = veiculoServico.Todos(pagina);
    return Results.Ok(veiculos);
}).WithTags("Veiculo");

app.MapGet("veiculos/{id}", ([FromRoute] int id, iVeiculoServico veiculoServico) =>
{
    var veiculo = veiculoServico.BuscaPorId(id);
    if (veiculo == null) return Results.NotFound();
    return Results.Ok(veiculo);
}).WithTags("Veiculo");

app.MapPut("veiculos/{id}", ([FromRoute] int id,
                             [FromBody] VeiculoDTO veiculoDTO,
                             iVeiculoServico veiculoServico) =>
{
    var veiculo = veiculoServico.BuscaPorId(id);
    if (veiculo == null) return Results.NotFound();
    var validacao = ValidaVeiculo(veiculoDTO);
    if (validacao.Mensagens.Count > 0)
    {
        return Results.UnprocessableEntity(validacao);
    }

    veiculo.Ano = veiculoDTO.Ano;
    veiculo.Marca = veiculoDTO.Marca;
    veiculo.Nome = veiculoDTO.Nome;
    veiculoServico.Atualizar(veiculo);
    return Results.Ok(veiculo);
}).WithTags("Veiculo");

app.MapDelete("veiculos/{id}", ([FromRoute] int id, iVeiculoServico veiculoServico) =>
{
    var veiculo = veiculoServico.BuscaPorId(id);
    if (veiculo == null) return Results.NotFound();
    veiculoServico.Apagar(veiculo);
    return Results.NoContent();
}).WithTags("Veiculo");


#endregion



ErrosDeValidacao ValidaVeiculo(VeiculoDTO veiculoDTO)
{
    var validacao = new ErrosDeValidacao
    {
        Mensagens = []
    };
    if (string.IsNullOrEmpty(veiculoDTO.Marca)) validacao.Mensagens.Add("A marca não pode ficar em branco");
    if (string.IsNullOrEmpty(veiculoDTO.Nome)) validacao.Mensagens.Add("O nome não pode ficar em branco");
    if (veiculoDTO.Ano <= 1950) validacao.Mensagens.Add("O ano não pode ser inferior a 1950");

    return validacao;
}

ErrosDeValidacao ValidaAdministrador(SalvarAdministradorDTO administradorDTO)
{
    var validacao = new ErrosDeValidacao
    {
        Mensagens = []
    };
    if (string.IsNullOrEmpty(administradorDTO.Email)) validacao.Mensagens.Add("O email não pode ficar em branco");
    if (string.IsNullOrEmpty(administradorDTO.Senha)) validacao.Mensagens.Add("A senha não pode ficar em branco");
    if (administradorDTO.Perfil == null) validacao.Mensagens.Add("O ano não pode ser inferior a 1950");

    return validacao;
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication();
app.UseAuthorization();

app.Run();


