using UnityEngine;
using UnityEditor;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;

namespace VirtualTokyoMatching.Tests
{
    /// <summary>
    /// Unity Editor utility for launching multiple Unity instances for VRChat multi-client testing
    /// Supports PC and Quest platform testing with different performance targets
    /// </summary>
    public class MultiClientTestLauncher : MonoBehaviour
    {
        [System.Serializable]
        public class TestUser
        {
            public string userID;
            public string displayName;
            public string platform;
            public int targetFPS;
        }

        [Header("Test Configuration")]
        public TestUser[] testUsers = new TestUser[]
        {
            new TestUser { userID = "test_user_001", displayName = "TestUser1", platform = "PC", targetFPS = 72 },
            new TestUser { userID = "test_user_002", displayName = "TestUser2", platform = "PC", targetFPS = 72 },
            new TestUser { userID = "test_user_003", displayName = "TestUser3", platform = "Quest", targetFPS = 60 },
            new TestUser { userID = "test_user_004", displayName = "TestUser4", platform = "Quest", targetFPS = 60 }
        };

        [Header("Test Settings")]
        public bool enableAutomatedTesting = true;
        public bool enablePerformanceMonitoring = true;
        public bool enableSyncValidation = true;
        public float testDurationMinutes = 10f;

        private List<Process> runningInstances = new List<Process>();
        private string projectPath;
        private string unityExecutablePath;

#if UNITY_EDITOR
        void Start()
        {
            projectPath = Directory.GetParent(Application.dataPath).FullName;
            unityExecutablePath = EditorApplication.applicationPath;
        }

        /// <summary>
        /// Launch multiple Unity Editor instances for multi-client testing
        /// </summary>
        [ContextMenu("Launch Multi-Client Test")]
        public void LaunchMultiClientTest()
        {
            UnityEngine.Debug.Log("[MultiClientTestLauncher] Starting multi-client test setup...");

            StopAllInstances(); // Clean up any existing instances

            foreach (var testUser in testUsers)
            {
                LaunchUserInstance(testUser);
            }

            UnityEngine.Debug.Log($"[MultiClientTestLauncher] Launched {testUsers.Length} test instances");

            if (enableAutomatedTesting)
            {
                StartAutomatedTestSequence();
            }
        }

        /// <summary>
        /// Launch single Unity instance for specific test user
        /// </summary>
        private void LaunchUserInstance(TestUser user)
        {
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = unityExecutablePath,
                    Arguments = BuildUnityArguments(user),
                    UseShellExecute = false,
                    CreateNoWindow = false
                };

                var process = Process.Start(startInfo);
                runningInstances.Add(process);

