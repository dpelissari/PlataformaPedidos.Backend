using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlataformaPedidos.Infraestrutura.Persistencia.Migracoes
{
    /// <inheritdoc />
    public partial class MigracaoInicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "mensagens_outbox",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    topico = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    conteudo = table.Column<string>(type: "text", nullable: false),
                    trace_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    ocorrido_em_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    processada_em_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mensagens_outbox", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "mensagens_processadas",
                columns: table => new
                {
                    mensagem_id = table.Column<Guid>(type: "uuid", nullable: false),
                    consumidor = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    processada_em_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mensagens_processadas", x => x.mensagem_id);
                });

            migrationBuilder.CreateTable(
                name: "pedidos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    cliente_id = table.Column<Guid>(type: "uuid", nullable: false),
                    endereco_cep = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    endereco_logradouro = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    endereco_numero = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    endereco_complemento = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    endereco_bairro = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    endereco_cidade = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    endereco_estado = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    data_criacao_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    motivo_cancelamento = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pedidos", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "itens_pedido",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    produto_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome_produto = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    quantidade = table.Column<int>(type: "integer", nullable: false),
                    preco_unitario_valor = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    preco_unitario_moeda = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    pedido_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_itens_pedido", x => x.id);
                    table.ForeignKey(
                        name: "FK_itens_pedido_pedidos_pedido_id",
                        column: x => x.pedido_id,
                        principalTable: "pedidos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_itens_pedido_pedido_id",
                table: "itens_pedido",
                column: "pedido_id");

            migrationBuilder.CreateIndex(
                name: "IX_mensagens_outbox_processada_em_utc",
                table: "mensagens_outbox",
                column: "processada_em_utc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "itens_pedido");

            migrationBuilder.DropTable(
                name: "mensagens_outbox");

            migrationBuilder.DropTable(
                name: "mensagens_processadas");

            migrationBuilder.DropTable(
                name: "pedidos");
        }
    }
}
