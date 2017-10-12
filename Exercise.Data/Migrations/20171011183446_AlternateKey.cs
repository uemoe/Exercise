using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace Exercise.Data.Migrations
{
    public partial class AlternateKey : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddUniqueConstraint(
                name: "AK_Races_OuterId",
                table: "Races",
                column: "OuterId");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Meetings_OuterId",
                table: "Meetings",
                column: "OuterId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropUniqueConstraint(
                name: "AK_Races_OuterId",
                table: "Races");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_Meetings_OuterId",
                table: "Meetings");
        }
    }
}
