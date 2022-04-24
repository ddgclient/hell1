
namespace Wrapper
{
    using System;
    using System.IO;

    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    internal class DirectoryHandler : IDirectoryHandler
    {
        internal DirectoryHandler()
        {
        }

        /// <inheritdoc/>
        public string ReadTextFromPath(string filePath)
        {
            if(File.Exists(filePath))
            {
                return File.ReadAllText(filePath);
            }
            else
            {
                throw new FileNotFoundException($"Could not find file at given path [{filePath}]");
            }
        }

        /// <inheritdoc/>
        public string[] RetrieveListOfDirectories(string parentDirectoryPath, string searchPattern)
        {
            return Directory.GetDirectories(
                parentDirectoryPath ?? throw new InvalidOperationException(), searchPattern, SearchOption.TopDirectoryOnly);
        }

        /// <inheritdoc/>
        public string[] RetrieveListOfDirectories(string parentDirectoryPath)
        {
            var localDirectory = Path.GetDirectoryName(parentDirectoryPath);
            return Directory.GetDirectories(
                localDirectory ?? throw new InvalidOperationException());
        }
    }
}
