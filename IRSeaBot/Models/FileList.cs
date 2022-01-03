using System.Collections.Generic;

namespace IRSeaBot.Models
{
    public class FileList<T> where T : IFileItem
    {
        public List<T> Items { get; set; } = new List<T>();
    }
}
