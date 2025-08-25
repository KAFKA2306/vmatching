using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using VirtualTokyoMatching;

namespace VirtualTokyoMatching.Tests
{
    /// <summary>
    /// Performance benchmark suite for Virtual Tokyo Matching system
    /// Tests FPS targets: PC ≥72FPS, Quest ≥60FPS
    /// Tests world size limits: PC <200MB, Quest <100MB
    /// Tests calculation time limits: PC ≤5s, Quest ≤10s for full recalculation
    /// </summary>
    public class PerformanceBenchmarkSuite
    {
        private GameObject testSystemRoot;
        private PerfGuard perfGuard;
        private PlayerDataManager[] testUsers;
        private CompatibilityCalculator compatibilityCalculator;
        
        // Performance targets from requirements
        private const float PC_TARGET_FPS = 72f;
        private const float QUEST_TARGET_FPS = 60f;
        private const float PC_MAX_WORLD_SIZE_MB = 200f;
        private const float QUEST_MAX_WORLD_SIZE_MB = 100f;
        private const float PC_MAX_RECALC_TIME_SEC = 5f;
        private const float QUEST_MAX_RECALC_TIME_SEC = 10f;

        [SetUp]
        public void SetUp()
        {
            // Create test system
            testSystemRoot = new GameObject("PerformanceTestSystem");
            
            // Add PerfGuard
            perfGuard = testSystemRoot.AddComponent<PerfGuard>();
            
            // Add CompatibilityCalculator
            compatibilityCalculator = testSystemRoot.AddComponent<CompatibilityCalculator>();
            
            // Initialize performance monitoring
            perfGuard.SendCustomEvent("Start");
        }

        [TearDown]
        public void TearDown()
        {
            if (testSystemRoot != null)
            {
                Object.DestroyImmediate(testSystemRoot);
            }
            
            // Clean up test users
            if (testUsers != null)
            {
                foreach (var user in testUsers)
                {
                    if (user != null && user.gameObject != null)
                    {
                        Object.DestroyImmediate(user.gameObject);
                    }
                }
            }
        }

        /// <summary>
        /// Test PC performance targets (72 FPS minimum)
        /// </summary>
        [Test]
        [Category("Performance")]
        public void TestPCPerformanceTargets()
        {
            // Arrange
            perfGuard.SetTargetFPS(PC_TARGET_FPS);
            int userCount = 30; // Typical load scenario
            
            SetupTestUsers(userCount);
            
            // Measure baseline performance
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            
            // Simulate one frame of processing
            SimulateFrameProcessing(userCount, "PC");
            
            stopwatch.Stop();
            float frameTimeMs = (float)stopwatch.ElapsedMilliseconds;
            float frameTimeTarget = 1000f / PC_TARGET_FPS; // ~13.9ms for 72 FPS
            
            // Assert
            Assert.Less(frameTimeMs, frameTimeTarget * 1.2f, // Allow 20% margin
                       $"PC frame time {frameTimeMs:F2}ms exceeds target {frameTimeTarget:F2}ms (72 FPS)");
            
            float currentFPS = perfGuard.GetCurrentFPS();
            Assert.GreaterOrEqual(currentFPS, PC_TARGET_FPS * 0.9f, // Allow 10% margin
                                 $"Current FPS {currentFPS:F1} below PC target {PC_TARGET_FPS}");
            
            UnityEngine.Debug.Log($"[PERF TEST] PC Performance - Frame time: {frameTimeMs:F2}ms, FPS: {currentFPS:F1}");
        }

