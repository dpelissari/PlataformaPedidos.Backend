# 003. Idempotência do consumidor de pagamento

## Status

Aceito

## Contexto

O Kafka pode reenviar `pedidos.pagamento-solicitado` depois de uma falha, de um rebalanceamento ou de uma nova publicação da mesma mensagem de outbox. Autorizar o pagamento de novo cobraria o pedido duas vezes ou confirmaria um pedido que já saiu de `AguardandoPagamento`.

## Decisão

Cada mensagem de outbox tem um `MensagemId`, enviado no cabeçalho Kafka. O `ConsumidorPagamentoPedido` consulta `mensagens_processadas` antes de chamar o gateway. Se o identificador já existe, a mensagem é ignorada. Depois de autorizar e atualizar o pedido, o mesmo identificador é gravado nessa tabela. Falha transitória não confirma o offset, para o broker reenviar. A reentrega encontra o registro e não chama o gateway outra vez.

## Consequências

A confirmação do pagamento acontece uma vez, mesmo com mensagem duplicada. O offset só avança depois do processamento bem-sucedido, então uma queda no meio do caminho não perde a mensagem. A tabela de processadas cresce com cada mensagem aceita e passa a fazer parte do fluxo, não só do log.
