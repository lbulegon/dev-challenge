
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

- DESCREVA AQUI O OBJETIVO DAS MODIFICAÇÕES...









MANUS


Entendido. Vou consolidar o Relatório Final de Recomendações da equipe de TI virtual e, em seguida, fornecer um Prompt Estratégico para instruir uma IA (como o Cursor) a executar as ações necessárias para completar o desafio Umbler.Relatório Final Consolidado de Recomendações (Desafio Umbler)Gerente de Projetos: Felipe
Arquiteto de Software: Carlos
Projeto Analisado: Desafio Umbler (C#/.NET Core)
Objetivo: Atender aos requisitos de melhoria listados no README.md do desafio.1. Recomendações de Arquitetura e Back-end (Carlos e Marcos)Problema IdentificadoAção RecomendadaBenefícioAlta Complexidade Ciclomática no Controller (Linha 81)Refatoração para Arquitetura em Camadas: Mover a lógica de negócio para uma camada de Services ou Handlers (ex: MediatR).Reduz a complexidade, aumenta a legibilidade e facilita a manutenção e os testes.Controller retornando Entidade de Domínio (Linha 82)Implementação de DTOs (Data Transfer Objects): Criar ViewModels específicas para o retorno da API.Evita a exposição de dados internos desnecessários (Id, Ttl, UpdatedAt) e padroniza o contrato da API.Falta de Validação no BackEnd (Linha 80)Validação Robusta: Implementar validação no BackEnd usando FluentValidation ou Data Annotations antes que a requisição prossiga.Previne erros 500 (Exceptions) e aumenta a segurança e a robustez da API.Dificuldade de Mockar a Infraestrutura (Linha 85)Inversão de Dependência: Extrair a lógica de consulta WHOIS/DNS para uma Interface (IWhoisDnsService).Isola o código de negócio da infraestrutura, permitindo o mocking em testes unitários.2. Recomendações de Qualidade e Testes (Larissa)Problema IdentificadoAção RecomendadaBenefícioCobertura de Testes Baixa (Linha 84)Aumento da Cobertura: Focar em testes unitários para a nova camada de Services e Handlers.Garante que a lógica de negócio esteja correta e previne regressões.Impossibilidade de Testar DomainController (Linha 84)Implementação de Mocking: Utilizar a biblioteca Moq para simular a IWhoisDnsService e o DbContext (que já usa InMemoryDatabase).Permite testar o Controller e o Service de forma isolada e rápida.Teste Unitário Comentado (Linha 88)Descomentar e Corrigir: Garantir que o teste unitário comentado seja descomentado e passe com sucesso.Cumpre um requisito obrigatório do desafio.3. Recomendações de Front-end (Júlia)Problema IdentificadoAção RecomendadaBenefícioDados Retornados Não Formatados (Linha 76)Formatação de Dados: Implementar lógica no FrontEnd (ou na ViewModel do BackEnd) para apresentar os dados de DNS de forma legível.Melhora a Experiência do Usuário (UX).Falta de Validação no FrontEnd (Linha 77)Validação de Domínio: Implementar validação de domínio (ex: regex) no lado do cliente antes de enviar a requisição.Melhora a UX, reduz a carga desnecessária no servidor.Uso de Vanilla-JS (Linha 78)Migração para Blazor: Sugestão de migrar o FrontEnd para Blazor (seja Server ou WebAssembly).Permite reutilizar a expertise em C# da equipe, moderniza o FrontEnd e mantém a solução no ecossistema .NET. (Alternativa: ReactJs para uma solução SPA completa).Prompt Estratégico para IA (Cursor/Editor)Este prompt instrui a IA a seguir o plano de ação da equipe de TI virtual, focando nas melhorias listadas no README.md.**Objetivo:** Refatorar e completar o projeto C#/.NET Core "Desafio Umbler" (localizado em src/Desafio.Umbler) para atender a todos os requisitos de melhoria listados no README.md.

**Plano de Ação Detalhado (Prioridade de Execução):**

1.  **Isolamento de Infraestrutura (Testes/Back-end):**
    *   Crie uma **Interface** (ex: `IWhoisDnsService`) para abstrair a lógica de consulta WHOIS e DNS.
    *   Refatore a implementação existente para usar essa Interface.
    *   Injete a Interface no *Controller* e/ou *Service* de domínio.

2.  **Refatoração de Back-end (Arquitetura):**
    *   Implemente a biblioteca **FluentValidation** para validação de modelos.
    *   Crie **DTOs (Data Transfer Objects)** ou **ViewModels** para o retorno da API, garantindo que propriedades internas (Id, Ttl, UpdatedAt) não sejam expostas.
    *   Refatore o *Controller* para ser *thin* (fino), movendo a lógica de negócio para uma camada de *Services* ou *Handlers* (utilize MediatR se for o padrão do projeto, caso contrário, use *Services*).

3.  **Qualidade e Testes (Larissa):**
    *   Utilize a biblioteca **Moq** para *mockar* a `IWhoisDnsService` nos testes unitários.
    *   Descomente e corrija o teste unitário que está comentado, garantindo que ele passe.
    *   Aumente a cobertura de testes unitários para a nova lógica de validação e a camada de *Services*.

4.  **Front-end (Júlia):**
    *   Implemente **validação de domínio** no lado do cliente (JavaScript/Razor) para evitar requisições inválidas.
    *   Implemente a **formatação de dados** no *FrontEnd* para apresentar as informações de DNS de forma legível.
    *   (Opcional, mas recomendado) Se o tempo permitir, inicie a migração do *FrontEnd* para **Blazor** para modernizar a interface.

5.  **Documentação:**
    *   Atualize o `README.md` na seção "Modificações" com um resumo claro das mudanças realizadas (Arquitetura em Camadas, Uso de DTOs, Inversão de Dependência, Testes Mockados, Validação).

**Instrução Final:** Execute as refatorações e implementações acima, priorizando a correção dos problemas de segurança e testabilidade (passos 1, 2 e 3). Ao final, forneça o código atualizado e o novo `README.md`.

