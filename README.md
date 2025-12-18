
# Desafio Umbler

Esta é uma aplicação web que recebe um domínio e mostra suas informações de DNS.

Este é um exemplo real de sistema que utilizamos na Umbler.

Ex: Consultar os dados de registro do dominio `umbler.com`

**Retorno:**
- Name servers (ns254.umbler.com)
- IP do registro A (177.55.66.99)
- Empresa que está hospedado (Umbler)

Essas informações são descobertas através de consultas nos servidores DNS e de WHOIS.

*Obs: WHOIS (pronuncia-se "ruís") é um protocolo específico para consultar informações de contato e DNS de domínios na internet.*

Nesta aplicação, os dados obtidos são salvos em um banco de dados, evitando uma segunda consulta desnecessaria, caso seu TTL ainda não tenha expirado.

*Obs: O TTL é um valor em um registro DNS que determina o número de segundos antes que alterações subsequentes no registro sejam efetuadas. Ou seja, usamos este valor para determinar quando uma informação está velha e deve ser renovada.*

Tecnologias Backend utilizadas:

- C#
- Asp.Net Core
- MySQL
- Entity Framework

Tecnologias Frontend utilizadas:

- Webpack
- Babel
- ES7

Para rodar o projeto você vai precisar instalar:

- dotnet Core SDK (https://www.microsoft.com/net/download/windows dotnet Core 6.0.201 SDK)
- Um editor de código, acoselhamos o Visual Studio ou VisualStudio Code. (https://code.visualstudio.com/)
- NodeJs v17.6.0 para "buildar" o FrontEnd (https://nodejs.org/en/)
- Um banco de dados MySQL (vc pode rodar localmente ou criar um site PHP gratuitamente no app da Umbler https://app.umbler.com/ que lhe oferece o banco Mysql adicionamente)

Com as ferramentas devidamente instaladas, basta executar os seguintes comandos:

Para "buildar" o javascript basta executar:

`npm install`
`npm run build`

Para Rodar o projeto:

Execute a migration no banco mysql:

`dotnet tool update --global dotnet-ef`
`dotnet tool ef database update`

E após: 

`dotnet run` (ou clique em "play" no editor do vscode)

# Objetivos:

Se você rodar o projeto e testar um domínio, verá que ele já está funcionando. Porém, queremos melhorar varios pontos deste projeto:

# FrontEnd

 - Os dados retornados não estão formatados, e devem ser apresentados de uma forma legível.
 - Não há validação no frontend permitindo que seja submetido uma requsição inválida para o servidor (por exemplo, um domínio sem extensão).
 - Está sendo utilizado "vanilla-js" para fazer a requisição para o backend, apesar de já estar configurado o webpack. O ideal seria utilizar algum framework mais moderno como ReactJs ou Blazor.  

# BackEnd

 - Não há validação no backend permitindo que uma requisição inválida prossiga, o que ocasiona exceptions (erro 500).
 - A complexidade ciclomática do controller está muito alta, o ideal seria utilizar uma arquitetura em camadas.
 - O DomainController está retornando a própria entidade de domínio por JSON, o que faz com que propriedades como Id, Ttl e UpdatedAt sejam mandadas para o cliente web desnecessariamente. Retornar uma ViewModel (DTO) neste caso seria mais aconselhado.

# Testes

 - A cobertura de testes unitários está muito baixa, e o DomainController está impossível de ser testado pois não há como "mockar" a infraestrutura.
 - O Banco de dados já está sendo "mockado" graças ao InMemoryDataBase do EntityFramework, mas as consultas ao Whois e Dns não. 

# Dica

- Este teste não tem "pegadinha", é algo pensado para ser simples. Aconselhamos a ler o código, e inclusive algumas dicas textuais deixadas nos testes unitários. 
- Há um teste unitário que está comentado, que obrigatoriamente tem que passar.
- Diferencial: criar mais testes.

# Entrega

- Enviei o link do seu repositório com o código atualizado.
- O repositório deve estar público para que possamos acessar..
- Modifique Este readme adicionando informações sobre os motivos das mudanças realizadas.

# Modificações:

## Melhorias Implementadas

### Backend

#### 1. Sistema de Logging Estruturado ✅
- Implementado logging detalhado em todos os pontos críticos da aplicação
- Facilita identificação de erros e rastreamento do fluxo
- Configurações específicas para desenvolvimento e produção

#### 2. Tratamento de Erros Melhorado ✅
- Adicionado tratamento adequado de exceções
- Retorno de códigos HTTP apropriados (400, 404, 500)
- Mensagens de erro descritivas para o cliente
- Validação básica de entrada (domínio não vazio)

#### 3. Correção da Lógica de TTL ✅
- Corrigido bug onde TTL era comparado em minutos ao invés de segundos
- Cache funciona corretamente, evitando atualizações prematuras
- Reduz chamadas desnecessárias a serviços externos

#### 4. Arquitetura em Camadas (Em Andamento) 🚧
- **Interfaces Criadas:**
  - `IWhoisService` - Abstração para consultas WHOIS
  - `IDnsService` - Abstração para consultas DNS
  - `IDomainRepository` - Abstração para acesso a dados
- **Implementações Criadas:**
  - `WhoisService` - Wrapper para WhoisClient (permite mock)
  - `DnsService` - Wrapper para LookupClient (permite mock)
  - `DomainRepository` - Implementação do Repository Pattern
- **ViewModels Criados:**
  - `DomainViewModel` - DTO para retorno da API (sem dados técnicos)
- **Próximos Passos:**
  - Criar `IDomainService` e `DomainService` para orquestração
  - Refatorar `DomainController` para usar serviços
  - Configurar injeção de dependência no `Startup.cs`

**Motivo das Mudanças:**
- Reduzir complexidade ciclomática do controller
- Permitir testabilidade adequada (mock de dependências)
- Separar responsabilidades seguindo princípios SOLID
- Facilitar manutenção e extensão do código

### Testes

#### Status Atual
- Estrutura criada para permitir mock de WhoisClient e DnsClient
- Teste obrigatório `Domain_Moking_WhoisClient()` será implementado após refatoração do controller
- Preparação para aumentar cobertura de testes unitários

**Motivo das Mudanças:**
- Tornar o código testável através de interfaces
- Permitir que o teste obrigatório seja implementado
- Facilitar criação de testes unitários isolados

## Melhorias em Andamento

### Backend
- [ ] Completar refatoração do DomainController
- [ ] Implementar validação robusta de formato de domínio
- [ ] Configurar injeção de dependência completa

### Testes
- [ ] Implementar teste obrigatório `Domain_Moking_WhoisClient()` (DEVE PASSAR)
- [ ] Aumentar cobertura de testes unitários

### Frontend
- [ ] Formatar exibição de resultados de forma legível
- [ ] Implementar validação de formato de domínio no frontend

## Considerações Técnicas

- **Arquitetura:** Implementação de arquitetura em camadas seguindo padrões Repository e Service Layer
- **SOLID:** Separação de responsabilidades, injeção de dependência e interfaces para desacoplamento
- **Testabilidade:** Interfaces criadas permitem mock adequado de dependências externas
- **Manutenibilidade:** Código organizado em camadas facilita manutenção e extensão



-- llbulegon Refatoração e testes

## Melhorias implementadas
- Refatoração da camada de serviços
- Tratamento de erros e timeouts
- Melhor organização do projeto
- Testes unitários básicos

## Considerações
Com mais tempo, eu adicionaria cache, observabilidade e CI.
