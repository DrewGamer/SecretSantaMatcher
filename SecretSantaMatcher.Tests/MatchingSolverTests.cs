using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using SecretSantaMatcher.Models;
using SecretSantaMatcher.Services;

namespace SecretSantaMatcher.Tests
{
    public class MatchingSolverTests
    {
        private List<Participant> CreateParticipantGroup(int count)
        {
            var list = new List<Participant>();
            for (int i = 1; i <= count; i++)
            {
                list.Add(new Participant
                {
                    Id = $"p{i}",
                    Name = $"Participant {i}",
                    Email = $"p{i}@example.com"
                });
            }
            return list;
        }

        [Fact]
        public void GenerateMatches_ShouldRequireAtLeastTwoParticipants()
        {
            // Arrange
            var participants = CreateParticipantGroup(1);

            // Act
            var result = MatchingSolver.GenerateMatches(participants);

            // Assert
            Assert.False(result.Success);
            Assert.Contains("At least 2 participants are required", result.ErrorMessage);
        }

        [Fact]
        public void GenerateMatches_BasicCorrectness_NoExclusions()
        {
            // Arrange
            int size = 5;
            var participants = CreateParticipantGroup(size);

            // Act
            var result = MatchingSolver.GenerateMatches(participants, preventMirrors: false);

            // Assert
            Assert.True(result.Success, $"Solver failed: {result.ErrorMessage}");
            Assert.Equal(size, result.Matches.Count);

            // Verify solver constraints:
            foreach (var p in participants)
            {
                Assert.True(result.Matches.ContainsKey(p.Id), $"Giver {p.Name} is not matched.");
                var receiverId = result.Matches[p.Id];
                
                // 1. Givers must never match with themselves
                Assert.NotEqual(p.Id, receiverId);

                // Check that the receiver is a valid participant
                Assert.Contains(participants, x => x.Id == receiverId);
            }

            // 2. Closed cycle / Bijection check:
            // Since N is small and everyone is matched exactly once, verify that
            // every receiver has exactly one giver (uniqueness of values in Matches).
            var receivers = result.Matches.Values.ToList();
            Assert.Equal(size, receivers.Distinct().Count());
        }

        [Fact]
        public void GenerateMatches_ShouldRespectExcludedParticipantIds()
        {
            // Arrange
            // 4 participants. Let's make directed exclusions:
            // p1 cannot buy for p2
            // p2 cannot buy for p3
            // p3 cannot buy for p4
            // p4 cannot buy for p1
            var participants = CreateParticipantGroup(4);
            participants[0].ExcludedParticipantIds.Add("p2");
            participants[1].ExcludedParticipantIds.Add("p3");
            participants[2].ExcludedParticipantIds.Add("p4");
            participants[3].ExcludedParticipantIds.Add("p1");

            // Act
            // Run multiple iterations to ensure randomness doesn't bypass exclusions
            for (int iter = 0; iter < 50; iter++)
            {
                var result = MatchingSolver.GenerateMatches(participants, preventMirrors: false);

                // Assert
                Assert.True(result.Success, $"Failed on iteration {iter}: {result.ErrorMessage}");
                
                foreach (var giver in participants)
                {
                    var receiverId = result.Matches[giver.Id];
                    Assert.NotEqual(giver.Id, receiverId);
                    Assert.DoesNotContain(receiverId, giver.ExcludedParticipantIds);
                }
            }
        }

        [Fact]
        public void GenerateMatches_ShouldRespectPreventMirrors()
        {
            // Arrange
            // With 4 participants, if we prevent mirrors:
            // Valid configurations are loops of length 4 (e.g. 1->2->3->4->1).
            // Invalid configurations would include two 2-person reciprocal loops (e.g. 1->2, 2->1, 3->4, 4->3).
            var participants = CreateParticipantGroup(4);

            // Act & Assert
            for (int iter = 0; iter < 100; iter++)
            {
                var result = MatchingSolver.GenerateMatches(participants, preventMirrors: true);
                Assert.True(result.Success, $"Failed with preventMirrors on iteration {iter}: {result.ErrorMessage}");

                foreach (var match in result.Matches)
                {
                    var giverId = match.Key;
                    var receiverId = match.Value;

                    // If giver buys for receiver, receiver must NOT buy for giver
                    if (result.Matches.TryGetValue(receiverId, out var reciprocalId))
                    {
                        Assert.NotEqual(giverId, reciprocalId);
                    }
                }
            }
        }

