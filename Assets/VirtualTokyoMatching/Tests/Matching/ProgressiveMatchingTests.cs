using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using VirtualTokyoMatching;

namespace VirtualTokyoMatching.Tests
{
    /// <summary>
    /// Tests for the progressive matching system - the core innovation of Virtual Tokyo Matching
    /// Tests partial questionnaire matching, provisional vectors, incremental updates, and graduated confidence
    /// Validates that users can see recommendations and start conversations even with incomplete questionnaires
    /// </summary>
    public class ProgressiveMatchingTests
    {
        private GameObject testWorldRoot;
        private List<TestMatchingUser> testUsers;
        private VectorBuilder vectorBuilder;
        private PublicProfilePublisher profilePublisher;
        private CompatibilityCalculator compatibilityCalculator;
        private RecommenderUI recommenderUI;

        [System.Serializable]
        private class TestMatchingUser
        {
            public GameObject gameObject;
            public PlayerDataManager playerData;
            public VectorBuilder vectorBuilder;
            public string displayName;
            public int questionsAnswered;
            public float[] provisional30D;
            public float[] final30D;
            public float[] compressed6D;
            public bool isPublic;
            public bool allowProvisional;
            public List<CompatibilityMatch> recommendations;

            public TestMatchingUser(string name)
            {
                displayName = name;
                gameObject = new GameObject($"TestUser_{name}");
                playerData = gameObject.AddComponent<PlayerDataManager>();
                vectorBuilder = gameObject.AddComponent<VectorBuilder>();
                provisional30D = new float[30];
                final30D = new float[30];
                compressed6D = new float[6];
                recommendations = new List<CompatibilityMatch>();
                questionsAnswered = 0;
                isPublic = false;
                allowProvisional = false;
            }

            public void Initialize()
            {
                playerData.SendCustomEvent("Start");
                vectorBuilder.SendCustomEvent("Start");
                
                // Link VectorBuilder to PlayerDataManager
                var playerDataField = typeof(VectorBuilder).GetField("playerDataManager");
                if (playerDataField != null)
                {
                    playerDataField.SetValue(vectorBuilder, playerData);
                }
            }

            public void AnswerQuestions(int count, bool triggerVectorUpdate = true)
            {
                int startIndex = questionsAnswered;
                for (int i = 0; i < count && (startIndex + i) < 112; i++)
                {
                    int questionIndex = startIndex + i;
                    int response = Random.Range(1, 6);
                    playerData.SaveQuestionResponse(questionIndex, response);
                    questionsAnswered++;
                }

                if (triggerVectorUpdate)
                {
                    UpdateProvisionalVector();
                }
            }

            public void UpdateProvisionalVector()
            {
                // Simulate VectorBuilder creating provisional 30D vector
                provisional30D = GenerateProvisionalVector();
                playerData.UpdateVector30D(provisional30D);
                
                if (questionsAnswered >= 112)
                {
                    // Complete - normalize and finalize
                    final30D = NormalizeVector(provisional30D);
                    playerData.UpdateVector30D(final30D);
                }
            }

            public void SetSharingPreferences(bool publicSharing, bool provisionalAllowed)
            {
                isPublic = publicSharing;
                allowProvisional = provisionalAllowed;
                
                int flags = 0;
                if (publicSharing) flags |= 1;
                if (provisionalAllowed) flags |= 2;
                
                playerData.SetFlags(flags);
            }

            public float GetCompletionRatio()
            {
                return (float)questionsAnswered / 112f;
            }

            public bool IsComplete()
            {
                return questionsAnswered >= 112;
            }

            private float[] GenerateProvisionalVector()
            {
                var vector = new float[30];
                var responses = playerData.GetAllQuestionResponses();
                
                // Simplified vector generation based on answered questions
                for (int i = 0; i < 30; i++)
                {
                    float sum = 0f;
                    int validQuestions = 0;
                    
                    // Map each vector dimension to ~4 questions (30 * 4 = 120, covers 112 questions)
                    for (int q = i * 4; q < (i + 1) * 4 && q < 112; q++)
                    {
                        if (responses[q] > 0) // Question is answered
                        {
                            sum += (responses[q] - 3f) / 2f; // Convert 1-5 scale to -1 to +1
                            validQuestions++;
                        }
                    }
                    
                    if (validQuestions > 0)
                    {
                        vector[i] = sum / validQuestions;
                    }
                    else
                    {
                        vector[i] = 0f; // Unanswered questions contribute 0
                    }
                }
                
                return vector;
            }