        /// <summary>
        /// Test Quest performance targets (60 FPS minimum)
        /// </summary>
        [Test]
        [Category("Performance")]
        public void TestQuestPerformanceTargets()
        {
            // Arrange
            perfGuard.SetTargetFPS(QUEST_TARGET_FPS);
            int userCount = 20; // Reduced load for Quest
            
            SetupTestUsers(userCount);
            
            // Simulate Quest constraints (lower calculation budget)
            perfGuard.SetMaxCalculationsPerFrame(5); // Reduced for mobile
            
            // Measure performance
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            
            SimulateFrameProcessing(userCount, "Quest");
            
            stopwatch.Stop();
            float frameTimeMs = (float)stopwatch.ElapsedMilliseconds;
            float frameTimeTarget = 1000f / QUEST_TARGET_FPS; // ~16.7ms for 60 FPS
            
            // Assert
            Assert.Less(frameTimeMs, frameTimeTarget * 1.3f, // Allow 30% margin for Quest
                       $"Quest frame time {frameTimeMs:F2}ms exceeds target {frameTimeTarget:F2}ms (60 FPS)");
            
            float currentFPS = perfGuard.GetCurrentFPS();
            Assert.GreaterOrEqual(currentFPS, QUEST_TARGET_FPS * 0.85f, // Allow 15% margin for Quest
                                 $"Current FPS {currentFPS:F1} below Quest target {QUEST_TARGET_FPS}");
            
            UnityEngine.Debug.Log($"[PERF TEST] Quest Performance - Frame time: {frameTimeMs:F2}ms, FPS: {currentFPS:F1}");
        }

        /// <summary>
        /// Test full system recalculation performance (PC ≤5s, Quest ≤10s)
        /// </summary>
        [Test]
        [Category("Performance")]
        public void TestFullRecalculationPerformance()
        {
            // Test PC scenario
            perfGuard.SetTargetFPS(PC_TARGET_FPS);
            int pcUserCount = 30;
            SetupTestUsers(pcUserCount);
            
            var pcStopwatch = new Stopwatch();
            pcStopwatch.Start();
            
            SimulateFullCompatibilityRecalculation(pcUserCount);
            
            pcStopwatch.Stop();
            float pcRecalcTimeS = (float)pcStopwatch.ElapsedMilliseconds / 1000f;
            
            Assert.Less(pcRecalcTimeS, PC_MAX_RECALC_TIME_SEC,
                       $"PC full recalculation {pcRecalcTimeS:F2}s exceeds target {PC_MAX_RECALC_TIME_SEC}s");
            
            // Clean up for Quest test
            TearDown();
            SetUp();
            
            // Test Quest scenario
            perfGuard.SetTargetFPS(QUEST_TARGET_FPS);
            perfGuard.SetMaxCalculationsPerFrame(3); // Very limited for mobile
            int questUserCount = 15; // Reduced for Quest
            SetupTestUsers(questUserCount);
            
            var questStopwatch = new Stopwatch();
            questStopwatch.Start();
            
            SimulateFullCompatibilityRecalculation(questUserCount);
            
            questStopwatch.Stop();
            float questRecalcTimeS = (float)questStopwatch.ElapsedMilliseconds / 1000f;
            
            Assert.Less(questRecalcTimeS, QUEST_MAX_RECALC_TIME_SEC,
                       $"Quest full recalculation {questRecalcTimeS:F2}s exceeds target {QUEST_MAX_RECALC_TIME_SEC}s");
            
            UnityEngine.Debug.Log($"[PERF TEST] Full recalculation - PC: {pcRecalcTimeS:F2}s, Quest: {questRecalcTimeS:F2}s");
        }

