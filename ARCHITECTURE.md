# Arquitetura do Caixa System

## Visão geral

O projeto segue uma arquitetura em camadas:

- `CaixaSystem.Domain`: entidades, objetos de valor, regras e contratos dos repositórios.
- `CaixaSystem.Application`: casos de uso, serviços e DTOs.
- `CaixaSystem.Infrastructure`: persistência no SQL Server com Dapper.
- `CaixaSystem.API`: endpoints HTTP, configuração e tratamento de erros.
- `CaixaSystem.Tests`: testes automatizados de domínio e aplicação.

As dependências apontam para o domínio. A aplicação depende dos contratos de repositório, enquanto a infraestrutura fornece suas implementações.

## Persistência

A persistência é feita exclusivamente no SQL Server:

- `SqlConnectionFactory` cria conexões com `Microsoft.Data.SqlClient`.
- `DapperTransactionRepository` grava e consulta transações.
- `DapperDailyBalanceRepository` grava e consulta saldos diários.
- `DapperDatabaseInitializer` cria o banco e as tabelas necessárias na inicialização.

Os repositórios são registrados por `AddInfrastructure(connectionString)`. A API não possui fallback de persistência: uma falha ao inicializar o SQL Server interrompe a inicialização e é registrada como erro.

## Fluxo de uma requisição

1. O controller recebe e valida a requisição HTTP.
2. O serviço de aplicação executa as regras do caso de uso.
3. As entidades e objetos de valor garantem as regras do domínio.
4. O serviço chama a interface de repositório apropriada.
5. O repositório Dapper executa o comando ou a consulta no SQL Server.
6. O controller devolve o DTO dentro da resposta padronizada da API.

## Injeção de dependência

```csharp
var connectionString = builder.Configuration
    .GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string não configurada.");

builder.Services.AddInfrastructure(connectionString);
```

O registro associa `ITransactionRepository` e `IDailyBalanceRepository` às implementações Dapper, além dos respectivos serviços de aplicação.

## Inicialização do banco

Durante a inicialização, a API resolve `IDatabaseInitializer` e executa `InitializeAsync()`. Isso garante a existência do banco e do esquema antes de aceitar requisições. Em caso de falha, a aplicação registra o erro e encerra, evitando operar sem persistência.

## Testabilidade

Os serviços dependem de interfaces do domínio. Testes unitários podem usar mocks dessas interfaces sem introduzir um mecanismo alternativo de persistência no código de produção.

## Evolução

Novos recursos devem manter as responsabilidades separadas:

1. definir regras e contratos no domínio;
2. implementar o caso de uso na aplicação;
3. implementar a persistência SQL na infraestrutura;
4. expor a operação pela API;
5. adicionar testes proporcionais ao risco da mudança.
