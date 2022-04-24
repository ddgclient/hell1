using System;
using System.Collections.Generic;
using System.Text;

namespace Wrapper
{
    /// <summary>
    /// Defines objects that implement methods for handling
    /// </summary>
    public interface IDirectoryHandler
    {
        /// <summary>
        /// Get all directory names that reside within a given directory.
        /// </summary>
        /// <param name="parentDirectoryPath"> Parent directory to query all subdirectories. </param>
        /// <param name="searchPattern"> Pattern to use when searching for particular directories. </param>
        /// <returns>List containing paths to all subdirectories of queried directory</returns>
        public abstract string[] RetrieveListOfDirectories(string parentDirectoryPath, string searchPattern);

        /// <summary>
        /// Get all directory paths that reside within a given directory.
        /// </summary>
        /// <param name="parentDirectoryPath"> Parent directory to query all subdirectories. </param>
        /// <returns>List containing paths to all subdirectories of queried directory</returns>
        public abstract string[] RetrieveListOfDirectories(string parentDirectoryPath);

        /// <summary>
        /// Get all text from file at given file path.
        /// </summary>
        /// <param name="filePath"> Path to file containing text. </param>
        /// <returns> String containing all text from filepath. </returns>
        public abstract string ReadTextFromPath(string filePath);
    }
}
