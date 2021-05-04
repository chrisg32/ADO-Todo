using System;
using Avalonia.Media;

namespace ADOTodo.Models
{
    public interface ITodoItem
    {
        public string Id { get; }
        public string Title { get; }
        public string? Description { get; set; }
        public DateTime? Date { get; }
        public string Url { get; }
        
        public string ItemType { get; }
        public Color Color { get; }
        public int ItemTypePriority { get; }
    }
}