                UnityEngine.Debug.Log($"[MultiClientTestLauncher] Launched instance for {user.displayName} ({user.platform})");
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogError($"[MultiClientTestLauncher] Failed to launch instance for {user.displayName}: {e.Message}");
            }
        }

        /// <summary>
        /// Build Unity command line arguments for test user
        /// </summary>
        private string BuildUnityArguments(TestUser user)
        {
            var args = new List<string>
            {
                $"-projectPath \"{projectPath}\"",
                $"-username \"{user.userID}\"",
                $"-displayName \"{user.displayName}\"",
                "-noUpm", // Disable Unity Package Manager for faster startup
                "-nographics false" // Keep graphics for VRChat testing
            };

            // Platform-specific arguments
            if (user.platform == "Quest")
            {
                args.Add("-buildTarget Android");
                args.Add($"-targetFPS {user.targetFPS}");
            }
            else
            {
                args.Add("-buildTarget StandaloneWindows64");
                args.Add($"-targetFPS {user.targetFPS}");
            }

            // Add test-specific defines
            args.Add("-define:VTM_MULTI_CLIENT_TEST");
            args.Add($"-define:VTM_USER_{user.userID.ToUpper()}");

            return string.Join(" ", args);
        }

        /// <summary>
        /// Start automated test sequence
        /// </summary>
        private void StartAutomatedTestSequence()
        {
            UnityEngine.Debug.Log("[MultiClientTestLauncher] Starting automated test sequence...");
            
            // Schedule test phases
            Invoke(nameof(PhaseOneDataStorageTests), 2f);
            Invoke(nameof(PhaseTwoProgressiveMatchingTests), 30f);
            Invoke(nameof(PhaseThreeSynchronizationTests), 60f);
            Invoke(nameof(PhaseFourPerformanceTests), 90f);
            Invoke(nameof(PhaseiveUserFlowTests), 120f);
            Invoke(nameof(CompleteTestSequence), testDurationMinutes * 60f);
        }

        /// <summary>
        /// Test Phase 1: Data Storage and Persistence
        /// </summary>
        private void PhaseOneDataStorageTests()
        {
            UnityEngine.Debug.Log("[MultiClientTestLauncher] Phase 1: Data Storage Tests");
            
            // Test scenarios:
            // - PlayerData save/load
            // - Cross-session recovery
            // - Incremental saves
            // - Retry mechanism on failure
            
            BroadcastTestCommand("START_DATA_STORAGE_TESTS");
        }

        /// <summary>
        /// Test Phase 2: Progressive Matching System
        /// </summary>
        private void PhaseTwoProgressiveMatchingTests()
        {
            UnityEngine.Debug.Log("[MultiClientTestLauncher] Phase 2: Progressive Matching Tests");
            
            // Test scenarios:
            // - Partial questionnaire matching
            // - Provisional vector updates
            // - Compatibility recalculation
            // - Recommendation list updates
            
            BroadcastTestCommand("START_PROGRESSIVE_MATCHING_TESTS");
        }

        /// <summary>
        /// Test Phase 3: Synchronization Tests
        /// </summary>
        private void PhaseThreeSynchronizationTests()
        {
            UnityEngine.Debug.Log("[MultiClientTestLauncher] Phase 3: Synchronization Tests");
            
            // Test scenarios:
            // - Public profile broadcasting
            // - Late-joiner support
            // - Visibility toggling
            // - 6D compression validation
            
            BroadcastTestCommand("START_SYNCHRONIZATION_TESTS");
        }

        /// <summary>
        /// Test Phase 4: Performance Tests
        /// </summary>
        private void PhaseFourPerformanceTests()
        {
            UnityEngine.Debug.Log("[MultiClientTestLauncher] Phase 4: Performance Tests");
            
            // Test scenarios:
            // - Frame rate maintenance
            // - Calculation budget management
            // - Distributed processing
            // - Adaptive throttling
            
            BroadcastTestCommand("START_PERFORMANCE_TESTS");
        }

        /// <summary>
        /// Test Phase 5: User Flow Tests
        /// </summary>
        private void PhaseiveUserFlowTests()
        {
            UnityEngine.Debug.Log("[MultiClientTestLauncher] Phase 5: User Flow Tests");
            
            // Test scenarios:
            // - Assessment resume
            // - 1-on-1 room creation
            // - Session timers
            // - Feedback recording
            
            BroadcastTestCommand("START_USER_FLOW_TESTS");
        }

        /// <summary>
        /// Complete test sequence and gather results
        /// </summary>
        private void CompleteTestSequence()
        {
            UnityEngine.Debug.Log("[MultiClientTestLauncher] Completing test sequence...");
            
            BroadcastTestCommand("COMPLETE_TESTS");
            
            Invoke(nameof(GatherTestResults), 5f);
        }

        /// <summary>
        /// Gather test results from all instances
        /// </summary>
        private void GatherTestResults()
        {
            UnityEngine.Debug.Log("[MultiClientTestLauncher] Gathering test results...");
            
            // Implementation would gather results from log files or shared data
            // For now, just stop all instances
            StopAllInstances();
        }

        /// <summary>
        /// Broadcast test command to all running instances
        /// </summary>
        private void BroadcastTestCommand(string command)
        {
            // In a real implementation, this would use IPC or file-based communication
            // to send commands to all running Unity instances
            UnityEngine.Debug.Log($"[MultiClientTestLauncher] Broadcasting command: {command}");
        }

        /// <summary>
        /// Stop all running Unity instances
        /// </summary>
        [ContextMenu("Stop All Instances")]
        public void StopAllInstances()
        {
            foreach (var process in runningInstances)
            {
                try
                {
                    if (process != null && !process.HasExited)
                    {
                        process.Kill();
                    }
                }
                catch (System.Exception e)
                {
                    UnityEngine.Debug.LogWarning($"[MultiClientTestLauncher] Failed to stop process: {e.Message}");
                }
            }
            
            runningInstances.Clear();
            UnityEngine.Debug.Log("[MultiClientTestLauncher] All instances stopped");
        }

        void OnApplicationQuit()
        {
            StopAllInstances();
        }

        void OnDestroy()
        {
            StopAllInstances();
        }
