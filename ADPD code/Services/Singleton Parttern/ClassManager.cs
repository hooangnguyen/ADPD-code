using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;

namespace ADPD_code.Patterns
{
    /// <summary>
    /// Singleton Pattern - ClassManager
    /// Quản lý tập trung tất cả các thao tác với Class
    /// Đảm bảo chỉ có 1 instance duy nhất trong toàn ứng dụng
    /// </summary>
    public sealed class ClassManager
    {
        #region Singleton Implementation (Thread-Safe with Double-Check Locking)

        private static ClassManager _instance = null;
        private static readonly object _lock = new object();

        // Private constructor - Ngăn việc tạo instance từ bên ngoài
        private ClassManager(ILogger<ClassManager> logger = null)
        {
            _logger = logger;
            _createdTime = DateTime.Now;
            _totalOperations = 0;

            LogInfo($"🔷 ClassManager instance created at {_createdTime}");
        }

        // Thuộc tính để truy cập instance (Lazy initialization)
        public static ClassManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new ClassManager();
                        }
                    }
                }
                return _instance;
            }
        }

        // Method khởi tạo với logger (gọi từ Program.cs)
        public static ClassManager GetInstance(ILogger<ClassManager> logger = null)
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new ClassManager(logger);
                    }
                }
            }
            return _instance;
        }

        #endregion

        #region Private Fields

        private readonly ILogger<ClassManager> _logger;
        private DateTime _createdTime;
        private int _totalOperations;

        #endregion

        #region Public Properties

        public int TotalOperations => _totalOperations;
        public DateTime CreatedTime => _createdTime;
        public TimeSpan Uptime => DateTime.Now - _createdTime;
        public bool IsActive => _instance != null;
        public string InstanceHashCode => GetHashCode().ToString();

        #endregion

        #region CRUD Operations (Tracked)

        /// <summary>
        /// Log operation CREATE
        /// </summary>
        public void LogCreate(string className)
        {
            IncrementOperation();
            LogInfo($"➕ CREATE: Class '{className}' created");
        }

        /// <summary>
        /// Log operation READ
        /// </summary>
        public void LogRead(int? classId = null)
        {
            IncrementOperation();
            if (classId.HasValue)
            {
                LogInfo($"📖 READ: Class ID {classId} retrieved");
            }
            else
            {
                LogInfo($"📖 READ: All classes retrieved");
            }
        }

        /// <summary>
        /// Log operation UPDATE
        /// </summary>
        public void LogUpdate(int classId, string className)
        {
            IncrementOperation();
            LogInfo($"✏️ UPDATE: Class ID {classId} ('{className}') updated");
        }

        /// <summary>
        /// Log operation DELETE
        /// </summary>
        public void LogDelete(int classId, string className)
        {
            IncrementOperation();
            LogInfo($"🗑️ DELETE: Class ID {classId} ('{className}') deleted");
        }

        #endregion

        #region Statistics

        /// <summary>
        /// Lấy thống kê của ClassManager
        /// </summary>
        public ClassManagerStats GetStats()
        {
            return new ClassManagerStats
            {
                TotalOperations = _totalOperations,
                CreatedTime = _createdTime,
                Uptime = Uptime,
                IsActive = IsActive,
                InstanceHashCode = InstanceHashCode
            };
        }

        #endregion

        #region Helper Methods

        private void IncrementOperation()
        {
            lock (_lock)
            {
                _totalOperations++;
            }
        }

        private void LogInfo(string message)
        {
            _logger?.LogInformation($"[ClassManager] {message}");
            Debug.WriteLine($"[ClassManager] {message}");
        }

        private void LogError(string message)
        {
            _logger?.LogError($"[ClassManager] {message}");
            Debug.WriteLine($"[ClassManager ERROR] {message}");
        }

        #endregion

        #region Testing Support

        /// <summary>
        /// Reset instance (chỉ dùng cho testing)
        /// </summary>
        public static void ResetInstance()
        {
            lock (_lock)
            {
                _instance = null;
                Debug.WriteLine("[ClassManager] 🔄 Instance reset for testing");
            }
        }

        #endregion
    }

    #region Supporting Classes

    /// <summary>
    /// Class chứa thống kê của ClassManager
    /// </summary>
    public class ClassManagerStats
    {
        public int TotalOperations { get; set; }
        public DateTime CreatedTime { get; set; }
        public TimeSpan Uptime { get; set; }
        public bool IsActive { get; set; }
        public string InstanceHashCode { get; set; }

        public override string ToString()
        {
            return $"Operations: {TotalOperations} | " +
                   $"Uptime: {Uptime.TotalMinutes:F1}m | " +
                   $"Hash: {InstanceHashCode} | " +
                   $"Status: {(IsActive ? "Active" : "Inactive")}";
        }
    }

    #endregion
}