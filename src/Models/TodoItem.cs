using System;
using Avalonia.Media;

namespace ADOTodo.Models
{
    public abstract class TodoItem : ITodoItem
    {
        public string Id { get; protected init; } = null!;
        public string Title { get; protected init; } = null!;
        public string? Description { get; set; }
        public DateTime? Date { get; protected init; }
        public string Url { get; protected init; } = null!;
        public abstract string ItemType { get; }
        public abstract Color Color { get; }
        public abstract int ItemTypePriority { get; }
    }
}