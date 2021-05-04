using System.IO;
using System.Text.Json;

namespace ADOTodo.Services
{
    public static class PersistenceModule
    {
        public static void SafeRename<TValue>(string oldPath, string newPath) where TValue : class
        {
            oldPath = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), oldPath);
            newPath = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), newPath);
            if (!File.Exists(oldPath) || File.Exists(newPath)) return;
            var json = File.ReadAllText(oldPath);
            var o = JsonSerializer.Deserialize<TValue>(json);
            if (o == default(TValue?)) return;
            File.Move(oldPath, newPath);
        }
        
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