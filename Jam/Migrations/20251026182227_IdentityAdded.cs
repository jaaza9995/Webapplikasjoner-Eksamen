using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Jam.Migrations
{
    /// <inheritdoc />
    public partial class IdentityAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AnswerOptions_Questions_QuestionId",
                table: "AnswerOptions");

            migrationBuilder.DropForeignKey(
                name: "FK_PlayingSessions_Scenes_CurrentSceneId",
                table: "PlayingSessions");

            migrationBuilder.DropTable(
                name: "Questions");

            migrationBuilder.DropTable(
                name: "Scenes");

            migrationBuilder.DropIndex(
                name: "IX_PlayingSessions_CurrentSceneId",
                table: "PlayingSessions");

            migrationBuilder.RenameColumn(
                name: "Code",
                table: "Stories",
                newName: "GameCode");

            migrationBuilder.RenameColumn(
                name: "SceneText",
                table: "AnswerOptions",
                newName: "FeedbackText");

            migrationBuilder.RenameColumn(
                name: "QuestionId",
                table: "AnswerOptions",
                newName: "QuestionSceneId");

            migrationBuilder.RenameIndex(
                name: "IX_AnswerOptions_QuestionId",
                table: "AnswerOptions",
                newName: "IX_AnswerOptions_QuestionSceneId");

            migrationBuilder.AddColumn<int>(
                name: "CurrentSceneType",
                table: "PlayingSessions",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    UserName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "INTEGER", nullable: false),
                    PasswordHash = table.Column<string>(type: "TEXT", nullable: true),
                    SecurityStamp = table.Column<string>(type: "TEXT", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "TEXT", nullable: true),
                    PhoneNumber = table.Column<string>(type: "TEXT", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "INTEGER", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EndingScenes",
                columns: table => new
                {
                    EndingSceneId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EndingType = table.Column<int>(type: "INTEGER", nullable: false),
                    EndingText = table.Column<string>(type: "TEXT", nullable: false),
                    StoryId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EndingScenes", x => x.EndingSceneId);
                    table.ForeignKey(
                        name: "FK_EndingScenes_Stories_StoryId",
                        column: x => x.StoryId,
                        principalTable: "Stories",
                        principalColumn: "StoryId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IntroScenes",
                columns: table => new
                {
                    IntroSceneId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    IntroText = table.Column<string>(type: "TEXT", nullable: false),
                    StoryId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IntroScenes", x => x.IntroSceneId);
                    table.ForeignKey(
                        name: "FK_IntroScenes_Stories_StoryId",
                        column: x => x.StoryId,
                        principalTable: "Stories",
                        principalColumn: "StoryId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QuestionScenes",
                columns: table => new
                {
                    QuestionSceneId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SceneText = table.Column<string>(type: "TEXT", nullable: false),
                    Question = table.Column<string>(type: "TEXT", nullable: false),
                    NextQuestionSceneId = table.Column<int>(type: "INTEGER", nullable: true),
                    StoryId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestionScenes", x => x.QuestionSceneId);
                    table.ForeignKey(
                        name: "FK_QuestionScenes_QuestionScenes_NextQuestionSceneId",
                        column: x => x.NextQuestionSceneId,
                        principalTable: "QuestionScenes",
                        principalColumn: "QuestionSceneId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_QuestionScenes_Stories_StoryId",
                        column: x => x.StoryId,
                        principalTable: "Stories",
                        principalColumn: "StoryId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RoleId = table.Column<string>(type: "TEXT", nullable: false),
                    ClaimType = table.Column<string>(type: "TEXT", nullable: true),
                    ClaimValue = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    ClaimType = table.Column<string>(type: "TEXT", nullable: true),
                    ClaimValue = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "TEXT", nullable: false),
                    ProviderKey = table.Column<string>(type: "TEXT", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "TEXT", nullable: true),
                    UserId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    RoleId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    LoginProvider = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Value = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Stories_GameCode",
                table: "Stories",
                column: "GameCode",
                unique: true,
                filter: "[GameCode] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EndingScenes_StoryId",
                table: "EndingScenes",
                column: "StoryId");

            migrationBuilder.CreateIndex(
                name: "IX_IntroScenes_StoryId",
                table: "IntroScenes",
                column: "StoryId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QuestionScenes_NextQuestionSceneId",
                table: "QuestionScenes",
                column: "NextQuestionSceneId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QuestionScenes_StoryId",
                table: "QuestionScenes",
                column: "StoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_AnswerOptions_QuestionScenes_QuestionSceneId",
                table: "AnswerOptions",
                column: "QuestionSceneId",
                principalTable: "QuestionScenes",
                principalColumn: "QuestionSceneId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AnswerOptions_QuestionScenes_QuestionSceneId",
                table: "AnswerOptions");

            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "EndingScenes");

            migrationBuilder.DropTable(
                name: "IntroScenes");

            migrationBuilder.DropTable(
                name: "QuestionScenes");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_Stories_GameCode",
                table: "Stories");

            migrationBuilder.DropColumn(
                name: "CurrentSceneType",
                table: "PlayingSessions");

            migrationBuilder.RenameColumn(
                name: "GameCode",
                table: "Stories",
                newName: "Code");

            migrationBuilder.RenameColumn(
                name: "QuestionSceneId",
                table: "AnswerOptions",
                newName: "QuestionId");

            migrationBuilder.RenameColumn(
                name: "FeedbackText",
                table: "AnswerOptions",
                newName: "SceneText");

            migrationBuilder.RenameIndex(
                name: "IX_AnswerOptions_QuestionSceneId",
                table: "AnswerOptions",
                newName: "IX_AnswerOptions_QuestionId");

            migrationBuilder.CreateTable(
                name: "Scenes",
                columns: table => new
                {
                    SceneId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    NextSceneId = table.Column<int>(type: "INTEGER", nullable: true),
                    StoryId = table.Column<int>(type: "INTEGER", nullable: false),
                    SceneText = table.Column<string>(type: "TEXT", nullable: false),
                    SceneType = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Scenes", x => x.SceneId);
                    table.ForeignKey(
                        name: "FK_Scenes_Scenes_NextSceneId",
                        column: x => x.NextSceneId,
                        principalTable: "Scenes",
                        principalColumn: "SceneId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Scenes_Stories_StoryId",
                        column: x => x.StoryId,
                        principalTable: "Stories",
                        principalColumn: "StoryId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Questions",
                columns: table => new
                {
                    QuestionId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SceneId = table.Column<int>(type: "INTEGER", nullable: false),
                    QuestionText = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Questions", x => x.QuestionId);
                    table.ForeignKey(
                        name: "FK_Questions_Scenes_SceneId",
                        column: x => x.SceneId,
                        principalTable: "Scenes",
                        principalColumn: "SceneId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlayingSessions_CurrentSceneId",
                table: "PlayingSessions",
                column: "CurrentSceneId");

            migrationBuilder.CreateIndex(
                name: "IX_Questions_SceneId",
                table: "Questions",
                column: "SceneId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Scenes_NextSceneId",
                table: "Scenes",
                column: "NextSceneId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Scenes_StoryId",
                table: "Scenes",
                column: "StoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_AnswerOptions_Questions_QuestionId",
                table: "AnswerOptions",
                column: "QuestionId",
                principalTable: "Questions",
                principalColumn: "QuestionId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PlayingSessions_Scenes_CurrentSceneId",
                table: "PlayingSessions",
                column: "CurrentSceneId",
                principalTable: "Scenes",
                principalColumn: "SceneId");
        }
    }
}
