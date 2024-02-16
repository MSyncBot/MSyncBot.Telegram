using System.Runtime.CompilerServices;
using MySql.Data.MySqlClient;

namespace MSyncBot.Telegram.Bot.Data;

public class User : global::Telegram.Bot.Types.User
{
    public User(global::Telegram.Bot.Types.User user)
    {
        FirstName = user.FirstName;
        LastName = user.LastName ?? null;
        Username = user.Username ?? null;
        Id = user.Id;
        IsRegistered = false;
    }
    public bool IsRegistered { get; set; }
    public bool IsAdministrator { get; set; }
    public int? AdministratorLevel { get; set; }

    /*public async Task AddAsync()
    {
        try
        {
            Bot.Logger.LogInformation(
                $"==========\n" +
                $"Новый пользователь!\n" +
                $"ID: {Id}\n" +
                $"Имя: {FirstName}\n" +
                $"Фамилия: {LastName ?? "Отсутствует"}\n" +
                $"Логин: {Username ?? "Отсутствует"}\n" +
                $"==========");

            Bot.Logger.LogProcess($"Добавляю пользователя {FirstName} ({Id}) в базу данных...");

            var sqlQuery = "INSERT INTO Users (Id, FirstName, LastName, UserName)" +
                " VALUES (@Id, @FirstName, @LastName, @UserName)";
            await Bot.Database.ExecuteNonQueryAsync(sqlQuery, new MySqlParameter("Id", Id.ToString()),
                new MySqlParameter("FirstName", FirstName), new MySqlParameter("LastName", LastName),
                new MySqlParameter("UserName", Username));

            Bot.Logger.LogSuccess($"Пользователь {FirstName} ({Id}) успешно добавлен в базу данных.");
        }
        catch (Exception ex) when (ex.Message.Contains("Duplicate entry"))
        {
            Bot.Logger.LogError($"Пользователь {FirstName} ({Id}) уже существует в базе!");
        }
        catch (Exception ex)
        {
            Bot.Logger.LogError(ex.ToString());
        }
    }

    public async Task GetInfoAsync()
    {
        MySqlDataReader reader = null;
        try
        {
            reader = await Bot.Database.ExecuteQueryAsync("CALL GetUserInfo(@userId)",
                new MySqlParameter("@userId", Id));
            if (!reader.HasRows) return;

            if (await reader.ReadAsync())
            {
                Id = reader.GetInt64("UserId");
                FirstName = reader.GetString("FirstName");
                LastName = reader.IsDBNull(reader.GetOrdinal("LastName"))
                    ? null
                    : reader.GetString(reader.GetOrdinal("LastName"));
                Username = reader.IsDBNull(reader.GetOrdinal("Username"))
                    ? null
                    : reader.GetString(reader.GetOrdinal("Username"));
                IsAdministrator = reader.GetBoolean("IsAdministrator");
                AdministratorLevel = reader.GetInt32("AdministratorLevel");
                IsRegistered = true;
            }
        }
        catch (Exception ex)
        {
            Bot.Logger.LogError(ex.ToString());
        }
        finally
        {
            await reader.CloseAsync();
        }
    }

    public async Task UpdateInfoAsync(global::Telegram.Bot.Types.User user)
    {
        try
        {
            FirstName = user.FirstName;
            LastName = user.LastName;
            Username = user.Username;
            Id = user.Id;

            var sqlQuery =
                "UPDATE Users SET " +
                "FirstName = @FirstName, " +
                "LastName = @LastName, " +
                "UserName = @UserName " +
                "WHERE Id = @Id";

            await Bot.Database.ExecuteNonQueryAsync(sqlQuery,
                new MySqlParameter("FirstName", FirstName),
                new MySqlParameter("LastName", LastName),
                new MySqlParameter("UserName", Username),
                new MySqlParameter("Id", Id.ToString()));
        }
        catch (Exception ex)
        {
            Bot.Logger.LogError(ex.ToString());
        }
    }*/
}