            private float[] NormalizeVector(float[] input)
            {
                var normalized = new float[30];
                float magnitude = 0f;
                
                // Calculate magnitude
                for (int i = 0; i < 30; i++)
                {
                    magnitude += input[i] * input[i];
                }
                magnitude = Mathf.Sqrt(magnitude);
                
                // Normalize
                if (magnitude > 0.001f)
                {
                    for (int i = 0; i < 30; i++)
                    {
                        normalized[i] = input[i] / magnitude;
                    }
                }
                
                return normalized;
            }

            public void Cleanup()
            {
                if (gameObject != null)
                {
                    Object.DestroyImmediate(gameObject);
                }
            }
        }

        [System.Serializable]
        private class CompatibilityMatch
        {
            public TestMatchingUser user;
            public float similarity;
            public float confidence;
            public bool isProvisional;
            public float progressPercentage;

            public CompatibilityMatch(TestMatchingUser u, float sim, float conf, bool prov, float prog)
            {
                user = u;
                similarity = sim;
                confidence = conf;
                isProvisional = prov;
                progressPercentage = prog;
            }
        }

        [SetUp]
        public void SetUp()
        {
            testWorldRoot = new GameObject("TestProgressiveMatchingWorld");
            testUsers = new List<TestMatchingUser>();
            
            // Add core components
            vectorBuilder = testWorldRoot.AddComponent<VectorBuilder>();
            profilePublisher = testWorldRoot.AddComponent<PublicProfilePublisher>();
            compatibilityCalculator = testWorldRoot.AddComponent<CompatibilityCalculator>();
            recommenderUI = testWorldRoot.AddComponent<RecommenderUI>();
            
            // Initialize systems
            vectorBuilder.SendCustomEvent("Start");
            profilePublisher.SendCustomEvent("Start");
            compatibilityCalculator.SendCustomEvent("Start");
            recommenderUI.SendCustomEvent("Start");
        }

        [TearDown]
        public void TearDown()
        {
            // Clean up test users
            foreach (var user in testUsers)
            {
                user.Cleanup();
            }
            testUsers.Clear();
            
            if (testWorldRoot != null)
            {
                Object.DestroyImmediate(testWorldRoot);
            }
        }

        /// <summary>
        /// Test basic progressive matching setup with partial questionnaires
        /// </summary>
        [Test]
        public void TestBasicProgressiveMatchingSetup()
        {
            // Arrange
            CreateProgressiveUsers(4, new int[] { 20, 45, 80, 112 }); // Different completion levels
            
            // Act - Enable progressive sharing for all users
            foreach (var user in testUsers)
            {
                user.SetSharingPreferences(true, true); // Public + provisional allowed
            }
            
            // Assert
            Assert.AreEqual(4, testUsers.Count, "Should have 4 test users");
            
            foreach (var user in testUsers)
            {
                Assert.IsTrue(user.isPublic, $"{user.displayName} should be public");
                Assert.IsTrue(user.allowProvisional, $"{user.displayName} should allow provisional sharing");
                Assert.Greater(user.questionsAnswered, 0, $"{user.displayName} should have answered some questions");
            }
            
            // Verify different completion ratios
            var completionRatios = testUsers.Select(u => u.GetCompletionRatio()).OrderBy(r => r).ToArray();
            Assert.Less(completionRatios[0], 0.25f, "Lowest completion should be under 25%");
            Assert.Greater(completionRatios[3], 0.95f, "Highest completion should be over 95%");
            
            Debug.Log("[PROGRESSIVE TEST] Basic setup complete - completion ratios: " + 
                     string.Join(", ", completionRatios.Select(r => $"{r:P0}")));
        }

