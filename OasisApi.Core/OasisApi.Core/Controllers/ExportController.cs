using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OasisApi.Core.Data;
using OasisApi.Core.Services;
using Oracle.ManagedDataAccess.Client; // Essencial
using System.Data;

namespace OasisApi.Core.Controllers
{
    [ApiController]
    [Route("api/v1/export")] // Requisito de Versionamento
    public class ExportController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly MongoDbService _mongoService;

        public ExportController(AppDbContext context, MongoDbService mongoService)
        {
            _context = context;
            _mongoService = mongoService;
        }

        // Este endpoint cumpre os Requisitos 4 e 5 do BD
        [HttpPost("mongodb/{empresaId}")]
        public async Task<IActionResult> ExportarParaMongoDb(int empresaId)
        {
            // 1. Preparar o parâmetro de SAÍDA (OUT) da procedure
            var jsonDatasetParam = new OracleParameter("p_json_dataset", OracleDbType.Clob, ParameterDirection.Output);
            jsonDatasetParam.Size = 50000; // Tamanho máximo do JSON

            try
            {
                // 2. CHAMA A PROCEDURE DO ORACLE

                string sql = "BEGIN PKG_BEM_ESTAR.SP_EXPORTAR_DATASET_EMPRESA(p_empresa_id => :p_empresa_id, p_json_dataset => :p_json_dataset); END;";

                await _context.Database.ExecuteSqlRawAsync(sql,
                    new OracleParameter("p_empresa_id", empresaId),
                    jsonDatasetParam
                );

                // 3. PEGAR O JSON DE VOLTA
                string jsonResult = jsonDatasetParam.Value.ToString();
                if (string.IsNullOrEmpty(jsonResult) || jsonResult.Contains("erro"))
                {
                    return BadRequest("Erro ao gerar JSON no Oracle: " + jsonResult);
                }

                // 4. ENVIA O JSON PARA O MONGODB

                await _mongoService.ImportarJsonAsync(jsonResult);

                return Ok(new { message = "Dados exportados do Oracle e importados para o MongoDB com sucesso!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro inesperado: {ex.Message}");
            }
        }
    }
}