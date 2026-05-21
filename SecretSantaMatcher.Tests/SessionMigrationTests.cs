using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Xunit;
using SecretSantaMatcher.Models;
using SecretSantaMatcher.Services;

namespace SecretSantaMatcher.Tests
{
    public class SessionMigrationTests
    {
        private void InvokeMigrateLegacySignificantOthers(List<Participant> participants)
        {
            // Instantiate MainWindow without running its constructor (bypassing WPF/InitializeComponent)
            var mainWindow = (MainWindow)RuntimeHelpers.GetUninitializedObject(typeof(MainWindow));

            // Retrieve the private method via reflection
            var method = typeof(MainWindow).GetMethod(
                "MigrateLegacySignificantOthers", 
                BindingFlags.NonPublic | BindingFlags.Instance
            );

            if (method == null)
            {
                throw new InvalidOperationException("Could not find private method 'MigrateLegacySignificantOthers' in MainWindow.");
            }

            // Invoke the private method
            method.Invoke(mainWindow, new object[] { participants });
        }

        [Fact]
        public void MigrateLegacySignificantOthers_ShouldEstablishMutualExclusions()
        {
            // Arrange
            // 2 participants who are legacy significant others of each other
            var p1 = new Participant { Id = "p1", Name = "Alice", SignificantOtherId = "p2" };
            var p2 = new Participant { Id = "p2", Name = "Bob", SignificantOtherId = "p1" };
            var participants = new List<Participant> { p1, p2 };

            // Act
            InvokeMigrateLegacySignificantOthers(participants);

            // Assert
            Assert.Contains("p2", p1.ExcludedParticipantIds);
            Assert.Contains("p1", p2.ExcludedParticipantIds);
        }

        [Fact]
        public void MigrateLegacySignificantOthers_ShouldCreateMutualExclusion_EvenIfOneSidedInLegacyData()
        {
            // Arrange
            // Legacy data might have a one-sided relationship: p1 lists p2 as SO, but p2 does not list p1
            var p1 = new Participant { Id = "p1", Name = "Alice", SignificantOtherId = "p2" };
            var p2 = new Participant { Id = "p2", Name = "Bob", SignificantOtherId = "" };
            var participants = new List<Participant> { p1, p2 };

            // Act
            InvokeMigrateLegacySignificantOthers(participants);

            // Assert
            // Both must exclude each other
            Assert.Contains("p2", p1.ExcludedParticipantIds);
            Assert.Contains("p1", p2.ExcludedParticipantIds);
        }

        [Fact]
        public void MigrateLegacySignificantOthers_ShouldNotOverwriteExistingExclusions()
        {
            // Arrange
            // If the participant already has exclusions (i.e. ExcludedParticipantIds.Count > 0),
            // it means it's not a legacy clean session, so the migration logic should skip it.
            var p1 = new Participant 
            { 
                Id = "p1", 
                Name = "Alice", 
                SignificantOtherId = "p2",
                ExcludedParticipantIds = new List<string> { "p3" } // already has another exclusion
            };
            var p2 = new Participant { Id = "p2", Name = "Bob", SignificantOtherId = "" };
            var p3 = new Participant { Id = "p3", Name = "Charlie", SignificantOtherId = "" };
            var participants = new List<Participant> { p1, p2, p3 };

            // Act
            InvokeMigrateLegacySignificantOthers(participants);

            // Assert
            // p1 should retain "p3" and NOT gain "p2" because ExcludedParticipantIds.Count was not 0
            Assert.Contains("p3", p1.ExcludedParticipantIds);
            Assert.DoesNotContain("p2", p1.ExcludedParticipantIds);
            // p2 should not gain "p1" either since the source p1 was skipped
            Assert.DoesNotContain("p1", p2.ExcludedParticipantIds);
        }

        [Fact]
        public void LegacyJsonSession_DeserializesAndMigratesCorrectly()
        {
            // Arrange
            // Legacy JSON structure mimicking older session saves with SignificantOtherId
            string legacyJson = @"
            {
                ""OrganizerName"": ""Santa Organizers"",
                ""SenderEmail"": ""organizer@example.com"",
                ""EmailSubject"": ""Test Exchange"",
                ""EmailBody"": ""Hello"",
                ""PreventMirrors"": true,
                ""Participants"": [
                    {
                        ""Id"": ""alice-id"",
                        ""Name"": ""Alice"",
                        ""Email"": ""alice@example.com"",
                        ""WishlistUrl"": ""http://wishlist/alice"",
                        ""SignificantOtherId"": ""bob-id"",
                        ""ExcludedParticipantIds"": []
                    },
                    {
                        ""Id"": ""bob-id"",
                        ""Name"": ""Bob"",
                        ""Email"": ""bob@example.com"",
                        ""WishlistUrl"": ""http://wishlist/bob"",
                        ""SignificantOtherId"": ""alice-id"",
                        ""ExcludedParticipantIds"": []
                    },
                    {
                        ""Id"": ""charlie-id"",
                        ""Name"": ""Charlie"",
                        ""Email"": ""charlie@example.com"",
                        ""WishlistUrl"": """",
                        ""SignificantOtherId"": """",
                        ""ExcludedParticipantIds"": []
                    },
                    {
                        ""Id"": ""david-id"",
                        ""Name"": ""David"",
                        ""Email"": ""david@example.com"",
                        ""WishlistUrl"": """",
                        ""SignificantOtherId"": """",
                        ""ExcludedParticipantIds"": []
                    }
                ]
            }";

            // Act
            var session = JsonSerializer.Deserialize<SessionData>(legacyJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.NotNull(session);
            Assert.Equal(4, session.Participants.Count);

            // Run migration
            InvokeMigrateLegacySignificantOthers(session.Participants);

            // Assert
            var alice = session.Participants.First(x => x.Id == "alice-id");
            var bob = session.Participants.First(x => x.Id == "bob-id");
            var charlie = session.Participants.First(x => x.Id == "charlie-id");

            // Exclusions must be set mutually between Alice and Bob
            Assert.Contains("bob-id", alice.ExcludedParticipantIds);
            Assert.Contains("alice-id", bob.ExcludedParticipantIds);

            // Charlie should have no exclusions
            Assert.Empty(charlie.ExcludedParticipantIds);

            // Run matching to verify solver respects this migrated legacy constraint
            var matchingResult = MatchingSolver.GenerateMatches(session.Participants, preventMirrors: true);
            Assert.True(matchingResult.Success, $"Matching failed: {matchingResult.ErrorMessage}");

            // Verify Alice and Bob did not match each other
            var aliceReceiver = matchingResult.Matches["alice-id"];
            var bobReceiver = matchingResult.Matches["bob-id"];

            Assert.NotEqual("bob-id", aliceReceiver);
            Assert.NotEqual("alice-id", bobReceiver);
        }
    }
}