        [Fact]
        public void GenerateMatches_Deadlock_GiverExcludesAll()
        {
            // Arrange
            // Participant 1 excludes all other participants (p2, p3, p4)
            var participants = CreateParticipantGroup(4);
            participants[0].ExcludedParticipantIds.AddRange(new[] { "p2", "p3", "p4" });

            // Act
            var result = MatchingSolver.GenerateMatches(participants, preventMirrors: false);

            // Assert
            Assert.False(result.Success);
            Assert.Contains("Deadlock detected", result.ErrorMessage);
            Assert.Contains("Participant(s) 'Participant 1' excluded all other possible recipients", result.ErrorMessage);
        }

        [Fact]
        public void GenerateMatches_Deadlock_ReceiverExcludedByAll()
        {
            // Arrange
            // Participant 1 is excluded by all other participants (p2, p3, p4 cannot buy for p1)
            var participants = CreateParticipantGroup(4);
            participants[1].ExcludedParticipantIds.Add("p1");
            participants[2].ExcludedParticipantIds.Add("p1");
            participants[3].ExcludedParticipantIds.Add("p1");

            // Act
            var result = MatchingSolver.GenerateMatches(participants, preventMirrors: false);

            // Assert
            Assert.False(result.Success);
            Assert.Contains("Deadlock detected", result.ErrorMessage);
            Assert.Contains("Participant(s) 'Participant 1' are excluded by all other participants", result.ErrorMessage);
        }

        [Fact]
        public void GenerateMatches_Deadlock_HighlyConstrainedImpossibleGraph()
        {
            // Arrange
            // 3 participants. 
            // p1 excludes p2
            // p2 excludes p3
            // p3 excludes p1
            // Wait, this graph has only one cycle direction: 1->3->2->1.
            // If we also prevent mirrors? No, this does not have mirrors anyway.
            // Let's create an impossible configuration:
            // p1 excludes p2, p3 (excludes everyone other than themselves) -> handled by GiverExcludesAll
            // Let's create a cyclic deadlock where no individual excludes everyone, but collectively they form a deadlock.
            // E.g., Size = 3.
            // p1 excludes p2
            // p2 excludes p1
            // p3 excludes p1 and p2 (so p3 can only buy for themselves, which is banned!)
            var participants = CreateParticipantGroup(3);
            participants[0].ExcludedParticipantIds.Add("p2");
            participants[1].ExcludedParticipantIds.Add("p1");
            participants[2].ExcludedParticipantIds.AddRange(new[] { "p1", "p2" }); // p3 excludes everyone else

            // Act
            var result = MatchingSolver.GenerateMatches(participants, preventMirrors: false);

            // Assert
            Assert.False(result.Success);
            Assert.Contains("Deadlock detected", result.ErrorMessage);
        }

        [Fact]
        public void GenerateMatches_PreventMirrorsImpossibleForSizeTwo()
        {
            // Arrange
            // 2 participants. If we prevent mirrors, a valid matching is mathematically impossible
            // because the only non-self matches are A->B and B->A, which is a mirror!
            var participants = CreateParticipantGroup(2);

            // Act
            var result = MatchingSolver.GenerateMatches(participants, preventMirrors: true);

            // Assert
            Assert.False(result.Success);
            Assert.Contains("Unable to find a valid matching", result.ErrorMessage);
        }

        [Fact]
        public void GenerateMatches_ComplexCyclicExclusions_SolvedSuccessfully()
        {
            // Arrange
            // Let's make a larger group of 8 people with complex but solvable exclusions
            var participants = CreateParticipantGroup(8);
            
            // p1 excludes p2, p3
            participants[0].ExcludedParticipantIds.AddRange(new[] { "p2", "p3" });
            // p2 excludes p4
            participants[1].ExcludedParticipantIds.Add("p4");
            // p3 excludes p1, p5
            participants[2].ExcludedParticipantIds.AddRange(new[] { "p1", "p5" });
            // p4 excludes p6
            participants[3].ExcludedParticipantIds.Add("p6");
            // p5 excludes p7
            participants[4].ExcludedParticipantIds.Add("p7");
            // p6 excludes p8
            participants[5].ExcludedParticipantIds.Add("p8");
            // p7 excludes p1
            participants[6].ExcludedParticipantIds.Add("p1");
            
            // Act
            var result = MatchingSolver.GenerateMatches(participants, preventMirrors: true);

            // Assert
            Assert.True(result.Success, $"Failed to solve solvable complex exclusions: {result.ErrorMessage}");
            Assert.Equal(8, result.Matches.Count);

            foreach (var giver in participants)
            {
                var receiverId = result.Matches[giver.Id];
                Assert.NotEqual(giver.Id, receiverId);
                Assert.DoesNotContain(receiverId, giver.ExcludedParticipantIds);
                
                // Mirror prevention check
                if (result.Matches.TryGetValue(receiverId, out var reciprocalId))
                {
                    Assert.NotEqual(giver.Id, reciprocalId);
                }
            }
        }
    }
}
