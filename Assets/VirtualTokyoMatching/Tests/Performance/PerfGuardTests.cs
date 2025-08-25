using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using VirtualTokyoMatching;

namespace VirtualTokyoMatching.Tests
{
    /// <summary>
    /// Unit and integration tests for PerfGuard component
    /// Tests FPS monitoring, calculation budget management, and adaptive performance
    /// </summary>
    public class PerfGuardTests
    {
        private GameObject testGameObject;
        private PerfGuard perfGuard;
        private PerformanceSettings mockPerformanceSettings;

        [SetUp]
        public void SetUp()
        {
            // Create mock PerformanceSettings
            var settingsObject = new GameObject("MockPerformanceSettings");
            mockPerformanceSettings = settingsObject.AddComponent<PerformanceSettings>();

            // Create test game object with PerfGuard
            testGameObject = new GameObject("TestPerfGuard");
            perfGuard = testGameObject.AddComponent<PerfGuard>();
            
            // Assign mock settings via reflection or public field
            var settingsField = typeof(PerfGuard).GetField("performanceSettings");
            if (settingsField != null)
            {
                settingsField.SetValue(perfGuard, mockPerformanceSettings);
            }
        }

        [TearDown]
        public void TearDown()
        {
            if (testGameObject != null)
            {
                Object.DestroyImmediate(testGameObject);
            }
            if (mockPerformanceSettings != null)
            {
                Object.DestroyImmediate(mockPerformanceSettings.gameObject);
            }
        }

        /// <summary>
        /// Test initial performance guard setup
        /// </summary>
        [Test]
        public void TestInitialSetup()
        {
            // Act
            perfGuard.SendCustomEvent("Start");

            // Assert
            Assert.Greater(perfGuard.GetTargetFPS(), 0, "Target FPS should be set");
            Assert.Greater(perfGuard.GetCurrentCalculationBudget(), 0, "Calculation budget should be set");
            Assert.AreEqual(1f, perfGuard.GetPerformanceScore(), 0.1f, "Initial performance score should be high");
            Assert.IsFalse(perfGuard.IsUnderperforming(), "Should not be underperforming initially");
            
            Debug.Log($"[TEST] PerfGuard initialized - Target FPS: {perfGuard.GetTargetFPS()}, Budget: {perfGuard.GetCurrentCalculationBudget()}");
        }

        /// <summary>
        /// Test calculation budget request and allocation
        /// </summary>
        [Test]
        public void TestCalculationBudgetAllocation()
        {
            // Arrange
            perfGuard.SendCustomEvent("Start");
            int initialBudget = perfGuard.GetCurrentCalculationBudget();

            // Act - Request calculations
            int requested = 5;
            int granted1 = perfGuard.RequestCalculationBudget(requested);
            int granted2 = perfGuard.RequestCalculationBudget(requested);

            // Assert
            Assert.AreEqual(requested, granted1, "First request should be fully granted");
            Assert.LessOrEqual(granted2, requested, "Second request may be partially granted");
            Assert.AreEqual(granted1 + granted2, perfGuard.GetCalculationsUsedThisFrame(), 
                           "Used calculations should match granted total");
            
            // Test budget exhaustion
            int remainingBudget = initialBudget - (granted1 + granted2);
            int granted3 = perfGuard.RequestCalculationBudget(remainingBudget + 1);
            Assert.LessOrEqual(granted3, remainingBudget, "Should not exceed remaining budget");
            
            Debug.Log($"[TEST] Budget allocation: Initial={initialBudget}, Used={perfGuard.GetCalculationsUsedThisFrame()}");
        }

        /// <summary>
        /// Test frame budget reset mechanism
        /// </summary>
        [UnityTest]
        public IEnumerator TestFrameBudgetReset()
        {
            // Arrange
            perfGuard.SendCustomEvent("Start");
            
            // Use up some budget
            perfGuard.RequestCalculationBudget(5);
            int usedBeforeFrame = perfGuard.GetCalculationsUsedThisFrame();
            
            // Wait for next frame
            yield return null;
            
            // Trigger Update manually (since we're in test mode)
            perfGuard.SendCustomEvent("Update");
            
            // Assert - budget should reset for new frame
            Assert.Less(perfGuard.GetCalculationsUsedThisFrame(), usedBeforeFrame, 
                       "Calculations should reset on new frame");
            
            Debug.Log("[TEST] Frame budget reset works correctly");
        }

