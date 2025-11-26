# 📖 Projeto OASIS - API Core (.NET)

## 💡 Conceito: App "Equilíbrio Híbrido"

Este projeto é a **API de Camada de Dados (Data Layer)** para a solução "Equilíbrio Híbrido" (OASIS), desenvolvida para a Global Solution da FIAP sobre o **Futuro do Trabalho**.

O objetivo do projeto OASIS é combater o *burnout* no trabalho híbrido usando IA para criar "Escudos de Foco" e "Rituais de Transição" para os funcionários.

Esta API .NET serve como a "ponte" robusta entre o banco de dados relacional (Oracle) e o restante da arquitetura de microsserviços.

### Arquitetura da Solução

Esta API (`OasisApi.Core`) não é consumida diretamente pelo frontend. Ela é a camada de dados que será chamada pela nossa API "cérebro" (Java/Spring). 

---
## 📺 Demonstração
**[CLIQUE AQUI PARA ASSISTIR AO VÍDEO NO YOUTUBE](https://youtu.be/l6i0huCIaQQ?si=DyX1Iw1WwPG6dNQ_)**
---
## 🚀 Tecnologias e Requisitos (`ADVANCED BUSINESS DEVELOPMENT WITH .NET`)

Este projeto foi desenvolvido em **.NET 8.0 (LTS)** e cumpre todos os requisitos da matéria, incluindo a implementação da funcionalidade de IA e o CRUD de sentenças de humor/sentimento (conforme implementado na Sprint 4):

* **✅ 1. Boas Práticas REST (30 pts)**
    * **Paginação, HATEOAS, Status Codes e Verbos HTTP:** Implementação completa no recurso principal (`/api/v1/usuarios`).
* **✅ 2. Monitoramento e Observabilidade (15 pts)**
    * **Health Check:** Endpoints `/health` (simples) e `/health/details` (JSON detalhado) para **Oracle** e **MongoDB**.
* **✅ 3. Versionamento da API (10 pts)**
    * Versões **v1** e **v2** estruturadas em rota (ex: `/api/v1/usuarios` e `/api/v2/usuarios`).
* **✅ 4. Integração e Persistência (30 pts)**
    * **Relacional (Oracle):** Arquitetura **Database-First** com Entity Framework Core e uso de **procedures PL/SQL** (`PKG_GERENCIAMENTO`).
    * **Não-Relacional (MongoDB):** Endpoint de exportação Oracle -> MongoDB Atlas (via `SP_EXPORTAR_DATASET_EMPRESA`).
* **✅ 5. Funcionalidade de IA (Sprint 4)**
    * **ML.NET:** Implementação do endpoint `/v1/ml/classificar-humor` para demonstrar o uso de Machine Learning na API.
* **✅ 6. Segurança (Sprint 4)**
    * **JWT:** Configuração de esquema de segurança `Bearer` no Swagger e exigência de autorização em endpoints críticos.
* **✅ 7. Testes Integrados (15 pts)**
    * Projeto `OasisApi.Core.Tests` com **xUnit** e `WebApplicationFactory` (substituindo Oracle por `InMemoryDatabase`).

---

## 🚀 Deploy e Teste ao Vivo (Azure)

**Link do Deploy (Swagger UI):**
[https://oasis-api-gs-bagkd6f7e7c6b4hv.westus-01.azurewebsites.net/](https://oasis-api-gs-bagkd6f7e7c6b4hv.westus-01.azurewebsites.net/index.html)

A URL acima carrega a interface do Swagger UI diretamente (esta é a página inicial), que foi habilitada em produção para fins de demonstração.

### ‼️ Instruções Importantes para Teste na Nuvem

* **Endpoints do Oracle (ex: `GET /api/v1/usuarios`, `GET /health/details`)**
    * **Status:** 🔴 **FALHARÁ** (Erro 500 - Timeout)
    * **Motivo (Esperado):** O firewall da FIAP bloqueia conexões externas de servidores na nuvem (Azure) ao banco de dados `oracle.fiap.com.br`.
    * **Prova de Funcionamento:** A prova completa de que a integração com o Oracle funciona está no **vídeo de demonstração** (gravado localmente).
* **Endpoints do MongoDB / ML.NET**
    * **Status:** 🟢 **FUNCIONARÁ** (O MongoDB Atlas permite a conexão externa).

---

## 🔧 Configuração e Instalação (Local)

Siga estes passos para executar o projeto localmente:

### 1. Pré-requisitos

* [.NET 8.0 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
* Um banco de dados Oracle (como o da FIAP ou local).
* Um banco de dados MongoDB (recomenda-se um cluster gratuito no [MongoDB Atlas](https://www.mongodb.com/cloud/atlas)).

### 2. Banco de Dados Oracle

* Execute o script `bd_oasis.sql` (versão 5, que inclui o povoamento completo) no seu banco de dados Oracle para criar todas as tabelas, pacotes e procedures.

### 3. Configuração (O Passo Mais Importante)

1.  Clone este repositório:
    ```bash
    git clone https://[SEU-REPOSITORIO-URL]/OasisApi.Core.git
    cd OasisApi.Core
    ```
2.  Edite o `appsettings.json` e **insira suas strings de conexão**:

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

### 4. Executar a Aplicação Localmente

1.  Abra um terminal na raiz do projeto (`OasisApi.Core`).
2.  Restaure os pacotes:
    ```bash
    dotnet restore
    ```
3.  Execute a aplicação:
    ```bash
    dotnet run
    ```
4.  A API estará disponível. Os endereços principais são:
    * **Swagger (Documentação):** `https://localhost:[PORTA]/` (a página inicial)
    * **Health Check Detalhado:** `https://localhost:[PORTA]/health/details`

---

## 🔬 Como Testar (xUnit)

O projeto `OasisApi.Core.Tests` contém os testes de integração automatizados.

1.  Abra a Solução (`.sln`) no Visual Studio.
2.  Abra o **Gerenciador de Testes** (Menu `Exibir` -> `Gerenciador de Testes`).
3.  Clique em **"Executar Todos os Testes"**.

---

## 🧪 Exemplo de Teste Rápido (CRUD de Usuários e ML.NET)

Este roteiro demonstra o ciclo CRUD completo e a integração (execute **localmente** para testar o Oracle e o ML.NET).

### 1. Testar o CRUD de Usuários (Oracle)

1. **(CREATE) Criar um usuário:** `POST /api/v1/usuarios`
2. **(READ) Ler os usuários:** `GET /api/v1/usuarios`
3. **(UPDATE) Atualizar o usuário:** `PUT /api/v1/usuarios/{id}`
4. **(DELETE) Deletar o usuário:** `DELETE /api/v1/usuarios/{id}`

### 2. Testar o ML.NET (Classificação de Humor)

* **Endpoint:** `POST /v1/ml/classificar-humor`
* **Ação:** Insira uma frase e execute.
* **Request Body Exemplo:**
    ```json
    {
      "SentimentText": "Este projeto está incrível e me fez rir!"
    }
    ```
* **Resultado Esperado:** `200 OK` com `ResultadoClassificacao` como **Positivo**.

### 3. Testar a Exportação (Oracle -> MongoDB)

* **Endpoint:** `POST /api/v1/export/mongodb/{empresaId}`
* **Parameters:** `empresaId: 1`
* **Resultado:** `200 OK`. (Verifique no MongoDB Atlas se a coleção `UsuariosDataset` foi populada).

---

## 👥 Integrantes do Grupo

| Nome | RM |
| :--- | :--- |
| Larissa de Freitas Moura | 555136 |
| Guilherme Francisco | 557648 |
