# RevSim-VR-Env
Virtual Reality Enviroment setup in Unity 2022.3.10f1
This enviroment is used for developing a simulation to help making ABI rehabilitation more immersive and engaging.

Unity 2022.3.10f1

## YawVR - Yaw2 
The Unity SDK found on yawvr.com uses a very outdated version of Unity (2017).
The code used here is very likely to be modified before use.

### SDK Issues
- Extreme frame rate dips
- Unsupported OpenXR version in demo
- Shaders not URP supported



## OpenXR
This project uses OpenXR to implement VR.
Currently the simulation is developed with use of the following VR set:
- HTC VIVE Pro 2
- VIVE controllers (2018) 2x
- Base station 2.0 2x

### Hand Tracking
Hand tracking is native in the OpenXR library.
RevSim will be using this feature in simulations. 

### Issues in need of fixing:
- Glitching hands
- Dissapearing hands in distance or specific poses
- Switching between Hands and Controllers


## Game Ideas

### Game 1: Reach objects
Player reaches for objects in a scene while sitting in the YawVR Yaw2.

Enviroments TBD
Scoring system based on reaction time and difficulty

### Game 2: Balance on raft
In this exercise the player is set on a raft on water. The raft will move on the waves, increasing in intensity and varying in patterns.
The waves are simulated on the YawVR Yaw2 and the player needs te keep balance.
Exercise trains the balance reflex of the player.

Scoring system TBD

### Game 3: Electric maze

### Game 4: Biking

### Game 5: 
