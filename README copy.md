# Mixed Reality Collaborative Horror Game

[![Unity Version](https://img.shields.io/badge/Unity-6000.1.14f1%2B-blue.svg)](https://unity3d.com/get-unity/download)
[![ML-Agents](https://img.shields.io/badge/ML--Agents-4.0.0-green.svg)](https://github.com/Unity-Technologies/ml-agents)
[![Meta XR SDK](https://img.shields.io/badge/Meta%20XR%20SDK-78.0.0-orange.svg)](https://developer.oculus.com/downloads/package/unity-integration/)

**A Sitting Mixed Reality Horror Experience** where players must solve puzzles to escape a virtual world while being hunted by an AI agent that learns from their habits. This 15-minute survival experience blends your physical environment with virtual terror, requiring constant vigilance of your real-world surroundings.

## Overview

This project combines the power of Unity ML-Agents with Meta XR SDK to create an intelligent horror experience where players must escape a virtual world while being stalked by an AI that adapts to their behavior patterns. The experience leverages mixed reality to blur the lines between virtual and physical space - players must check under their desk, glance at hallways, and remain constantly aware of their real-world surroundings as they solve interconnected puzzles within a 15-minute survival scenario.

### Key Features

- **AI-Powered Predator**: Intelligent agent that learns from player habits and adapts hunting strategies
- **Sitting MR Experience**: Designed for seated gameplay that utilizes your physical environment
- **Escape-Focused Gameplay**: 15-minute survival scenario with two possible outcomes - escape or elimination
- **Environmental Awareness**: Players must monitor both virtual and real-world spaces simultaneously
- **Sequential Puzzle System**: Five interconnected challenges that must be solved to achieve freedom
- **Voice Recognition Integration**: Vosk-powered speech recognition for voice-activated puzzle elements
- **Adaptive Learning**: AI studies player behavior patterns to become increasingly effective
- **Physical Integration**: Real-world elements (desk, hallways) become part of the horror experience
- **Interactive Horror Elements**: "Peepers" entities that require visual deterrence and psychological pressure

## Game Mechanics

### AI Predator System
The `MonsterAgent` uses reinforcement learning to:
- Study and adapt to individual player habits and behavior patterns
- Choose optimal positioning to maximize psychological pressure
- Time appearances to exploit moments of player vulnerability
- Utilize speed boosts strategically during critical escape moments
- Learn from player field-of-view patterns and environmental awareness

**Observation Space:**
- Player FOV hit counts for each monster location (4 values)
- Monster positions (3D coordinates × 4 monsters = 12 values)
- Player camera position (3D coordinates)
- Timing information (timer difference, loss timer, random timer)
- Speed boost usage data
- Current monster index

**Action Space:**
- **Movement Choice** (Discrete): Select monster location (0-3)
- **Speed Boost** (Discrete): Use speed boost ability (0-1)

### Puzzle Progression System

#### 1. Wire Puzzle (`WirePuzzleLogic`)
- **Objective**: Press 4 buttons in the correct randomized sequence
- **Visual Feedback**: Target lights turn red, completed lights turn green
- **Failure Condition**: Wrong button press resets the puzzle
- **Completion**: Unlocks Simon Says puzzle and spawns collectible items

#### 2. Simon Says (`SimonLogic`) 
- **Objective**: Complete 9 progressive stages of memory sequences
- **Mechanics**: Watch button light patterns, repeat sequences accurately  
- **Progression**: Each stage adds one more button to remember
- **Visual System**: Dynamic button materials and dot light progression indicators
- **Completion**: Unlocks socket puzzle area and progresses to Peepers Event

#### 3. Peepers Event (`PeepersBehaviour`)
- **Objective**: Deter 7 "peeper" entities by maintaining eye contact for 2 seconds each
- **Mechanics**: Peepers spawn randomly in the environment and must be watched to completion
- **Threat System**: Peepers that aren't watched within the time limit trigger game over
- **Visual Feedback**: Shake effects intensify while being watched, UI fill indicator shows progress
- **Completion**: Successfully deterring all peepers unlocks the Cut Puzzle

#### 4. Cut Puzzle (`WireCutController`)
- **Objective**: Cut specific wires in the correct sequence to disable security systems
- **Mechanics**: Players must identify and cut the correct wires while avoiding traps
- **Failure Condition**: Cutting wrong wires may trigger penalties or reset sequences
- **Completion**: Opens access to the Final Wire Puzzle area

#### 5. Socket Puzzle (`LogManager`)
- **Objective**: Collect and place 3 colored cells (Red, Green, Blue) in correct sockets
- **Progression**: Each cell placed unlocks the next socket slot
- **Integration**: Connects with lighting system and voice recognition for atmospheric effects
- **Victory Condition**: All 3 cells correctly placed triggers escape sequence

### Vision & Detection System

The `PlayerFOVRaycast` creates realistic monster detection:
- **Dual-Cone FOV**: Horizontal and vertical ray casting for comprehensive coverage
- **Configurable Parameters**: View angle, distance, ray count for performance tuning
- **Hit Tracking**: Monitors which monster locations are in player's sight
- **Debug Visualization**: Real-time ray visualization for development

### Environmental Systems

#### Lighting Control (`LightingLogic`)
- **Dynamic Atmosphere**: Puzzle-specific lighting zones
- **Progress Feedback**: Visual indicators for puzzle states
- **Win/Lose States**: Dramatic lighting changes for game outcomes
- **Material Management**: Button state visualization system

#### Audio Integration (`MicrophoneBehaviour`, `VoskSpeechRecognizer`)
- **Voice Recognition**: Vosk-powered offline speech recognition for voice-activated puzzle elements
- **Keyword Detection**: Configurable voice commands for puzzle interaction (e.g., "open" command)
- **Real-time Processing**: Low-latency microphone input processing with waveform analysis
- **Voice Detection**: Microphone input processing for team communication
- **Immersive Audio**: Spatial audio cues for monster presence and puzzle feedback
- **Dynamic Sound Effects**: Context-aware audio feedback for puzzle interactions and peeper encounters

#### Voice Recognition System (`Vosk/`)
The integrated Vosk speech recognition system provides:
- **Offline Processing**: No internet connection required for voice recognition
- **Multiple Model Support**: Support for various language models and recognition accuracy levels
- **Batch Processing**: Efficient handling of continuous audio streams
- **Custom Grammar**: Configurable grammar rules for specific voice commands
- **Unity Integration**: Seamless integration with Unity's audio system and microphone input

## Technical Architecture

### Core Components

### Scene Structure

The project is organized into several key scenes that serve different purposes in the horror experience:

#### Main Scenes
```
Assets/Scenes/
├── StartScene.unity           # Initial entry point and menu interface
├── AgentOffice.unity          # Primary gameplay scene with AI agent training
└── DeathRoom.unity           # Game over sequence and elimination scenario
```

#### Legacy Development Scenes
```
Assets/Scenes/Old Office Scenes/
├── MainOffice.unity          # Original office environment prototype
├── OfficeScene.unity         # Early iteration of office layout
├── UpdateOffice.unity        # Refined office design
├── NathanielScene.unity      # Individual developer test scene
├── SocketPuzzle.unity        # Isolated socket puzzle testing
├── SpawnRoom.unity           # Monster spawn mechanics testing
├── AgentOffice 1.unity       # Alternative agent configuration
├── OffAgentOffice.unity      # Non-agent controlled monster testing
└── SampleScene.unity         # Unity default scene template
```

#### Scene Descriptions

- **StartScene**: Entry point featuring the initial game setup, VR calibration, and user onboarding for the mixed reality experience
- **AgentOffice**: The core gameplay environment where players experience the full 15-minute survival scenario with AI-powered monster, puzzle systems, and environmental integration
- **DeathRoom**: Handles the elimination outcome, providing closure to failed escape attempts and AI learning feedback
- **Legacy Scenes**: Development iterations preserved for reference, testing individual components, and comparing design evolution

### Script Structure
```
Assets/Scripts/
├── MonsterAgent.cs              # ML-Agents reinforcement learning controller
├── Monster/
│   ├── MonsterBehaviour.cs      # Monster movement and timing logic
│   └── MonsterCollisionHandler.cs # Player detection and collision events
├── Puzzles/
│   ├── WirePuzzle/             # Sequential button pressing challenge
│   ├── Simon Says/             # Memory sequence puzzle
│   ├── Peepers Event/          # Visual deterrence puzzle system
│   │   ├── PeepersBehaviour.cs # Peeper spawning and management logic
│   │   └── PeepersCollisionHandler.cs # Individual peeper interaction handling
│   ├── Cut Puzzle/             # Wire cutting challenge
│   └── Socket Puzzle/          # Collectible placement system
├── Vosk/                       # Voice recognition system
│   ├── VoskSpeechRecognizer.cs # Unity integration for speech recognition
│   ├── VoskRecognizer.cs       # Core recognition functionality
│   └── Model.cs                # Speech model management
├── Audio/
│   └── ImpactSound.cs          # Dynamic audio feedback system
├── PlayerFOVRaycast.cs         # Advanced vision detection system
├── LightingLogic.cs            # Environmental lighting controller
├── GameFlow.cs                 # Overall puzzle progression management
└── Environment/                # Interactive environment objects
```

### Dependencies

- **Unity ML-Agents 4.0.0**: Reinforcement learning framework
- **Meta XR SDK 78.0.0**: VR/AR functionality and hand tracking
- **Vosk Speech Recognition**: Offline speech-to-text processing for voice interactions
- **Unity Input System 1.14.2**: Modern input handling
- **Universal Render Pipeline 17.1.0**: Optimized rendering for VR
- **ProBuilder 6.0.7**: Level geometry creation tools

## Setup Instructions

### Prerequisites
- Unity 6000.1.14f1 or later
- Meta Quest headset (Quest 2/Pro/3 supported)
- Python 3.8+ with ML-Agents toolkit (for training custom models)

### Installation

1. **Clone the Repository**
   ```bash
   git clone https://github.com/rmit-computing-technologies/ssss-super-squad-scary-squad.git
   cd ssss-super-squad-scary-squad
   ```

2. **Open in Unity**
   - Launch Unity Hub
   - Click "Open Project" and select the cloned folder
   - Unity will automatically import packages and dependencies

3. **Configure XR Settings**
   - Navigate to Edit → Project Settings → XR Plug-in Management
   - Enable OpenXR for target platforms
   - Configure Meta XR interaction profiles

4. **Speech Recognition Setup**
   - Download Vosk model from [Vosk Models](https://alphacephei.com/vosk/models)
   - Place the model in `Assets/StreamingAssets/vosk-model-small-en-us-0.15/`
   - Ensure microphone permissions are enabled for your platform

5. **ML-Agents Setup** (Optional - for training)
   ```bash
   pip install mlagents==1.0.0
   pip install torch torchvision
   ```

### Quick Start

1. **Play Mode Testing**
   - Open the main scene
   - Press Play in Unity Editor
   - Use WASD + Mouse for desktop testing

2. **Mixed Reality Mode**
   - Connect Meta Quest headset
   - Ensure you're in a seated position with clear desk space
   - Enable passthrough mode to see your physical environment
   - Build and deploy to device via Unity

3. **AI Training** (Advanced)
   - Configure training parameters in `MonsterAgent.cs`
   - Run: `mlagents-learn config/monster_config.yaml --run-id=monster-training`

## Gameplay Flow

**Duration**: ~15 minutes per session

1. **Initialization**: AI predator begins studying your behavior patterns and environmental usage
2. **Wire Puzzle**: Solve sequential button challenges while maintaining awareness of your physical surroundings
3. **Simon Says**: Complete memory sequences while the AI learns your concentration patterns  
4. **Peepers Event**: Deter supernatural entities through sustained visual contact while they attempt psychological manipulation
5. **Cut Puzzle**: Navigate wire-cutting challenges under increasing time pressure
6. **Socket Collection**: Find and place colored cells using voice commands and physical interaction
7. **Final Moments**: Two possible outcomes - successful escape or elimination by the adaptive AI
8. **Environmental Vigilance**: Throughout the experience, monitor your desk area, nearby hallways, and physical space

**Victory Condition**: Complete all puzzles within the time limit while avoiding detection
**Failure Condition**: AI successfully hunts you down using learned behavioral patterns

## Performance Considerations

- **Ray Casting Optimization**: Adjustable ray count and distance for performance tuning
- **VR Frame Rate**: Optimized for 90Hz VR rendering  
- **Speech Recognition**: Offline processing ensures low latency and privacy protection
- **ML-Agents Inference**: Efficient neural network inference for real-time AI decisions
- **Memory Management**: Smart object pooling for puzzle components and peeper entities
- **Audio Optimization**: Efficient audio streaming and dynamic sound effect management

## Development Team

Part of the **Super Squad Scary Squad (SSSS)** collaborative project at RMIT University, focusing on advanced mixed reality experiences and AI-driven gameplay mechanics.

## Recent Updates

### Latest Features (October 2025)
- **Enhanced Audio System**: Comprehensive sound effect improvements and spatial audio integration
- **Vosk Speech Recognition**: Full offline voice recognition system for immersive voice interactions
- **Peepers Event Puzzle**: New supernatural entity deterrence mechanic requiring visual attention
- **Cut Puzzle Integration**: Additional wire-cutting challenge for enhanced puzzle progression  
- **Dynamic Lighting System**: Expanded environmental lighting control for atmospheric storytelling
- **Improved Game Flow**: Streamlined puzzle progression with better state management
- **Performance Optimizations**: Audio bug fixes and improved system stability

## Contributing

We welcome contributions to enhance the horror experience and AI behaviors:

1. Fork the repository
2. Create feature branches for new puzzle types or AI improvements  
3. Test thoroughly in both desktop and VR modes
4. Submit pull requests with detailed descriptions

## Research Applications

This project serves as a research platform for:
- **Behavioral AI Learning**: How AI can adapt to individual player habits in real-time
- **Mixed Reality Horror Design**: Integrating physical environments into virtual fear experiences
- **Sitting VR Experiences**: Designing compelling MR content for seated users
- **Psychological Pressure Systems**: Using AI learning to create escalating tension
- **Environmental Awareness Gaming**: Blending real and virtual spatial awareness
- **Voice-Activated VR Interfaces**: Integrating speech recognition for immersive interactions
- **Multi-Modal Puzzle Design**: Combining visual, auditory, and voice-based challenge systems

## License

This project is developed for educational and research purposes at RMIT University. Please respect academic integrity guidelines when referencing or building upon this work.

---

**Contact**: For technical questions or collaboration opportunities, please reach out through the RMIT Computing Technologies department.