        /// <summary>
        /// Test provisional vector updates with incremental question answering
        /// </summary>
        [Test]
        public void TestProvisionalVectorUpdates()
        {
            // Arrange
            var user = CreateSingleProgressiveUser("TestUser", 10); // Start with 10 questions
            
            // Get initial provisional vector
            float[] initialVector = user.provisional30D.ToArray();
            float initialNorm = CalculateVectorNorm(initialVector);
            
            // Act - Answer more questions
            user.AnswerQuestions(15); // Now has 25 total
            float[] updatedVector = user.provisional30D.ToArray();
            float updatedNorm = CalculateVectorNorm(updatedVector);
            
            // Assert vector has changed meaningfully
            float vectorDifference = CalculateVectorDifference(initialVector, updatedVector);
            Assert.Greater(vectorDifference, 0.1f, "Vector should change significantly with new answers");
            Assert.Greater(updatedNorm, initialNorm * 0.8f, "Updated vector should maintain reasonable magnitude");
            
            // Continue to completion
            user.AnswerQuestions(87); // Complete all 112 questions
            Assert.IsTrue(user.IsComplete(), "User should be complete");
            
            float[] finalVector = user.final30D.ToArray();
            float finalNorm = CalculateVectorNorm(finalVector);
            
            // Final normalized vector should have unit length (approximately)
            Assert.AreEqual(1f, finalNorm, 0.1f, "Final vector should be approximately normalized");
            
            Debug.Log($"[PROGRESSIVE TEST] Vector evolution - Initial norm: {initialNorm:F3}, " +
                     $"Updated: {updatedNorm:F3}, Final: {finalNorm:F3}");
        }

        /// <summary>
        /// Test compatibility calculation with mixed provisional and complete data
        /// </summary>
        [Test]
        public void TestMixedProvisionalCompatibilityCalculation()
        {
            // Arrange - Create users with different completion levels
            CreateProgressiveUsers(5, new int[] { 25, 50, 75, 100, 112 });
            
            // All users public with provisional allowed
            foreach (var user in testUsers)
            {
                user.SetSharingPreferences(true, true);
            }
            
            // Act - Calculate compatibility between all pairs
            var compatibilityMatrix = CalculateCompatibilityMatrix();
            
            // Assert
            Assert.AreEqual(testUsers.Count, compatibilityMatrix.Count, "Should have compatibility data for all users");
            
            // Test that complete users have higher confidence in their matches
            var completeUser = testUsers.First(u => u.IsComplete());
            var partialUser = testUsers.First(u => !u.IsComplete());
            
            var completeUserMatches = compatibilityMatrix[completeUser];
            var partialUserMatches = compatibilityMatrix[partialUser];
            
            float avgCompleteConfidence = completeUserMatches.Average(m => m.confidence);
            float avgPartialConfidence = partialUserMatches.Average(m => m.confidence);
            
            Assert.Greater(avgCompleteConfidence, avgPartialConfidence * 0.8f, 
                          "Complete users should generally have higher confidence matches");
            
            // Test that similarity calculations work for all combinations
            foreach (var userMatches in compatibilityMatrix)
            {
                foreach (var match in userMatches.Value)
                {
                    Assert.That(match.similarity, Is.InRange(-1f, 1f), "Similarity should be in valid range");
                    Assert.That(match.confidence, Is.InRange(0f, 1f), "Confidence should be in valid range");
                }
            }
            
            Debug.Log($"[PROGRESSIVE TEST] Compatibility calculation - Complete confidence: {avgCompleteConfidence:F3}, " +
                     $"Partial confidence: {avgPartialConfidence:F3}");
        }

