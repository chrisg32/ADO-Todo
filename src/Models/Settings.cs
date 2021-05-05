using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using CG.Commons.Util;

namespace ADOTodo.Models
{
    public class Settings
    {
        [JsonIgnore]
        [IgnoreDataMember]
        private const string Passphrase = "1F268C5E-FF0D-4C11-AE13-F0845348D2A7";
        public string? EncryptedToken { get; set; }
        public string? Project { get; set; }
        public string? Server { get; set; }

        [JsonIgnore]
        [IgnoreDataMember]
        public string? Token
        {
            get => EncryptedToken == null ? null : StringCipher.Decrypt(EncryptedToken, Passphrase);
            set => EncryptedToken = value == null ? null : StringCipher.Encrypt(value, Passphrase);
        }

        public PrSettings Mine { get; set; } = new PrSettings(ThreadFilterLevel.All);
    }
}