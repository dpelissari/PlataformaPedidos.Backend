# 002. Cache Redis para pedido e CEP

## Status

Aceito

## Contexto

A consulta de CEP chama um serviço externo. A consulta de pedido por id é repetida pela tela enquanto o pagamento está em andamento. A API e o worker são processos separados, então um cache só na memória de um deles não vale para o outro.

## Decisão

O Redis guarda a consulta de CEP por 24 horas, na chave `ceps:{cep}`, e a consulta do pedido por 10 minutos, na chave `pedidos:{id}`. A leitura olha o cache antes do serviço externo ou do banco. Solicitar pagamento, cancelar e a confirmação feita pelo worker removem a chave do pedido, para a próxima leitura não devolver o status antigo.

## Consequências

A segunda consulta do mesmo CEP não chama o serviço externo. API e worker enxergam a mesma invalidação. O Redis fora do ar impede a consulta que depende do cache, em vez de seguir direto para a origem. O endereço de um CEP raro fica desatualizado até expirar as 24 horas.