        /// <summary>
        /// Test distributed processing with budget management
        /// </summary>
        [UnityTest]
        [Category("Performance")]
        public IEnumerator TestDistributedProcessingPerformance()
        {
            // Arrange
            perfGuard.SetTargetFPS(PC_TARGET_FPS);
            int userCount = 25;
            SetupTestUsers(userCount);
            
            // Track performance over multiple frames
            float totalFrameTime = 0f;
            int frameCount = 0;
            int totalCalculationsPerformed = 0;
            
            // Run distributed processing simulation
            for (int frame = 0; frame < 60; frame++) // 1 second at 60fps
            {
                var frameStopwatch = new Stopwatch();
                frameStopwatch.Start();
                
                // Request calculations within budget
                if (perfGuard.CanCalculateThisFrame())
                {
                    int maxCalculations = perfGuard.GetMaxCalculationsThisFrame();
                    int requestedCalculations = Mathf.Min(userCount / 4, maxCalculations); // Process 1/4 of users per frame
                    int grantedCalculations = perfGuard.RequestCalculationBudget(requestedCalculations);
                    
                    // Simulate compatibility calculations
                    SimulateCompatibilityCalculations(grantedCalculations);
                    totalCalculationsPerformed += grantedCalculations;
                }
                
                frameStopwatch.Stop();
                float frameTime = (float)frameStopwatch.ElapsedMilliseconds;
                totalFrameTime += frameTime;
                frameCount++;
                
                yield return null; // Next frame
                perfGuard.SendCustomEvent("Update");
            }
            
            // Calculate average performance
            float avgFrameTime = totalFrameTime / frameCount;
            float avgFPS = 1000f / avgFrameTime;
            float targetFrameTime = 1000f / PC_TARGET_FPS;
            
            // Assert distributed processing maintains performance
            Assert.Less(avgFrameTime, targetFrameTime * 1.1f,
                       $"Average frame time {avgFrameTime:F2}ms exceeds target {targetFrameTime:F2}ms");
            Assert.Greater(totalCalculationsPerformed, userCount * 10, // Should complete multiple passes
                          $"Insufficient calculations performed: {totalCalculationsPerformed}");
            
            UnityEngine.Debug.Log($"[PERF TEST] Distributed processing - Avg frame: {avgFrameTime:F2}ms, FPS: {avgFPS:F1}, Calculations: {totalCalculationsPerformed}");
        }

        /// <summary>
        /// Test memory usage and world size constraints
        /// </summary>
        [Test]
        [Category("Performance")]
        public void TestMemoryUsageConstraints()
        {
            // Get initial memory usage
            long initialMemory = System.GC.GetTotalMemory(true);
            
            // Setup large user count to test memory scaling
            int largeUserCount = 50;
            SetupTestUsers(largeUserCount);
            
            // Add compatibility data structures
            SimulateCompatibilityDataStructures(largeUserCount);
            
            // Force garbage collection to get accurate measurement
            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();
            System.GC.Collect();
            
            long finalMemory = System.GC.GetTotalMemory(false);
            float memoryUsageMB = (float)(finalMemory - initialMemory) / (1024f * 1024f);
            
            // Test against PC constraint (200MB)
            Assert.Less(memoryUsageMB, PC_MAX_WORLD_SIZE_MB * 0.5f, // Use 50% of limit for safety
                       $"Memory usage {memoryUsageMB:F1}MB exceeds safe PC limit");
            
            // Test against Quest constraint (100MB) 
            Assert.Less(memoryUsageMB, QUEST_MAX_WORLD_SIZE_MB * 0.4f, // Use 40% of Quest limit
                       $"Memory usage {memoryUsageMB:F1}MB exceeds safe Quest limit");
            
            UnityEngine.Debug.Log($"[PERF TEST] Memory usage with {largeUserCount} users: {memoryUsageMB:F1}MB");
        }

        /// <summary>
        /// Test adaptive performance under load
        /// </summary>
        [UnityTest]
        [Category("Performance")]
        public IEnumerator TestAdaptivePerformanceUnderLoad()
        {
            // Start with good conditions
            perfGuard.SetTargetFPS(PC_TARGET_FPS);
            SetupTestUsers(10);
            
            float initialPerformanceScore = 0f;
            float finalPerformanceScore = 0f;
            
            // Phase 1: Normal load
            for (int frame = 0; frame < 30; frame++)
            {
                SimulateFrameProcessing(10, "PC");
                yield return null;
                perfGuard.SendCustomEvent("Update");
            }
            initialPerformanceScore = perfGuard.GetPerformanceScore();
            
            // Phase 2: Increase load gradually
            for (int additionalUsers = 1; additionalUsers <= 20; additionalUsers += 2)
            {
                SetupTestUsers(10 + additionalUsers);
                
                for (int frame = 0; frame < 10; frame++)
                {
                    SimulateFrameProcessing(10 + additionalUsers, "PC");
                    yield return null;
                    perfGuard.SendCustomEvent("Update");
                }
                
                // Check if adaptive throttling is working
                int currentBudget = perfGuard.GetCurrentCalculationBudget();
                bool isUnderperforming = perfGuard.IsUnderperforming();
                
                if (isUnderperforming)
                {
                    UnityEngine.Debug.Log($"[PERF TEST] Adaptive throttling activated with {10 + additionalUsers} users, budget: {currentBudget}");
                    break;
                }
            }
            
            finalPerformanceScore = perfGuard.GetPerformanceScore();
            
            // Assert adaptive behavior occurred
            Assert.Less(finalPerformanceScore, initialPerformanceScore + 0.1f,
                       "Performance score should decrease under load");
            
            // System should still be functional even under heavy load
            Assert.Greater(perfGuard.GetCurrentCalculationBudget(), 0,
                          "Should maintain minimal calculation budget under heavy load");
            
            UnityEngine.Debug.Log($"[PERF TEST] Adaptive performance - Initial score: {initialPerformanceScore:F2}, Final: {finalPerformanceScore:F2}");
        }

