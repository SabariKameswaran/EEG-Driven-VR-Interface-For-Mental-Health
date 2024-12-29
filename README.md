# NeuroVR: EEG-Based VR Monitoring for Athletes

**NeuroVR** is an innovative system that integrates **EEG (Electroencephalogram) technology**, **Virtual Reality (VR)**, and **Artificial Intelligence (AI)** to monitor and enhance athletes' mental states during training. By providing real-time feedback, personalized insights, and immersive environments, NeuroVR helps athletes optimize their performance, manage stress, and achieve peak mental focus.

---

## Key Features

### **1. Real-Time Brainwave Monitoring**
- Utilizes **EEG devices** ( Muse 2) to track brainwave activity (alpha, beta, theta, delta bands).
- Detects mental states such as focus, calmness, and stress in real-time.

### **2. Immersive VR Training**
- Offers a **Unity-based VR environment** tailored to the athlete's sport.
- Provides an engaging, distraction-free space for training and performance improvement.

### **3. AI-Driven Insights**
- Processes EEG data using machine learning models to classify mental states.
- Generates **detailed reports** with actionable feedback and long-term performance metrics.

### **4. Gamified Visualization**
- Displays brainwave data and mental state feedback through a game-like interface in **Unity**.
- Coaches and athletes can see real-time visual cues (e.g., rotating objects) for intuitive feedback.

### **5. Personalized Feedback**
- Provides suggestions for mental training exercises based on detected patterns.
- Adapts to individual athletes, offering a tailored approach to mental resilience.

---

## System Architecture

1. **Data Collection**: EEG devices capture brainwave signals during VR simulations.
2. **Data Processing**: Signal processing algorithms filter noise and extract relevant features.
3. **Visualization**: Brainwave metrics are displayed in real-time on the GUI.
4. **Reporting**: AI models generate session-specific insights and long-term metrics.

---

## Installation and Usage

### Prerequisites
- **EEG Devices**: Compatible with  Muse 2 headset.
- **VR Hardware**: A VR headset supporting Unity-based environments.
- **Software Requirements**:
  - Unity (Version 2022 or later)
  - Python (for signal processing and AI models)
  - Streamlit (for GUI and generative AI reports)
  - Groq API

### Setup
1. Clone the repository:
   ```bash
   git clone https://github.com/shaunak/neurovr.git
   cd neurovr
   ```
2. Install Python and Streamlit dependencies:
   ```bash
   pip install -r requirements.txt
   ```
3. Connect EEG and VR hardware.

---

## Unity Commands

### **1. Athlete VR Adaptive Therapy**
Run the **Dockstation Scene** in Unity for adaptive VR therapy scenarios:
   - Open Unity Editor.
   - Navigate to the `Scenes` folder.
   - Select `Dockstation.unity` and click **Play**.

### **2. Coach GUI**
Run the **Titlescene** in Unity for the coach interface:
   - Open Unity Editor.
   - Navigate to the `Scenes` folder.
   - Select `Titlescene.unity` and click **Play**.

---

## Generative AI Commands

### **1. Install Requirements**
Ensure all dependencies for the Generative AI module are installed:
   ```bash
   pip install -r requirements.txt
   ```

### **2. Signal Processing Interface**
Run the **signal processing module** via Streamlit:
   ```bash
   streamlit run signal_processing.py
   ```

### **3. Generative AI Reports**
Launch the **Generative AI reporting module**:
   ```bash
   streamlit run genai.py
   ```

---

## Applications

- **Sports Training**: Optimize mental performance for athletes in individual and team sports.
- **Mental Health Therapy**: Assist in stress management and resilience training.
- **Research**: Provide neuroscientific insights for sports psychology and cognitive training.

---

## Future Developments

- **Wearable Integration**: Add biometric data (heart rate, sweat levels) for comprehensive profiling.
- **Cloud Storage**: Enable data access and analytics from multiple locations.
- **Multi-Sport Support**: Customize VR environments for diverse sports scenarios.
- **Remote Training**: Facilitate virtual coaching with real-time feedback.

---

## Feedback and Contributions

We value your feedback and invite contributions to improve NeuroVR. If you have suggestions, feature requests, or bug reports, please open an issue in this repository or contact us directly.

---



## License

This project is licensed under the **MIT License**. See the [LICENSE](LICENSE) file for details.

---

### *Empowering athletes with technology to achieve their full potential.*