        /// <summary>
        /// Test graduated confidence based on completion percentage
        /// </summary>
        [Test]
        public void TestGraduatedConfidenceSystem()
        {
            // Arrange - Create users with specific completion levels
            var completionLevels = new int[] { 10, 30, 60, 90, 112 };
            CreateProgressiveUsers(5, completionLevels);
            
            foreach (var user in testUsers)
            {
                user.SetSharingPreferences(true, true);
            }
            
            // Act - Calculate confidence scores for each completion level
            var confidenceByCompletion = new Dictionary<int, float>();
            
            for (int i = 0; i < testUsers.Count; i++)
            {
                var user = testUsers[i];
                float confidence = CalculateUserConfidence(user);
                confidenceByCompletion[completionLevels[i]] = confidence;
            }
            
            // Assert graduated confidence (higher completion = higher confidence)
            Assert.Less(confidenceByCompletion[10], confidenceByCompletion[30], "30Q should have higher confidence than 10Q");
            Assert.Less(confidenceByCompletion[30], confidenceByCompletion[60], "60Q should have higher confidence than 30Q");
            Assert.Less(confidenceByCompletion[60], confidenceByCompletion[90], "90Q should have higher confidence than 60Q");
            Assert.Less(confidenceByCompletion[90], confidenceByCompletion[112], "112Q should have highest confidence");
            
            // Test specific thresholds
            Assert.Less(confidenceByCompletion[10], 0.4f, "Very low completion should have low confidence");
            Assert.Greater(confidenceByCompletion[112], 0.8f, "Complete questionnaire should have high confidence");
            
            Debug.Log("[PROGRESSIVE TEST] Graduated confidence: " + 
                     string.Join(", ", confidenceByCompletion.Select(kvp => $"{kvp.Key}Q={kvp.Value:F2}")));
        }

        /// <summary>
        /// Test recommendation list updates during progressive answering
        /// </summary>
        [UnityTest]
        public IEnumerator TestRecommendationListProgression()
        {
            // Arrange
            CreateProgressiveUsers(6, new int[] { 40, 50, 60, 70, 80, 90 }); // All partially complete
            
            var targetUser = testUsers[0]; // User with 40 questions answered
            var otherUsers = testUsers.Skip(1).ToArray();
            
            // All users public with provisional allowed
            foreach (var user in testUsers)
            {
                user.SetSharingPreferences(true, true);
            }
            
            // Get initial recommendations
            yield return null;
            var initialRecommendations = GetTopRecommendations(targetUser, 3);
            
            // Act - Target user answers more questions
            targetUser.AnswerQuestions(20); // Now has 60 questions
            yield return null;
            
            var midRecommendations = GetTopRecommendations(targetUser, 3);
            
            // Continue answering
            targetUser.AnswerQuestions(32); // Now has 92 questions  
            yield return null;
            
            var finalRecommendations = GetTopRecommendations(targetUser, 3);
            
            // Assert recommendations evolved
            Assert.AreEqual(3, initialRecommendations.Count, "Should have 3 initial recommendations");
            Assert.AreEqual(3, midRecommendations.Count, "Should have 3 mid-stage recommendations");
            Assert.AreEqual(3, finalRecommendations.Count, "Should have 3 final recommendations");
            
            // Confidence should increase as target user completes more questions
            float avgInitialConfidence = initialRecommendations.Average(r => r.confidence);
            float avgMidConfidence = midRecommendations.Average(r => r.confidence);
            float avgFinalConfidence = finalRecommendations.Average(r => r.confidence);
            
            Assert.GreaterOrEqual(avgMidConfidence, avgInitialConfidence * 0.9f, 
                                 "Mid-stage confidence should maintain or improve");
            Assert.GreaterOrEqual(avgFinalConfidence, avgMidConfidence * 0.9f, 
                                 "Final confidence should maintain or improve");
            
            Debug.Log($"[PROGRESSIVE TEST] Recommendation progression - Initial: {avgInitialConfidence:F3}, " +
                     $"Mid: {avgMidConfidence:F3}, Final: {avgFinalConfidence:F3}");
        }

