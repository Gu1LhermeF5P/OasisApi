using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OasisApi.Core.Data;
using OasisApi.Core.Models;
using OasisApi.Core.Dtos; 
using OasisApi.Core.Dtos.V2; 
using OasisApi.Core.Helpers; 
using Oracle.ManagedDataAccess.Client;
using System.Data;

namespace OasisApi.Core.Controllers.V2
{
    [ApiController]
    [Route("api/v2/usuarios")] 
    public class UsuariosV2Controller : ControllerBase
    {
        private readonly AppDbContext _context;

        public UsuariosV2Controller(AppDbContext context)
        {
            _context = context;
        }

        // --- GET (READ ALL) V2 ---
        [HttpGet(Name = "GetUsuariosV2")]
        public async Task<IActionResult> GetUsuariosV2([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var totalRecords = await _context.Usuarios.CountAsync();
            var usuarios = await _context.Usuarios
                                         .Skip((pageNumber - 1) * pageSize)
                                         .Take(pageSize)
                                         .AsNoTracking()
                                         .ToListAsync();

            var usuariosDto = usuarios.Select(u => {
                var dto = UsuarioDtoV2.FromUsuario(u);
                dto.Links.Add(new LinkDto(Url.Action(nameof(GetUsuarioPorIdV2), new { id = u.UsuarioId }), "self", "GET"));
                return dto;
            }).ToList();

            var pagedResult = new PagedResult<UsuarioDtoV2>(usuariosDto, pageNumber, pageSize, totalRecords);

            pagedResult.Links.Add(new LinkDto(Url.Action(nameof(GetUsuariosV2), new { pageNumber, pageSize }), "self", "GET"));
            if (pageNumber < pagedResult.TotalPages)
                pagedResult.Links.Add(new LinkDto(Url.Action(nameof(GetUsuariosV2), new { pageNumber = pageNumber + 1, pageSize }), "next", "GET"));
            if (pageNumber > 1)
                pagedResult.Links.Add(new LinkDto(Url.Action(nameof(GetUsuariosV2), new { pageNumber = pageNumber - 1, pageSize }), "prev", "GET"));

            return Ok(pagedResult);
        }

        // --- GET (READ ONE) V2 ---
        [HttpGet("{id}", Name = "GetUsuarioPorIdV2")]
        public async Task<IActionResult> GetUsuarioPorIdV2(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null) return NotFound();

            var usuarioDto = UsuarioDtoV2.FromUsuario(usuario);
            usuarioDto.Links.Add(new LinkDto(Url.Action(nameof(GetUsuarioPorIdV2), new { id = usuario.UsuarioId }), "self", "GET"));
            return Ok(usuarioDto);
        }

        // --- ADICIONADO: POST (CREATE) V2 ---
        
        [HttpPost(Name = "CriarUsuarioV2")]
        public async Task<IActionResult> CriarUsuarioV2([FromBody] CriarUsuarioDto dto)
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

        // --- ADICIONADO: PUT (UPDATE) V2 ---
        
        [HttpPut("{id}", Name = "AtualizarUsuarioV2")]
        public async Task<IActionResult> AtualizarUsuarioV2(int id, [FromBody] UpdateUsuarioDto dto)
        {
            try
            {
                var parameters = new[]
                {
                    new OracleParameter("p_usuario_id", OracleDbType.Int32, id, ParameterDirection.Input),
                    new OracleParameter("p_nome", OracleDbType.Varchar2, dto.NomeCompleto, ParameterDirection.Input),
                    new OracleParameter("p_cargo", OracleDbType.Varchar2, dto.Cargo, ParameterDirection.Input),
                    new OracleParameter("p_fuso", OracleDbType.Varchar2, dto.FusoHorario, ParameterDirection.Input)
                };
                string sql = "BEGIN PKG_GERENCIAMENTO.SP_UPDATE_USUARIO(:p_usuario_id, :p_nome, :p_cargo, :p_fuso); END;";

                await _context.Database.ExecuteSqlRawAsync(sql, parameters);

                return Ok("Usuário atualizado com sucesso.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao executar procedure: {ex.Message}");
            }
        }

        // --- ADICIONADO: DELETE (DELETE) V2 ---
        [HttpDelete("{id}", Name = "DeletarUsuarioV2")]
        public async Task<IActionResult> DeletarUsuarioV2(int id)
        {
            try
            {
                var parameters = new[]
                {
                    new OracleParameter("p_usuario_id", OracleDbType.Int32, id, ParameterDirection.Input)
                };
                string sql = "BEGIN PKG_GERENCIAMENTO.SP_DELETE_USUARIO(:p_usuario_id); END;";

                await _context.Database.ExecuteSqlRawAsync(sql, parameters);

                return NoContent(); 
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao executar procedure: {ex.Message}");
            }
        }
    }
}