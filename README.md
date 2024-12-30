# EEG-Driven Virtual Reality Interventions for Mental Health

**EEG-Driven Virtual Reality Interventions for Mental Health** is a cutting-edge project that integrates EEG (Electroencephalogram) technology, Virtual Reality (VR), and Generative AI to provide personalized mental health interventions. By offering adaptive VR scenarios and real-time analysis, this project aims to revolutionize mental health care for athletes, coaches, and general users.

---

## Key Features

### **1. Real-Time EEG Monitoring**
- Tracks brainwave activity (alpha, beta, theta, delta bands).
- Detects mental states such as focus, stress, and relaxation in real-time.

### **2. Adaptive VR Scenarios**
- Immersive VR environments tailored for therapeutic interventions.
- Adaptive feedback for personalized mental health support.

### **3. Generative AI Insights**
- Processes EEG data using advanced AI models.
- Generates detailed mental health reports and actionable feedback.

### **4. Modular System Design**
- Separate modules for athletes, coaches, and data analysis.
- Easy to integrate and expand.

---

## Folder Structure

```
Repository ->
   Athlete VR Scenario       # Contains Unity scenes for athlete-focused VR interventions.
   Coach GUI                 # Unity-based GUI for coaches to monitor and interact.
   Signal and Gen AI Analysis# Python modules for signal processing and generative AI.
   Unity - Python BackEnd    # Backend integration between Unity and Python.
```

---

## Installation and Usage

### Prerequisites
- Unity (Version 2022 or later).
- Python 3.8+.
- VR hardware compatible with Unity.
- EEG devices (e.g., Muse 2, Emotiv Epoch X).

### Setup
1. Clone the repository:
   ```bash
   git clone https://github.com/SabariKameswaran/Team-Dexians-EEG-Driven-VR-Interface-0xDay-Hack-Day.git
   cd Team-Dexians-EEG-Driven-VR-Interface-0xDay-Hack-Day
   ```
2. Install Python dependencies:
   ```bash
   pip install -r requirements.txt
   ```
3. Connect EEG and VR hardware.

---

## Commands

### **For Unity**

1. **Athlete VR Adaptive Therapy**:
   - Run the **Dockstation Scene** for adaptive therapy scenarios:
     - Open Unity Editor.
     - Navigate to the `Athlete VR Scenario/Asset/Scenes` folder.
     - Select `Dockstation.unity` and click **Play**.
   
![00](https://github.com/user-attachments/assets/fd750ac9-ccf1-4a75-a9ef-d37b91430d6f)


2. **Coach GUI**:
   - Run the **Titlescene** for the coach interface:
     - Open Unity Editor.
     - Navigate to the `Coach GUI/Asset/0xDay Demo` folder.
     - Select `Athlete GUI.unity` and click **Play**.

![1 final](https://github.com/user-attachments/assets/25890eea-a4d6-45bc-94a5-097a2053aca2)


### **For Generative AI**

1. **Install Requirements**:
   ```bash
   pip install -r requirements.txt
   ```

2. **Signal Processing Interface**:
   - Run the signal processing module:
     ```bash
     streamlit run signal_processing.py
     ```

3. **Generative AI Reports**:
   - Launch the Generative AI module:
     ```bash
     streamlit run genai.py
     ```

---

## Applications

- **Athlete Training**: Improve mental resilience and focus.
- **Mental Health Therapy**: Adaptive VR for relaxation and stress management.
- **Research**: Advanced insights into EEG and VR applications.

---

## Credits

This project was developed by **Team Dexians**:

1. **Sabari Kameswaran S**
2. **Shaunak J**
3. **Praveen S**
4. **Mohesh T**
5. **Ganesh R**

---

### *Innovating mental health care with EEG and VR technology.*