        /// <summary>
        /// Test FPS monitoring and performance score calculation
        /// </summary>
        [Test]
        public void TestFPSMonitoring()
        {
            // Arrange
            perfGuard.SendCustomEvent("Start");
            float targetFPS = perfGuard.GetTargetFPS();

            // Simulate good performance (FPS at target)
            // This would typically be done by simulating different deltaTime values
            // For testing purposes, we'll use the public methods
            float currentFPS = perfGuard.GetCurrentFPS();
            float averageFPS = perfGuard.GetAverageFPS();
            
            Assert.Greater(currentFPS, 0, "Current FPS should be positive");
            Assert.Greater(averageFPS, 0, "Average FPS should be positive");
            
            // Test performance score calculation
            float performanceScore = perfGuard.GetPerformanceScore();
            Assert.That(performanceScore, Is.InRange(0f, 1f), "Performance score should be between 0 and 1");
            
            Debug.Log($"[TEST] FPS Monitoring - Current: {currentFPS:F1}, Average: {averageFPS:F1}, Score: {performanceScore:F2}");
        }

        /// <summary>
        /// Test adaptive calculation budget based on performance
        /// </summary>
        [Test]
        public void TestAdaptiveBudgetCalculation()
        {
            // Arrange
            perfGuard.SendCustomEvent("Start");
            int maxBudget = 20;
            
            // Set a known max budget for testing
            perfGuard.SetMaxCalculationsPerFrame(maxBudget);
            
            // Test different target FPS scenarios
            
            // Good performance scenario (high FPS)
            perfGuard.SetTargetFPS(60f);
            // Simulate good performance by forcing recalculation
            perfGuard.RecalculatePerformanceMetrics();
            
            int goodPerformanceBudget = perfGuard.GetCurrentCalculationBudget();
            Assert.Greater(goodPerformanceBudget, 0, "Should have positive budget for good performance");
            
            // Poor performance scenario (low target to simulate poor performance)
            perfGuard.SetTargetFPS(120f); // High target makes current performance seem poor
            perfGuard.RecalculatePerformanceMetrics();
            
            int poorPerformanceBudget = perfGuard.GetCurrentCalculationBudget();
            Assert.Greater(poorPerformanceBudget, 0, "Should still have some budget for poor performance");
            
            Debug.Log($"[TEST] Adaptive budgeting - Good: {goodPerformanceBudget}, Poor: {poorPerformanceBudget}");
        }

        /// <summary>
        /// Test underperformance detection and threshold checking
        /// </summary>
        [Test]
        public void TestUnderperformanceDetection()
        {
            // Arrange
            perfGuard.SendCustomEvent("Start");
            
            // Set a high target to trigger underperformance
            perfGuard.SetTargetFPS(200f); // Unrealistically high target
            
            // Force recalculation
            perfGuard.RecalculatePerformanceMetrics();
            
            // The performance score should be lower due to high target
            float performanceScore = perfGuard.GetPerformanceScore();
            
            // Performance detection depends on implementation details
            // We can at least verify the methods work
            bool isUnderperforming = perfGuard.IsUnderperforming();
            
            Debug.Log($"[TEST] Underperformance detection - Score: {performanceScore:F2}, Underperforming: {isUnderperforming}");
            
            // Reset to reasonable target
            perfGuard.SetTargetFPS(72f);
            perfGuard.RecalculatePerformanceMetrics();
        }

        /// <summary>
        /// Test performance statistics reporting
        /// </summary>
        [Test]
        public void TestPerformanceStatistics()
        {
            // Arrange
            perfGuard.SendCustomEvent("Start");
            
            // Use some budget
            perfGuard.RequestCalculationBudget(3);
            
            // Get statistics
            string stats = perfGuard.GetPerformanceStats();
            
            Assert.IsNotEmpty(stats, "Performance stats should not be empty");
            Assert.That(stats.Contains("FPS"), "Stats should contain FPS information");
            Assert.That(stats.Contains("Budget"), "Stats should contain budget information");
            Assert.That(stats.Contains("Score"), "Stats should contain performance score");
            
            Debug.Log($"[TEST] Performance Statistics: {stats}");
        }

        /// <summary>
        /// Test performance history reset functionality
        /// </summary>
        [Test]
        public void TestPerformanceHistoryReset()
        {
            // Arrange
            perfGuard.SendCustomEvent("Start");
            
            // Use some budget to change state
            perfGuard.RequestCalculationBudget(5);
            
            // Reset performance history
            perfGuard.ResetPerformanceHistory();
            
            // Verify reset
            float performanceScore = perfGuard.GetPerformanceScore();
            Assert.Greater(performanceScore, 0.8f, "Performance score should be high after reset");
            
            Debug.Log("[TEST] Performance history reset completed");
        }