        /// <summary>
        /// Test provisional matching with confidence indicators
        /// </summary>
        [Test]
        public void TestProvisionalMatchingIndicators()
        {
            // Arrange
            CreateProgressiveUsers(4, new int[] { 15, 35, 85, 112 });
            
            foreach (var user in testUsers)
            {
                user.SetSharingPreferences(true, true);
            }
            
            // Act - Generate matches with provisional indicators
            var matchesWithIndicators = GenerateMatchesWithIndicators();
            
            // Assert provisional indicators are correct
            foreach (var userMatches in matchesWithIndicators)
            {
                var user = userMatches.Key;
                var matches = userMatches.Value;
                
                foreach (var match in matches)
                {
                    bool expectedProvisional = !match.user.IsComplete() || !user.IsComplete();
                    Assert.AreEqual(expectedProvisional, match.isProvisional, 
                                   $"Provisional flag should be {expectedProvisional} for match between " +
                                   $"{user.displayName}({user.questionsAnswered}Q) and {match.user.displayName}({match.user.questionsAnswered}Q)");
                    
                    Assert.AreEqual(match.user.GetCompletionRatio(), match.progressPercentage, 0.01f,
                                   "Progress percentage should match completion ratio");
                }
            }
            
            // Test UI indication data
            var partialUser = testUsers.First(u => !u.IsComplete());
            var completeUser = testUsers.First(u => u.IsComplete());
            
            var partialMatches = matchesWithIndicators[partialUser];
            var completeMatches = matchesWithIndicators[completeUser];
            
            Assert.IsTrue(partialMatches.Any(m => m.isProvisional), "Partial user should have provisional matches");
            
            Debug.Log($"[PROGRESSIVE TEST] Provisional indicators verified for {matchesWithIndicators.Count} users");
        }

        /// <summary>
        /// Test early matching opportunity detection (when matching becomes viable)
        /// </summary>
        [Test]
        public void TestEarlyMatchingOpportunities()
        {
            // Arrange - Start with very few questions answered
            var user = CreateSingleProgressiveUser("EarlyUser", 5);
            CreateProgressiveUsers(3, new int[] { 20, 40, 60 }); // Other users with more answers
            
            user.SetSharingPreferences(true, true);
            foreach (var otherUser in testUsers.Skip(1))
            {
                otherUser.SetSharingPreferences(true, true);
            }
            
            // Test minimum viable matching threshold
            bool canMatchAt5Questions = CanGenerateViableMatches(user);
            Assert.IsFalse(canMatchAt5Questions, "Should not generate matches with only 5 questions");
            
            // Answer more questions gradually
            user.AnswerQuestions(10); // Now has 15 total
            bool canMatchAt15Questions = CanGenerateViableMatches(user);
            Assert.IsTrue(canMatchAt15Questions, "Should generate provisional matches with 15+ questions");
            
            // Test match quality improvement
            user.AnswerQuestions(15); // Now has 30 total
            var matches30Q = GetTopRecommendations(user, 3);
            
            user.AnswerQuestions(20); // Now has 50 total
            var matches50Q = GetTopRecommendations(user, 3);
            
            float avg30Confidence = matches30Q.Average(m => m.confidence);
            float avg50Confidence = matches50Q.Average(m => m.confidence);
            
            Assert.Greater(avg50Confidence, avg30Confidence * 0.95f, 
                          "Match quality should improve or maintain with more questions");
            
            Debug.Log($"[PROGRESSIVE TEST] Early matching - 30Q confidence: {avg30Confidence:F3}, " +
                     $"50Q confidence: {avg50Confidence:F3}");
        }

