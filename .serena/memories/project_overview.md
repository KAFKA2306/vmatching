# Virtual Tokyo Matching - Project Overview

## Project Purpose
Virtual Tokyo Matching is a Unity-based VRChat world that implements a sophisticated personality-based matchmaking system. The project creates a VRChat world where users complete a 112-question personality assessment and are matched with compatible users for 1-on-1 private conversations.

## Key Features
- **Progressive Matching**: Users can see recommendations even with incomplete questionnaires
- **112-Question Personality Assessment**: 5-choice questions with pause/resume functionality  
- **30D → 6D Vector Compression**: Privacy-preserving personality vector reduction
- **Real-time Compatibility**: Cosine similarity matching with top 3 recommendations
- **Private Session Rooms**: 1-on-1 conversation spaces with 15-minute timers
- **Auto-generated Profiles**: No manual profiles - everything generated from assessment data
- **Privacy Controls**: Comprehensive safety and visibility controls

## Tech Stack
- **Platform**: Unity 2022.3 LTS + VRChat SDK3 Worlds + UdonSharp
- **Language**: C# (UdonSharp variant for VRChat compatibility)
- **Package Management**: VRChat Creator Companion (VCC) + VPM CLI
- **Development Environment**: Ubuntu 22.04 LTS (Linux)
- **Target Platforms**: PC (Windows) and Quest (Android)

## Architecture Highlights
- **Event-driven Design**: All components communicate via UdonSharp events
- **Distributed Processing**: Frame-limited calculations with PerfGuard system
- **Data Privacy**: Raw answers stored locally, only 6D condensed vectors shared publicly
- **Performance Optimized**: Target 72FPS PC / 60FPS Quest with size limits <200MB PC / <100MB Quest

## Project Status (Phase 3 Complete)
✅ **Implementation Status**: All 9 core UdonSharp scripts completed and VRChat SDK3 compliant  
✅ **Development Environment**: Ubuntu + VCC/VPM CLI + Unity setup verified  
✅ **Project Structure**: Complete Assets folder organization with Scripts, ScriptableObjects, Tests  
⚠️ **Unity Project**: Created at `/home/kafka/projects/VirtualTokyoMatching`, scene construction pending  
🔄 **Next Phase**: Scene construction, UI prefabs, and system integration