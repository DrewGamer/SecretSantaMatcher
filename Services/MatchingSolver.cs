using System;
using System.Collections.Generic;
using System.Linq;
using SecretSantaMatcher.Models;

namespace SecretSantaMatcher.Services
{
    public class MatchingResult
    {
        public bool Success { get; set; }
        public Dictionary<string, string> Matches { get; set; } = new(); // Key: Giver Id, Value: Receiver Id
        public string ErrorMessage { get; set; } = string.Empty;
    }

    public static class MatchingSolver
    {
        private static readonly Random _random = new();

        public static MatchingResult GenerateMatches(List<Participant> participants, bool preventMirrors = false)
        {
            if (participants == null || participants.Count < 2)
            {
                return new MatchingResult
                {
                    Success = false,
                    ErrorMessage = "At least 2 participants are required to perform a matching."
                };
            }

            // Shuffle participants first to introduce randomness
            var shuffledGivers = participants.OrderBy(x => _random.Next()).ToList();
            var availableReceivers = new List<Participant>(participants);
            
            // Dictionary to store the selected match (GiverId -> ReceiverId)
            var matches = new Dictionary<string, string>();

            if (Solve(shuffledGivers, 0, availableReceivers, matches, preventMirrors))
            {
                return new MatchingResult
                {
                    Success = true,
                    Matches = matches
                };
            }

            // If we couldn't solve, analyze the bottleneck to give a helpful message
            string errorDetails = AnalyzeConstraints(participants, preventMirrors);
            return new MatchingResult
            {
                Success = false,
                ErrorMessage = $"Unable to find a valid matching that respects all constraints.\n\n{errorDetails}"
            };
        }

        private static bool Solve(
            List<Participant> givers, 
            int giverIndex, 
            List<Participant> availableReceivers, 
            Dictionary<string, string> matches,
            bool preventMirrors)
        {
            // Base case: All givers matched successfully
            if (giverIndex >= givers.Count)
            {
                return true;
            }

            var giver = givers[giverIndex];

            // Shuffle available receivers to randomize matching selections at each step
            var shuffledCandidates = availableReceivers.OrderBy(x => _random.Next()).ToList();

            foreach (var receiver in shuffledCandidates)
            {
                // Constraint Checks:
                // 1. Cannot buy for self
                if (receiver.Id == giver.Id)
                    continue;

                // 2. Directed exclusions check
                if (giver.ExcludedParticipantIds != null && giver.ExcludedParticipantIds.Contains(receiver.Id))
                    continue;

                // 3. Prevent mirror matches (reciprocal pairings: A buys for B, and B buys for A)
                if (preventMirrors && matches.TryGetValue(receiver.Id, out var directReceiver) && directReceiver == giver.Id)
                    continue;

                // Attempt to assign
                matches[giver.Id] = receiver.Id;
                availableReceivers.Remove(receiver);

                // Recurse to next giver
                if (Solve(givers, giverIndex + 1, availableReceivers, matches, preventMirrors))
                {
                    return true;
                }

                // Backtrack if assignment leads to a dead end
                matches.Remove(giver.Id);
                availableReceivers.Add(receiver);
            }

            return false;
        }

        private static string AnalyzeConstraints(List<Participant> participants, bool preventMirrors)
        {
            int total = participants.Count;
            if (total < 2)
            {
                return "You need at least 2 participants.";
            }

            // Check if any participant has excluded all other (total - 1) members
            var giversExcludingAll = new List<Participant>();
            foreach (var p in participants)
            {
                var otherIds = new HashSet<string>(participants.Where(x => x.Id != p.Id).Select(x => x.Id));
                var actualExclusions = p.ExcludedParticipantIds != null 
                    ? p.ExcludedParticipantIds.Intersect(otherIds).Count() 
                    : 0;

                if (actualExclusions >= total - 1)
                {
                    giversExcludingAll.Add(p);
                }
            }

            // Check if any participant is excluded by all other (total - 1) members
            var receiversExcludedByAll = new List<Participant>();
            foreach (var p in participants)
            {
                int exclusionCount = 0;
                foreach (var other in participants)
                {
                    if (other.Id == p.Id) continue;
                    if (other.ExcludedParticipantIds != null && other.ExcludedParticipantIds.Contains(p.Id))
                    {
                        exclusionCount++;
                    }
                }

                if (exclusionCount >= total - 1)
                {
                    receiversExcludedByAll.Add(p);
                }
            }

            if (giversExcludingAll.Any())
            {
                string names = string.Join(", ", giversExcludingAll.Select(p => $"'{p.DisplayName}'"));
                return $"Deadlock detected: Participant(s) {names} excluded all other possible recipients, leaving them with no one to buy a gift for.";
            }

            if (receiversExcludedByAll.Any())
            {
                string names = string.Join(", ", receiversExcludedByAll.Select(p => $"'{p.DisplayName}'"));
                return $"Deadlock detected: Participant(s) {names} are excluded by all other participants, leaving them with no possible secret givers.";
            }

            // Fallback: cyclic deadlock or general bottleneck description
            string detail = "This usually happens when too many participants have exclusion constraints";
            if (preventMirrors)
            {
                detail += " or mirror match prevention constraints";
            }
            detail += " relative to the small size of the group, leaving no mathematical permutations where everyone is assigned a valid secret recipient (e.g., a cyclic exclusion deadlock).";
            return detail;
        }
    }
}