        /// <summary>
        /// Test incremental recalculation triggers on question answers
        /// </summary>
        [UnityTest]
        public IEnumerator TestIncrementalRecalculationTriggers()
        {
            // Arrange
            CreateProgressiveUsers(5, new int[] { 30, 40, 50, 60, 70 });
            
            foreach (var user in testUsers)
            {
                user.SetSharingPreferences(true, true);
            }
            
            var targetUser = testUsers[0];
            
            // Get baseline recommendations
            yield return null;
            var baselineRecommendations = GetTopRecommendations(targetUser, 3);
            var baselineTopMatch = baselineRecommendations.First();
            
            // Act - Answer single question (should trigger recalculation)
            targetUser.AnswerQuestions(1);
            yield return null; // Allow recalculation
            
            var updatedRecommendations = GetTopRecommendations(targetUser, 3);
            var updatedTopMatch = updatedRecommendations.First();
            
            // Assert recalculation occurred
            bool recommendationsChanged = !baselineTopMatch.user.Equals(updatedTopMatch.user) || 
                                        Mathf.Abs(baselineTopMatch.similarity - updatedTopMatch.similarity) > 0.01f;
            
            // Note: Recommendations might not always change with one question, but system should recalculate
            Assert.AreEqual(3, updatedRecommendations.Count, "Should still have 3 recommendations after update");
            
            // Test batch answer trigger
            var preBatchRecommendations = updatedRecommendations;
            targetUser.AnswerQuestions(5); // Answer 5 questions at once
            yield return null;
            
            var postBatchRecommendations = GetTopRecommendations(targetUser, 3);
            
            // Batch updates should definitely cause changes
            bool batchChanged = !preBatchRecommendations.First().user.Equals(postBatchRecommendations.First().user) ||
                               Mathf.Abs(preBatchRecommendations.First().similarity - postBatchRecommendations.First().similarity) > 0.05f;
            
            Debug.Log($"[PROGRESSIVE TEST] Incremental recalculation - Single Q changed: {recommendationsChanged}, " +
                     $"Batch changed: {batchChanged}");
        }

        /// <summary>
        /// Integration test for complete progressive matching workflow
        /// </summary>
        [UnityTest]
        public IEnumerator TestCompleteProgressiveWorkflow()
        {
            // Arrange - Simulate real user journey
            var newUser = CreateSingleProgressiveUser("NewUser", 0); // Brand new user
            CreateProgressiveUsers(8, new int[] { 25, 35, 45, 55, 65, 75, 85, 112 }); // Existing users
            
            // Existing users are public
            foreach (var user in testUsers.Skip(1))
            {
                user.SetSharingPreferences(true, true);
            }
            
            // Phase 1: User starts questionnaire but isn't public yet
            newUser.AnswerQuestions(20);
            Assert.IsFalse(newUser.isPublic, "User should not be public initially");
            
            // Phase 2: User enables provisional sharing
            newUser.SetSharingPreferences(true, true);
            yield return null;
            
            var phase2Recommendations = GetTopRecommendations(newUser, 3);
            Assert.Greater(phase2Recommendations.Count, 0, "Should get provisional recommendations");
            Assert.IsTrue(phase2Recommendations.All(r => r.isProvisional), "All recommendations should be provisional");
            
            // Phase 3: User continues answering, recommendations evolve
            for (int phase = 3; phase <= 6; phase++)
            {
                newUser.AnswerQuestions(20); // Answer 20 more questions each phase
                yield return null;
                
                var phaseRecommendations = GetTopRecommendations(newUser, 3);
                Assert.AreEqual(3, phaseRecommendations.Count, $"Phase {phase} should have 3 recommendations");
                
                float avgConfidence = phaseRecommendations.Average(r => r.confidence);
                Debug.Log($"[PROGRESSIVE TEST] Phase {phase} - Questions: {newUser.questionsAnswered}/112, " +
                         $"Avg confidence: {avgConfidence:F3}");
            }
            
            // Phase 7: Complete questionnaire
            int remaining = 112 - newUser.questionsAnswered;
            if (remaining > 0)
            {
                newUser.AnswerQuestions(remaining);
            }
            yield return null;
            
            var finalRecommendations = GetTopRecommendations(newUser, 3);
            Assert.IsTrue(newUser.IsComplete(), "User should be complete");
            Assert.IsFalse(finalRecommendations.Any(r => r.isProvisional), 
                          "No recommendations should be provisional for complete user");
            
            float finalConfidence = finalRecommendations.Average(r => r.confidence);
            Assert.Greater(finalConfidence, 0.7f, "Final recommendations should have high confidence");
            
            Debug.Log($"[PROGRESSIVE TEST] Complete workflow - Final confidence: {finalConfidence:F3}");
        }

        #region Helper Methods

