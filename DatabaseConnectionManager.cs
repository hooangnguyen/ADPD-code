// Add this method to the DatabaseConnectionManager class
public IReadOnlyList<DbContextConnectionInfo> GetActiveConnections()
{
    // If you have a way to track active connections, return them here.
    // Otherwise, return an empty list or implement the tracking logic.
    return new List<DbContextConnectionInfo>();
}

// You will also need to define the DbContextConnectionInfo class if it does not exist:
public class DbContextConnectionInfo
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public TimeSpan Age => DateTime.Now - CreatedAt;
}