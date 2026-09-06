using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace eVote360.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ModuloEleccionesYVotacion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AsignacionesCandidatoPuesto_Candidatos_CandidatoId",
                table: "AsignacionesCandidatoPuesto");

            migrationBuilder.DropForeignKey(
                name: "FK_AsignacionesCandidatoPuesto_Elecciones_EleccionId",
                table: "AsignacionesCandidatoPuesto");

            migrationBuilder.DropForeignKey(
                name: "FK_AsignacionesCandidatoPuesto_PuestosElectivos_PuestoElectivoId",
                table: "AsignacionesCandidatoPuesto");

            migrationBuilder.DropForeignKey(
                name: "FK_Candidatos_PartidosPoliticos_PartidoPoliticoId",
                table: "Candidatos");

            migrationBuilder.DropForeignKey(
                name: "FK_Candidatos_PuestosElectivos_PuestoElectivoId",
                table: "Candidatos");

            migrationBuilder.DropIndex(
                name: "IX_Candidatos_PuestoElectivoId",
                table: "Candidatos");

            migrationBuilder.DropIndex(
                name: "IX_AsignacionesCandidatoPuesto_EleccionId",
                table: "AsignacionesCandidatoPuesto");

            migrationBuilder.DropColumn(
                name: "PuestoElectivoId",
                table: "Candidatos");

            migrationBuilder.DropColumn(
                name: "Activo",
                table: "AsignacionesCandidatoPuesto");

            migrationBuilder.RenameColumn(
                name: "EleccionId",
                table: "AsignacionesCandidatoPuesto",
                newName: "PartidoPoliticoId");

            migrationBuilder.AddColumn<int>(
                name: "Estado",
                table: "Elecciones",
                type: "int",
                nullable: false,
                defaultValue: 1);

            // Conserva el estado de las elecciones existentes antes de eliminar las columnas booleanas
            migrationBuilder.Sql("UPDATE Elecciones SET Estado = CASE WHEN Finalizada = 1 THEN 3 WHEN Activa = 1 THEN 2 ELSE 1 END");

            migrationBuilder.DropColumn(name: "Activa", table: "Elecciones");
            migrationBuilder.DropColumn(name: "Finalizada", table: "Elecciones");

            // Las asignaciones pasan a pertenecer al partido de origen del candidato
            migrationBuilder.Sql("UPDATE a SET a.PartidoPoliticoId = c.PartidoPoliticoId FROM AsignacionesCandidatoPuesto a INNER JOIN Candidatos c ON c.Id = a.CandidatoId");


            migrationBuilder.AddColumn<DateTime>(
                name: "FechaActivacion",
                table: "Elecciones",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaCreacion",
                table: "Elecciones",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaFinalizacion",
                table: "Elecciones",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaRespuesta",
                table: "AlianzasPoliticas",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Vigente",
                table: "AlianzasPoliticas",
                type: "bit",
                nullable: false,
                defaultValue: false);

            // Las solicitudes aceptadas existentes son alianzas vigentes
            migrationBuilder.Sql("UPDATE AlianzasPoliticas SET Vigente = 1 WHERE Estado = 2");
            migrationBuilder.Sql("UPDATE Elecciones SET FechaCreacion = FechaEleccion");

            migrationBuilder.CreateTable(
                name: "CandidaturasEleccion",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EleccionId = table.Column<int>(type: "int", nullable: false),
                    PartidoPoliticoId = table.Column<int>(type: "int", nullable: false),
                    CandidatoId = table.Column<int>(type: "int", nullable: false),
                    PuestoElectivoId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CandidaturasEleccion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CandidaturasEleccion_Candidatos_CandidatoId",
                        column: x => x.CandidatoId,
                        principalTable: "Candidatos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CandidaturasEleccion_Elecciones_EleccionId",
                        column: x => x.EleccionId,
                        principalTable: "Elecciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CandidaturasEleccion_PartidosPoliticos_PartidoPoliticoId",
                        column: x => x.PartidoPoliticoId,
                        principalTable: "PartidosPoliticos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CandidaturasEleccion_PuestosElectivos_PuestoElectivoId",
                        column: x => x.PuestoElectivoId,
                        principalTable: "PuestosElectivos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CodigosVerificacion",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CiudadanoId = table.Column<int>(type: "int", nullable: false),
                    EleccionId = table.Column<int>(type: "int", nullable: false),
                    Codigo = table.Column<string>(type: "nvarchar(6)", maxLength: 6, nullable: false),
                    FechaGeneracion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaExpiracion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Usado = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CodigosVerificacion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CodigosVerificacion_Ciudadanos_CiudadanoId",
                        column: x => x.CiudadanoId,
                        principalTable: "Ciudadanos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CodigosVerificacion_Elecciones_EleccionId",
                        column: x => x.EleccionId,
                        principalTable: "Elecciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ParticipacionesEleccion",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EleccionId = table.Column<int>(type: "int", nullable: false),
                    CiudadanoId = table.Column<int>(type: "int", nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParticipacionesEleccion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ParticipacionesEleccion_Ciudadanos_CiudadanoId",
                        column: x => x.CiudadanoId,
                        principalTable: "Ciudadanos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ParticipacionesEleccion_Elecciones_EleccionId",
                        column: x => x.EleccionId,
                        principalTable: "Elecciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Votos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EleccionId = table.Column<int>(type: "int", nullable: false),
                    PuestoElectivoId = table.Column<int>(type: "int", nullable: false),
                    CandidatoId = table.Column<int>(type: "int", nullable: true),
                    PartidoPoliticoId = table.Column<int>(type: "int", nullable: true),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Votos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Votos_Candidatos_CandidatoId",
                        column: x => x.CandidatoId,
                        principalTable: "Candidatos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Votos_Elecciones_EleccionId",
                        column: x => x.EleccionId,
                        principalTable: "Elecciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Votos_PartidosPoliticos_PartidoPoliticoId",
                        column: x => x.PartidoPoliticoId,
                        principalTable: "PartidosPoliticos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Votos_PuestosElectivos_PuestoElectivoId",
                        column: x => x.PuestoElectivoId,
                        principalTable: "PuestosElectivos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_CorreoElectronico",
                table: "Usuarios",
                column: "CorreoElectronico",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_NombreUsuario",
                table: "Usuarios",
                column: "NombreUsuario",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PartidosPoliticos_Siglas",
                table: "PartidosPoliticos",
                column: "Siglas",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AsignacionesCandidatoPuesto_PartidoPoliticoId_CandidatoId",
                table: "AsignacionesCandidatoPuesto",
                columns: new[] { "PartidoPoliticoId", "CandidatoId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AsignacionesCandidatoPuesto_PartidoPoliticoId_PuestoElectivoId",
                table: "AsignacionesCandidatoPuesto",
                columns: new[] { "PartidoPoliticoId", "PuestoElectivoId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CandidaturasEleccion_CandidatoId",
                table: "CandidaturasEleccion",
                column: "CandidatoId");

            migrationBuilder.CreateIndex(
                name: "IX_CandidaturasEleccion_EleccionId_PartidoPoliticoId_PuestoElectivoId",
                table: "CandidaturasEleccion",
                columns: new[] { "EleccionId", "PartidoPoliticoId", "PuestoElectivoId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CandidaturasEleccion_PartidoPoliticoId",
                table: "CandidaturasEleccion",
                column: "PartidoPoliticoId");

            migrationBuilder.CreateIndex(
                name: "IX_CandidaturasEleccion_PuestoElectivoId",
                table: "CandidaturasEleccion",
                column: "PuestoElectivoId");

            migrationBuilder.CreateIndex(
                name: "IX_CodigosVerificacion_CiudadanoId_EleccionId",
                table: "CodigosVerificacion",
                columns: new[] { "CiudadanoId", "EleccionId" });

            migrationBuilder.CreateIndex(
                name: "IX_CodigosVerificacion_EleccionId",
                table: "CodigosVerificacion",
                column: "EleccionId");

            migrationBuilder.CreateIndex(
                name: "IX_ParticipacionesEleccion_CiudadanoId",
                table: "ParticipacionesEleccion",
                column: "CiudadanoId");

            migrationBuilder.CreateIndex(
                name: "IX_ParticipacionesEleccion_EleccionId_CiudadanoId",
                table: "ParticipacionesEleccion",
                columns: new[] { "EleccionId", "CiudadanoId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Votos_CandidatoId",
                table: "Votos",
                column: "CandidatoId");

            migrationBuilder.CreateIndex(
                name: "IX_Votos_EleccionId_PuestoElectivoId",
                table: "Votos",
                columns: new[] { "EleccionId", "PuestoElectivoId" });

            migrationBuilder.CreateIndex(
                name: "IX_Votos_PartidoPoliticoId",
                table: "Votos",
                column: "PartidoPoliticoId");

            migrationBuilder.CreateIndex(
                name: "IX_Votos_PuestoElectivoId",
                table: "Votos",
                column: "PuestoElectivoId");

            migrationBuilder.AddForeignKey(
                name: "FK_AsignacionesCandidatoPuesto_Candidatos_CandidatoId",
                table: "AsignacionesCandidatoPuesto",
                column: "CandidatoId",
                principalTable: "Candidatos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AsignacionesCandidatoPuesto_PartidosPoliticos_PartidoPoliticoId",
                table: "AsignacionesCandidatoPuesto",
                column: "PartidoPoliticoId",
                principalTable: "PartidosPoliticos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AsignacionesCandidatoPuesto_PuestosElectivos_PuestoElectivoId",
                table: "AsignacionesCandidatoPuesto",
                column: "PuestoElectivoId",
                principalTable: "PuestosElectivos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Candidatos_PartidosPoliticos_PartidoPoliticoId",
                table: "Candidatos",
                column: "PartidoPoliticoId",
                principalTable: "PartidosPoliticos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AsignacionesCandidatoPuesto_Candidatos_CandidatoId",
                table: "AsignacionesCandidatoPuesto");

            migrationBuilder.DropForeignKey(
                name: "FK_AsignacionesCandidatoPuesto_PartidosPoliticos_PartidoPoliticoId",
                table: "AsignacionesCandidatoPuesto");

            migrationBuilder.DropForeignKey(
                name: "FK_AsignacionesCandidatoPuesto_PuestosElectivos_PuestoElectivoId",
                table: "AsignacionesCandidatoPuesto");

            migrationBuilder.DropForeignKey(
                name: "FK_Candidatos_PartidosPoliticos_PartidoPoliticoId",
                table: "Candidatos");

            migrationBuilder.DropTable(
                name: "CandidaturasEleccion");

            migrationBuilder.DropTable(
                name: "CodigosVerificacion");

            migrationBuilder.DropTable(
                name: "ParticipacionesEleccion");

            migrationBuilder.DropTable(
                name: "Votos");

            migrationBuilder.DropIndex(
                name: "IX_Usuarios_CorreoElectronico",
                table: "Usuarios");

            migrationBuilder.DropIndex(
                name: "IX_Usuarios_NombreUsuario",
                table: "Usuarios");

            migrationBuilder.DropIndex(
                name: "IX_PartidosPoliticos_Siglas",
                table: "PartidosPoliticos");

            migrationBuilder.DropIndex(
                name: "IX_AsignacionesCandidatoPuesto_PartidoPoliticoId_CandidatoId",
                table: "AsignacionesCandidatoPuesto");

            migrationBuilder.DropIndex(
                name: "IX_AsignacionesCandidatoPuesto_PartidoPoliticoId_PuestoElectivoId",
                table: "AsignacionesCandidatoPuesto");

            migrationBuilder.DropColumn(
                name: "Estado",
                table: "Elecciones");

            migrationBuilder.DropColumn(
                name: "FechaActivacion",
                table: "Elecciones");

            migrationBuilder.DropColumn(
                name: "FechaCreacion",
                table: "Elecciones");

            migrationBuilder.DropColumn(
                name: "FechaFinalizacion",
                table: "Elecciones");

            migrationBuilder.DropColumn(
                name: "FechaRespuesta",
                table: "AlianzasPoliticas");

            migrationBuilder.DropColumn(
                name: "Vigente",
                table: "AlianzasPoliticas");

            migrationBuilder.RenameColumn(
                name: "PartidoPoliticoId",
                table: "AsignacionesCandidatoPuesto",
                newName: "EleccionId");

            migrationBuilder.AddColumn<bool>(
                name: "Activa",
                table: "Elecciones",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Finalizada",
                table: "Elecciones",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "PuestoElectivoId",
                table: "Candidatos",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Activo",
                table: "AsignacionesCandidatoPuesto",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Candidatos_PuestoElectivoId",
                table: "Candidatos",
                column: "PuestoElectivoId");

            migrationBuilder.CreateIndex(
                name: "IX_AsignacionesCandidatoPuesto_EleccionId",
                table: "AsignacionesCandidatoPuesto",
                column: "EleccionId");

            migrationBuilder.AddForeignKey(
                name: "FK_AsignacionesCandidatoPuesto_Candidatos_CandidatoId",
                table: "AsignacionesCandidatoPuesto",
                column: "CandidatoId",
                principalTable: "Candidatos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AsignacionesCandidatoPuesto_Elecciones_EleccionId",
                table: "AsignacionesCandidatoPuesto",
                column: "EleccionId",
                principalTable: "Elecciones",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AsignacionesCandidatoPuesto_PuestosElectivos_PuestoElectivoId",
                table: "AsignacionesCandidatoPuesto",
                column: "PuestoElectivoId",
                principalTable: "PuestosElectivos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Candidatos_PartidosPoliticos_PartidoPoliticoId",
                table: "Candidatos",
                column: "PartidoPoliticoId",
                principalTable: "PartidosPoliticos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Candidatos_PuestosElectivos_PuestoElectivoId",
                table: "Candidatos",
                column: "PuestoElectivoId",
                principalTable: "PuestosElectivos",
                principalColumn: "Id");
        }
    }
}
