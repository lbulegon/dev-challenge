# ⚙️ Configurações Avançadas - Desafio Umbler

**Data:** 21/12/2025

---

## 📋 Visão Geral

Este documento descreve todas as configurações avançadas disponíveis na aplicação, incluindo novas funcionalidades de cache e TTL.

---

## 🔧 Configurações de Domínio

### Arquivo: `appsettings.json`

```json
{
  "DomainSettings": {
    "MinimumTtlSeconds": 60,
    "MemoryCacheExpirationMinutes": 5
  }
}
```

### Propriedades

#### `MinimumTtlSeconds`

- **Tipo:** `int`
- **Padrão:** `60`
- **Unidade:** Segundos
- **Descrição:** TTL mínimo em segundos que será aplicado a todos os domínios, mesmo que o DNS retorne um TTL menor.
- **Objetivo:** Evitar consultas excessivas aos serviços externos (DNS e WHOIS).
- **Recomendação:**
  - **Desenvolvimento:** 60 segundos (1 minuto)
  - **Produção:** 300 segundos (5 minutos)

**Exemplo de Uso:**
```json
{
  "DomainSettings": {
    "MinimumTtlSeconds": 120  // Força TTL mínimo de 2 minutos
  }
}
```

**Comportamento:**
- Se DNS retorna TTL = 30 segundos e `MinimumTtlSeconds` = 60
- O sistema usará TTL efetivo = **60 segundos** (máximo entre os dois)

---

#### `MemoryCacheExpirationMinutes`

- **Tipo:** `int`
- **Padrão:** `5`
- **Unidade:** Minutos
- **Descrição:** Tempo de expiração do cache em memória antes de expirar automaticamente.
- **Objetivo:** Reduzir consultas ao banco de dados mantendo dados recentes em memória.
- **Recomendação:**
  - **Desenvolvimento:** 5 minutos
  - **Produção:** 10 minutos (ou mais, dependendo do volume)

**Exemplo de Uso:**
```json
{
  "DomainSettings": {
    "MemoryCacheExpirationMinutes": 10  // Cache válido por 10 minutos
  }
}
```

**Comportamento:**
- Domínios consultados são mantidos em memória por X minutos
- Após expiração, próxima consulta busca no banco de dados
- Cache é invalidado automaticamente quando domínio é atualizado

---

## 🗄️ Connection String

### Arquivo: `appsettings.json`

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=69.169.102.84;Port=3306;Database=sinapum;Uid=sinapum_user;Pwd=sinapum_pass;"
  }
}
```

### Propriedades

#### `DefaultConnection`

- **Tipo:** `string`
- **Formato:** Connection String do MySQL
- **Componentes:**
  - `Server`: Endereço do servidor MySQL
  - `Port`: Porta do MySQL (geralmente 3306)
  - `Database`: Nome do banco de dados
  - `Uid`: Usuário do banco
  - `Pwd`: Senha do banco

**Exemplo:**
```
Server=localhost;Port=3306;Database=umbler_db;Uid=root;Pwd=senha123;
```

---

## 📝 Logging

### Arquivo: `appsettings.json`

```json
{
  "Logging": {
    "IncludeScopes": false,
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.EntityFrameworkCore": "Warning",
      "Microsoft.Hosting.Lifetime": "Information",
      "Desafio.Umbler": "Information"
    }
  }
}
```

### Níveis de Log

- **Trace:** Informações muito detalhadas (não recomendado para produção)
- **Debug:** Informações para debugging
- **Information:** Informações gerais sobre o fluxo da aplicação
- **Warning:** Avisos sobre eventos inesperados
- **Error:** Erros que impedem operações específicas
- **Critical:** Falhas críticas que podem fazer a aplicação parar

### Recomendações

**Desenvolvimento:**
```json
{
  "Desafio.Umbler": "Debug"  // Logs detalhados
}
```

**Produção:**
```json
{
  "Desafio.Umbler": "Information",  // Apenas informações importantes
  "Microsoft": "Warning"  // Apenas warnings do framework
}
```

---

## 🎛️ MemoryCache (Configuração Programática)

### Arquivo: `Startup.cs`

```csharp
services.AddMemoryCache(options =>
{
    options.SizeLimit = 1024; // Limite de 1024 itens no cache
});
```

### Opções Disponíveis

#### `SizeLimit`

- **Tipo:** `int?`
- **Padrão:** `null` (sem limite)
- **Descrição:** Número máximo de itens no cache
- **Uso:** Controlar uso de memória quando necessário

**Exemplo:**
```csharp
services.AddMemoryCache(options =>
{
    options.SizeLimit = 2048;  // Limite de 2048 itens
});
```

---

## 📊 Configurações Recomendadas por Ambiente

### Desenvolvimento

```json
{
  "DomainSettings": {
    "MinimumTtlSeconds": 60,
    "MemoryCacheExpirationMinutes": 5
  },
  "Logging": {
    "LogLevel": {
      "Desafio.Umbler": "Debug"
    }
  }
}
```

**Justificativa:**
- TTL baixo permite testes frequentes
- Cache curto facilita testes de atualização
- Logs detalhados para debugging

---

### Produção

```json
{
  "DomainSettings": {
    "MinimumTtlSeconds": 300,
    "MemoryCacheExpirationMinutes": 10
  },
  "Logging": {
    "LogLevel": {
      "Desafio.Umbler": "Information",
      "Microsoft": "Warning"
    }
  }
}
```

**Justificativa:**
- TTL maior reduz carga nos serviços externos
- Cache mais longo melhora performance
- Logs menos verbosos reduzem overhead

---

## 🔄 Aplicando Mudanças

### 1. Editar `appsettings.json`

```json
{
  "DomainSettings": {
    "MinimumTtlSeconds": 120,  // Novo valor
    "MemoryCacheExpirationMinutes": 8  // Novo valor
  }
}
```

### 2. Reiniciar a Aplicação

As configurações são carregadas na inicialização. É necessário reiniciar:

```bash
# Parar aplicação (Ctrl+C)
# Iniciar novamente
dotnet run
```

### 3. Verificar Logs

As configurações são logadas na inicialização:

```
info: Desafio.Umbler.Services.DomainService[0]
      Configurações carregadas: MinimumTtlSeconds=120, MemoryCacheExpirationMinutes=8
```

---

## 🧪 Testando Configurações

### Teste de TTL Mínimo

1. Configurar `MinimumTtlSeconds` = 120
2. Consultar domínio novo
3. Verificar logs para confirmar TTL efetivo ≥ 120

### Teste de Cache

1. Configurar `MemoryCacheExpirationMinutes` = 5
2. Consultar domínio
3. Consultar o mesmo domínio novamente em < 5 minutos
4. Verificar nos logs que segunda consulta usa cache

---

## 📚 Referências

- [Configuration in ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/)
- [Memory Cache Configuration](https://docs.microsoft.com/en-us/aspnet/core/performance/caching/memory)
- [Connection Strings Reference](https://www.connectionstrings.com/mysql/)

---

**Última Atualização:** 21/12/2025

