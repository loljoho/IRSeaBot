using IRSeaBot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IRSeaBot.Factories
{
    public class FileItemFactory : IFileItemFactory
    {
        public static string GetFilePath(Type type)
        {
            string filePath;
            if (type == typeof(SeenUser))
            {
                filePath = SeenUser.FileFolder;
            }
            else
            {
                filePath = Like.FileFolder;
            }
            return filePath;
        }

        public static IFileItem CreateFile(string[] input, FileTypes type)
        {
            return type switch
            {
                FileTypes.Seen => CreateSeen(input),
                FileTypes.Likes => CreateLike(input),
                _ => null,
            };
        }

        private static SeenUser CreateSeen(string[] input)
        {
            try
            {
                SeenUser user = new()
                {
                    Key = input[0],
                    Message = input[1],
                    Timestamp = DateTime.Parse(input[2])
                };
                return user;
            }
            catch
            {
                return null;
            }
        }

        private static Like CreateLike(string[] input)
        {
            try
            {
                Like like = new()
                {
                    Key = input[0],
                    Score = int.Parse(input[1]),
                };
                return like;
            }
            catch
            {
                return null;
            }
        }
    }
}
