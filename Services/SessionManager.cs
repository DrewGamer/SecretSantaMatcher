using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using SecretSantaMatcher.Models;

namespace SecretSantaMatcher.Services
{
    public class SessionData
    {
        public List<Participant> Participants { get; set; } = new();
        public string OrganizerName { get; set; } = string.Empty;
        public string SenderEmail { get; set; } = string.Empty;
        public string EmailSubject { get; set; } = string.Empty;
        public string EmailBody { get; set; } = string.Empty;
        public string SavedPassword { get; set; } = string.Empty; // Obfuscated
        public bool RememberPassword { get; set; } = false;
    }

    public static class SessionManager
    {
        private static readonly string AppDataFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), 
            "SecretSantaMatcher"
        );
        private static readonly string SessionFilePath = Path.Combine(AppDataFolder, "secretsanta_session.json");

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true
        };

        // Standard XOR key for simple obfuscation to avoid storing Gmail app passwords in plaintext
        private static readonly byte[] ObfuscationKey = new byte[] { 0x53, 0x65, 0x63, 0x72, 0x65, 0x74, 0x53, 0x61, 0x6e, 0x74, 0x61, 0x32, 0x30, 0x32, 0x36 };

        public static void SaveSession(SessionData data)
        {
            try
            {
                if (!Directory.Exists(AppDataFolder))
                {
                    Directory.CreateDirectory(AppDataFolder);
                }

                // Prepare data for saving
                var saveData = new SessionData
                {
                    Participants = data.Participants,
                    OrganizerName = data.OrganizerName,
                    SenderEmail = data.SenderEmail,
                    EmailSubject = data.EmailSubject,
                    EmailBody = data.EmailBody,
                    RememberPassword = data.RememberPassword
                };

                if (data.RememberPassword && !string.IsNullOrEmpty(data.SavedPassword))
                {
                    saveData.SavedPassword = Obfuscate(data.SavedPassword);
                }
                else
                {
                    saveData.SavedPassword = string.Empty;
                }

                string json = JsonSerializer.Serialize(saveData, JsonOptions);
                File.WriteAllText(SessionFilePath, json, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to auto-save session: {ex.Message}");
            }
        }

        public static SessionData LoadSession()
        {
            try
            {
                if (!File.Exists(SessionFilePath))
                {
                    return GetDefaultSession();
                }

                string json = File.ReadAllText(SessionFilePath, Encoding.UTF8);
                var data = JsonSerializer.Deserialize<SessionData>(json, JsonOptions);

                if (data != null)
                {
                    if (data.RememberPassword && !string.IsNullOrEmpty(data.SavedPassword))
                    {
                        data.SavedPassword = Deobfuscate(data.SavedPassword);
                    }
                    else
                    {
                        data.SavedPassword = string.Empty;
                    }
                    return data;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load session: {ex.Message}");
            }

            return GetDefaultSession();
        }

        public static void ExportSession(string filePath, SessionData data)
        {
            var saveData = new SessionData
            {
                Participants = data.Participants,
                OrganizerName = data.OrganizerName,
                SenderEmail = data.SenderEmail,
                EmailSubject = data.EmailSubject,
                EmailBody = data.EmailBody,
                RememberPassword = false, // Do not export password details for security
                SavedPassword = string.Empty
            };

            string json = JsonSerializer.Serialize(saveData, JsonOptions);
            File.WriteAllText(filePath, json, Encoding.UTF8);
        }

        public static SessionData ImportSession(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("Selected session file could not be found.");
            }

            string json = File.ReadAllText(filePath, Encoding.UTF8);
            var data = JsonSerializer.Deserialize<SessionData>(json, JsonOptions);
            if (data == null)
            {
                throw new InvalidDataException("Invalid file format. Could not load session data.");
            }

            // Ensure password fields are empty on import for safety
            data.SavedPassword = string.Empty;
            data.RememberPassword = false;

            return data;
        }

        public static SessionData GetDefaultSession()
        {
            return new SessionData
            {
                Participants = new List<Participant>(),
                EmailSubject = "Secret Santa Gift Exchange!",
                EmailBody = "Hi {Giver},\n\nWelcome to our Secret Santa! You have been matched to buy a gift for:\n\n🎅 {Receiver}\n\nHere is their wishlist link: {Wishlist}\n\nHave fun and keep it a secret!\n\nBest,\n{Organizer}",
                OrganizerName = "",
                SenderEmail = "",
                SavedPassword = "",
                RememberPassword = false
            };
        }

        // Simple symmetric XOR-based obfuscation
        private static string Obfuscate(string plainText)
        {
            if (string.IsNullOrEmpty(plainText)) return string.Empty;

            byte[] input = Encoding.UTF8.GetBytes(plainText);
            byte[] output = new byte[input.Length];

            for (int i = 0; i < input.Length; i++)
            {
                output[i] = (byte)(input[i] ^ ObfuscationKey[i % ObfuscationKey.Length]);
            }

            return Convert.ToBase64String(output);
        }

        private static string Deobfuscate(string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText)) return string.Empty;

            try
            {
                byte[] input = Convert.FromBase64String(cipherText);
                byte[] output = new byte[input.Length];

                for (int i = 0; i < input.Length; i++)
                {
                    output[i] = (byte)(input[i] ^ ObfuscationKey[i % ObfuscationKey.Length]);
                }

                return Encoding.UTF8.GetString(output);
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}
