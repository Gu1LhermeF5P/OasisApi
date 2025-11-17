Projeto OASIS - API Core (.NET)

💡 Conceito: App "Equilíbrio Híbrido"

Este projeto é a API de Camada de Dados (Data Layer) para a solução "Equilíbrio Híbrido" (OASIS), desenvolvida para a Global Solution da FIAP sobre o Futuro do Trabalho.

O objetivo do projeto OASIS é combater o burnout no trabalho híbrido usando IA para criar "Escudos de Foco" e "Rituais de Transição" para os funcionários.

Esta API .NET serve como a "ponte" robusta entre o banco de dados relacional (Oracle) e o restante da arquitetura de microsserviços.

Arquitetura da Solução

Esta API (OasisApi.Core) não é consumida diretamente pelo frontend. Ela é a camada de dados que será chamada pela nossa API "cérebro" (Java/Spring).

🚀 Tecnologias e Requisitos (.NET)

Este projeto foi desenvolvido em .NET 8.0 (LTS) e cumpre todos os requisitos da matéria ADVANCED BUSINESS DEVELOPMENT WITH .NET:

✅ 1. Boas Práticas REST (30 pts)

Paginação: Implementada no endpoint GET /api/v1/usuarios com a classe PagedResult<T>.

HATEOAS: Implementado no GET /api/v1/usuarios, com links self, next e prev para navegação, e links self em cada recurso.

Status Codes: Uso correto de 200 OK, 201 Created, 204 No Content, 404 Not Found e 500 Internal Server Error.

Verbos HTTP: Implementação completa de GET, POST, PUT e DELETE.

✅ 2. Monitoramento e Observabilidade (15 pts)

Health Check: Endpoints /health (simples) e /health/details (JSON detalhado) implementados para verificar a conectividade com o Oracle.

Logging: O logging padrão do .NET está configurado para capturar informações e erros.

✅ 3. Versionamento da API (10 pts)

A API está estruturada com versionamento em rota.

v1: /api/v1/usuarios (retorna DTO padrão).

v2: /api/v2/usuarios (retorna DTO V2, que inclui o fusoHorario, provando a evolução da API sem quebrar a v1).

✅ 4. Integração e Persistência (30 pts)

Integração Relacional (Oracle): A API utiliza uma arquitetura Database-First com Entity Framework Core. Todas as transações (INSERT, UPDATE, DELETE) são feitas de forma segura, chamando procedures PL/SQL (PKG_GERENCIAMENTO).

Integração Não-Relacional (MongoDB): A API possui um endpoint (/api/v1/export/mongodb/{id}) que chama uma procedure Oracle (SP_EXPORTAR_DATASET_EMPRESA) para gerar um JSON manual e, em seguida, importa esses dados para uma coleção no MongoDB Atlas.

✅ 5. Testes Integrados (15 pts)

Um projeto separado (OasisApi.Core.Tests) usa xUnit para rodar testes de integração.

Os testes usam WebApplicationFactory para iniciar a API em memória, substituindo o Oracle por um InMemoryDatabase e "mockando" (simulando) o MongoDbService.

🔧 Configuração e Instalação

Siga estes passos para executar o projeto localmente:

1. Pré-requisitos

.NET 8.0 SDK

Um banco de dados Oracle (como o da FIAP ou local).

Um banco de dados MongoDB (recomenda-se um cluster gratuito no MongoDB Atlas).

2. Banco de Dados Oracle

Execute o script bd_oasis.sql (versão 5, que inclui o povoamento completo) no seu banco de dados Oracle para criar todas as tabelas, pacotes e procedures.

3. Configuração (O Passo Mais Importante)

Clone este repositório:

git clone https://[SEU-REPOSITORIO-URL]/OasisApi.Core.git
cd OasisApi.Core


Edite o appsettings.json e insira suas strings de conexão:

{
  "ConnectionStrings": {
    "OracleDbConnection": "User Id=SEU_RM;Password=SUA_SENHA_ORACLE;Data Source=oracle.fiap.com.br:1521/ORCL;",
    "MongoDbConnection": "mongodb+srv://SEU_USUARIO_MONGO:SUA_SENHA_MONGO@seucluster.mongodb.net/"
  },
  "MongoDbSettings": {
    "DatabaseName": "OasisEquilibrioDb",
    "CollectionName": "UsuariosDataset"
  }
  // ... (logging, etc.)
}


4. Executar a Aplicação Localmente

Abra um terminal na raiz do projeto (OasisApi.Core).

Restaure os pacotes:

dotnet restore


Execute a aplicação:

dotnet run


A API estará disponível. Os endereços principais são:

Swagger (Documentação): https://localhost:[PORTA]/ (a página inicial)

Health Check Detalhado: https://localhost:[PORTA]/health/details

🔬 Como Testar (xUnit)

O projeto OasisApi.Core.Tests contém os testes de integração automatizados.

