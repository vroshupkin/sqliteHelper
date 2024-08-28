// See https://aka.ms/new-console-template for more information
using Microsoft.Data.Sqlite;

SqliteHelper sqlite = new("Чёрная.sqlite");

string columnName = "Consumables";
List<string> columns = ["protectiveCap", "protectiveShield", "nozzleCasing", "nozzle", "swirling", "electrode", "tube"];
columns.ForEach(
    column => sqlite.AlterColumn(columnName, column, "text")
);

class SqliteHelper
{
    private readonly string databaseName;
    private readonly string connectionString;

    public SqliteHelper(string _databaseName)
    {
        databaseName = _databaseName;
        connectionString = $@"Data Source={databaseName}";
    }

    // Меняет в таблице тип колонки
    public void AlterColumn(string tableName, string columnName, string type)
    {
        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();
            // Begin a transaction
            using (var transaction = connection.BeginTransaction())
            {
                try
                {
                    using (var command = connection.CreateCommand())
                    {
                        // 1. Переименование колонки в COPY_{columnName}
                        // 2. Создание пустой колонки с нужным названием и нужным типом данных
                        // 3. Копирование данных в колонку с нужным названием
                        // 4. Удаление копии колонки
                        List<string> commands = [
                            $"ALTER TABLE   {tableName} RENAME COLUMN    {columnName} TO COPY_{columnName};",
                            $"ALTER TABLE   {tableName} ADD COLUMN       {columnName} {type};",
                            $"UPDATE        {tableName} SET              {columnName} = COPY_{columnName};",
                            $"ALTER TABLE   {tableName} DROP COLUMN COPY_{columnName};"
                        ];

                        commands.ForEach(cmd =>
                        {
                            command.CommandText = cmd;
                            command.ExecuteNonQuery();
                        });
                    }

                    // Commit the transaction
                    transaction.Commit();
                    Console.WriteLine($"Тип данных колонки {columnName} успешно изменен на {type}");
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    Console.WriteLine($"Транзакция отменена с ошибкой: {ex.Message}");
                }
            }
            connection.Close();
        }
    }
}

