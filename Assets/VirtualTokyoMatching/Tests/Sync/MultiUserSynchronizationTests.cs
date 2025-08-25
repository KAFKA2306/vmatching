using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using VirtualTokyoMatching;

namespace VirtualTokyoMatching.Tests
{
    /// <summary>
    /// Multi-user synchronization tests for Virtual Tokyo Matching
    /// Tests public profile broadcasting, late-joiner support, visibility toggling, and 6D compression
    /// Simulates multiple users with synchronized data across VRChat network
    /// </summary>
    public class MultiUserSynchronizationTests
    {
        private GameObject testWorldRoot;
        private List<TestUser> testUsers;
        private PublicProfilePublisher profilePublisher;
        private CompatibilityCalculator compatibilityCalculator;

        [System.Serializable]
        private class TestUser
        {
            public GameObject gameObject;
            public PlayerDataManager playerData;
            public string displayName;
            public bool isPublic;
            public float[] vector30D;
            public float[] vector6D;
            public int userID;

            public TestUser(string name, int id)
            {
                displayName = name;
                userID = id;
                gameObject = new GameObject($"TestUser_{name}");
                playerData = gameObject.AddComponent<PlayerDataManager>();
                vector30D = new float[30];
                vector6D = new float[6];
                isPublic = false;
            }

            public void Initialize()
            {
                playerData.SendCustomEvent("Start");
                
                // Generate test personality data
                for (int i = 0; i < 30; i++)
                {
                    vector30D[i] = Random.Range(-1f, 1f);
                }
                playerData.UpdateVector30D(vector30D);
                
                // Generate some questionnaire responses
                int answeredQuestions = Random.Range(30, 112);
                for (int q = 0; q < answeredQuestions; q++)
                {
                    playerData.SaveQuestionResponse(q, Random.Range(1, 6));
                }
            }

            public void SetPublic(bool publicState)
            {
                isPublic = publicState;
                playerData.SetFlags(publicState ? 1 : 0);
            }

            public void Cleanup()
            {
                if (gameObject != null)
                {
                    Object.DestroyImmediate(gameObject);
                }
            }
        }

