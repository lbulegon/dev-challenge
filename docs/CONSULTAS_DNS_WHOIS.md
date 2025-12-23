# 🔍 O que cada Consulta Retorna: DNS e WHOIS

Este documento explica detalhadamente o que cada tipo de consulta retorna no sistema.

---

## 📡 Consulta DNS (`DnsService`)

A consulta DNS é realizada através do `DnsService` e retorna informações sobre o registro DNS do domínio.

### Método: `QueryAsync(string domain)`

**Retorna:** `DnsQueryResult`

```csharp
public class DnsQueryResult
{
    public string IpAddress { get; set; }    // Endereço IP do domínio
    public int Ttl { get; set; }             // Time To Live em segundos
    public bool HasRecord { get; set; }      // Indica se encontrou registro
}
```

#### O que é retornado:

1. **`IpAddress`** (string)
   - O endereço IPv4 do domínio (registro A)
   - Exemplo: `"187.84.237.146"`
   - Pode ser `null` se nenhum registro A for encontrado

2. **`Ttl`** (int)
   - Time To Live em segundos
   - Indica por quanto tempo o registro pode ser cacheado
   - Exemplo: `3600` (1 hora)
   - Usado para determinar quando a informação precisa ser atualizada

3. **`HasRecord`** (bool)
   - `true` se encontrou um registro A válido
   - `false` se não encontrou nenhum registro

#### Como funciona:

- Primeiro tenta consultar com `QueryType.A` (mais confiável)
- Se falhar, tenta com `QueryType.ANY` como fallback
- Timeout de 10 segundos para a consulta completa

---

### Método: `GetNameServersAsync(string domain)`

**Retorna:** `List<string>`

#### O que é retornado:

- Lista de **Name Servers** (servidores DNS responsáveis pelo domínio)
- Exemplo: `["ns254.umbler.com", "ns255.umbler.com"]`
- Retorna lista vazia `[]` em caso de erro (não bloqueia a resposta)

#### Como funciona:

- Consulta registros do tipo `NS` (Name Server)
- Timeout de 5 segundos
- Extrai apenas os nomes dos servidores DNS

---

## 📋 Consulta WHOIS (`WhoisService`)

A consulta WHOIS é realizada através do `WhoisService` e retorna informações sobre o registro do domínio.

### Método: `QueryAsync(string query)`

**Retorna:** `WhoisResponse` (da biblioteca Whois.NET)

#### O que é retornado (da biblioteca):

1. **`Raw`** (string)
   - Texto bruto completo do WHOIS
   - Pode ter centenas de linhas
   - Formato varia conforme o TLD (.com, .br, etc.)
   - Exemplo:
     ```
     Domain Name: umbler.com
     Registry Domain ID: 1234567890
     Registrar WHOIS Server: whois.example.com
     Registrar URL: http://www.example.com
     Updated Date: 2024-11-26T00:05:03Z
     Creation Date: 2023-01-15T10:30:00Z
     Registrar Registration Expiration Date: 2025-01-15T10:30:00Z
     Registrar: Example Registrar Inc.
     ...
     ```

2. **`OrganizationName`** (string)
   - Nome da organização responsável pelo IP/domínio
   - Usado para preencher o campo `HostedAt` (empresa hospedadora)
   - Exemplo: `"RedeHost Internet Ltda."`
   - Pode ser `null` ou vazio se não disponível

3. **Outros campos** (da biblioteca Whois.NET)
   - A biblioteca pode fornecer outros campos, mas o sistema usa principalmente `Raw` e `OrganizationName`

---

### Método: `ParseWhoisDataAsync(string whoisRaw)`

**Retorna:** `WhoisData` (objeto estruturado)

Este método processa o texto bruto do WHOIS e extrai informações estruturadas.

#### O que é retornado:

```csharp
public class WhoisData
{
    // Informações do Registro
    public string DomainName { get; set; }                    // Nome do domínio
    public string RegistryDomainId { get; set; }              // ID do registro
    public string RegistrarWhoisServer { get; set; }         // Servidor WHOIS do registrar
    public string RegistrarUrl { get; set; }                  // URL do registrar
    public DateTime? UpdatedDate { get; set; }                // Data da última atualização
    public DateTime? CreationDate { get; set; }               // Data de criação
    public DateTime? ExpirationDate { get; set; }             // Data de expiração
    public string Registrar { get; set; }                    // Nome do registrar
    public string RegistrarIanaId { get; set; }              // IANA ID do registrar
    public List<string> DomainStatus { get; set; }            // Status do domínio (ex: "clientTransferProhibited")
    public string DnsSec { get; set; }                        // Status DNSSEC
    public string RegistrarAbuseContactEmail { get; set; }    // Email de abuso
    public string RegistrarAbuseContactPhone { get; set; }    // Telefone de abuso
    public DateTime? LastUpdateOfWhoisDatabase { get; set; }  // Última atualização do banco WHOIS

    // IDs de Registro
    public string RegistryRegistrantId { get; set; }          // ID do titular
    public string RegistryAdminId { get; set; }                // ID do admin
    public string RegistryTechId { get; set; }                 // ID do técnico

    // Contatos
    public WhoisContact Registrant { get; set; }              // Titular do domínio
    public WhoisContact Admin { get; set; }                    // Contato administrativo
    public WhoisContact Tech { get; set; }                     // Contato técnico
}
```