Abra a Solução (.sln) no Visual Studio.

Abra o Gerenciador de Testes (Menu Exibir -> Gerenciador de Testes).

Clique em "Executar Todos os Testes".

🚀 Deploy e Teste ao Vivo (Azure)

Link do Deploy: https://oasis-api-gs-bagkd6f7e7c6b4hv.westus-01.azurewebsites.net/

O link acima carrega a interface do Swagger UI diretamente, que foi habilitada em produção para fins de demonstração.

‼️ Instruções Importantes para Teste na Nuvem

Ao testar a API no link de deploy acima, observe o seguinte comportamento:

Endpoints do Oracle (ex: GET /api/v1/usuarios, GET /health/details)

Status: FALHARÁ (Erro 500 - Timeout)

Motivo (Esperado): O firewall da FIAP bloqueia conexões externas de servidores na nuvem (como o Azure) ao banco de dados oracle.fiap.com.br.

Prova de Funcionamento: A prova completa de que a integração com o Oracle funciona está no vídeo de demonstração (gravado localmente), onde a conexão é permitida.

Endpoints do MongoDB (ex: POST /api/v1/export/mongodb/...)

Status: FUNCIONARÁ

Motivo: O firewall do MongoDB Atlas foi configurado para 0.0.0.0/0 (Allow Access from Anywhere), permitindo a conexão do servidor do Azure.

API Endpoints (Principais)

Aqui estão os principais endpoints demonstrados neste projeto:

Health Check

GET /health

Função: Verifica se a API está "viva" (liveness probe).

Resposta (Sucesso): Healthy

GET /health/details

Função: Verifica a saúde da API e de todos os seus serviços dependentes (readiness probe), como o Oracle.

Resposta (Sucesso): Um JSON detalhado com o status Healthy para o OracleDB-Check. (Nota: Falhará no deploy, como explicado acima).

Usuários (CRUD) - v1 e v2

POST /api/v1/usuarios (ou /v2)

Função: Cria um novo usuário. Chama a procedure SP_INSERT_USUARIO.

Resposta (Sucesso): 201 Created

GET /api/v1/usuarios (ou /v2)

Função: Lista usuários com paginação e HATEOAS.

Resposta (Sucesso): 200 OK (com o objeto PagedResult)

PUT /api/v1/usuarios/{id} (ou /v2)

Função: Atualiza um usuário. Chama a procedure SP_UPDATE_USUARIO.

Resposta (Sucesso): 200 OK

DELETE /api/v1/usuarios/{id} (ou /v2)

Função: Deleta um usuário. Chama a procedure SP_DELETE_USUARIO.

Resposta (Sucesso): 204 No Content

Versionamento (V2)

GET /api/v2/usuarios

Função: Lista usuários usando o UsuarioDtoV2 (que inclui o campo fusoHorario).

Resposta (Sucesso): 200 OK

Integração MongoDB

POST /api/v1/export/mongodb/{empresaId}

Função: O teste principal. Chama a procedure SP_EXPORTAR_DATASET_EMPRESA do Oracle e importa o JSON resultante para o MongoDB Atlas.

Resposta (Sucesso): 200 OK

🧪 Exemplo de Teste Rápido (Usando Swagger)

Este roteiro demonstra o ciclo CRUD completo e a integração.

1. (CREATE) Criar um usuário:

Endpoint: POST /api/v1/usuarios

Request Body:

{
  "empresaId": 1,
  "nomeCompleto": "Debora Lemos (Teste API)",
  "email": "debora.lemos@oasis.tech",
  "cargo": "Engenheira de QA",
  "fusoHorario": "America/Sao_Paulo"
}


Resultado: 201 Created.

2. (READ) Ler os usuários:

Endpoint: GET /api/v1/usuarios

Ação: Execute o GET e encontre o usuarioId da "Debora Lemos" que você acabou de criar (vamos supor que seja 13).

3. (UPDATE) Atualizar o usuário:

Endpoint: PUT /api/v1/usuarios/{id}

Parameters: id: 13

Request Body:

{
  "nomeCompleto": "Debora Lemos (Cargo Atualizado)",
  "cargo": "Gerente de QA",
  "fusoHorario": "America/Recife"
}


Resultado: 200 OK.

4. (DELETE) Deletar o usuário:

Endpoint: DELETE /api/v1/usuarios/{id}

Parameters: id: 13

Resultado: 204 No Content. (Se você rodar o GET de novo, a "Debora" terá sumido).

5. (EXPORT) Testar a integração Oracle -> MongoDB:

Endpoint: POST /api/v1/export/mongodb/{empresaId}

Parameters: empresaId: 1

Resultado: 200 OK. (Se você checar no MongoDB Atlas, os dados do Oracle (incluindo os 5 usuários da "Oasis Tech") terão sido importados).

👥 Integrantes do Grupo

Nome

RM

Larissa de Freitas Moura

555136

Guilherme Francisco

557648
