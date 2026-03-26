# Echoes Of Me 🎭

A 2D platform-puzzle game developed for Global Game Jam 2026 under the theme "Mask". 

![EOM](https://github.com/user-attachments/assets/1ee2f7d9-8167-4b2a-b1fc-8993e02c5d65)

## 📌 Overview
**Echoes Of Me** explores a "dual-reality" mechanic where the player equips a mask to dynamically reveal hidden platforms and alter the timeline. I served as the sole programmer for this project, working closely with newly met artists and designers to deliver a polished, playable build under a strict deadline.

* **Engine:** Unity (2021.3.45f2)
* * **Language:** C#
* **Role:** Sole Programmer
* **Context:** Game Jam 

## 👥 The Team

* **Wei Qi:** - Game Programmer, Mechanics Designer

* **Jing Le** - Story writer 1, UI Designer 1, Sound Designer 1, Trans Scene Editor 1, Level Designer 3

* **Xiao Tong** - Story writer 2, UI Designer 2, Sound Designer 2,  Trans Scene Editor 2

* **Derrick** - Game designer, Level Designer 1, Pixel Artist

* **Jason** -  Game designer, Level designer 2, Mechanics Designer

## ⚙️ Key Systems & Architecture
To achieve the timeline-swapping mechanics smoothly within the time limit, I built the following core systems:

* **Dynamic Timeline Reveal (`MaskDetector.cs`)**: Utilized Unity's `SpriteMask` component driven by an `AnimationCurve` within a Coroutine for a smooth, expanding visual reveal. 
* **Decoupled Physics Toggling**: Attached a 2D Trigger to the mask's radius. As the visual mask expands, `OnTriggerEnter2D` detects actors tagged as "Hidden" and dynamically toggles their physical collisions on/off. This ensured players could only interact with platforms actively enveloped by the mask.
* **Rapid Iteration Controller**: Developed the 2D physics-based character controller and custom puzzle logic from scratch.

## 🎮 Play the Game
Experience the finished game jam submission on [itch.io](https://limwq.itch.io/echoes-of-me).

## 🚀 How to Run the Project Locally
1. Clone this repository.
2. Open Unity Hub and click `Add Project from Disk`.
3. Select the cloned folder.
