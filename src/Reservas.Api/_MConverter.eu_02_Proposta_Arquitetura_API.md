---
title: Proposta de Arquitetura da API
---

**ASP NET Core em monolito modular com DDD pragmatico**

Documento para alinhamento entre parceiros \| 23 de novembro de 2026 \| Status: proposta para aprovacao

# Sintese da proposta

A API sera construida em ASP.NET Core como um monolito modular. Cada dominio tera limites explicitos, mas a aplicacao sera implantada inicialmente como uma unica unidade. Essa abordagem preserva simplicidade operacional no MVP e cria uma base segura para separar processos somente quando escala, equipe ou carga justificarem.

**Decisao recomendada:** aplicar DDD nos dominios que concentram regras, SOLID onde reduzir acoplamento e uma Clean Architecture simplificada dentro de cada modulo. O BFF sera tratado como opcao evolutiva e nao como componente obrigatorio do MVP.

# Decisoes principais

| **Tema** | **Proposta** | **Limite pragmatico** |
|:---|----|----|
| Estilo | Monolito modular | Uma implantacao inicial, com limites internos por dominio. |
| Modelagem | DDD pragmatico | Entidades ricas apenas onde existem regras e invariantes relevantes. |
| Principios | SOLID seletivo | Abstracoes somente quando houver variacao, integracao ou ganho de teste. |
| Camadas | Clean Architecture simplificada | Domain, Application, Infrastructure e Presentation por modulo. |
| Contrato | REST JSON e OpenAPI | Endpoints orientados a recursos e casos de uso. |
| BFF | Sob demanda | Criar apenas para clientes com contratos ou agregacoes realmente diferentes. |

# Objetivos arquiteturais

- Proteger regras centrais de agenda, disponibilidade e cancelamento.

- Manter os modulos independentes o suficiente para evolucao e testes.

- Evitar custos operacionais de microsservicos antes de existir necessidade.

- Permitir integracoes externas sem levar detalhes de fornecedores ao dominio.

# Modulos iniciais do MVP

| **Modulo** | **Responsabilidade principal** |
|:---|----|
| Identity | Usuarios, credenciais, sessoes, papeis e permissoes. |
| Accounts | Empresas, unidades, configuracoes e contexto multi tenant. |
| Customers | Clientes, contatos e historico basico. |
| Catalog | Servicos, categorias, duracao, precos e ativacao. |
| Professionals | Profissionais, especialidades e servicos executados. |
| Scheduling | Disponibilidade, bloqueios, conflitos, reservas e estados. |
| Notifications | Templates, preferencias, solicitacoes e registro de entrega. |
| Platform | Outbox, auditoria tecnica, integracoes e elementos compartilhados. |

## Estrutura interna de um modulo

> Modules/Scheduling/
>
> Domain/ entidades, value objects, regras e eventos
>
> Application/ casos de uso, comandos, consultas e validacao
>
> Infrastructure/ EF Core, repositorios e integracoes
>
> Presentation/ endpoints, contratos HTTP e autorizacao

## Fluxo de uma requisicao

Endpoint -\> validacao e autorizacao -\> caso de uso -\> dominio -\> persistencia -\> resposta. O endpoint nao implementa regra de negocio e o dominio nao conhece HTTP, EF Core, provedores de pagamento ou servicos de mensagens.

## Aplicacao pragmatica de DDD e SOLID

- Usar aggregates e value objects para disponibilidade, reserva, intervalos, cancelamento e capacidade.

- Manter cadastros simples como operacoes diretas e bem validadas quando nao houver comportamento complexo.

- Criar interfaces para notificacoes, pagamentos, armazenamento, relogio e outros pontos variaveis.

- Evitar interfaces artificiais para classes com uma unica implementacao e nenhuma variacao prevista.

# Seguranca e operacao

| **Area** | **Diretriz inicial** |
|:---|----|
| Autenticacao | JWT de curta duracao com refresh token rotacionado. |
| Autorizacao | Politicas por papel, permissao e contexto da empresa. |
| Multi tenancy | account_id derivado do usuario autenticado e aplicado em todas as consultas. |
| Validacao | Entradas validadas na borda e invariantes verificadas no dominio. |
| Observabilidade | Logs estruturados, correlacao, metricas, traces e health checks. |
| Tarefas | BackgroundService no MVP; processo separado somente quando necessario. |
| Entrega | Containers e configuracao externa, sem dependencia do provedor de hospedagem. |

# BFF e evolucao

No MVP, React consumira diretamente a API. Endpoints de leitura podem retornar visoes adequadas ao dashboard, calendario e pagina publica. Um BFF separado passa a ser considerado quando painel, aplicativo mobile, white labels ou parceiros externos exigirem contratos, autenticacao ou agregacoes significativamente diferentes.

# Fora do MVP

- Microsservicos, service mesh e broker de mensagens obrigatorio.

- CQRS com infraestrutura separada para leitura e escrita.

- BFF dedicado, API Gateway complexo e multiplos bancos por modulo.

- Orquestracao distribuida de transacoes e event sourcing.

# Riscos e tratamento

| **Risco** | **Tratamento proposto** |
|:---|----|
| Monolito perder limites | Dependencias entre modulos revisadas e contratos internos explicitos. |
| DDD gerar excesso de classes | Aplicar modelagem rica somente nos dominios com regras reais. |
| Vazamento entre tenants | Filtros globais, testes de isolamento e chaves compostas no banco. |
| Integracoes bloquearem requisicoes | Outbox e processamento assincrono quando a confiabilidade exigir. |

# Pontos para aprovacao dos parceiros

1.  Aprovar o monolito modular como arquitetura da primeira versao.

2.  Confirmar os modulos que pertencem ao nucleo obrigatorio do MVP.

3.  Aprovar DDD e SOLID como ferramentas pragmaticas, sem obrigacao cerimonial.

4.  Registrar BFF e microsservicos como decisoes futuras condicionadas a necessidade comprovada.
