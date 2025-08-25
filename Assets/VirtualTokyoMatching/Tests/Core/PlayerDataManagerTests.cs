using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using VirtualTokyoMatching;

namespace VirtualTokyoMatching.Tests
{
    /// <summary>
    /// Unit and integration tests for PlayerDataManager component
    /// Tests data persistence, retry mechanisms, and progressive data management
    /// </summary>
    public class PlayerDataManagerTests
    {
        private GameObject testGameObject;
        private PlayerDataManager playerDataManager;

        [SetUp]
        public void SetUp()
        {
            // Create test game object with PlayerDataManager
            testGameObject = new GameObject("TestPlayerDataManager");
            playerDataManager = testGameObject.AddComponent<PlayerDataManager>();
        }

        [TearDown]
        public void TearDown()
        {
            if (testGameObject != null)
            {
                Object.DestroyImmediate(testGameObject);
            }
        }

        /// <summary>
        /// Test initial data loading and default values
        /// </summary>
        [Test]
        public void TestInitialDataLoading()
        {
            // Act - PlayerDataManager should initialize automatically in Start()
            
            // Wait for initialization (simulated)
            playerDataManager.SendCustomEvent("Start");
            
            // Assert
            Assert.AreEqual(0, playerDataManager.CurrentProgress, "Initial progress should be 0");
            Assert.AreEqual(0, playerDataManager.Flags, "Initial flags should be 0");
            Assert.IsFalse(playerDataManager.IsPublicSharingEnabled, "Public sharing should be disabled initially");
            Assert.IsFalse(playerDataManager.IsProvisionalSharingEnabled, "Provisional sharing should be disabled initially");
            
            Debug.Log("[TEST] PlayerDataManager initialized correctly with default values");
        }

        /// <summary>
        /// Test saving and retrieving individual question responses
        /// </summary>
        [Test]
        public void TestQuestionResponseSaving()
        {
            // Arrange
            playerDataManager.SendCustomEvent("Start");
            int questionIndex = 5;
            int response = 3;

            // Act
            playerDataManager.SaveQuestionResponse(questionIndex, response);

            // Assert
            Assert.AreEqual(response, playerDataManager.GetQuestionResponse(questionIndex), 
                           $"Question {questionIndex} should have response {response}");
            Assert.AreEqual(1, playerDataManager.CurrentProgress, "Progress should increment after saving first answer");
            
            Debug.Log($"[TEST] Successfully saved and retrieved Q{questionIndex + 1} = {response}");
        }

        /// <summary>
        /// Test progressive questionnaire completion tracking
        /// </summary>
        [Test]
        public void TestProgressiveCompletion()
        {
            // Arrange
            playerDataManager.SendCustomEvent("Start");

            // Act - Answer first 10 questions
            for (int i = 0; i < 10; i++)
            {
                playerDataManager.SaveQuestionResponse(i, 4); // Answer with rating 4
            }

            // Assert
            Assert.AreEqual(10, playerDataManager.CurrentProgress, "Progress should be 10 after answering 10 questions");
            Assert.AreEqual(0.089f, playerDataManager.GetCompletionPercentage(), 0.01f, "Completion percentage should be ~8.9%");
            Assert.IsFalse(playerDataManager.IsAssessmentComplete(), "Assessment should not be complete");
            Assert.AreEqual(10, playerDataManager.GetNextUnansweredQuestion(), "Next unanswered should be question 10 (0-indexed)");
            
            Debug.Log("[TEST] Progressive completion tracking works correctly");
        }

        /// <summary>
        /// Test boundary conditions for question responses
        /// </summary>
        [Test]
        public void TestQuestionResponseBoundaries()
        {
            // Arrange
            playerDataManager.SendCustomEvent("Start");

            // Test invalid question indices
            playerDataManager.SaveQuestionResponse(-1, 3); // Should be ignored
            playerDataManager.SaveQuestionResponse(112, 3); // Should be ignored (max is 111)
            
            // Test invalid response values
            playerDataManager.SaveQuestionResponse(0, 0); // Should be ignored (min is 1)
            playerDataManager.SaveQuestionResponse(0, 6); // Should be ignored (max is 5)

            // Assert
            Assert.AreEqual(0, playerDataManager.CurrentProgress, "Progress should remain 0 after invalid saves");
            Assert.AreEqual(0, playerDataManager.GetQuestionResponse(0), "Question 0 should remain unanswered");

            // Test valid boundaries
            playerDataManager.SaveQuestionResponse(0, 1); // Min valid response
            playerDataManager.SaveQuestionResponse(111, 5); // Max valid question and response

            Assert.AreEqual(2, playerDataManager.CurrentProgress, "Progress should be 2 after valid saves");
            Assert.AreEqual(1, playerDataManager.GetQuestionResponse(0), "Question 0 should have response 1");
            Assert.AreEqual(5, playerDataManager.GetQuestionResponse(111), "Question 111 should have response 5");
            
            Debug.Log("[TEST] Boundary condition validation works correctly");
        }

