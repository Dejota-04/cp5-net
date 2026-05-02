# Checkpoint 5: Programação de API com Microservices e Mensageria

**Instituição:** FIAP - Faculdade de Informática e Administração Paulista
**Curso:** Tecnologia em Análise e Desenvolvimento de Sistemas (TDS)
**Professor:** Dr. Marcel Stefan Wagner
**Repositório GitHub:** https://github.com/Dejota-04/cp5-net
**Link do Vídeo (YouTube):** [INSERIR LINK AQUI]

## 👥 Integrantes do Grupo
* Daniel Santana Corrêa Batista [RM559622]

* Wendell Nascimento Dourado [RA559336]

* Jonas de Jesus Campos de Oliveira [RM561144]

---

## 🏗️ Descrição do Projeto

Este projeto implementa uma arquitetura de mensageria assíncrona utilizando RabbitMQ e .NET (C#), dividida em microsserviços baseados em console. O objetivo é trafegar informações de dois domínios distintos (Frutas de Época e Registros de Usuários Hortifruti), passando por uma camada intermediária de validação antes de chegar aos consumidores finais.

---

## 🐇 Topologia RabbitMQ

Optamos pelo uso de uma **Topic Exchange**, que oferece maior flexibilidade no roteamento das mensagens entre os Senders, o Serviço de Validação e os Receivers, utilizando *Routing Keys* específicas.

* **Exchange:** `fiap.checkpoint.topic` (Tipo: `topic`)

### Filas e Routing Keys
| Domínio | Fila (Queue) | Routing Key | Origem -> Destino |
| :--- | :--- | :--- | :--- |
| **Frutas** | `queue.frutas.unvalidated` | `fruta.request` | Sender 1 -> Validation Service |
| **Frutas** | `queue.frutas.validated` | `fruta.validated` | Validation Service -> Receiver 1 |
| **Usuários** | `queue.usuarios.unvalidated` | `usuario.request` | Sender 2 -> Validation Service |
| **Usuários** | `queue.usuarios.validated` | `usuario.validated` | Validation Service -> Receiver 2 |

---

## ⚙️ Processo de Validação

O projeto `Validation.Service` atua como o nó central de orquestração (consumindo e publicando). Ele opera com confirmação manual (`autoAck: false`) para garantir a integridade da mensagem em caso de falha no processamento.

**1. Fluxo Sender 1 / Receiver 1 (Frutas):**
* O `Validation.Service` consome a mensagem da fila `unvalidated`.
* Altera o status da flag `IsValidated` para `true`.
* Publica o DTO atualizado na Exchange utilizando a routing key `fruta.validated`.
* Realiza o *Ack* manual para remover a mensagem original da fila de entrada.

**2. Fluxo Sender 2 / Receiver 2 (Usuários):**
* O `Validation.Service` consome a mensagem contendo os dados do usuário.
* Altera o status da flag `IsValidated` para `true`.
* Publica o DTO na Exchange utilizando a routing key `usuario.validated`.
* Realiza o *Ack* manual.

---

## 🚀 Exemplo de Execução

**Ordem de Inicialização (5 terminais):**
1. `dotnet run --project Validation.Service` (Responsável por declarar as Exchanges e Queues no broker).
2. `dotnet run --project Receiver1.Frutas`
3. `dotnet run --project Receiver2.Usuarios`
4. `dotnet run --project Sender1.Frutas`
5. `dotnet run --project Sender2.Usuarios`

**Exemplo de log - Fluxo de Usuários:**
* **Sender 2:** `[x] Enviado Registro de: João da Silva às 01/05/2026 20:36:48`
* **Validation:** `[Usuários] Validando dados de: João da Silva...`
* **Receiver 2:**
  ```text
  [Novo Usuário Recebido]
  - Nome: João da Silva
  - Endereço: Rua das Laranjeiras, 123
  - RG: 123456789
  - CPF: 11122233344
  - Data/Hora Registro: 01/05/2026 20:36:48
  - Status de Validação: VALIDADO