#### Estrutura de Contato (`WhoisContact`):

```csharp
public class WhoisContact
{
    public string Name { get; set; }           // Nome do contato
    public string Organization { get; set; }   // Organização
    public string Street { get; set; }          // Endereço (rua)
    public string City { get; set; }            // Cidade
    public string State { get; set; }           // Estado/Província
    public string PostalCode { get; set; }      // CEP/Código Postal
    public string Country { get; set; }         // País
    public string Phone { get; set; }           // Telefone
    public string PhoneExt { get; set; }        // Extensão do telefone
    public string Fax { get; set; }             // Fax
    public string FaxExt { get; set; }          // Extensão do fax
    public string Email { get; set; }           // Email
}
```

#### Exemplo de dados extraídos:

```json
{
  "DomainName": "umbler.com",
  "Registrar": "Example Registrar Inc.",
  "CreationDate": "2023-01-15T10:30:00Z",
  "ExpirationDate": "2025-01-15T10:30:00Z",
  "UpdatedDate": "2024-11-26T00:05:03Z",
  "DomainStatus": ["clientTransferProhibited", "clientUpdateProhibited"],
  "Registrant": {
    "Name": "João Silva",
    "Organization": "Empresa XYZ Ltda.",
    "Email": "contato@empresa.com",
    "Phone": "+55 11 1234-5678",
    "City": "São Paulo",
    "State": "SP",
    "Country": "BR"
  },
  "Admin": { ... },
  "Tech": { ... }
}
```

---

## 🔄 Como as Consultas são Usadas no Sistema

### Fluxo de Consulta Completo:

1. **Consulta WHOIS do Domínio**
   - `WhoisService.QueryAsync(domainName)`
   - Retorna `WhoisResponse` com texto bruto (`Raw`) e `OrganizationName`
   - O texto bruto é salvo no banco de dados no campo `WhoIs`

2. **Consulta DNS do Domínio**
   - `DnsService.QueryAsync(domainName)`
   - Retorna `DnsQueryResult` com IP, TTL e flag `HasRecord`
   - O IP é salvo no banco de dados

3. **Consulta WHOIS do IP**
   - `WhoisService.QueryAsync(ipAddress)`
   - Retorna `WhoisResponse` com `OrganizationName`
   - O `OrganizationName` é usado para preencher `HostedAt` (empresa hospedadora)

4. **Consulta Name Servers**
   - `DnsService.GetNameServersAsync(domainName)`
   - Retorna lista de Name Servers
   - **Não é salvo no banco**, sempre consultado em tempo real

5. **Parse do WHOIS Bruto**
   - `WhoisParser.Parse(whoisRaw)`
   - Processa o texto bruto e extrai `WhoisData` estruturado
   - Usado para exibir informações formatadas na interface

---

## 📊 Resumo Comparativo

| Consulta | Retorna | Uso Principal | Cacheado? |
|----------|---------|---------------|-----------|
| **DNS (A Record)** | IP, TTL, HasRecord | Obter endereço IP do domínio | ✅ Sim (banco + memória) |
| **DNS (NS Records)** | Lista de Name Servers | Exibir servidores DNS | ❌ Não (sempre em tempo real) |
| **WHOIS (Domínio)** | Texto bruto + OrganizationName | Informações de registro | ✅ Sim (banco + memória) |
| **WHOIS (IP)** | OrganizationName | Identificar empresa hospedadora | ✅ Sim (banco + memória) |
| **WHOIS Parseado** | WhoisData estruturado | Exibir informações formatadas | ✅ Sim (calculado do texto bruto) |

---

## 🎯 Campos Utilizados na Interface

### Exibidos na Interface:

- ✅ **IP** (`DnsQueryResult.IpAddress`)
- ✅ **HostedAt** (`WhoisResponse.OrganizationName` do IP)
- ✅ **Name Servers** (`DnsService.GetNameServersAsync`)
- ✅ **Informações WHOIS Estruturadas** (`WhoisData` parseado)
- ✅ **Texto Bruto WHOIS** (`WhoisResponse.Raw`)

### Salvos no Banco de Dados:

- ✅ **Name** (domínio normalizado)
- ✅ **Ip** (do DNS)
- ✅ **Ttl** (do DNS, com mínimo aplicado)
- ✅ **WhoIs** (texto bruto do WHOIS do domínio)
- ✅ **HostedAt** (OrganizationName do WHOIS do IP)
- ✅ **UpdatedAt** (timestamp da última atualização)

---

## 📝 Observações Importantes

1. **Name Servers não são salvos no banco**
   - Sempre consultados em tempo real via DNS
   - Permite verificar mudanças nos servidores DNS

2. **TTL Mínimo**
   - O sistema aplica um TTL mínimo configurável
   - Evita consultas muito frequentes ao mesmo domínio
   - Configurado em `MinimumTtlSeconds`

3. **Cache em Duas Camadas**
   - **L1 (Memória)**: Cache rápido em memória (configurável)
   - **L2 (Banco)**: Persistência em MySQL
   - Respeita o TTL para determinar quando atualizar

4. **Formato WHOIS Varia**
   - Diferentes TLDs têm formatos diferentes
   - O parser suporta formatos internacionais e brasileiros (.br)
   - Alguns campos podem não estar disponíveis dependendo do TLD

