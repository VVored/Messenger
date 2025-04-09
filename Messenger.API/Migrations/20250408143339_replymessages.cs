using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Messenger.API.Migrations
{
    /// <inheritdoc />
    public partial class replymessages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RepliableMessageId",
                table: "Messages",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Messages_RepliableMessageId",
                table: "Messages",
                column: "RepliableMessageId");

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_Messages_RepliableMessageId",
                table: "Messages",
                column: "RepliableMessageId",
                principalTable: "Messages",
                principalColumn: "MessageId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Messages_Messages_RepliableMessageId",
                table: "Messages");

            migrationBuilder.DropIndex(
                name: "IX_Messages_RepliableMessageId",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "RepliableMessageId",
                table: "Messages");
        }
    }
}