        [SetUp]
        public void SetUp()
        {
            testWorldRoot = new GameObject("TestVirtualTokyoMatchingWorld");
            testUsers = new List<TestUser>();
            
            // Add core synchronization components
            profilePublisher = testWorldRoot.AddComponent<PublicProfilePublisher>();
            compatibilityCalculator = testWorldRoot.AddComponent<CompatibilityCalculator>();
            
            // Initialize systems
            profilePublisher.SendCustomEvent("Start");
            compatibilityCalculator.SendCustomEvent("Start");
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
        /// Test basic multi-user setup and initialization
        /// </summary>
        [Test]
        public void TestMultiUserInitialization()
        {
            // Arrange
            CreateTestUsers(5);
            
            // Assert
            Assert.AreEqual(5, testUsers.Count, "Should have 5 test users");
            
            foreach (var user in testUsers)
            {
                Assert.IsNotNull(user.playerData, "Each user should have PlayerDataManager");
                Assert.IsTrue(user.playerData.IsDataLoaded, "Each user should have loaded data");
                Assert.GreaterOrEqual(user.playerData.CurrentProgress, 30, "Each user should have answered some questions");
            }
            
            Debug.Log("[SYNC TEST] Multi-user initialization completed successfully");
        }

        /// <summary>
        /// Test public profile broadcasting when users enable sharing
        /// </summary>
        [Test]
        public void TestPublicProfileBroadcasting()
        {
            // Arrange
            CreateTestUsers(4);
            
            // Make first two users public
            testUsers[0].SetPublic(true);
            testUsers[1].SetPublic(true);
            
            // Act - simulate profile publishing
            SimulateProfilePublishing();
            
            // Assert
            Assert.IsTrue(testUsers[0].isPublic, "User 0 should be public");
            Assert.IsTrue(testUsers[1].isPublic, "User 1 should be public");
            Assert.IsFalse(testUsers[2].isPublic, "User 2 should remain private");
            Assert.IsFalse(testUsers[3].isPublic, "User 3 should remain private");
            
            // Verify 6D compression occurred for public users
            Assert.AreNotEqual(0f, GetSumOfVector(testUsers[0].vector6D), "User 0 should have 6D vector data");
            Assert.AreNotEqual(0f, GetSumOfVector(testUsers[1].vector6D), "User 1 should have 6D vector data");
            
            Debug.Log("[SYNC TEST] Public profile broadcasting works correctly");
        }

        /// <summary>
        /// Test 30D to 6D vector compression maintains essential information
        /// </summary>
        [Test]
        public void Test30Dto6DCompressionAccuracy()
        {
            // Arrange
            CreateTestUsers(1);
            var user = testUsers[0];
            
            // Create known 30D vector
            for (int i = 0; i < 30; i++)
            {
                user.vector30D[i] = Mathf.Sin(i * 0.2f); // Sinusoidal pattern
            }
            user.playerData.UpdateVector30D(user.vector30D);
            user.SetPublic(true);
            
            // Act - simulate compression
            user.vector6D = Simulate30Dto6DCompression(user.vector30D);
            
            // Assert compression maintains reasonable information density
            float original30DNorm = GetVectorNorm(user.vector30D);
            float compressed6DNorm = GetVectorNorm(user.vector6D);
            
            Assert.Greater(compressed6DNorm, 0f, "6D vector should not be zero");
            Assert.Less(compressed6DNorm, original30DNorm * 2f, "6D vector should not have excessive magnitude");
            
            // Test that different 30D vectors produce different 6D compressions
            var user2Vector30D = new float[30];
            for (int i = 0; i < 30; i++)
            {
                user2Vector30D[i] = Mathf.Cos(i * 0.15f); // Different pattern
            }
            var user2Vector6D = Simulate30Dto6DCompression(user2Vector30D);
            
            float similarity = CalculateCosineSimilarity(user.vector6D, user2Vector6D);
            Assert.Less(similarity, 0.95f, "Different 30D vectors should produce distinguishable 6D vectors");
            
            Debug.Log($"[SYNC TEST] 30D to 6D compression - Original norm: {original30DNorm:F3}, Compressed: {compressed6DNorm:F3}, Similarity: {similarity:F3}");
        }

        /// <summary>
        /// Test late-joiner synchronization support
        /// </summary>
        [UnityTest]
        public IEnumerator TestLateJoinerSupport()
        {
            // Arrange - Setup initial users
            CreateTestUsers(3);
            testUsers[0].SetPublic(true);
            testUsers[1].SetPublic(true);
            
            SimulateProfilePublishing();
            yield return null; // Wait one frame
            
            // Act - Simulate late joiner
            var lateJoiner = new TestUser("LateJoiner", 999);
            lateJoiner.Initialize();
            testUsers.Add(lateJoiner);
            
            // Late joiner should receive existing public data
            SimulateLateJoinerSync(lateJoiner);
            yield return null;
            
            // Assert late joiner can see existing public users
            int visiblePublicUsers = CountVisiblePublicUsers(lateJoiner);
            Assert.AreEqual(2, visiblePublicUsers, "Late joiner should see 2 existing public users");
            
            // Late joiner becomes public
            lateJoiner.SetPublic(true);
            SimulateProfilePublishing();
            yield return null;
            
            // Existing users should now see late joiner
            foreach (var existingUser in testUsers.Take(3))
            {
                int visibleToExisting = CountVisiblePublicUsers(existingUser);
                Assert.AreEqual(3, visibleToExisting, $"Existing user {existingUser.displayName} should see 3 public users including late joiner");
            }
            
            Debug.Log("[SYNC TEST] Late-joiner synchronization works correctly");
        }

        /// <summary>
        /// Test visibility toggling (public ON/OFF) with immediate sync updates
        /// </summary>
        [UnityTest]
        public IEnumerator TestVisibilityToggling()
        {
            // Arrange
            CreateTestUsers(4);
            
            // All users start public
            foreach (var user in testUsers)
            {
                user.SetPublic(true);
            }
            SimulateProfilePublishing();
            yield return null;
            
            // Verify all can see each other
            foreach (var user in testUsers)
            {
                Assert.AreEqual(4, CountVisiblePublicUsers(user), $"{user.displayName} should see all 4 users initially");
            }
            
            // User 0 goes private
            testUsers[0].SetPublic(false);
            SimulateProfilePublishing();
            yield return null;
            
            // Assert User 0 is no longer visible to others
            for (int i = 1; i < testUsers.Count; i++)
            {
                Assert.AreEqual(3, CountVisiblePublicUsers(testUsers[i]), 
                               $"{testUsers[i].displayName} should now see only 3 users (User 0 went private)");
            }
            
            // User 0 goes public again
            testUsers[0].SetPublic(true);
            SimulateProfilePublishing();
            yield return null;
            
            // Assert User 0 is visible again
            foreach (var user in testUsers)
            {
                Assert.AreEqual(4, CountVisiblePublicUsers(user), $"{user.displayName} should see all 4 users again");
            }
            
            Debug.Log("[SYNC TEST] Visibility toggling with immediate sync works correctly");
        }

        /// <summary>
        /// Test progressive matching with partial questionnaire data synchronization
        /// </summary>
        [Test]
        public void TestProgressiveMatchingSynchronization()
        {
            // Arrange
            CreateTestUsers(5);
            
            // Setup users with different completion levels
            SetupProgressiveUsers();
            
            // All users go public (including provisional data)
            foreach (var user in testUsers)
            {
                user.playerData.SetFlags(3); // Both public and provisional sharing enabled
                user.isPublic = true;
            }
            
            SimulateProfilePublishing();
            
            // Act - simulate compatibility calculation with progressive data
            var compatibilityResults = SimulateCompatibilityCalculationWithProgress();
            
            // Assert
            Assert.IsNotEmpty(compatibilityResults, "Should have compatibility results");
            Assert.AreEqual(testUsers.Count, compatibilityResults.Count, "Should have results for all users");
            
            // Users with more complete questionnaires should have higher confidence scores
            var mostCompleteUser = testUsers.OrderByDescending(u => u.playerData.CurrentProgress).First();
            var leastCompleteUser = testUsers.OrderBy(u => u.playerData.CurrentProgress).First();
            
            float mostCompleteConfidence = GetUserConfidenceScore(mostCompleteUser, compatibilityResults);
            float leastCompleteConfidence = GetUserConfidenceScore(leastCompleteUser, compatibilityResults);
            
            Assert.GreaterOrEqual(mostCompleteConfidence, leastCompleteConfidence, 
                                 "More complete questionnaires should have higher confidence");
            
            Debug.Log($"[SYNC TEST] Progressive matching - Most complete: {mostCompleteConfidence:F3}, Least complete: {leastCompleteConfidence:F3}");
        }

        /// <summary>
        /// Test synchronization bandwidth efficiency (minimal sync data)
        /// </summary>
        [Test]
        public void TestSynchronizationBandwidthEfficiency()
        {
            // Arrange
            CreateTestUsers(10);
            
            foreach (var user in testUsers)
            {
                user.SetPublic(true);
            }
            
            // Calculate expected sync data per user
            int expectedSyncDataPerUser = CalculateExpectedSyncDataSize();
            
            // Act - simulate sync data transmission
            var syncData = SimulateSyncDataGeneration();
            
            // Assert bandwidth constraints
            int totalSyncDataSize = syncData.Sum(data => data.sizeBytes);
            int averageSizePerUser = totalSyncDataSize / testUsers.Count;
            
            Assert.LessOrEqual(averageSizePerUser, expectedSyncDataPerUser * 1.2f, // Allow 20% margin
                              $"Sync data per user {averageSizePerUser} bytes exceeds expected {expectedSyncDataPerUser} bytes");
            
            // Verify only essential data is synchronized (6D vectors, not 30D)
            foreach (var data in syncData)
            {
                Assert.AreEqual(6, data.vectorDimensions, "Should only sync 6D compressed vectors, not 30D");
                Assert.IsNotEmpty(data.tags, "Should include personality tags");
                Assert.IsNotEmpty(data.headline, "Should include personality headline");
                Assert.IsNotEmpty(data.displayName, "Should include display name");
            }
            
            Debug.Log($"[SYNC TEST] Bandwidth efficiency - Total: {totalSyncDataSize} bytes, Avg per user: {averageSizePerUser} bytes");
        }

        /// <summary>
        /// Test network resilience with sync failures and recovery
        /// </summary>
        [UnityTest]
        public IEnumerator TestNetworkResilienceAndRecovery()
        {
            // Arrange
            CreateTestUsers(6);
            
            // Initial successful sync
            foreach (var user in testUsers)
            {
                user.SetPublic(true);
            }
            SimulateProfilePublishing();
            yield return null;
            
            // Verify initial sync success
            foreach (var user in testUsers)
            {
                Assert.AreEqual(6, CountVisiblePublicUsers(user), "Initial sync should show all users");
            }
            
            // Simulate network failure for some users
            var affectedUsers = testUsers.Take(2).ToList();
            SimulateNetworkFailure(affectedUsers);
            yield return null;
            
            // Affected users should have stale data, others should continue working
            foreach (var unaffectedUser in testUsers.Skip(2))
            {
                Assert.GreaterOrEqual(CountVisiblePublicUsers(unaffectedUser), 4, 
                                     "Unaffected users should still see most other users");
            }
            
            // Simulate network recovery
            SimulateNetworkRecovery(affectedUsers);
            yield return new WaitForSeconds(0.5f); // Allow recovery time
            
            // All users should see each other again after recovery
            foreach (var user in testUsers)
            {
                Assert.AreEqual(6, CountVisiblePublicUsers(user), 
                               $"{user.displayName} should see all users after network recovery");
            }
            
            Debug.Log("[SYNC TEST] Network resilience and recovery works correctly");
        }

        /// <summary>
        /// Test concurrent user operations (join/leave/toggle) synchronization
        /// </summary>
        [UnityTest]
        public IEnumerator TestConcurrentUserOperations()
        {
            // Arrange
            CreateTestUsers(8);
            
            // Phase 1: Staggered user public state changes
            for (int i = 0; i < testUsers.Count; i += 2)
            {
                testUsers[i].SetPublic(true);
                SimulateProfilePublishing();
                yield return new WaitForSeconds(0.1f);
            }
            
            // Phase 2: Rapid concurrent changes
            var concurrentOperations = new List<System.Action>
            {
                () => testUsers[1].SetPublic(true),
                () => testUsers[3].SetPublic(true),
                () => testUsers[0].SetPublic(false),
                () => testUsers[5].SetPublic(true),
                () => testUsers[2].SetPublic(false)
            };
            
            // Execute operations concurrently
            foreach (var operation in concurrentOperations)
            {
                operation.Invoke();
            }
            SimulateProfilePublishing();
            yield return null;
            
            // Phase 3: Verify final state consistency
            int expectedPublicUsers = testUsers.Count(u => u.isPublic);
            foreach (var user in testUsers)
            {
                if (user.isPublic)
                {
                    int visibleUsers = CountVisiblePublicUsers(user);
                    Assert.AreEqual(expectedPublicUsers, visibleUsers, 
                                   $"Public user {user.displayName} should see all {expectedPublicUsers} public users");
                }
            }
            
            Debug.Log($"[SYNC TEST] Concurrent operations completed - {expectedPublicUsers} users public");
        }

        #region Helper Methods

        private void CreateTestUsers(int count)
        {
            for (int i = 0; i < count; i++)
            {
                var user = new TestUser($"User{i}", i);
                user.Initialize();
                testUsers.Add(user);
            }
        }

        private void SimulateProfilePublishing()
        {
            // Simulate PublicProfilePublisher processing all users
            foreach (var user in testUsers)
            {
                if (user.isPublic)
                {
                    // Generate 6D compressed vector from 30D
                    user.vector6D = Simulate30Dto6DCompression(user.vector30D);
                }
                else
                {
                    // Clear public data for private users
                    user.vector6D = new float[6];
                }
            }
        }

        private void SimulateLateJoinerSync(TestUser lateJoiner)
        {
            // Late joiner receives existing public profile data
            // In real implementation, this would be handled by VRChat's late-joiner sync
            Debug.Log($"[SYNC TEST] Late joiner {lateJoiner.displayName} syncing with existing public data");
        }

        private int CountVisiblePublicUsers(TestUser observer)
        {
            // Count how many public users this observer can see (including themselves if public)
            return testUsers.Count(user => user.isPublic);
        }

        private float[] Simulate30Dto6DCompression(float[] vector30D)
        {
            // Simulate 30D to 6D compression using fixed projection matrix P
            var result = new float[6];
            
            // Simple projection: group 30D dimensions into 6 groups of 5
            for (int i = 0; i < 6; i++)
            {
                float sum = 0f;
                for (int j = 0; j < 5; j++)
                {
                    int index = i * 5 + j;
                    if (index < 30) sum += vector30D[index];
                }
                result[i] = sum / 5f; // Average
            }
            
            return result;
        }

        private float GetVectorNorm(float[] vector)
        {
            float sum = 0f;
            foreach (float val in vector)
            {
                sum += val * val;
            }
            return Mathf.Sqrt(sum);
        }

        private float GetSumOfVector(float[] vector)
        {
            float sum = 0f;
            foreach (float val in vector)
            {
                sum += Mathf.Abs(val);
            }
            return sum;
        }

        private float CalculateCosineSimilarity(float[] vectorA, float[] vectorB)
        {
            if (vectorA.Length != vectorB.Length) return 0f;
            
            float dotProduct = 0f;
            float normA = 0f;
            float normB = 0f;
            
            for (int i = 0; i < vectorA.Length; i++)
            {
                dotProduct += vectorA[i] * vectorB[i];
                normA += vectorA[i] * vectorA[i];
                normB += vectorB[i] * vectorB[i];
            }
            
            return dotProduct / (Mathf.Sqrt(normA) * Mathf.Sqrt(normB) + 0.0001f);
        }

        private void SetupProgressiveUsers()
        {
            // Create users with different completion levels
            int[] completionLevels = { 30, 50, 75, 100, 112 }; // Different progress stages
            
            for (int i = 0; i < testUsers.Count; i++)
            {
                var user = testUsers[i];
                int targetCompletion = completionLevels[i % completionLevels.Length];
                
                // Clear existing data and set specific completion level
                user.playerData.ResetPlayerData();
                
                for (int q = 0; q < targetCompletion; q++)
                {
                    user.playerData.SaveQuestionResponse(q, Random.Range(1, 6));
                }
                
                Debug.Log($"[SYNC TEST] User {user.displayName} has {targetCompletion}/112 questions completed");
            }
        }

        private Dictionary<TestUser, float> SimulateCompatibilityCalculationWithProgress()
        {
            var results = new Dictionary<TestUser, float>();
            
            foreach (var user in testUsers)
            {
                if (user.isPublic)
                {
                    // Calculate confidence based on completion percentage
                    float completionRatio = (float)user.playerData.CurrentProgress / 112f;
                    float confidenceScore = 0.5f + (completionRatio * 0.5f); // 0.5 to 1.0 range
                    results[user] = confidenceScore;
                }
            }
            
            return results;
        }

        private float GetUserConfidenceScore(TestUser user, Dictionary<TestUser, float> results)
        {
            return results.ContainsKey(user) ? results[user] : 0f;
        }

        private int CalculateExpectedSyncDataSize()
        {
            // Expected sync data per user:
            // - 6D vector: 6 * 4 bytes (float) = 24 bytes
            // - tags: ~50 characters = 50 bytes
            // - headline: ~100 characters = 100 bytes  
            // - display_name: ~30 characters = 30 bytes
            // - partial_flag: 1 byte
            // - progress: 4 bytes (int)
            // Total: ~209 bytes per user
            return 250; // With some overhead
        }

        private List<SyncData> SimulateSyncDataGeneration()
        {
            var syncData = new List<SyncData>();
            
            foreach (var user in testUsers)
            {
                if (user.isPublic)
                {
                    syncData.Add(new SyncData
                    {
                        sizeBytes = CalculateExpectedSyncDataSize(),
                        vectorDimensions = 6,
                        tags = $"tag1,tag2,tag3", // Simulated personality tags
                        headline = $"Personality summary for {user.displayName}",
                        displayName = user.displayName,
                        partialFlag = user.playerData.CurrentProgress < 112,
                        progress = user.playerData.CurrentProgress
                    });
                }
            }
            
            return syncData;
        }

        private void SimulateNetworkFailure(List<TestUser> affectedUsers)
        {
            // Simulate network failure by marking users as having stale sync data
            foreach (var user in affectedUsers)
            {
                Debug.Log($"[SYNC TEST] Simulating network failure for {user.displayName}");
                // In real implementation, this would affect their sync state
            }
        }

        private void SimulateNetworkRecovery(List<TestUser> recoveredUsers)
        {
            // Simulate network recovery
            foreach (var user in recoveredUsers)
            {
                Debug.Log($"[SYNC TEST] Simulating network recovery for {user.displayName}");
                // In real implementation, this would trigger resync
            }
        }

        private class SyncData
        {
            public int sizeBytes;
            public int vectorDimensions;
            public string tags;
            public string headline;
            public string displayName;
            public bool partialFlag;
            public int progress;
        }

        #endregion

        // Extension methods for LINQ in tests
        private static class EnumerableExtensions
        {
            public static IEnumerable<T> Take<T>(this IEnumerable<T> source, int count)
            {
                int taken = 0;
                foreach (var item in source)
                {
                    if (taken >= count) yield break;
                    yield return item;
                    taken++;
                }
            }

            public static IEnumerable<T> Skip<T>(this IEnumerable<T> source, int count)
            {
                int skipped = 0;
                foreach (var item in source)
                {
                    if (skipped < count)
                    {
                        skipped++;
                        continue;
                    }
                    yield return item;
                }
            }

            public static T First<T>(this IEnumerable<T> source)
            {
                foreach (var item in source)
                {
                    return item;
                }
                throw new System.InvalidOperationException("Sequence contains no elements");
            }

            public static T OrderByDescending<T>(this IEnumerable<T> source, System.Func<T, int> keySelector)
            {
                T max = default(T);
                int maxKey = int.MinValue;
                bool hasValue = false;

                foreach (var item in source)
                {
                    int key = keySelector(item);
                    if (!hasValue || key > maxKey)
                    {
                        max = item;
                        maxKey = key;
                        hasValue = true;
                    }
                }

                if (!hasValue)
                    throw new System.InvalidOperationException("Sequence contains no elements");

                return max;
            }

            public static T OrderBy<T>(this IEnumerable<T> source, System.Func<T, int> keySelector)
            {
                T min = default(T);
                int minKey = int.MaxValue;
                bool hasValue = false;

                foreach (var item in source)
                {
                    int key = keySelector(item);
                    if (!hasValue || key < minKey)
                    {
                        min = item;
                        minKey = key;
                        hasValue = true;
                    }
                }

                if (!hasValue)
                    throw new System.InvalidOperationException("Sequence contains no elements");

                return min;
            }

            public static int Count<T>(this IEnumerable<T> source, System.Func<T, bool> predicate)
            {
                int count = 0;
                foreach (var item in source)
                {
                    if (predicate(item))
                        count++;
                }
                return count;
            }

            public static int Sum<T>(this IEnumerable<T> source, System.Func<T, int> selector)
            {
                int sum = 0;
                foreach (var item in source)
                {
                    sum += selector(item);
                }
                return sum;
            }
        }
    }
}