using System;
using System.Text.Json.Serialization;

namespace SecretSantaMatcher.Models
{
    public class Participant
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string WishlistUrl { get; set; } = string.Empty;
        
        // Stores the Id of another participant who is their significant other (to prevent matching them)
        public string SignificantOtherId { get; set; } = string.Empty;

        [JsonIgnore]
        public string DisplayName => string.IsNullOrWhiteSpace(Name) ? "New Participant" : Name;

        public override string ToString() => Name;
    }
}
