# 001. Outbox para publicar eventos de pedido

## Status

Aceito

## Contexto

Criar um pedido, solicitar pagamento e cancelar gravam o agregado e, ao mesmo tempo, produzem um evento. Publicar no Kafka dentro do mesmo método da API acopla o commit do PostgreSQL à disponibilidade do broker. Se o banco confirma e o Kafka falha, o evento se perde. Se o Kafka aceita e o banco desfaz a transação, o consumidor enxerga um pedido que não existe.

## Decisão

O evento é gravado na tabela `mensagens_outbox` na mesma transação do pedido. O `ProcessadorOutboxWorker` lê as mensagens pendentes, publica no tópico correspondente e só então marca a data de processamento.

Os tópicos são `pedidos.criado`, `pedidos.pagamento-solicitado`, `pedidos.confirmado` e `pedidos.cancelado`. A chave da mensagem é o identificador do pedido, para preservar a ordem dele na partição.

## Consequências

O pedido e o evento são confirmados juntos. Uma falha do Kafka atrasa a publicação, sem desfazer o pedido. O worker precisa estar em execução para o pagamento seguir adiante. A publicação deixa de ser imediata e passa a depender do intervalo da outbox.
