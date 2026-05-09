# 🇮🇳 TrueRead: AR Devanagari Learning Ecosystem

![Unity](https://img.shields.io/badge/Unity-100000?style=for-the-badge&logo=unity&logoColor=white)
![C#](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=c-sharp&logoColor=white)
![ONNX](https://img.shields.io/badge/ONNX-005CED?style=for-the-badge&logo=onnx&logoColor=white)
![Edge ML](https://img.shields.io/badge/Unity_Sentis-FF7B00?style=for-the-badge)

TrueRead is an Augmented Reality mobile application designed to revolutionize the pedagogy of the Hindi script (Devanagari). By combining a custom Convolutional Neural Network (CNN) with the Unity Sentis inference engine, TrueRead performs **zero-latency, 100% offline edge inference** to recognize handwritten characters and instantly project 3D educational models and phonetic audio into the real world.

---

## 📸 Application Interface & Features

![TrueRead UI Screenshots](./photo-collage.png.jpg)

*(From top-left to bottom-right)*
1. **Secure Login:** Clean, accessible entry point for user profiles.
2. **Model Selection Hub:** Modular architecture allowing users to select the Devanagari OCR model (with future scope for generalized Object Recognition).
3. **Bilingual Dashboard (Hindi UI):** A custom event-driven Localization Manager instantly toggles the entire UI between English and Hindi. Tracks daily streaks, mastery (12/46), and accuracy.
4. **Real-Time AR Scanner:** The core edge-inference loop where the camera detects characters and overlays 3D models with zero network latency.
5. **Interactive Quiz Mode:** A dynamic spaced-repetition quiz to reinforce learning, asking users to identify Devanagari characters via multiple choice.
6. **Gamified Progression System:** A comprehensive mastery dashboard tracking Experience Points (XP), session streaks, and unlockable achievements (e.g., *First Step*, *On Fire!*, *Hindi Hero*) using persistent local storage (`PlayerPrefs`).

---

## ✨ Key Technical Capabilities

* **Real-Time Edge Inference:** Runs a 46-class CNN entirely on-device using Unity Sentis. No cloud APIs, zero network latency, and complete data privacy.
* **Multi-Modal AR Feedback:** Instantly overlays 3D object models (FBX), TextMeshPro typography, and native text-to-speech audio pronunciation.
* **Matrix Transformation Pipeline:** Implements dynamic geometric correction and horizontal flipping of `WebCamTexture` data to align real-world hardware input with the model's training tensors.
* **Optimized for Mobile:** Designed to run smoothly on standard Android hardware (e.g., ARM64, Snapdragon 600 series equivalent) without thermal throttling.

---

## 🧠 Machine Learning Architecture

The core vision model targets all **46 Devanagari classes** (33 consonants, 10 digits, 3 conjuncts). It was trained on a massively augmented dataset of 92,000 images, achieving robust generalization against high intra-class handwriting variance.

* **Training Environment:** Python, TensorFlow/Keras, Google Colab
* **Export Format:** ONNX (Open Neural Network Exchange)
* **Image Input:** 32x32 Grayscale spatial tensors
* **Validation Accuracy:** >95%
* **Initial F1 Score:** ~99.2%

*Dataset Reference:* [Devanagari Handwritten Character Dataset (UCI)](https://doi.org/10.24432/C5XS53)

---

## 🚀 How to Run Locally

1. Clone this repository to your local machine.
2. Open the `Unity_App` folder using **Unity 2022.3.x LTS** (Ensure Android Build Support is installed via Unity Hub).
3. The `.onnx` inference model is already mapped within the Sentis worker environment.
4. Navigate to `Scenes/MainMenu` in the Unity Editor and press Play to experience the UI flow.
5. To test the AR scanner in the editor, open `Scenes/ScanScene` (requires an active webcam).

---
*Developed by Tanishq Arun Ingole (IIIT Pune) as a B.Tech Major Project.*