#endif

        /// <summary>
        /// Manual test controls for runtime use
        /// </summary>
        [ContextMenu("Generate Test Report")]
        public void GenerateTestReport()
        {
            var report = $"Multi-Client Test Report - {System.DateTime.Now}\n";
            report += $"Test Duration: {testDurationMinutes} minutes\n";
            report += $"Test Users: {testUsers.Length}\n";
            report += $"Performance Monitoring: {(enablePerformanceMonitoring ? "Enabled" : "Disabled")}\n";
            report += $"Sync Validation: {(enableSyncValidation ? "Enabled" : "Disabled")}\n";
            
            UnityEngine.Debug.Log(report);
            
            // Write to file
            string filePath = Path.Combine(Application.persistentDataPath, "VTM_Test_Report.txt");
            File.WriteAllText(filePath, report);
            
            UnityEngine.Debug.Log($"Test report saved to: {filePath}");
        }
    }

#if UNITY_EDITOR
    /// <summary>
    /// Editor window for multi-client test management
    /// </summary>
    [System.Serializable]
    public class MultiClientTestWindow : EditorWindow
    {
        private MultiClientTestLauncher launcher;

        [MenuItem("VirtualTokyoMatching/Multi-Client Test Window")]
        public static void ShowWindow()
        {
            GetWindow<MultiClientTestWindow>("VTM Multi-Client Tests");
        }

        void OnGUI()
        {
            GUILayout.Label("Virtual Tokyo Matching - Multi-Client Tests", EditorStyles.boldLabel);
            
            if (launcher == null)
            {
                launcher = FindObjectOfType<MultiClientTestLauncher>();
                if (launcher == null)
                {
                    GUILayout.Label("MultiClientTestLauncher not found in scene");
                    if (GUILayout.Button("Create MultiClientTestLauncher"))
                    {
                        var go = new GameObject("MultiClientTestLauncher");
                        launcher = go.AddComponent<MultiClientTestLauncher>();
                    }
                    return;
                }
            }

            EditorGUILayout.Space();

            if (GUILayout.Button("Launch Multi-Client Test", GUILayout.Height(30)))
            {
                launcher.LaunchMultiClientTest();
            }

            if (GUILayout.Button("Stop All Instances", GUILayout.Height(30)))
            {
                launcher.StopAllInstances();
            }

            if (GUILayout.Button("Generate Test Report", GUILayout.Height(30)))
            {
                launcher.GenerateTestReport();
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Test Configuration:", EditorStyles.boldLabel);
            
            launcher.enableAutomatedTesting = EditorGUILayout.Toggle("Automated Testing", launcher.enableAutomatedTesting);
            launcher.enablePerformanceMonitoring = EditorGUILayout.Toggle("Performance Monitoring", launcher.enablePerformanceMonitoring);
            launcher.enableSyncValidation = EditorGUILayout.Toggle("Sync Validation", launcher.enableSyncValidation);
            launcher.testDurationMinutes = EditorGUILayout.FloatField("Test Duration (minutes)", launcher.testDurationMinutes);
        }
    }
#endif
}