using System;
using System.IO;
using System.Text.Json;
using Avalonia;

namespace ADOTodo.Services
{
    public static class PersistenceModule
    {
        public static TValue Load<TValue>(string path) where TValue : new()
        {
            path = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), path);
            if (!File.Exists(path)) return new TValue();
            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<TValue>(json) ?? new TValue();
        }
        
        public static void Save<TValue>(string path, TValue value)
        {
            path = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), path);
            var json = JsonSerializer.Serialize(value);
            File.WriteAllText(path, json);
        }
    }
}