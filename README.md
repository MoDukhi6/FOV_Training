# 🧠 Visual Training App (Unity)

A Unity-based visual field training application designed to improve reaction time, fixation control, and peripheral awareness through multiple progressive stages.

---

## 🚀 Features

- 🎯 Multi-stage visual training system (A → E progression)
- 👁️ Fixation point control (size & vertical position)
- ⚡ Reaction time measurement
- 🧠 Adaptive difficulty system
- 📊 Detailed session reports
- 📧 Email report sending
- 🎮 Keyboard-based interaction (SPACE / ESC)
- 🪟 Windowed launcher interface

---

## 🧩 Project Structure

### 🔧 Config
- **TrainingConfigSO** – Stores all training parameters (timing, sizes, motion, fixation).
- **TrainingProgram** – Defines the structure and order of training stages.

---

### ⚙️ Core
- **AdaptiveDifficulty** – Adjusts difficulty dynamically based on performance.
- **ApplyPrefsToConfig** – Loads launcher settings into runtime config.
- **AppManager** – Manages app states and scene transitions.
- **FixationManager** – Controls fixation point behavior and interaction.
- **StageSequenceController** – Handles stage progression logic.
- **TrainingSession** – Core session logic (timing, scoring, reporting).
- **UserInput** – Handles user input (space key interactions).
- **VisualAngleUtils** – Converts visual angles (degrees) to world space.
- **XRLoaderController** – Handles XR/VR initialization.

---

### 📊 Logging
- **SessionLogger** – Saves session reports (JSON + TXT).
- **SessionResult** – Stores all performance metrics and results.

---

### 🖥️ UI
- **LauncherUIController** – Controls launcher UI, validation, and actions.
- **ProgramStorage** – Handles local storage of program/session data.

---

### 🎯 Stages

#### General
- **ITrainingStage** – Interface for all training stages.
- **StimulusSpawner** – Spawns stimuli based on visual angles.

---

#### Stage A – Static Match
- **StageA_BoardUI** – Displays shape pairs.
- **StageA_StaticMatch** – Match identical/different shapes.

---

#### Stage B
- **StageB_StaticMatch** – More complex static matching.
- **StageB_Motion** – Adds motion to stimuli.

---

#### Stage C – Motion
- **StageC_BoardUI** – UI for moving stimuli.
- **StageC_Motion** – Motion logic.
- **StageC_MovingMatch** – Matching with moving objects.

---

#### Stage C1 – Reaction
- **StageC1_BoardUI** – UI for reaction tasks.
- **StageC1_ReactionToTarget** – Reaction time training.

---

#### Stage D – Scene Interaction
- **StageD_BoardUI** – Scene-based UI.
- **StageD_FridgeTargets** – Target detection tasks.
- **StageD_SceneFit** – Scene understanding exercises.

---

#### Stage E – Road Crossing
- **StageE_BoardUI** – Road + danger zone UI.
- **StageE_RoadCrossing** – Crossing scenario logic.
- **StageE_SafeToCross** – Determines safe vs dangerous timing.

---

### 🧭 Additional Systems
- **Hemifield** – Handles left/right visual field logic.
- **RoadScenario** – Base logic for road-based simulations.

---

## 🎮 How It Works

1. User enters:
   - Name & ID
   - Training settings (timing, motion, fixation)

2. Press **Start**
3. Training begins
4. User interacts using:
   - `SPACE` → respond
   - `ESC` → exit session

5. System:
   - Tracks performance
   - Adjusts difficulty
   - Generates report

6. After session:
   - Report shown in launcher
   - Can be sent via email

---

