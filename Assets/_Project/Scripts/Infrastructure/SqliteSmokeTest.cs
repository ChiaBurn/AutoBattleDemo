using System;
using System.IO;
using Microsoft.Data.Sqlite;
using UnityEngine;

/// <summary>
/// Minimal smoke test for Microsoft.Data.Sqlite in Unity.
///
/// Purpose:
/// 1. Verify that Microsoft.Data.Sqlite can compile in Unity.
/// 2. Verify that the SQLite native provider can be initialized.
/// 3. Verify that Unity can create, insert, and read a SQLite database file.
///
/// This is a temporary diagnostic script.
/// It is not part of the final battle system.
/// </summary>
public sealed class SqliteSmokeTest : MonoBehaviour
{
    private const string DatabaseFileName = "battle_smoke_test.db";

    private void Start()
    {
        RunSmokeTest();
    }

    private void RunSmokeTest()
    {
        try
        {
            // Microsoft.Data.Sqlite depends on SQLitePCLRaw.
            // In Unity, explicitly initializing the provider is safer than assuming auto-initialization.
            SQLitePCL.Batteries_V2.Init();

            string databasePath = Path.Combine(UnityEngine.Application.persistentDataPath, DatabaseFileName);
            string connectionString = $"Data Source={databasePath}";

            using SqliteConnection connection = new SqliteConnection(connectionString);
            connection.Open();

            CreateTable(connection);
            int insertedId = InsertTestRow(connection);
            string message = ReadTestRow(connection, insertedId);

            Debug.Log(
                $"[SQLite Smoke Test] Success. " +
                $"InsertedId={insertedId}, Message=\"{message}\", DatabasePath=\"{databasePath}\""
            );
        }
        catch (Exception exception)
        {
            Debug.LogError($"[SQLite Smoke Test] Failed.\n{exception}");
        }
    }

    private static void CreateTable(SqliteConnection connection)
    {
        using SqliteCommand command = connection.CreateCommand();
        command.CommandText =
            @"
            CREATE TABLE IF NOT EXISTS SmokeTestLog
            (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Message TEXT NOT NULL,
                CreatedAt TEXT NOT NULL
            );
            ";

        command.ExecuteNonQuery();
    }

    private static int InsertTestRow(SqliteConnection connection)
    {
        using SqliteCommand command = connection.CreateCommand();
        command.CommandText =
            @"
            INSERT INTO SmokeTestLog (Message, CreatedAt)
            VALUES ($message, $createdAt);

            SELECT last_insert_rowid();
            ";

        command.Parameters.AddWithValue("$message", "Hello from Unity SQLite smoke test.");
        command.Parameters.AddWithValue("$createdAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

        object? result = command.ExecuteScalar();

        if (result == null)
        {
            throw new InvalidOperationException("Failed to retrieve last_insert_rowid().");
        }

        return Convert.ToInt32(result);
    }

    private static string ReadTestRow(SqliteConnection connection, int id)
    {
        using SqliteCommand command = connection.CreateCommand();
        command.CommandText =
            @"
            SELECT Message
            FROM SmokeTestLog
            WHERE Id = $id;
            ";

        command.Parameters.AddWithValue("$id", id);

        object? result = command.ExecuteScalar();

        if (result == null)
        {
            throw new InvalidOperationException($"Smoke test row not found. Id={id}");
        }

        return Convert.ToString(result) ?? string.Empty;
    }
}