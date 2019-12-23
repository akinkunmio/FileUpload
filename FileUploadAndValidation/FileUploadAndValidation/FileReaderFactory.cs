using System;
using System.Collections.Generic;
using FilleUploadCore.FileReaders;

namespace FileUploadAndValidation
{
    internal class FileHandlerFactory
    {
        private readonly Dictionary<string, IFileReader> _fileHandlers;

        public FileHandlerFactory()
        {
            _fileHandlers = new Dictionary<string, IFileReader>();
        }

        internal IFileReader FindOrDefault(string extension)
        {
            if (_fileHandlers.TryGetValue(extension, out IFileReader fileReader))
                return fileReader;

            return null;
        }

        internal void Register(string extension, IFileReader fileReader)
        {
            _fileHandlers.Add(extension, fileReader);
        }
    }
}