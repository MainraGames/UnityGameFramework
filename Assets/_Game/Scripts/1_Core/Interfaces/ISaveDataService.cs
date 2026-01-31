namespace _Game.Scripts.Core.Interfaces
{
    /// <summary>
    /// Interface for data persistence operations.
    /// </summary>
    /// <remarks>
    /// Implementations should handle serialization and storage of game data.
    /// Common implementations include PlayerPrefs, file-based, or cloud storage.
    /// </remarks>
    public interface ISaveDataService
    {
        /// <summary>
        /// Saves data with the specified key.
        /// </summary>
        /// <typeparam name="T">Type of data to save.</typeparam>
        /// <param name="key">Unique identifier for the data.</param>
        /// <param name="data">Data to save.</param>
        void Save<T>(string key, T data);
        
        /// <summary>
        /// Loads data with the specified key.
        /// </summary>
        /// <typeparam name="T">Type of data to load.</typeparam>
        /// <param name="key">Unique identifier for the data.</param>
        /// <returns>Loaded data, or default if not found.</returns>
        T Load<T>(string key);
        
        /// <summary>
        /// Deletes data with the specified key.
        /// </summary>
        /// <param name="key">Unique identifier for the data to delete.</param>
        /// <returns>True if data was deleted, false if not found.</returns>
        bool Delete(string key);
        
        /// <summary>
        /// Checks if data exists for the specified key.
        /// </summary>
        /// <param name="key">Unique identifier to check.</param>
        /// <returns>True if data exists, false otherwise.</returns>
        bool Exists(string key);
        
        /// <summary>
        /// Deletes all saved data.
        /// </summary>
        void DeleteAll();
    }
}