using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ADPD_code.Data;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Diagnostics;

namespace ADPD_code.Patterns
{
    /// <summary>
    /// Singleton Pattern - Quản lý kết nối Database (Simplified Version)
    /// Không track disposal để tránh vấn đề với DbContext override
    /// </summary>
    public sealed class DatabaseConnectionManager
    {
        #region Singleton Implementation

        private static DatabaseConnectionManager _instance = null;
        private static readonly object _lock = new object();

        private readonly ConcurrentDictionary<Guid, ActiveConnection> _activeConnections;
        #endregion

        #region Private Fields

        private readonly DbContextOptions<ApplicationDbContext> _options;
        private int _connectionCount = 0;
        private DateTime _lastConnectionTime;
        private DateTime _createdTime;
        private readonly ILogger<DatabaseConnectionManager> _logger;

        #endregion

        #region Constructor

        private DatabaseConnectionManager(
            DbContextOptions<ApplicationDbContext> options,
            ILogger<DatabaseConnectionManager> logger = null)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _logger = logger;
            _createdTime = DateTime.Now;
            _lastConnectionTime = _createdTime;
            _activeConnections = new ConcurrentDictionary<Guid, ActiveConnection>();

            LogInfo($"DatabaseConnectionManager initialized at {_createdTime}");
        }
        #endregion

        #region Singleton Access

        public static DatabaseConnectionManager GetInstance(
            DbContextOptions<ApplicationDbContext> options,
            ILogger<DatabaseConnectionManager> logger = null)
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new DatabaseConnectionManager(options, logger);
                    }
                }
            }
            return _instance;
        }

        public static DatabaseConnectionManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    throw new InvalidOperationException(
                        "DatabaseConnectionManager has not been initialized. Call GetInstance() first.");
                }
                return _instance;
            }
        }

        #endregion

        #region DbContext Creation

        public ApplicationDbContext CreateDbContext()
        {
            var connection = new ActiveConnection();
            _activeConnections.TryAdd(connection.Id, connection);

            Interlocked.Increment(ref _connectionCount);
            _lastConnectionTime = DateTime.Now;

            LogInfo($"Creating DbContext #{_connectionCount} [ID: {connection.Id}]");

            return new ApplicationDbContext(_options, (id) => _activeConnections.TryRemove(id, out _));
        }

        #endregion

        #region Connection Testing

        public bool TestConnection()
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                using var context = CreateDbContext();
                var canConnect = context.Database.CanConnect();

                stopwatch.Stop();
                LogInfo($"Connection test: {(canConnect ? "SUCCESS" : "FAILED")} ({stopwatch.ElapsedMilliseconds}ms)");
                return canConnect;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                LogError($"Connection test EXCEPTION ({stopwatch.ElapsedMilliseconds}ms): {ex.Message}");
                return false;
            }
        }

        public async Task<bool> TestConnectionAsync()
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                using var context = CreateDbContext();
                var canConnect = await context.Database.CanConnectAsync();

                stopwatch.Stop();
                LogInfo($"Async connection test: {(canConnect ? "SUCCESS" : "FAILED")} ({stopwatch.ElapsedMilliseconds}ms)");
                return canConnect;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                LogError($"Async connection test EXCEPTION ({stopwatch.ElapsedMilliseconds}ms): {ex.Message}");
                return false;
            }
        }

        public string GetConnectionString()
        {
            try
            {
                using var context = CreateDbContext();
                return context.Database.GetConnectionString();
            }
            catch (Exception ex)
            {
                LogError($"Error getting connection string: {ex.Message}");
                return null;
            }
        }

        #endregion

        #region Statistics

        public ConnectionStats GetStats()
        {
            return new ConnectionStats
            {
                TotalConnections = _connectionCount,
                ActiveConnections = _activeConnections.Count,
                ConnectionPoolSize = 20, // Hardcoded for now
                LastConnectionTime = _lastConnectionTime,
                ManagerCreatedTime = _createdTime,
                Uptime = DateTime.Now - _createdTime,
                IsActive = _instance != null
            };
        }

        public ICollection<ActiveConnection> GetActiveConnections()
        {
            return _activeConnections.Values;
        }

        #endregion

        #region Logging

        private void LogInfo(string message)
        {
            _logger?.LogInformation($"[Singleton] {message}");
            Debug.WriteLine($"[Singleton] {message}");
        }

        private void LogError(string message)
        {
            _logger?.LogError($"[Singleton] {message}");
            Debug.WriteLine($"[Singleton ERROR] {message}");
        }

        #endregion

        #region Testing Support

        public static void ResetInstance()
        {
            lock (_lock)
            {
                _instance = null;
                Debug.WriteLine("[Singleton] Instance reset for testing");
            }
        }

        #endregion
    }

    #region Supporting Classes

    public class ConnectionStats
    {
        public int TotalConnections { get; set; }
        public int ActiveConnections { get; set; }
        public int ConnectionPoolSize { get; set; }
        public DateTime LastConnectionTime { get; set; }
        public DateTime ManagerCreatedTime { get; set; }
        public TimeSpan Uptime { get; set; }
        public bool IsActive { get; set; }

        public override string ToString()
        {
            return $"Total: {TotalConnections} | " +
                   $"Active: {ActiveConnections} / {ConnectionPoolSize} | " +
                   $"Last: {LastConnectionTime:HH:mm:ss} | " +
                   $"Uptime: {Uptime.TotalMinutes:F1}m | " +
                   $"Status: {(IsActive ? "Active" : "Inactive")}";
        }
    }

    public class ActiveConnection
    {
        public Guid Id { get; }
        public DateTime CreatedAt { get; }
        public TimeSpan Age => DateTime.Now - CreatedAt;

        public ActiveConnection()
        {
            Id = Guid.NewGuid();
            CreatedAt = DateTime.Now;
        }
    }


    #endregion
}