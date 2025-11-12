using Microsoft.AspNetCore.Mvc;
using OasisApi.Core.Data;
using OasisApi.Core.Services;
using Oracle.ManagedDataAccess.Client; 
using System.Data;
using Oracle.ManagedDataAccess.Types;  

namespace OasisApi.Core.Controllers
{
    [ApiController]
    [Route("api/v1/export")]
    public class ExportController : ControllerBase
    {
        private readonly MongoDbService _mongoService;
        // Vamos precisar da string de conexão diretamente
        private readonly string _oracleConnectionString;

        // O construtor foi modificado para receber IConfiguration
        public ExportController(MongoDbService mongoService, IConfiguration configuration)
        {
            _mongoService = mongoService;
            // Pegamos a string de conexão do appsettings.json
            _oracleConnectionString = configuration.GetConnectionString("OracleDbConnection");
        }

        [HttpPost("mongodb/{empresaId}")]
        public async Task<IActionResult> ExportarParaMongoDb(int empresaId)
        {
            string jsonResult = ""; // Variável para armazenar o JSON

            // --- INÍCIO DA CORREÇÃO (MÉTODO MANUAL) ---
            // Vamos usar uma conexão Oracle manual para ter controle total sobre o CLOB

            // 1. Cria a conexão usando a string do appsettings
            using (var connection = new OracleConnection(_oracleConnectionString))
            {
                // 2. Cria o comando que chama a procedure
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "BEGIN PKG_BEM_ESTAR.SP_EXPORTAR_DATASET_EMPRESA(p_empresa_id => :p_empresa_id, p_json_dataset => :p_json_dataset); END;";
                    command.CommandType = CommandType.Text;

                    // 3. Prepara o parâmetro de ENTRADA (IN)
                    command.Parameters.Add(new OracleParameter("p_empresa_id", OracleDbType.Int32, empresaId, ParameterDirection.Input));

                    // 4. Prepara o parâmetro de SAÍDA (OUT)
                    var jsonDatasetParam = new OracleParameter("p_json_dataset", OracleDbType.Clob, ParameterDirection.Output);
                    command.Parameters.Add(jsonDatasetParam);

                    try
                    {
                        // 5. ABRE a conexão manualmente
                        await connection.OpenAsync();

                        // 6. CHAMA A PROCEDURE
                        await command.ExecuteNonQueryAsync();

                        // 7. LÊ O CLOB (enquanto a conexão ainda está ABERTA)
                        // Esta é a correção para o ORA-50045
                        var clobResult = (OracleClob)jsonDatasetParam.Value;
                        jsonResult = clobResult.Value;
                    }
                    catch (Exception ex)
                    {
                        return StatusCode(500, $"Erro ao executar a procedure Oracle: {ex.Message}");
                    }
                    // 8. A conexão é fechada automaticamente aqui pelo 'using'
                }
            }
            // --- FIM DA CORREÇÃO ---

            // 9. O código abaixo (MongoDB) é executado APÓS a conexão Oracle fechar
            try
            {
                if (string.IsNullOrEmpty(jsonResult) || jsonResult.Contains("erro"))
                {
                    return BadRequest("Erro ao gerar JSON no Oracle (retorno vazio ou com erro): " + jsonResult);
                }

                // 10. ENVIA O JSON PARA O MONGODB
                await _mongoService.ImportarJsonAsync(jsonResult);

                return Ok(new { message = "Dados exportados do Oracle e importados para o MongoDB com sucesso!" });
            }
            catch (Exception ex)
            {
                // Este 'catch' pegará erros do MongoDB (ex: JSON.Parse)
                return StatusCode(500, $"Erro ao importar para o MongoDB: {ex.Message}");
            }
        }
    }
}