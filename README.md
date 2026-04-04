# 🎯 FOV Training VR App

A Unity-based visual training application designed to improve peripheral vision, reaction time, and visual awareness through structured training stages.

---

## 🚀 Features

- Multi-stage visual training system (Stage A → E)
- Adaptive difficulty based on performance
- Fixation point training (attention & reaction)
- Road-crossing safety simulation
- Color modes for color-blind users
- Session reporting + email export
- Clean launcher UI with configurable settings

---

## 🧠 Training Flow

1. User enters:
   - Name
   - ID
   - Eye (Right / Left)

2. User configures:
   - Timing
   - Motion
   - Fixation settings
   - Color mode

3. Training starts:
   - Stages progress automatically
   - Requires ≥80% success to advance

4. Session ends:
   - Report generated
   - Displayed in launcher
   - Can be sent via email

---

## 🎨 Color Modes (Accessibility)

Supports multiple visual modes:

- **Normal**
- **Protanopia (Red-blind)**
- **Deuteranopia (Green-blind)**
- **Tritanopia (Blue-blind)**

Each mode uses optimized color palettes for better visibility.

---

## 🗂️ Project Structure

### 📁 Config
- `TrainingConfigSO` – ScriptableObject with training settings
- `TrainingProgram` – Defines stage flow

### 📁 Core
- `AdaptiveDifficulty` – Adjusts difficulty based on performance
- `ApplyPrefsToConfig` – Loads launcher settings into config
- `AppManager` – Handles app state (menu, training, report)
- `FixationManager` – Controls fixation point behavior
- `StageSequenceController` – Manages stage transitions
- `TrainingSession` – Core session logic + scoring
- `UserInput` – Handles input (space, etc.)
- `VisualAngleUtils` – Converts visual angles to meters
- `XRLoaderController` – Initializes XR

### 📁 Logging
- `SessionLogger` – Saves reports (JSON + TXT)
- `SessionResult` – Stores session data

### 📁 UI
- `LauncherUIController` – Main launcher logic
- `ProgramStorage` – Saves user/program settings

### 📁 Stages
- `StageA_StaticMatch` – Static shape matching
- `StageB_StaticMatch` – Extended matching
- `StageC_MovingMatch` – Moving shapes
- `StageD_SceneFit` – Scene-based tasks
- `StageE_SafeToCross` – Road crossing safety

- `StageA_BoardUI`, `StageC_BoardUI`, etc. – UI for each stage
- `StimulusSpawner` – Spawns stimuli in VR space
- `ITrainingStage` – Interface for all stages

---

## 📊 Report Example
Name: Maya Cohen  
ID: 987654321  
Training left eye  

Total Correct Answers: 18 of 25 , 72%  
Fixation interactions: 12 of 15 , 80%  
True answers for same shapes: 10 of 14 , 71%  
False presses for different shapes: 4 of 20 , 20%  
Average answering speed is 0.93 seconds  
Training Time: 6 minutes


---

## ⌨️ Controls

- **SPACE** → Respond
- **ESC** → End training and return to launcher

---

## 🖥️ Build

- Platform: Windows
- Mode: Windowed application
- Scene flow:
  - LauncherScene → TrainingVRScene → LauncherScene

---

## 📧 Reports

- Reports saved as:
- `.json`
- `.txt`
- Send via SMTP (Gmail supported)

---

## ⚙️ Requirements

- Unity (URP optional)
- OpenXR enabled
- Windows PC

---

## 🧩 Future Improvements

- More adaptive difficulty logic
- Better RTL (Hebrew) UI handling
- Additional training stages
- Cloud report storage

---

## 👨‍💻 Author

Developed by Mohammad Dukhi  
