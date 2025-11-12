using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OasisApi.Core.Data;
using OasisApi.Core.Models;
using OasisApi.Core.Dtos;
using OasisApi.Core.Helpers;
using Oracle.ManagedDataAccess.Client;
using System.Data;

namespace OasisApi.Core.Controllers
{
    [ApiController]
    [Route("api/v1/usuarios")] // Versionamento (Requisito .NET)
    public class UsuariosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UsuariosController(AppDbContext context)
        {
            _context = context;
        }

        // --- REQUISITO .NET: GET com Paginação e HATEOAS ---
        [HttpGet(Name = "GetUsuarios")]
        public async Task<IActionResult> GetUsuarios([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var totalRecords = await _context.Usuarios.CountAsync();
            var usuarios = await _context.Usuarios
                                         .Skip((pageNumber - 1) * pageSize)
                                         .Take(pageSize)
                                         .AsNoTracking()
                                         .ToListAsync();

            var usuariosDto = usuarios.Select(u => {
                var dto = UsuarioDto.FromUsuario(u);
                dto.Links.Add(new LinkDto(Url.Action(nameof(GetUsuarioPorId), new { id = u.UsuarioId }), "self", "GET"));
                return dto;
            }).ToList();

            var pagedResult = new PagedResult<UsuarioDto>(usuariosDto, pageNumber, pageSize, totalRecords);

            pagedResult.Links.Add(new LinkDto(Url.Action(nameof(GetUsuarios), new { pageNumber, pageSize }), "self", "GET"));
            if (pageNumber < pagedResult.TotalPages)
                pagedResult.Links.Add(new LinkDto(Url.Action(nameof(GetUsuarios), new { pageNumber = pageNumber + 1, pageSize }), "next", "GET"));
            if (pageNumber > 1)
                pagedResult.Links.Add(new LinkDto(Url.Action(nameof(GetUsuarios), new { pageNumber = pageNumber - 1, pageSize }), "prev", "GET"));

            return Ok(pagedResult);
        }

        // --- Endpoint de apoio para HATEOAS ---
        [HttpGet("{id}", Name = "GetUsuarioPorId")]
        public async Task<IActionResult> GetUsuarioPorId(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null) return NotFound();

            var usuarioDto = UsuarioDto.FromUsuario(usuario);
            usuarioDto.Links.Add(new LinkDto(Url.Action(nameof(GetUsuarioPorId), new { id = usuario.UsuarioId }), "self", "GET"));
            return Ok(usuarioDto);
        }

        // --- REQUISITO BD: Chamada de Procedure (INSERT) ---
        [HttpPost(Name = "CriarUsuario")]
        public async Task<IActionResult> CriarUsuario([FromBody] CriarUsuarioDto dto)
        {
            try
            {
                var parameters = new[]
                {
                    new OracleParameter("p_empresa_id", OracleDbType.Int32, dto.EmpresaId, ParameterDirection.Input),
                    new OracleParameter("p_nome", OracleDbType.Varchar2, dto.NomeCompleto, ParameterDirection.Input),
                    new OracleParameter("p_email", OracleDbType.Varchar2, dto.Email, ParameterDirection.Input),
                    new OracleParameter("p_cargo", OracleDbType.Varchar2, dto.Cargo, ParameterDirection.Input),
                    new OracleParameter("p_fuso", OracleDbType.Varchar2, dto.FusoHorario, ParameterDirection.Input)
                };

                string sql = "BEGIN PKG_GERENCIAMENTO.SP_INSERT_USUARIO(:p_empresa_id, :p_nome, :p_email, :p_cargo, :p_fuso); END;";

                await _context.Database.ExecuteSqlRawAsync(sql, parameters);

                return StatusCode(201, "Usuário criado com sucesso via procedure.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao executar procedure: {ex.Message}");
            }
        }
    }
}