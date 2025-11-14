# Projeto OASIS - API Core (.NET)

## 💡 Conceito: App "Equilíbrio Híbrido"

Este projeto é a **API de Camada de Dados (Data Layer)** para a solução "Equilíbrio Híbrido" (OASIS), desenvolvida para a Global Solution da FIAP sobre o **Futuro do Trabalho**.

O objetivo do projeto OASIS é combater o *burnout* no trabalho híbrido usando IA para criar "Escudos de Foco" e "Rituais de Transição" para os funcionários.

Esta API .NET serve como a "ponte" robusta entre o banco de dados relacional (Oracle) e o restante da arquitetura de microsserviços.

### Arquitetura da Solução

Esta API (`OasisApi.Core`) não é consumida diretamente pelo frontend. Ela é a camada de dados que será chamada pela nossa API "cérebro" (Java/Spring).



---

## 🚀 Tecnologias e Requisitos (.NET)

Este projeto foi desenvolvido em **.NET 8.0 (LTS)** e cumpre todos os requisitos da matéria `ADVANCED BUSINESS DEVELOPMENT WITH .NET`:

* **✅ 1. Boas Práticas REST (30 pts)**
    * **Paginação:** Implementada no endpoint `GET /api/v1/usuarios` com a classe `PagedResult<T>`.
    * **HATEOAS:** Implementado no `GET /api/v1/usuarios`, com links `self`, `next` e `prev` para navegação, e links `self` em cada recurso.
    * **Status Codes:** Uso correto de `200 OK`, `201 Created`, `204 No Content`, `404 Not Found` e `500 Internal Server Error`.
    * **Verbos HTTP:** Implementação completa de `GET`, `POST`, `PUT` e `DELETE`.

* **✅ 2. Monitoramento e Observabilidade (15 pts)**
    * **Health Check:** Endpoint `/health` implementado, que verifica a conectividade com o banco de dados Oracle.
    * **Logging:** O logging padrão do .NET está configurado para capturar informações e erros.

* **✅ 3. Versionamento da API (10 pts)**
    * A API está estruturada com versionamento em rota.
    * **v1:** `/api/v1/usuarios` (retorna DTO padrão).
    * **v2:** `/api/v2/usuarios` (retorna DTO V2, que inclui o `fusoHorario`, provando a evolução da API sem quebrar a v1).

* **✅ 4. Integração e Persistência (30 pts)**
    * **Integração Relacional (Oracle):** A API utiliza uma arquitetura **Database-First** com Entity Framework Core. Todas as transações (INSERT, UPDATE, DELETE) são feitas de forma segura, chamando **procedures PL/SQL** (`PKG_GERENCIAMENTO`).
    * **Integração Não-Relacional (MongoDB):** A API possui um endpoint (`/api/v1/export/mongodb/{id}`) que chama uma procedure Oracle (`SP_EXPORTAR_DATASET_EMPRESA`) para gerar um JSON manual e, em seguida, importa esses dados para uma coleção no **MongoDB Atlas**.

* **✅ 5. Testes Integrados (15 pts)**
    * Um projeto separado (`OasisApi.Core.Tests`) usa **xUnit** para rodar testes de integração.
    * Os testes usam `WebApplicationFactory` para iniciar a API em memória, substituindo o Oracle por um `InMemoryDatabase` e "mockando" (simulando) o `MongoDbService` para garantir testes rápidos e isolados.

---

## 🔧 Configuração e Instalação

Siga estes passos para executar o projeto localmente:

### 1. Pré-requisitos

* [.NET 8.0 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
* Um banco de dados Oracle (como o da FIAP ou local).
* Um banco de dados MongoDB (recomenda-se um cluster gratuito no [MongoDB Atlas](https://www.mongodb.com/cloud/atlas)).

### 2. Banco de Dados Oracle

* Execute o script `bd_oasis.sql` (versão 5, que inclui o povoamento completo) no seu banco de dados Oracle para criar todas as tabelas, pacotes e procedures.

### 3. Configuração (O Passo Mais Importante)

1.  Clone este repositório:
    ```bash
    git clone https://[SEU-REPOSITORIO-URL]/OasisApi.Core.git
    cd OasisApi.Core
    ```
2.  Renomeie o arquivo `appsettings.Development.json.example` (se existir) para `appsettings.Development.json`, ou edite diretamente o `appsettings.json`.
3.  Abra `appsettings.json` e **insira suas strings de conexão**:

    ```json
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
    ```

### 4. Executar a Aplicação

1.  Abra um terminal na raiz do projeto (`OasisApi.Core`).
2.  Restaure os pacotes:
    ```bash
    dotnet restore
    ```
3.  Execute a aplicação:
    ```bash
    dotnet run
    ```
4.  A API estará disponível. Os endereços principais são:
    * **Swagger (Documentação):** `https://localhost:[PORTA]/swagger`
    * **Health Check:** `https://localhost:[PORTA]/health`

---

## 🔬 Como Testar (xUnit)

O projeto `OasisApi.Core.Tests` contém os testes de integração automatizados.

1.  Abra a Solução (`.sln`) no Visual Studio.
2.  Abra o **Gerenciador de Testes** (Menu `Exibir` -> `Gerenciador de Testes`).
3.  Clique em **"Executar Todos os Testes"**.



---

## API Endpoints (Principais)

Aqui estão os principais endpoints demonstrados neste projeto:

### Health Check

* `GET /health`
    * **Função:** Verifica a saúde da API e a conexão com o Oracle.
    * **Resposta (Sucesso):** `Healthy`

### Usuários (CRUD)

* `POST /api/v1/usuarios`
    * **Função:** Cria um novo usuário. Chama a procedure `SP_INSERT_USUARIO`.
    * **Resposta (Sucesso):** `201 Created`

* `GET /api/v1/usuarios`
    * **Função:** Lista usuários com paginação e HATEOAS.
    * **Resposta (Sucesso):** `200 OK` (com o objeto `PagedResult`)

* `PUT /api/v1/usuarios/{id}`
    * **Função:** Atualiza um usuário. Chama a procedure `SP_UPDATE_USUARIO`.
    * **Resposta (Sucesso):** `200 OK`

* `DELETE /api/v1/usuarios/{id}`
    * **Função:** Deleta um usuário. Chama a procedure `SP_DELETE_USUARIO`.
    * **Resposta (Sucesso):** `204 No Content`

### Versionamento (V2)

* `GET /api/v2/usuarios`
    * **Função:** Lista usuários usando o `UsuarioDtoV2` (que inclui o campo `fusoHorario`).
    * **Resposta (Sucesso):** `200 OK`

### Integração MongoDB

* `POST /api/v1/export/mongodb/{empresaId}`
    * **Função:** O teste principal. Chama a procedure `SP_EXPORTAR_DATASET_EMPRESA` do Oracle e importa o JSON resultante para o MongoDB Atlas.
    * **Resposta (Sucesso):** `200 OK`

---

---

## 👥 Integrantes do Grupo

| Nome | RM |
|------|-----|
| Larissa de Freitas Moura | 555136 |
| Guilherme Francisco | 557648 |

---