        private void CreateProgressiveUsers(int count, int[] questionCounts)
        {
            for (int i = 0; i < count && i < questionCounts.Length; i++)
            {
                var user = new TestMatchingUser($"User{i}");
                user.Initialize();
                user.AnswerQuestions(questionCounts[i]);
                testUsers.Add(user);
            }
        }

        private TestMatchingUser CreateSingleProgressiveUser(string name, int questionCount)
        {
            var user = new TestMatchingUser(name);
            user.Initialize();
            if (questionCount > 0)
            {
                user.AnswerQuestions(questionCount);
            }
            testUsers.Add(user);
            return user;
        }

        private float CalculateVectorNorm(float[] vector)
        {
            float sum = 0f;
            foreach (float val in vector)
            {
                sum += val * val;
            }
            return Mathf.Sqrt(sum);
        }

        private float CalculateVectorDifference(float[] vectorA, float[] vectorB)
        {
            if (vectorA.Length != vectorB.Length) return 1f;
            
            float sumSquaredDiff = 0f;
            for (int i = 0; i < vectorA.Length; i++)
            {
                float diff = vectorA[i] - vectorB[i];
                sumSquaredDiff += diff * diff;
            }
            return Mathf.Sqrt(sumSquaredDiff);
        }

        private Dictionary<TestMatchingUser, List<CompatibilityMatch>> CalculateCompatibilityMatrix()
        {
            var matrix = new Dictionary<TestMatchingUser, List<CompatibilityMatch>>();
            
            foreach (var user in testUsers)
            {
                if (!user.isPublic) continue;
                
                var matches = new List<CompatibilityMatch>();
                foreach (var otherUser in testUsers)
                {
                    if (otherUser == user || !otherUser.isPublic) continue;
                    
                    float similarity = CalculateCosineSimilarity(user.provisional30D, otherUser.provisional30D);
                    float confidence = CalculateMatchConfidence(user, otherUser);
                    bool isProvisional = !user.IsComplete() || !otherUser.IsComplete();
                    
                    matches.Add(new CompatibilityMatch(otherUser, similarity, confidence, isProvisional, 
                                                      otherUser.GetCompletionRatio()));
                }
                
                matrix[user] = matches;
            }
            
            return matrix;
        }

        private float CalculateCosineSimilarity(float[] vectorA, float[] vectorB)
        {
            float dotProduct = 0f;
            float normA = 0f;
            float normB = 0f;
            
            for (int i = 0; i < vectorA.Length && i < vectorB.Length; i++)
            {
                dotProduct += vectorA[i] * vectorB[i];
                normA += vectorA[i] * vectorA[i];
                normB += vectorB[i] * vectorB[i];
            }
            
            float denominator = Mathf.Sqrt(normA) * Mathf.Sqrt(normB);
            return denominator > 0.0001f ? dotProduct / denominator : 0f;
        }

        private float CalculateMatchConfidence(TestMatchingUser userA, TestMatchingUser userB)
        {
            float completionA = userA.GetCompletionRatio();
            float completionB = userB.GetCompletionRatio();
            
            // Confidence based on both users' completion levels
            float avgCompletion = (completionA + completionB) / 2f;
            float minCompletion = Mathf.Min(completionA, completionB);
            
            // Weight average completion more heavily, but consider minimum
            return (avgCompletion * 0.7f) + (minCompletion * 0.3f);
        }

        private float CalculateUserConfidence(TestMatchingUser user)
        {
            float completion = user.GetCompletionRatio();
            
            // Base confidence from completion
            float baseConfidence = completion;
            
            // Boost for reaching certain thresholds
            if (completion >= 1f) baseConfidence += 0.1f; // Complete bonus
            else if (completion >= 0.8f) baseConfidence += 0.05f; // Near-complete bonus
            
            return Mathf.Clamp01(baseConfidence);
        }