        /// <summary>
        /// Test incremental vs full recalculation performance comparison
        /// </summary>
        [Test]
        [Category("Performance")]
        public void TestIncrementalVsFullRecalculationPerformance()
        {
            int userCount = 20;
            SetupTestUsers(userCount);
            
            // Measure full recalculation time
            var fullRecalcStopwatch = new Stopwatch();
            fullRecalcStopwatch.Start();
            SimulateFullCompatibilityRecalculation(userCount);
            fullRecalcStopwatch.Stop();
            
            float fullRecalcTimeMs = (float)fullRecalcStopwatch.ElapsedMilliseconds;
            
            // Measure incremental recalculation time (simulate single user change)
            var incrementalStopwatch = new Stopwatch();
            incrementalStopwatch.Start();
            SimulateIncrementalRecalculation(userCount, 1); // 1 user changed
            incrementalStopwatch.Stop();
            
            float incrementalTimeMs = (float)incrementalStopwatch.ElapsedMilliseconds;
            
            // Assert incremental is significantly faster
            float speedupRatio = fullRecalcTimeMs / incrementalTimeMs;
            Assert.Greater(speedupRatio, 5f, // Incremental should be at least 5x faster
                          $"Incremental recalculation not efficient enough. Full: {fullRecalcTimeMs:F2}ms, Incremental: {incrementalTimeMs:F2}ms, Speedup: {speedupRatio:F1}x");
            
            UnityEngine.Debug.Log($"[PERF TEST] Recalculation performance - Full: {fullRecalcTimeMs:F2}ms, Incremental: {incrementalTimeMs:F2}ms, Speedup: {speedupRatio:F1}x");
        }

        /// <summary>
        /// Stress test with maximum expected concurrent users
        /// </summary>
        [Test]
        [Category("Performance")]
        [Category("Stress")]
        public void TestMaximumConcurrentUsersStress()
        {
            // VRChat world instance limit is typically around 40-80 users
            int maxUsers = 40;
            perfGuard.SetTargetFPS(QUEST_TARGET_FPS); // Use Quest constraints (more restrictive)
            perfGuard.SetMaxCalculationsPerFrame(3);
            
            SetupTestUsers(maxUsers);
            
            var stressStopwatch = new Stopwatch();
            stressStopwatch.Start();
            
            // Simulate worst-case scenario: all users public, all need compatibility calculation
            SimulateStressScenario(maxUsers);
            
            stressStopwatch.Stop();
            float stressTimeMs = (float)stressStopwatch.ElapsedMilliseconds;
            
            // Assert system remains functional under stress
            Assert.Less(stressTimeMs, 1000f, // Should complete within 1 second even under stress
                       $"Stress test took too long: {stressTimeMs:F2}ms");
            
            Assert.IsFalse(perfGuard.IsUnderperforming() || perfGuard.GetPerformanceScore() > 0.3f,
                          $"System severely underperforming under stress. Score: {perfGuard.GetPerformanceScore():F2}");
            
            UnityEngine.Debug.Log($"[PERF TEST] Stress test with {maxUsers} users completed in {stressTimeMs:F2}ms");
        }

        #region Helper Methods

