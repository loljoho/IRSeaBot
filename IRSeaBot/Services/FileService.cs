//using IRSeaBot.Factories;
//using IRSeaBot.Models;
//using Microsoft.Extensions.Configuration;
//using Newtonsoft.Json;
//using System;
//using System.Collections.Concurrent;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Threading;
//using System.Threading.Tasks;

//namespace IRSeaBot.Services
//{
//    public abstract class FileService : IFileService
//    {
//        protected static SemaphoreSlim semaphore = new(1);
//        protected const string folderName = "IRSeaBot";
//        protected string filePath;
//        protected char delimter;
//        protected IConfiguration configuration;
//        public async Task<FileItem> GetFile(string key, FileTypes type)
//        {
//            string pathString = GetPath();
//            ConcurrentDictionary<string, FileItem> fileDict = new ConcurrentDictionary<string, FileItem>();
//            string resultString = String.Empty;
//            await semaphore.WaitAsync();
//            using (StreamReader reader = new(pathString))
//            {
//                try
//                {
//                    resultString = await reader.ReadToEndAsync();
//                }
//                catch (Exception e)
//                {
//                    Console.WriteLine(e.Message);
//                }
//                finally
//                {
//                    semaphore.Release();
//                }
//            }
//            if (!String.IsNullOrWhiteSpace(resultString))
//            {
//                FileList list;
//                switch (type)
//                {
//                    case FileTypes.Seen:
//                        list = ParseFile(resultString, type) as SeenList;
//                        break;
//                    case FileTypes.Likes:
//                        list = ParseFile(resultString, type) as LikeList;
//                        break;
//                    default:
//                        list = null;
//                        break;
//                }

//                string[] parsed = resultString.Split(delimter);
//                Parallel.ForEach(parsed, p =>
//                {
//                    if (!String.IsNullOrWhiteSpace(p))
//                    {
//                        FileItem file = ParseFile(p);
//                        if (file != null) fileDict.TryAdd(file.Key, file);
//                    }
//                });
//            }
//            bool found = fileDict.TryGetValue(key, out FileItem file);
//            if (found) return file;
//            else return null;
//        }

//        public virtual FileList ParseFile(string input, FileTypes type)
//        {
//            try
//            {
//                switch (type)
//                {
//                    case FileTypes.Seen:
//                        SeenList seenList = JsonConvert.DeserializeObject<SeenList>(input);
//                        return seenList;
//                    case FileTypes.Likes:
//                        LikeList likeList = JsonConvert.DeserializeObject<LikeList>(input);
//                        return likeList;
//                    default:
//                        return null;
//                }
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine(ex);
//                return null;
//            }


//            //input = input.Trim();
//            //string[] split = input.Split(delimter);
//            //foreach (string s in split)
//            //{
//            //    string[] parsed = s.Split("|");
//            //    IFileItem file = _fileFactory.CreateIFile(split);
//            //}

//            //return file;
//        }

//        public string GetPath()
//        {
//            var systemPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
//            var complete = Path.Combine(systemPath, folderName);
//            Directory.CreateDirectory(complete);
//            var pathString = Path.Combine(complete, filePath);
//            semaphore.Wait();
//            try
//            {
//                if (!File.Exists(pathString))
//                {
//                    using FileStream fs = File.Create(pathString);
//                    fs.Write(null);
//                }
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine(ex.Message);
//            }
//            finally
//            {
//                semaphore.Release();
//            }

//            return pathString;
//        }

//        public async Task WriteFile(FileItem newItem)
//        {
//            string pathString = GetPath();
//            IFileList list = new();
//            ConcurrentDictionary<string, FileItem> fileItems = new();
//            string readString = String.Empty;
//            await semaphore.WaitAsync();
//            using (StreamReader reader = new(pathString))
//            {
//                try
//                {
//                    readString = await reader.ReadToEndAsync();
//                }
//                catch (Exception e)
//                {
//                    Console.WriteLine(e.Message);
//                }
//                finally
//                {
//                    semaphore.Release();
//                }
//            }

//            if (!string.IsNullOrWhiteSpace(readString))
//            {
//                string[] parsed = readString.Split(delimter);
//                Parallel.ForEach(parsed, item =>
//                {
//                    if (!string.IsNullOrWhiteSpace(item))
//                    {
//                        FileItem fileItem = ParseFile(item);
//                        fileItems.TryAdd(fileItem.Key, fileItem);
//                    }
//                });
//            }
//            if (!fileItems.ContainsKey(newItem.Key))
//            {
//                fileItems.TryAdd(newItem.Key, newItem);
//            }
//            else
//            {
//                ProcessFile(newItem, fileItems);
//            }

//            var tempFile = Path.GetTempFileName();
//            await semaphore.WaitAsync();
//            try
//            {
//                using StreamWriter output = new StreamWriter(tempFile);
//                foreach (KeyValuePair<string, FileItem> kv in fileItems)
//                {
//                    await output.WriteLineAsync(kv.Value.Seralize());
//                }
//                File.Delete(pathString);
//                File.Move(tempFile, pathString);
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine(ex.Message);
//            }
//            finally
//            {
//                semaphore.Release();
//            }
//        }

//        public virtual ConcurrentDictionary<string, FileItem> ProcessFile(FileItem fileItem, ConcurrentDictionary<string, FileItem> fileDict)
//        {
//            fileDict.TryAdd(fileItem.Key, fileItem);
//            return fileDict;
//        }
//    }
//}
