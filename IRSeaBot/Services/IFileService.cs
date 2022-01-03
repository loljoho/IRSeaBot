using IRSeaBot.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IRSeaBot.Services
{
    public interface IFileService<T> : IBotService where T : IFileItem
    {
        Task<FileList<T>> GetFileList();
        string GetPath();
        Task<T> GetFileItem(string key);

        Task<T> WriteFile(T newItem);
    }
}
