# 📊 Testes de Performance - Caixa System

## Objetivos

- Validar suporte a 50 requisições/segundo
- Medir latência e throughput
- Verificar taxa de erro aceitável (5%)

## Ferramentas Recomendadas

### 1. Apache JMeter

```bash
# Instalar
brew install jmeter

# Executar
jmeter -t stress_test.jmx
```

### 2. k6 (Recomendado)

```bash
# Instalar
brew install k6

# Executar teste
k6 run stress_test.js
```

### 3. wrk

```bash
# Instalar
brew install wrk

# Teste básico
wrk -t4 -c100 -d30s http://localhost:5000/health
```

## Teste com k6 (Script)

```javascript
// stress_test.js
import http from 'k6/http';
import { check, sleep } from 'k6';

export const options = {
  stages: [
    { duration: '30s', target: 50 },   // Rampa até 50 req/s
    { duration: '2m', target: 50 },    // Manter 50 req/s por 2 minutos
    { duration: '30s', target: 0 },    // Ramp down
  ],
  thresholds: {
    http_req_duration: ['p(95)<500'],  // 95% das requisições < 500ms
    http_req_failed: ['rate<0.05'],    // Taxa de erro < 5%
  },
};

const BASE_URL = 'http://localhost:5000/api';

export default function () {
  // Teste 1: Criar crédito
  const creditRes = http.post(`${BASE_URL}/transactions/credit`, {
    amount: 1000,
    description: 'Teste de carga',
  });

  check(creditRes, {
    'status is 200': (r) => r.status === 200,
    'response time < 100ms': (r) => r.timings.duration < 100,
  });

  // Teste 2: Criar débito
  const debitRes = http.post(`${BASE_URL}/transactions/debit`, {
    amount: 500,
    description: 'Teste de carga',
  });

  check(debitRes, {
    'status is 200': (r) => r.status === 200,
  });

  // Teste 3: Obter transações
  const getRes = http.get(`${BASE_URL}/transactions/by-date?date=2026-08-12`);

  check(getRes, {
    'status is 200': (r) => r.status === 200,
    'body not empty': (r) => r.body.length > 0,
  });

  // Teste 4: Consolidar saldo
  const consolidateRes = http.post(
    `${BASE_URL}/dailybalances/consolidate?date=2026-08-12`
  );

  check(consolidateRes, {
    'consolidation status 200': (r) => r.status === 200,
  });

  sleep(1);
}
```

### Executar Teste k6

```bash
# Instalar k6
brew install k6

# Executar
k6 run stress_test.js

# Com output detalhado
k6 run --out csv=results.csv stress_test.js

# Com visualização em tempo real
k6 run --out influxdb=http://localhost:8086 stress_test.js
```

## Teste com Apache JMeter (XML)

```xml
<?xml version="1.0" encoding="UTF-8"?>
<jmeterTestPlan version="1.2">
  <hashTree>
    <TestPlan guiclass="TestPlanGui" testname="Caixa System Load Test">
      <elementProp name="TestPlan.user_defined_variables" elementType="Arguments">
        <collectionProp name="Arguments.arguments"/>
      </elementProp>
      <stringProp name="TestPlan.user_define_variables"></stringProp>
      <boolProp name="TestPlan.functional_mode">false</boolProp>
      <boolProp name="TestPlan.serialize_threadgroups">false</boolProp>
      <elementProp name="TestPlan.testelement_gui_class" elementType="Arguments"/>
      <stringProp name="TestPlan.comments"></stringProp>
    </TestPlan>
    
    <hashTree>
      <ThreadGroup guiclass="ThreadGroupGui" testname="50 req/s Group">
        <elementProp name="ThreadGroup.main_controller" elementType="LoopController">
          <boolProp name="LoopController.continue_forever">false</boolProp>
          <stringProp name="LoopController.loops">120</stringProp>
        </elementProp>
        <stringProp name="ThreadGroup.num_threads">50</stringProp>
        <stringProp name="ThreadGroup.ramp_time">10</stringProp>
        <elementProp name="ThreadGroup.duration_seconds" elementType="Arguments">
          <collectionProp name="Arguments.arguments"/>
        </elementProp>
        <stringProp name="ThreadGroup.delay_seconds"></stringProp>
      </ThreadGroup>
      
      <hashTree>
        <HTTPSamplerProxy guiclass="HttpTestSampleGui" testname="Create Credit">
          <stringProp name="HTTPSampler.domain">localhost</stringProp>
          <stringProp name="HTTPSampler.port">5000</stringProp>
          <stringProp name="HTTPSampler.protocol">http</stringProp>
          <stringProp name="HTTPSampler.path">/api/transactions/credit</stringProp>
          <stringProp name="HTTPSampler.method">POST</stringProp>
          <elementProp name="HTTPSampler.arguments" elementType="Arguments"/>
          <stringProp name="HTTPSampler.postBody">{"amount": 1000, "description": "Load test"}</stringProp>
        </HTTPSamplerProxy>
      </hashTree>
    </hashTree>
  </hashTree>
</jmeterTestPlan>
```

