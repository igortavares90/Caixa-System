# Docker

O ambiente Docker executa a API e o SQL Server. A aplicação usa Dapper para toda a persistência.

## Iniciar

```bash
docker compose up --build
```

Serviços expostos:

- API: `http://localhost:5000`
- Swagger em desenvolvimento: `http://localhost:5000/swagger`
- SQL Server: `localhost:1433`

O `docker-compose.yml` configura `ConnectionStrings__DefaultConnection` para que a API acesse o serviço `sqlserver`. O health check impede que a API seja iniciada antes de o banco estar pronto.

## Verificar

```bash
docker compose ps
docker compose logs api
```

O endpoint abaixo pode ser usado para verificar a API:

```bash
curl http://localhost:5000/health
```

## Encerrar

```bash
docker compose down
```

Os dados permanecem no volume `sqlserver_data`. Para remover também esse volume, use explicitamente `docker compose down -v` apenas quando quiser apagar os dados locais.

## Configuração

Em outros ambientes, informe uma connection string SQL Server pela variável:

```text
ConnectionStrings__DefaultConnection
```

Na primeira execução, `DapperDatabaseInitializer` cria o banco e as tabelas necessárias.
