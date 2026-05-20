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

                // 2. Cannot buy for significant other (check both directions for robustness)
                if (!string.IsNullOrEmpty(giver.SignificantOtherId) && receiver.Id == giver.SignificantOtherId)
                    continue;
                if (!string.IsNullOrEmpty(receiver.SignificantOtherId) && giver.Id == receiver.SignificantOtherId)
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

            // Count significant other pairs
            int coupleCount = 0;
            var processed = new HashSet<string>();
            foreach (var p in participants)
            {
                if (!string.IsNullOrEmpty(p.SignificantOtherId) && !processed.Contains(p.Id))
                {
                    processed.Add(p.Id);
                    processed.Add(p.SignificantOtherId);
                    coupleCount++;
                }
            }

            if (total == 2 && coupleCount > 0)
            {
                return "With only 2 participants who are significant others, it is mathematically impossible to match them because both would have to buy for their significant other.";
            }

            if (coupleCount * 2 > total - 1)
            {
                return $"You have {coupleCount} pairs of significant others out of {total} total participants. There are too many constraints relative to the group size to make a complete, secret circle.";
            }

            // General bottleneck analysis
            string detail = "This usually happens when too many participants have mutual 'significant other' exclusions";
            if (preventMirrors)
            {
                detail += " or mirror match prevention constraints";
            }
            detail += " relative to the small size of the group, leaving no mathematical permutations where everyone is assigned a valid secret recipient.";
            return detail;
        }
    }
}