        /// <summary>
        /// Test 30D vector data management
        /// </summary>
        [Test]
        public void TestVector30DManagement()
        {
            // Arrange
            playerDataManager.SendCustomEvent("Start");
            float[] testVector = new float[30];
            for (int i = 0; i < 30; i++)
            {
                testVector[i] = (float)i / 30f; // Values from 0 to ~0.97
            }

            // Act
            playerDataManager.UpdateVector30D(testVector);
            float[] retrievedVector = playerDataManager.GetVector30D();

            // Assert
            Assert.AreEqual(30, retrievedVector.Length, "Retrieved vector should have 30 elements");
            for (int i = 0; i < 30; i++)
            {
                Assert.AreEqual(testVector[i], retrievedVector[i], 0.001f, $"Vector element {i} should match");
            }

            // Test invalid inputs
            playerDataManager.UpdateVector30D(null); // Should be ignored
            playerDataManager.UpdateVector30D(new float[29]); // Wrong size, should be ignored
            float[] vectorAfterInvalid = playerDataManager.GetVector30D();
            
            for (int i = 0; i < 30; i++)
            {
                Assert.AreEqual(testVector[i], vectorAfterInvalid[i], 0.001f, 
                               $"Vector element {i} should be unchanged after invalid updates");
            }
            
            Debug.Log("[TEST] 30D vector management works correctly");
        }

        /// <summary>
        /// Test flags management (public sharing, provisional sharing)
        /// </summary>
        [Test]
        public void TestFlagsManagement()
        {
            // Arrange
            playerDataManager.SendCustomEvent("Start");

            // Test public sharing flag (bit 0)
            playerDataManager.SetFlags(1); // Binary: 001
            Assert.IsTrue(playerDataManager.IsPublicSharingEnabled, "Public sharing should be enabled");
            Assert.IsFalse(playerDataManager.IsProvisionalSharingEnabled, "Provisional sharing should still be disabled");

            // Test provisional sharing flag (bit 1)
            playerDataManager.SetFlags(2); // Binary: 010
            Assert.IsFalse(playerDataManager.IsPublicSharingEnabled, "Public sharing should be disabled");
            Assert.IsTrue(playerDataManager.IsProvisionalSharingEnabled, "Provisional sharing should be enabled");

            // Test both flags
            playerDataManager.SetFlags(3); // Binary: 011
            Assert.IsTrue(playerDataManager.IsPublicSharingEnabled, "Public sharing should be enabled");
            Assert.IsTrue(playerDataManager.IsProvisionalSharingEnabled, "Provisional sharing should be enabled");

            // Test clearing flags
            playerDataManager.SetFlags(0); // Binary: 000
            Assert.IsFalse(playerDataManager.IsPublicSharingEnabled, "Public sharing should be disabled");
            Assert.IsFalse(playerDataManager.IsProvisionalSharingEnabled, "Provisional sharing should be disabled");
            
            Debug.Log("[TEST] Flags management works correctly");
        }

        /// <summary>
        /// Test complete assessment detection
        /// </summary>
        [Test]
        public void TestCompleteAssessmentDetection()
        {
            // Arrange
            playerDataManager.SendCustomEvent("Start");

            // Answer all 112 questions
            for (int i = 0; i < 112; i++)
            {
                playerDataManager.SaveQuestionResponse(i, (i % 5) + 1); // Cycle through responses 1-5
            }

            // Assert
            Assert.AreEqual(112, playerDataManager.CurrentProgress, "Progress should be 112");
            Assert.AreEqual(1.0f, playerDataManager.GetCompletionPercentage(), 0.001f, "Completion should be 100%");
            Assert.IsTrue(playerDataManager.IsAssessmentComplete(), "Assessment should be complete");
            Assert.AreEqual(-1, playerDataManager.GetNextUnansweredQuestion(), "No questions should be unanswered");
            
            Debug.Log("[TEST] Complete assessment detection works correctly");
        }