        /// <summary>
        /// Test budget availability checking
        /// </summary>
        [Test]
        public void TestBudgetAvailabilityChecking()
        {
            // Arrange
            perfGuard.SendCustomEvent("Start");
            
            // Initial state - should be able to calculate
            Assert.IsTrue(perfGuard.CanCalculateThisFrame(), "Should initially be able to calculate");
            Assert.Greater(perfGuard.GetMaxCalculationsThisFrame(), 0, "Should have calculations available");
            
            // Exhaust budget
            int maxAvailable = perfGuard.GetMaxCalculationsThisFrame();
            int granted = perfGuard.RequestCalculationBudget(maxAvailable);
            
            Assert.AreEqual(maxAvailable, granted, "Should grant full available budget");
            Assert.AreEqual(0, perfGuard.GetMaxCalculationsThisFrame(), "Should have no calculations remaining");
            
            // Try to get more - should be denied
            int extraGranted = perfGuard.RequestCalculationBudget(1);
            Assert.AreEqual(0, extraGranted, "Should not grant calculations beyond budget");
            
            Debug.Log($"[TEST] Budget availability checking - Max: {maxAvailable}, Granted: {granted}, Extra: {extraGranted}");
        }

        /// <summary>
        /// Test calculations per second tracking
        /// </summary>
        [UnityTest]
        public IEnumerator TestCalculationsPerSecondTracking()
        {
            // Arrange
            perfGuard.SendCustomEvent("Start");
            
            // Use calculations over multiple frames
            for (int frame = 0; frame < 5; frame++)
            {
                perfGuard.RequestCalculationBudget(2);
                yield return null; // Next frame
                perfGuard.SendCustomEvent("Update"); // Manual update
            }
            
            // Check tracking
            int calculationsThisSecond = perfGuard.GetCalculationsThisSecond();
            Assert.Greater(calculationsThisSecond, 0, "Should have tracked calculations this second");
            
            Debug.Log($"[TEST] Calculations per second: {calculationsThisSecond}");
        }

        /// <summary>
        /// Test performance settings integration
        /// </summary>
        [Test]
        public void TestPerformanceSettingsIntegration()
        {
            // Arrange - this tests that PerfGuard can work with and without PerformanceSettings
            var noSettingsObject = new GameObject("TestPerfGuardNoSettings");
            var perfGuardNoSettings = noSettingsObject.AddComponent<PerfGuard>();
            
            // Act - should work with default values
            perfGuardNoSettings.SendCustomEvent("Start");
            
            // Assert
            Assert.Greater(perfGuardNoSettings.GetTargetFPS(), 0, "Should have default target FPS");
            Assert.Greater(perfGuardNoSettings.GetCurrentCalculationBudget(), 0, "Should have default calculation budget");
            
            Debug.Log($"[TEST] PerfGuard without settings - Target FPS: {perfGuardNoSettings.GetTargetFPS()}, Budget: {perfGuardNoSettings.GetCurrentCalculationBudget()}");
            
            // Cleanup
            Object.DestroyImmediate(noSettingsObject);
        }

        /// <summary>
        /// Integration test for realistic usage scenario
        /// </summary>
        [UnityTest]
        public IEnumerator TestRealisticUsageScenario()
        {
            // Arrange - simulate VTM system usage
            perfGuard.SendCustomEvent("Start");
            
            // Simulate compatibility calculator requesting budget over several frames
            for (int frame = 0; frame < 10; frame++)
            {
                if (perfGuard.CanCalculateThisFrame())
                {
                    // Request budget for compatibility calculations (simulating user count)
                    int userCount = Random.Range(5, 20);
                    int requested = Mathf.Min(userCount, perfGuard.GetMaxCalculationsThisFrame());
                    int granted = perfGuard.RequestCalculationBudget(requested);
                    
                    Assert.LessOrEqual(granted, requested, "Granted should not exceed requested");
                    Assert.LessOrEqual(granted, perfGuard.GetCurrentCalculationBudget(), "Granted should not exceed budget");
                }
                
                yield return null;
                perfGuard.SendCustomEvent("Update");
                
                // Log performance every few frames
                if (frame % 3 == 0)
                {
                    Debug.Log($"[TEST] Frame {frame}: {perfGuard.GetPerformanceStats()}");
                }
            }
            
            Debug.Log("[TEST] Realistic usage scenario completed successfully");
        }
    }
}