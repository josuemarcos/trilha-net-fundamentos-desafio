using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using  MinimalApi;
using MinimalApi.Dominio.DTOs;
using MinimalApi.Dominio.Entidades;
using MinimalApi.Dominio.Enums;
using MinimalApi.Dominio.Interfaces;
using MinimalApi.Dominio.ModelViews;
using MinimalApi.Dominio.Servicos;
using MinimalApi.Infraestrutura.Db;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
        key = Configuration.GetSection("Jwt").ToString();
    }
    public IConfiguration Configuration { get; set; }
    private readonly string? key;

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(option =>
        {
            option.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateLifetime = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                ValidateIssuer = false,
                ValidateAudience = false
            };
        });

        services.AddAuthorization();

        services.AddScoped<iAdministradorServico, AdministradorServico>();
        services.AddScoped<iVeiculoServico, VeiculoServico>();

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Insira o token JWT aqui"
            });
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });

        services.AddDbContext<DbContexto>(options =>
        {
            options.UseMySql(Configuration.GetConnectionString("mysql"),
            ServerVersion.AutoDetect(Configuration.GetConnectionString("mysql")));
        });
    }
    
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseSwagger();
        app.UseSwaggerUI();

        app.UseRouting();
        
        app.UseAuthentication();
        app.UseAuthorization();


        app.UseEndpoints(endpoints =>
        {
            #region Home
            endpoints.MapGet("/", () => Results.Json(new Home())).WithTags("Home");
            #endregion

            #region Administradores
            string GerarToken(Administrador administrador)
            {
                if (string.IsNullOrEmpty(key)) return string.Empty;
                var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
                var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

                var claims = new List<Claim>()
                {
                    new("Email", administrador.Email),
                    new("Perfil", administrador.Perfil),
                    new(ClaimTypes.Role, administrador.Perfil)
                };

                var token = new JwtSecurityToken(
                    expires: DateTime.Now.AddDays(1),
                    signingCredentials: credentials,
                    claims: claims
                );

                return new JwtSecurityTokenHandler().WriteToken(token);

            }
            endpoints.MapPost("administradores/login", ([FromBody] LoginDTO loginDTO, iAdministradorServico administradorServico) =>
            {
                var administrador = administradorServico.Login(loginDTO);
                if (administrador != null)
                {
                    string token = GerarToken(administrador);
                    return Results.Ok(
                        new AdministradorLogado()
                        {
                            Email = administrador.Email,
                            Perfil = administrador.Perfil,
                            Token = token
                        }
                    );
                }
                return Results.Unauthorized();
            }).AllowAnonymous().WithTags("Administrador");

            endpoints.MapPost("administradores", ([FromBody] SalvarAdministradorDTO administradorDTO, iAdministradorServico administradorServico) =>
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
            }).RequireAuthorization().RequireAuthorization(new AuthorizeAttribute { Roles = "adm" }).WithTags("Administrador");

            endpoints.MapGet("administradores", ([FromQuery] int? pagina, iAdministradorServico administradorServico) =>
            {
                var administradoresEntidade = administradorServico.Todos(pagina);
                var administradoresDTO = new List<BuscarAdministradorDTO>();

                foreach (var administrador in administradoresEntidade)
                {
                    administradoresDTO.Add(new BuscarAdministradorDTO
                    {
                        Email = administrador.Email,
                        Perfil = (Perfil)Enum.Parse(typeof(Perfil), administrador.Perfil)
                    });
                }
                return Results.Ok(administradoresDTO);
            }).RequireAuthorization().RequireAuthorization(new AuthorizeAttribute { Roles = "adm" }).WithTags("Administrador");

            endpoints.MapGet("administradores/{id}", ([FromRoute] int id, iAdministradorServico administradorServico) =>
            {
                var administrador = administradorServico.BuscaPorId(id);
                if (administrador == null) return Results.NotFound();
                var administradorDTO = new BuscarAdministradorDTO
                {
                    Email = administrador.Email,
                    Perfil = (Perfil)Enum.Parse(typeof(Perfil), administrador.Perfil)
                };
                return Results.Ok(administradorDTO);
            }).RequireAuthorization().RequireAuthorization(new AuthorizeAttribute { Roles = "adm" }).WithTags("Administrador");

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
            #endregion

            #region Veiculos
            endpoints.MapPost("veiculos/login", ([FromBody] VeiculoDTO veiculoDTO, iVeiculoServico veiculoServico) =>
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

            }).RequireAuthorization()
            .RequireAuthorization(new AuthorizeAttribute { Roles = "adm" })
            .WithTags("Veiculo");

            endpoints.MapGet("veiculos", ([FromQuery] int? pagina, iVeiculoServico veiculoServico) =>
            {
                var veiculos = veiculoServico.Todos(pagina);
                return Results.Ok(veiculos);
            }).RequireAuthorization()
            .RequireAuthorization(new AuthorizeAttribute { Roles = "adm, editor" })
            .WithTags("Veiculo");

            endpoints.MapGet("veiculos/{id}", ([FromRoute] int id, iVeiculoServico veiculoServico) =>
            {
                var veiculo = veiculoServico.BuscaPorId(id);
                if (veiculo == null) return Results.NotFound();
                return Results.Ok(veiculo);
            }).RequireAuthorization()
            .WithTags("Veiculo")
            .RequireAuthorization(new AuthorizeAttribute { Roles = "adm, editor" });

            endpoints.MapPut("veiculos/{id}", ([FromRoute] int id,
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
            }).RequireAuthorization()
            .RequireAuthorization(new AuthorizeAttribute { Roles = "adm" })
            .WithTags("Veiculo");

            endpoints.MapDelete("veiculos/{id}", ([FromRoute] int id, iVeiculoServico veiculoServico) =>
            {
                var veiculo = veiculoServico.BuscaPorId(id);
                if (veiculo == null) return Results.NotFound();
                veiculoServico.Apagar(veiculo);
                return Results.NoContent();
            }).RequireAuthorization()
            .RequireAuthorization(new AuthorizeAttribute { Roles = "adm" })
            .WithTags("Veiculo");

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

            #endregion
        });
    }
        
        
}