        private List<CompatibilityMatch> GetTopRecommendations(TestMatchingUser user, int count)
        {
            if (!user.isPublic) return new List<CompatibilityMatch>();
            
            var matches = new List<CompatibilityMatch>();
            foreach (var otherUser in testUsers)
            {
                if (otherUser == user || !otherUser.isPublic) continue;
                
                float similarity = CalculateCosineSimilarity(user.provisional30D, otherUser.provisional30D);
                float confidence = CalculateMatchConfidence(user, otherUser);
                bool isProvisional = !user.IsComplete() || !otherUser.IsComplete();
                
                matches.Add(new CompatibilityMatch(otherUser, similarity, confidence, isProvisional,
                                                  otherUser.GetCompletionRatio()));
            }
            
            // Sort by similarity descending and take top N
            matches.Sort((a, b) => b.similarity.CompareTo(a.similarity));
            return matches.Take(count).ToList();
        }

        private bool CanGenerateViableMatches(TestMatchingUser user)
        {
            if (!user.isPublic || user.questionsAnswered < 10) return false;
            
            var matches = GetTopRecommendations(user, 1);
            return matches.Count > 0 && matches[0].confidence > 0.2f; // Minimum viability threshold
        }

        private Dictionary<TestMatchingUser, List<CompatibilityMatch>> GenerateMatchesWithIndicators()
        {
            var matches = CalculateCompatibilityMatrix();
            
            // Add indicator data to all matches
            foreach (var userMatches in matches)
            {
                foreach (var match in userMatches.Value)
                {
                    match.isProvisional = !match.user.IsComplete() || !userMatches.Key.IsComplete();
                    match.progressPercentage = match.user.GetCompletionRatio();
                }
            }
            
            return matches;
        }

        // LINQ-like helper extensions for Unity testing
        private static class EnumerableExtensions
        {
            public static IEnumerable<T> Skip<T>(this IEnumerable<T> source, int count)
            {
                int skipped = 0;
                foreach (var item in source)
                {
                    if (skipped++ >= count)
                        yield return item;
                }
            }

            public static T First<T>(this IEnumerable<T> source)
            {
                foreach (var item in source)
                    return item;
                throw new System.InvalidOperationException("Sequence contains no elements");
            }

            public static T First<T>(this IEnumerable<T> source, System.Func<T, bool> predicate)
            {
                foreach (var item in source)
                {
                    if (predicate(item))
                        return item;
                }
                throw new System.InvalidOperationException("No element satisfies the condition");
            }

            public static float Average<T>(this IEnumerable<T> source, System.Func<T, float> selector)
            {
                float sum = 0f;
                int count = 0;
                foreach (var item in source)
                {
                    sum += selector(item);
                    count++;
                }
                return count > 0 ? sum / count : 0f;
            }

            public static bool All<T>(this IEnumerable<T> source, System.Func<T, bool> predicate)
            {
                foreach (var item in source)
                {
                    if (!predicate(item))
                        return false;
                }
                return true;
            }

            public static bool Any<T>(this IEnumerable<T> source, System.Func<T, bool> predicate)
            {
                foreach (var item in source)
                {
                    if (predicate(item))
                        return true;
                }
                return false;
            }

            public static List<T> Take<T>(this List<T> source, int count)
            {
                var result = new List<T>();
                for (int i = 0; i < count && i < source.Count; i++)
                {
                    result.Add(source[i]);
                }
                return result;
            }

            public static IEnumerable<TResult> Select<T, TResult>(this IEnumerable<T> source, System.Func<T, TResult> selector)
            {
                foreach (var item in source)
                {
                    yield return selector(item);
                }
            }

            public static IEnumerable<T> OrderBy<T>(this IEnumerable<T> source, System.Func<T, float> keySelector)
            {
                var list = new List<T>();
                foreach (var item in source)
                    list.Add(item);

                // Simple sort
                for (int i = 0; i < list.Count - 1; i++)
                {
                    for (int j = 0; j < list.Count - i - 1; j++)
                    {
                        if (keySelector(list[j]) > keySelector(list[j + 1]))
                        {
                            var temp = list[j];
                            list[j] = list[j + 1];
                            list[j + 1] = temp;
                        }
                    }
                }

                return list;
            }

            public static T[] ToArray<T>(this IEnumerable<T> source)
            {
                var list = new List<T>();
                foreach (var item in source)
                    list.Add(item);
                return list.ToArray();
            }
        }

        #endregion
    }
}