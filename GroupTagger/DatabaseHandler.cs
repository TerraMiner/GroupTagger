using MySqlConnector;

namespace GroupTagger;

public class DatabaseHandler(GroupTagger plugin) {
    private string _databaseAddress = "";

    public string GetQuery1(int accountId, int sid, long currentTime) {
        return $"SELECT account_id, `group` FROM vip_users " +
               $"WHERE account_id = {accountId} " +
               $"AND sid = {sid} " +
               $"AND (expires > {currentTime} OR expires = 0);";
    }

    public string GetQuery2(int sid, long currentTime, string accountIds) {
        return $"SELECT account_id, `group` FROM vip_users " +
               $"WHERE sid = {sid} " +
               $"AND (expires > {currentTime} OR expires = 0) " +
               $"AND account_id IN ({accountIds});";
    }
    
    public string GetQuery3(int sid, long currentTime) {
        return $"DELETE FROM vip_users " +
               $"WHERE sid = {sid} " +
               $"AND expires <= {currentTime} AND expires > 0;";
    }


    public void InitializeDatabaseAddress(string value) {
        _databaseAddress = value;
    }

    public async Task Query(string query, Action<MySqlDataReader> handler) {
        try {
            using (var connection = new MySqlConnection(_databaseAddress)) {
                await connection.OpenAsync();
                var comm = new MySqlCommand(query, connection);
                var reader = await comm.ExecuteReaderAsync();
                while (await reader.ReadAsync()) {
                    handler(reader);
                }
            }
        }
        catch (MySqlException ex) {
            plugin.Logger.Print($"Database error: {ex}");
        }
    }
}