        /// <summary>
        /// Test data reset functionality
        /// </summary>
        [Test]
        public void TestDataReset()
        {
            // Arrange - Set up some data first
            playerDataManager.SendCustomEvent("Start");
            playerDataManager.SaveQuestionResponse(0, 3);
            playerDataManager.SaveQuestionResponse(1, 4);
            playerDataManager.SetFlags(3);
            
            float[] testVector = new float[30];
            for (int i = 0; i < 30; i++) testVector[i] = 0.5f;
            playerDataManager.UpdateVector30D(testVector);

            // Act
            playerDataManager.ResetPlayerData();

            // Assert
            Assert.AreEqual(0, playerDataManager.CurrentProgress, "Progress should be reset to 0");
            Assert.AreEqual(0, playerDataManager.Flags, "Flags should be reset to 0");
            Assert.AreEqual(0, playerDataManager.GetQuestionResponse(0), "Question responses should be reset");
            Assert.AreEqual(0, playerDataManager.GetQuestionResponse(1), "Question responses should be reset");
            
            float[] resetVector = playerDataManager.GetVector30D();
            for (int i = 0; i < 30; i++)
            {
                Assert.AreEqual(0f, resetVector[i], 0.001f, $"Vector element {i} should be reset to 0");
            }
            
            Debug.Log("[TEST] Data reset functionality works correctly");
        }

        /// <summary>
        /// Test that all question responses are properly managed
        /// </summary>
        [Test]
        public void TestAllQuestionResponsesRetrieval()
        {
            // Arrange
            playerDataManager.SendCustomEvent("Start");
            
            // Set up test data - answer questions with pattern
            int[] expectedResponses = new int[112];
            for (int i = 0; i < 112; i++)
            {
                int response = (i % 5) + 1; // Pattern: 1,2,3,4,5,1,2,3,4,5,...
                if (i % 10 == 0) response = 0; // Leave every 10th question unanswered
                
                if (response > 0)
                {
                    playerDataManager.SaveQuestionResponse(i, response);
                }
                expectedResponses[i] = response;
            }

            // Act
            int[] allResponses = playerDataManager.GetAllQuestionResponses();

            // Assert
            Assert.AreEqual(112, allResponses.Length, "Should return all 112 question responses");
            for (int i = 0; i < 112; i++)
            {
                Assert.AreEqual(expectedResponses[i], allResponses[i], $"Question {i} response should match expected");
            }
            
            // Count answered questions (should be 112 - 12 = 100, since every 10th is unanswered)
            int answeredCount = 0;
            for (int i = 0; i < 112; i++)
            {
                if (allResponses[i] > 0) answeredCount++;
            }
            Assert.AreEqual(101, answeredCount, "Should have 101 answered questions"); // 0,10,20,30,40,50,60,70,80,90,100,110 = 12 unanswered
            
            Debug.Log($"[TEST] All question responses retrieval works correctly - {answeredCount}/112 answered");
        }

        /// <summary>
        /// Integration test for typical user workflow
        /// </summary>
        [Test]
        public void TestTypicalUserWorkflow()
        {
            // Arrange
            playerDataManager.SendCustomEvent("Start");

            // Simulate user starting questionnaire
            Assert.AreEqual(0, playerDataManager.GetNextUnansweredQuestion(), "First question should be 0");

            // Answer first few questions
            playerDataManager.SaveQuestionResponse(0, 4);
            playerDataManager.SaveQuestionResponse(1, 2);
            playerDataManager.SaveQuestionResponse(2, 5);

            Assert.AreEqual(3, playerDataManager.CurrentProgress, "Progress should be 3");
            Assert.AreEqual(3, playerDataManager.GetNextUnansweredQuestion(), "Next question should be 3");

            // Enable public sharing (provisional)
            playerDataManager.SetFlags(2); // Provisional sharing only
            Assert.IsTrue(playerDataManager.IsProvisionalSharingEnabled, "Provisional sharing should be enabled");

            // Update vector (would be calculated by VectorBuilder in real scenario)
            float[] provisionalVector = new float[30];
            for (int i = 0; i < 30; i++) provisionalVector[i] = 0.1f * i;
            playerDataManager.UpdateVector30D(provisionalVector);

            // Continue answering
            for (int i = 3; i < 20; i++)
            {
                playerDataManager.SaveQuestionResponse(i, (i % 5) + 1);
            }

            Assert.AreEqual(20, playerDataManager.CurrentProgress, "Progress should be 20");
            Assert.AreEqual(20, playerDataManager.GetNextUnansweredQuestion(), "Next question should be 20");

            // Enable full public sharing
            playerDataManager.SetFlags(3); // Both public and provisional
            Assert.IsTrue(playerDataManager.IsPublicSharingEnabled, "Public sharing should be enabled");
            Assert.IsTrue(playerDataManager.IsProvisionalSharingEnabled, "Provisional sharing should remain enabled");

            Debug.Log("[TEST] Typical user workflow simulation completed successfully");
        }
    }
}