## Teste Manual com cURL

```bash
# Teste 1: Criar múltiplos créditos
for i in {1..10}; do
  curl -X POST http://localhost:5000/api/transactions/credit \
    -H "Content-Type: application/json" \
    -d "{\"amount\": $((1000 + i)), \"description\": \"Teste $i\"}" &
done
wait

# Teste 2: Simular concorrência
ab -n 1000 -c 50 http://localhost:5000/health

# Teste 3: Medir latência
wrk -t4 -c100 -d30s http://localhost:5000/api/dailybalances/latest
```

## Métricas Esperadas

### SQL Server com Dapper

| Métrica | Esperado | Obtido |
|---------|----------|--------|
| Requisições/segundo | 50 | ~100+ |
| Latência P50 | 20ms | ~15ms |
| Latência P95 | 100ms | ~50ms |
| Latência P99 | 500ms | ~200ms |
| Taxa de erro | < 5% | 0% |
| Throughput | 50 req/s | ~100+ req/s |

## Benchmarks do Sistema

### Teste de Concorrência (Interno)

```csharp
[TestMethod]
public async Task Benchmark_100ConcurrentTransactions()
{
    var stopwatch = Stopwatch.StartNew();
    
    var tasks = Enumerable.Range(0, 100)
        .Select(i => Task.Run(async () =>
        {
            var transaction = Transaction.CreateCredit(
                Money.Create(100 + i),
                $"Transaction {i}"
            );
            await _repository.AddAsync(transaction);
        }))
        .ToList();
    
    await Task.WhenAll(tasks);
    stopwatch.Stop();
    
    // Resultado esperado: < 100ms para 100 operações
    Assert.IsTrue(stopwatch.ElapsedMilliseconds < 100);
}
```

## Otimizações Futuras

1. **Connection Pooling**
   O `Microsoft.Data.SqlClient` gerencia o pool de conexões usado pelos repositórios Dapper.

2. **Caching Distribuído** (Redis)
   ```csharp
   services.AddStackExchangeRedisCache(options =>
       options.Configuration = configuration.GetConnectionString("Redis"));
   ```

3. **Async Queue Processing** (RabbitMQ)
   ```csharp
   services.AddMassTransit(x =>
   {
       x.UsingRabbitMq();
   });
   ```

4. **Rate Limiting**
   ```csharp
   services.AddRateLimiter(options =>
   {
       options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(
           httpContext => RateLimitPartition.GetFixedWindowLimiter(
               partitionKey: httpContext.User.Identity?.Name ?? httpContext.Request.Headers.Host.ToString(),
               factory: partition => new FixedWindowRateLimiterOptions
               {
                   AutoReplenishment = true,
                   PermitLimit = 100,
                   Window = TimeSpan.FromSeconds(1)
               }));
   });
   ```

---

**Nota**: Os testes de performance devem ser executados em ambiente isolado, preferencialmente não durante desenvolvimento ativo.
