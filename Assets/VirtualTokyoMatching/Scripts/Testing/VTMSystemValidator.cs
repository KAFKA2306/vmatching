using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using System;

namespace VirtualTokyoMatching.Testing
{
    /// <summary>
    /// Runtime validation system for Virtual Tokyo Matching components.
    /// Performs system integrity checks and reports issues for debugging.
    /// </summary>
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class VTMSystemValidator : UdonSharpBehaviour
    {
        [Header("Validation Settings")]
        public bool runValidationOnStart = true;
        public bool enableContinuousValidation = false;
        public float validationInterval = 10f;
        
        [Header("Component References")]
        public PlayerDataManager playerDataManager;
        public DiagnosisController diagnosisController;
        public VectorBuilder vectorBuilder;
        public CompatibilityCalculator compatibilityCalculator;
        public PublicProfilePublisher[] profilePublishers;
        public RecommenderUI recommenderUI;
        public SessionRoomManager sessionRoomManager;
        public PerfGuard perfGuard;
        public ValuesSummaryGenerator summaryGenerator;
        public SafetyController safetyController;
        
        [Header("Configuration Assets")]
        public QuestionDatabase questionDatabase;
        public VectorConfiguration vectorConfig;
        public SummaryTemplates summaryTemplates;
        public PerformanceSettings performanceSettings;
        
        [Header("Validation Results")]
        [SerializeField] private int totalChecks = 0;
        [SerializeField] private int passedChecks = 0;
        [SerializeField] private int failedChecks = 0;
        [SerializeField] private int warningChecks = 0;
        [SerializeField] private bool lastValidationPassed = false;
        [SerializeField] private string lastValidationTime = "";
        
        // Internal state
        private bool isValidating = false;
        private float lastValidationTimestamp = 0f;
        
        void Start()
        {
            if (runValidationOnStart)
            {
                ValidateSystem();
            }
            
            if (enableContinuousValidation)
            {
                InvokeRepeating(nameof(ValidateSystem), validationInterval, validationInterval);
            }
        }
        
        /// <summary>
        /// Public method to trigger system validation
        /// </summary>
        public void ValidateSystem()
        {
            if (isValidating)
            {
                Debug.LogWarning("[VTM Validator] Validation already in progress, skipping...");
                return;
            }
            
            isValidating = true;
            ResetValidationCounters();
            
            Debug.Log("[VTM Validator] ===== Starting System Validation =====");
            
            // Core component validation
            ValidatePlayerDataManager();
            ValidateDiagnosisController();
            ValidateVectorBuilder();
            ValidateCompatibilityCalculator();
            ValidateProfilePublishers();
            ValidateRecommenderUI();
            ValidateSessionRoomManager();
            ValidatePerfGuard();
            ValidateSummaryGenerator();
            ValidateSafetyController();
            
            // Configuration validation
            ValidateQuestionDatabase();
            ValidateVectorConfiguration();
            ValidateSummaryTemplates();
            ValidatePerformanceSettings();
            
            // System integration validation
            ValidateComponentWiring();
            ValidateEventChains();
            ValidatePerformanceConstraints();
            
            FinishValidation();
            isValidating = false;
        }
        
        private void ResetValidationCounters()
        {
            totalChecks = 0;
            passedChecks = 0;
            failedChecks = 0;
            warningChecks = 0;
            lastValidationTimestamp = Time.time;
            lastValidationTime = DateTime.Now.ToString("HH:mm:ss");
        }
        
        private void ValidatePlayerDataManager()
        {
            string component = "PlayerDataManager";
            CheckNotNull(playerDataManager, component);
            
            if (playerDataManager != null)
            {
                CheckEventTargetArray(playerDataManager.onDataLoadedTargets, $"{component}.onDataLoadedTargets");
                CheckEventTargetArray(playerDataManager.onDataSavedTargets, $"{component}.onDataSavedTargets");
                CheckEventTargetArray(playerDataManager.onDataResetTargets, $"{component}.onDataResetTargets");
            }
        }
        
        private void ValidateDiagnosisController()
        {
            string component = "DiagnosisController";
            CheckNotNull(diagnosisController, component);
            
            if (diagnosisController != null)
            {
                CheckNotNull(diagnosisController.playerDataManager, $"{component}.playerDataManager");
                CheckNotNull(diagnosisController.vectorBuilder, $"{component}.vectorBuilder");
                CheckNotNull(diagnosisController.questionDatabase, $"{component}.questionDatabase");
                CheckNotNull(diagnosisController.assessmentPanel, $"{component}.assessmentPanel");
                CheckNotNull(diagnosisController.questionText, $"{component}.questionText");
                CheckArray(diagnosisController.choiceButtons, 5, $"{component}.choiceButtons");
                CheckArray(diagnosisController.choiceTexts, 5, $"{component}.choiceTexts");
                CheckEventTargetArray(diagnosisController.onQuestionAnsweredTargets, $"{component}.onQuestionAnsweredTargets");
                CheckEventTargetArray(diagnosisController.onAssessmentCompleteTargets, $"{component}.onAssessmentCompleteTargets");
            }
        }
        
        private void ValidateVectorBuilder()
        {
            string component = "VectorBuilder";
            CheckNotNull(vectorBuilder, component);
            
            if (vectorBuilder != null)
            {
                CheckNotNull(vectorBuilder.playerDataManager, $"{component}.playerDataManager");
                CheckNotNull(vectorBuilder.vectorConfig, $"{component}.vectorConfig");
                CheckNotNull(vectorBuilder.questionDatabase, $"{component}.questionDatabase");
                CheckEventTargetArray(vectorBuilder.onVectorUpdatedTargets, $"{component}.onVectorUpdatedTargets");
                CheckEventTargetArray(vectorBuilder.onVectorFinalizedTargets, $"{component}.onVectorFinalizedTargets");
            }
        }
        
        private void ValidateCompatibilityCalculator()
        {
            string component = "CompatibilityCalculator";
            CheckNotNull(compatibilityCalculator, component);
            
            if (compatibilityCalculator != null)
            {
                CheckNotNull(compatibilityCalculator.perfGuard, $"{component}.perfGuard");
                CheckNotNull(compatibilityCalculator.playerDataManager, $"{component}.playerDataManager");
                CheckEventTargetArray(compatibilityCalculator.onCalculationCompleteTargets, $"{component}.onCalculationCompleteTargets");
                CheckEventTargetArray(compatibilityCalculator.onRecommendationsUpdatedTargets, $"{component}.onRecommendationsUpdatedTargets");
                
                // Check configuration values
                if (compatibilityCalculator.maxRecommendations <= 0)
                {
                    LogWarning($"{component}.maxRecommendations should be > 0");
                }
                
                if (compatibilityCalculator.recalculationInterval <= 0)
                {
                    LogWarning($"{component}.recalculationInterval should be > 0");
                }
            }
        }
        
        private void ValidateProfilePublishers()
        {
            string component = "ProfilePublishers";
            CheckArray(profilePublishers, 1, component, false); // At least 1 required
            
            if (profilePublishers != null && profilePublishers.Length > 0)
            {
                for (int i = 0; i < profilePublishers.Length; i++)
                {
                    var publisher = profilePublishers[i];
                    if (publisher != null)
                    {
                        CheckNotNull(publisher.playerDataManager, $"{component}[{i}].playerDataManager");
                        CheckNotNull(publisher.vectorBuilder, $"{component}[{i}].vectorBuilder");
                        CheckNotNull(publisher.vectorConfig, $"{component}[{i}].vectorConfig");
                        CheckNotNull(publisher.summaryGenerator, $"{component}[{i}].summaryGenerator");
                        CheckEventTargetArray(publisher.onProfilePublishedTargets, $"{component}[{i}].onProfilePublishedTargets");
                        CheckEventTargetArray(publisher.onProfileHiddenTargets, $"{component}[{i}].onProfileHiddenTargets");
                    }
                }
            }
        }
        
        private void ValidateRecommenderUI()
        {
            string component = "RecommenderUI";
            CheckNotNull(recommenderUI, component);
            
            if (recommenderUI != null)
            {
                CheckNotNull(recommenderUI.compatibilityCalculator, $"{component}.compatibilityCalculator");
                CheckNotNull(recommenderUI.sessionRoomManager, $"{component}.sessionRoomManager");
                CheckNotNull(recommenderUI.summaryGenerator, $"{component}.summaryGenerator");
                // Additional UI component checks would go here
            }
        }
        
        private void ValidateSessionRoomManager()
        {
            string component = "SessionRoomManager";
            CheckNotNull(sessionRoomManager, component);
            
            if (sessionRoomManager != null)
            {
                CheckNotNull(sessionRoomManager.playerDataManager, $"{component}.playerDataManager");
                CheckArray(sessionRoomManager.sessionRooms, 1, $"{component}.sessionRooms", false);
                CheckNotNull(sessionRoomManager.lobbySpawnPoint, $"{component}.lobbySpawnPoint");
                CheckEventTargetArray(sessionRoomManager.onSessionStartedTargets, $"{component}.onSessionStartedTargets");
                CheckEventTargetArray(sessionRoomManager.onSessionEndedTargets, $"{component}.onSessionEndedTargets");
            }
        }
        
        private void ValidatePerfGuard()
        {
            string component = "PerfGuard";
            CheckNotNull(perfGuard, component);
            
            if (perfGuard != null)
            {
                CheckNotNull(perfGuard.performanceSettings, $"{component}.performanceSettings");
                CheckEventTargetArray(perfGuard.onPerformanceWarningTargets, $"{component}.onPerformanceWarningTargets");
                CheckEventTargetArray(perfGuard.onFrameRateDropTargets, $"{component}.onFrameRateDropTargets");
            }
        }
        
        private void ValidateSummaryGenerator()
        {
            string component = "ValuesSummaryGenerator";
            CheckNotNull(summaryGenerator, component);
            
            if (summaryGenerator != null)
            {
                CheckNotNull(summaryGenerator.summaryTemplates, $"{component}.summaryTemplates");
                CheckNotNull(summaryGenerator.vectorConfig, $"{component}.vectorConfig");
            }
        }
        
        private void ValidateSafetyController()
        {
            string component = "SafetyController";
            CheckNotNull(safetyController, component);
            
            if (safetyController != null)
            {
                CheckNotNull(safetyController.playerDataManager, $"{component}.playerDataManager");
                CheckNotNull(safetyController.sessionRoomManager, $"{component}.sessionRoomManager");
                CheckEventTargetArray(safetyController.onPrivacyChangedTargets, $"{component}.onPrivacyChangedTargets");
                CheckEventTargetArray(safetyController.onEmergencyHideTargets, $"{component}.onEmergencyHideTargets");
            }
        }
        
        private void ValidateQuestionDatabase()
        {
            string component = "QuestionDatabase";
            CheckNotNull(questionDatabase, component);
            
            if (questionDatabase != null)
            {
                CheckArray(questionDatabase.questions, 112, $"{component}.questions");
                
                if (questionDatabase.questions != null && questionDatabase.questions.Length == 112)
                {
                    LogPass($"{component} has correct number of questions (112)");
                    
                    // Validate question content
                    int invalidQuestions = 0;
                    for (int i = 0; i < questionDatabase.questions.Length; i++)
                    {
                        var question = questionDatabase.questions[i];
                        if (question == null || string.IsNullOrEmpty(question.text) || 
                            question.choices == null || question.choices.Length != 5 ||
                            question.weights == null || question.weights.Length != 5 ||
                            question.targetAxis < 0 || question.targetAxis >= 30)
                        {
                            invalidQuestions++;
                        }
                    }
                    
                    if (invalidQuestions > 0)
                    {
                        LogError($"{component} has {invalidQuestions} invalid questions");
                    }
                    else
                    {
                        LogPass($"{component} all questions are valid");
                    }
                }
            }
        }
        
        private void ValidateVectorConfiguration()
        {
            string component = "VectorConfiguration";
            CheckNotNull(vectorConfig, component);
            
            if (vectorConfig != null)
            {
                CheckArray(vectorConfig.axisNames, 30, $"{component}.axisNames");
                CheckArray(vectorConfig.reducedAxisNames, 6, $"{component}.reducedAxisNames");
                
                // Additional matrix validation would go here
                LogPass($"{component} basic structure is valid");
            }
        }
        
        private void ValidateSummaryTemplates()
        {
            string component = "SummaryTemplates";
            CheckNotNull(summaryTemplates, component);
            
            if (summaryTemplates != null)
            {
                CheckArray(summaryTemplates.axisTemplates, 30, $"{component}.axisTemplates");
                
                if (summaryTemplates.headlineTemplates != null && summaryTemplates.headlineTemplates.Length > 0)
                {
                    LogPass($"{component} has {summaryTemplates.headlineTemplates.Length} headline templates");
                }
                else
                {
                    LogWarning($"{component} has no headline templates");
                }
            }
        }
        
        private void ValidatePerformanceSettings()
        {
            string component = "PerformanceSettings";
            CheckNotNull(performanceSettings, component);
            
            if (performanceSettings != null)
            {
                if (performanceSettings.maxCalculationsPerFrame > 0)
                {
                    LogPass($"{component}.maxCalculationsPerFrame = {performanceSettings.maxCalculationsPerFrame}");
                }
                else
                {
                    LogError($"{component}.maxCalculationsPerFrame must be > 0");
                }
                
                if (performanceSettings.targetFrameRatePC > 0 && performanceSettings.targetFrameRateQuest > 0)
                {
                    LogPass($"{component} frame rate targets set (PC:{performanceSettings.targetFrameRatePC}, Quest:{performanceSettings.targetFrameRateQuest})");
                }
                else
                {
                    LogError($"{component} frame rate targets must be > 0");
                }
            }
        }
        
        private void ValidateComponentWiring()
        {
            string check = "Component Wiring";
            
            // Check if core components reference each other correctly
            bool wiringComplete = true;
            
            if (diagnosisController != null && diagnosisController.vectorBuilder != vectorBuilder)
            {
                LogError($"{check}: DiagnosisController.vectorBuilder not wired to main VectorBuilder");
                wiringComplete = false;
            }
            
            if (vectorBuilder != null && vectorBuilder.playerDataManager != playerDataManager)
            {
                LogError($"{check}: VectorBuilder.playerDataManager not wired to main PlayerDataManager");
                wiringComplete = false;
            }
            
            if (compatibilityCalculator != null && compatibilityCalculator.playerDataManager != playerDataManager)
            {
                LogError($"{check}: CompatibilityCalculator.playerDataManager not wired to main PlayerDataManager");
                wiringComplete = false;
            }
            
            if (wiringComplete)
            {
                LogPass($"{check}: Core component wiring appears correct");
            }
        }
        
        private void ValidateEventChains()
        {
            string check = "Event Chains";
            
            // Check if event target arrays are properly set up for the core workflow
            bool eventChainsValid = true;
            
            if (playerDataManager != null)
            {
                if (playerDataManager.onDataLoadedTargets == null || playerDataManager.onDataLoadedTargets.Length == 0)
                {
                    LogWarning($"{check}: PlayerDataManager has no onDataLoadedTargets");
                    eventChainsValid = false;
                }
            }
            
            if (diagnosisController != null)
            {
                if (diagnosisController.onQuestionAnsweredTargets == null || diagnosisController.onQuestionAnsweredTargets.Length == 0)
                {
                    LogWarning($"{check}: DiagnosisController has no onQuestionAnsweredTargets");
                    eventChainsValid = false;
                }
            }
            
            if (eventChainsValid)
            {
                LogPass($"{check}: Event chains appear to be configured");
            }
        }
        
        private void ValidatePerformanceConstraints()
        {
            string check = "Performance Constraints";
            
            // Check if performance settings are reasonable
            if (performanceSettings != null)
            {
                if (performanceSettings.maxCalculationsPerFrame > 20)
                {
                    LogWarning($"{check}: maxCalculationsPerFrame ({performanceSettings.maxCalculationsPerFrame}) might be too high for stable performance");
                }
                
                if (performanceSettings.targetFrameRatePC < 60)
                {
                    LogWarning($"{check}: targetFrameRatePC ({performanceSettings.targetFrameRatePC}) is below recommended 72 FPS");
                }
                
                if (performanceSettings.targetFrameRateQuest < 45)
                {
                    LogWarning($"{check}: targetFrameRateQuest ({performanceSettings.targetFrameRateQuest}) is below recommended 60 FPS");
                }
                
                LogPass($"{check}: Performance constraints evaluated");
            }
        }
        
        private void FinishValidation()
        {
            lastValidationPassed = failedChecks == 0;
            
            string status = lastValidationPassed ? "PASSED" : "FAILED";
            string color = lastValidationPassed ? "green" : "red";
            
            Debug.Log($"[VTM Validator] ===== Validation {status} =====");
            Debug.Log($"[VTM Validator] Total Checks: {totalChecks}");
            Debug.Log($"[VTM Validator] <color=green>Passed: {passedChecks}</color>");
            if (warningChecks > 0) Debug.Log($"[VTM Validator] <color=yellow>Warnings: {warningChecks}</color>");
            if (failedChecks > 0) Debug.Log($"[VTM Validator] <color=red>Failed: {failedChecks}</color>");
            
            if (!lastValidationPassed)
            {
                Debug.LogError($"[VTM Validator] System validation failed with {failedChecks} critical issues. Check configuration and wiring.");
            }
            else if (warningChecks > 0)
            {
                Debug.LogWarning($"[VTM Validator] System validation passed but has {warningChecks} warnings that should be addressed.");
            }
            else
            {
                Debug.Log("[VTM Validator] <color=green>System validation passed with no issues! ✓</color>");
            }
        }
        
        // Helper methods for validation checks
        private void CheckNotNull(object obj, string name)
        {
            totalChecks++;
            if (obj == null)
            {
                LogError($"{name} is null");
            }
            else
            {
                LogPass($"{name} is assigned");
            }
        }
        
        private void CheckArray<T>(T[] array, int expectedLength, string name, bool exactMatch = true)
        {
            totalChecks++;
            if (array == null)
            {
                LogError($"{name} is null");
            }
            else if (exactMatch && array.Length != expectedLength)
            {
                LogError($"{name} length is {array.Length}, expected {expectedLength}");
            }
            else if (!exactMatch && array.Length < expectedLength)
            {
                LogError($"{name} length is {array.Length}, expected at least {expectedLength}");
            }
            else
            {
                LogPass($"{name} has correct length ({array.Length})");
            }
        }
        
        private void CheckEventTargetArray(UdonBehaviour[] targets, string name)
        {
            totalChecks++;
            if (targets == null || targets.Length == 0)
            {
                LogWarning($"{name} has no event targets");
            }
            else
            {
                int nullTargets = 0;
                foreach (var target in targets)
                {
                    if (target == null) nullTargets++;
                }
                
                if (nullTargets > 0)
                {
                    LogWarning($"{name} has {nullTargets} null targets out of {targets.Length}");
                }
                else
                {
                    LogPass($"{name} has {targets.Length} valid targets");
                }
            }
        }
        
        private void LogPass(string message)
        {
            passedChecks++;
            Debug.Log($"[VTM Validator] ✓ {message}");
        }
        
        private void LogWarning(string message)
        {
            warningChecks++;
            Debug.LogWarning($"[VTM Validator] ⚠ {message}");
        }
        
        private void LogError(string message)
        {
            failedChecks++;
            Debug.LogError($"[VTM Validator] ✗ {message}");
        }
        
        /// <summary>
        /// Public method to get validation summary
        /// </summary>
        public string GetValidationSummary()
        {
            return $"Validation Summary (Last: {lastValidationTime}):\\n" +
                   $"Total: {totalChecks}, Passed: {passedChecks}, Warnings: {warningChecks}, Failed: {failedChecks}\\n" +
                   $"Status: {(lastValidationPassed ? "PASSED" : "FAILED")}";
        }
        
        /// <summary>
        /// Check if last validation passed
        /// </summary>
        public bool IsSystemValid()
        {
            return lastValidationPassed;
        }
    }
}