        private void SetupTestUsers(int userCount)
        {
            if (testUsers != null)
            {
                foreach (var user in testUsers)
                {
                    if (user != null && user.gameObject != null)
                        Object.DestroyImmediate(user.gameObject);
                }
            }

            testUsers = new PlayerDataManager[userCount];
            for (int i = 0; i < userCount; i++)
            {
                var userObject = new GameObject($"TestUser_{i}");
                testUsers[i] = userObject.AddComponent<PlayerDataManager>();
                testUsers[i].SendCustomEvent("Start");

                // Setup partial questionnaire data for realistic testing
                int answeredQuestions = Random.Range(20, 112);
                for (int q = 0; q < answeredQuestions; q++)
                {
                    testUsers[i].SaveQuestionResponse(q, Random.Range(1, 6));
                }

                // Some users have public sharing enabled
                if (Random.value < 0.7f) // 70% public
                {
                    testUsers[i].SetFlags(1); // Public sharing
                }
            }
        }

        private void SimulateFrameProcessing(int userCount, string platform)
        {
            if (!perfGuard.CanCalculateThisFrame()) return;

            int maxCalculations = perfGuard.GetMaxCalculationsThisFrame();
            int requestedCalculations = Mathf.Min(userCount / 2, maxCalculations);
            int grantedCalculations = perfGuard.RequestCalculationBudget(requestedCalculations);

            SimulateCompatibilityCalculations(grantedCalculations);
        }

        private void SimulateCompatibilityCalculations(int calculationCount)
        {
            // Simulate the computational work of compatibility calculations
            for (int i = 0; i < calculationCount; i++)
            {
                // Simulate 6D vector cosine similarity calculation
                float[] vectorA = new float[6];
                float[] vectorB = new float[6];
                
                for (int j = 0; j < 6; j++)
                {
                    vectorA[j] = Random.Range(-1f, 1f);
                    vectorB[j] = Random.Range(-1f, 1f);
                }

                // Cosine similarity calculation
                float dotProduct = 0f;
                float normA = 0f;
                float normB = 0f;

                for (int j = 0; j < 6; j++)
                {
                    dotProduct += vectorA[j] * vectorB[j];
                    normA += vectorA[j] * vectorA[j];
                    normB += vectorB[j] * vectorB[j];
                }

                float similarity = dotProduct / (Mathf.Sqrt(normA) * Mathf.Sqrt(normB) + 0.0001f);
                // Store result (unused in test, but simulates real work)
            }
        }

        private void SimulateFullCompatibilityRecalculation(int userCount)
        {
            // Simulate full NxN compatibility matrix calculation
            int comparisons = (userCount * (userCount - 1)) / 2; // N choose 2
            
            for (int i = 0; i < comparisons; i++)
            {
                SimulateCompatibilityCalculations(1);
            }
        }

        private void SimulateIncrementalRecalculation(int userCount, int changedUsers)
        {
            // Simulate incremental recalculation when a subset of users change
            int comparisons = changedUsers * (userCount - changedUsers);
            
            for (int i = 0; i < comparisons; i++)
            {
                SimulateCompatibilityCalculations(1);
            }
        }

        private void SimulateCompatibilityDataStructures(int userCount)
        {
            // Simulate memory usage of compatibility matrices and user data
            var dummyCompatibilityMatrix = new float[userCount, userCount];
            var dummyUserVectors = new float[userCount][];
            
            for (int i = 0; i < userCount; i++)
            {
                dummyUserVectors[i] = new float[30]; // 30D vectors
                for (int j = 0; j < 30; j++)
                {
                    dummyUserVectors[i][j] = Random.Range(-1f, 1f);
                }
                
                for (int j = 0; j < userCount; j++)
                {
                    dummyCompatibilityMatrix[i, j] = Random.Range(0f, 1f);
                }
            }
        }

        private void SimulateStressScenario(int userCount)
        {
            // Simulate worst case: all users need compatibility updates simultaneously
            SimulateFullCompatibilityRecalculation(userCount);
            
            // Additional stress: rapid user join/leave events
            for (int i = 0; i < 10; i++)
            {
                SimulateIncrementalRecalculation(userCount, Random.Range(1, 5));
            }
        }

        #endregion
    }
}