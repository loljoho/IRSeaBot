using IRSeaBot.Factories;
using IRSeaBot.Models;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace IRSeaBot.Services
{
    public class GenericFileService<T> : IFileService<T> where T : class, IFileItem
    {
        private static SemaphoreSlim semaphore = new(1);
        private const string folderName = "IRSeaBot";

        public async Task<FileList<T>> GetFileList()
        {
            string pathString = GetPath();
            string resultString = string.Empty;
            FileList<T> fileList = new();
            await semaphore.WaitAsync();
            using (StreamReader reader = new(pathString))
            {
                try
                {
                    resultString = await reader.ReadToEndAsync();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
                finally
                {
                    semaphore.Release();
                }
            }
            if (!string.IsNullOrWhiteSpace(resultString))
            {             
                try
                {
                    fileList = JsonConvert.DeserializeObject(resultString, typeof(FileList<T>)) as FileList<T>;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
            return fileList;
        }

        public async Task<T> GetFileItem(string key)
        {
            FileList<T> list = await GetFileList();
            if (list.Items.Count == 0) { return null; }
            else
            {
                T item = list.Items.FirstOrDefault(x => x.Key == key.Trim());
                if (item == null) return null;
                else return item;
            }
        }

        public string GetPath()
        {
            var systemPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var complete = Path.Combine(systemPath, folderName);
            Directory.CreateDirectory(complete);
            string filePath = FileItemFactory.GetFilePath(typeof(T));
            var pathString = Path.Combine(complete, filePath);
            semaphore.Wait();
            try
            {
                if (!File.Exists(pathString))
                {
                    using FileStream fs = File.Create(pathString);
                    fs.Write(null);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                semaphore.Release();
            }

            return pathString;
        }

        public async Task<T> WriteFile(T newItem)
        {
            string pathString = GetPath();
            FileList<T> list = await GetFileList();
            T item = list.Items.FirstOrDefault(x => x.Key == newItem.Key);
            if (item == null)
            {
                list.Items.Add(newItem);
            }
            else
            {
                list.Items.Remove(item); //remove old item
                item = ProcessFileItem(item, newItem);
                list.Items.Add(item); //add new item
            }

            var tempFile = Path.GetTempFileName();
            string json = JsonConvert.SerializeObject(list);
            await semaphore.WaitAsync();
            try
            {
                using (StreamWriter output = new StreamWriter(tempFile))
                {
                    await output.WriteLineAsync(json);
                }        
                File.Delete(pathString);
                File.Move(tempFile, pathString);
                return item;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
            finally
            {
                semaphore.Release();
            }
        }
        private static T ProcessFileItem(T old, T newFile)
        {
            if(typeof(T) == typeof(SeenUser))
            {
                return newFile;
            }else
            {
                Like oldLike = old as Like;
                Like newLike = newFile as Like;
                if (newLike.Score == 1)
                {
                    oldLike.Score++;
                }
                else oldLike.Score--;
                return oldLike as T;
            }
        }
    }
}
