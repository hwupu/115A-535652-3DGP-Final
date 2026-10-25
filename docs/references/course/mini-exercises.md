# Mini-project exercises (individual)

Source: `3D Game Programming Project Exercises_202609.docx`

## Rules
- **Individual work.** Each member picks **at least 4** exercises and implements them **on their own**. All source code must be written by the member, and every program must run in **real time**.
- **Demo one exercise per demo day** to the TAs (dates are on the course schedule). **Upload the programs to E3** before the deadline.
- **Evaluation:** demo the program, present the source code, explain the method, and **modify the program live based on the TAs' instructions**.
- **No overlap with assignments.** Reusing assignment scripts or models is allowed, but reused content **does not count**. Only new, substantially different work counts, and during the demo we must **clearly point out which parts are new**.
- Allowed sources: course code, our assignments, or our own contributions to the course project. Exercises highly relevant to the course project are **strongly suggested**.
- **Third-party code** (internet, Unity samples) must be modified with **at least 3 new features**. In that case the scene must also **create at least 20 objects with a loop we write ourselves**, placed reasonably, and the scene must look attractive. List the reference links.
- Engine: Ogre3D, Unity, Unreal or others.

### Proposing our own exercise
Send it to the TAs **at least one week before** the demo day. Format:
- Name (at least two words).
- Problem/technique description, 50–120 words (Chinese or English).
- It must show something on screen.
- Some objects (e.g., characters, monsters) must be animated.
- At least one clearly described technique.
- **At least 50 objects** in the environment.

## Exercise catalogue

### 4.A Scene/Model
- **4.A.1 Animation of a hierarchical model.** A hierarchical model with ≥ 2 joints and 3 objects (A, B, C), plus at least one model with ≥ 5 components. Rotate A about one axis and B about a different axis. Animate C periodically (sin/cos) along B's local y-axis. Keys change the joint rotation directions.
- **4.A.2 Character navigation.** Mouse and/or keyboard control of a **human-like character**. ≥ 10 obstacles. The environment includes at least one of: another character with ≥ 2 legs, a house, a tree, or a water stream. **Legs animate properly** while moving and while standing.
- **4.A.3 Game portals.** Control a character. **≥ 3 portals** teleport the character to other locations. **≥ 1 particle system at each portal**, plus a particle burst around the character after teleporting.

### 4.B AI
- **4.B.1 A\* path finding.** An environment with obstacles. **Click the floor to set a target.** Compute the path and show it with small spheres (count proportional to path length). The character walks continuously along the path with animation. A new target can be set at any time, orientation changes smoothly, and the character never jumps between positions.
- **4.B.2 AI characters.** System-controlled characters follow the player character through a **maze with multiple paths** to the player. Smooth animation. When near the player, they **attack**.

### 4.C Simulation/Animation
- **4.C.1 Mass-spring system.** **≥ 10 threads of ≥ 10 particles** connected by springs (neighbours may also be connected). Each thread has one particle fixed to an obstacle (e.g., a sphere). Simulate under gravity. Keys change the gravity direction.
- **4.C.2 Group steering.** **≥ 3 steering behaviours** for a group (wander, pursuit, arrival, evasion, escape). Agents sometimes move close together but never overlap. **≥ 30 agents**, all clearly visible. Agents can be spheres, characters, cubes or cylinders (see the course slides). Keys switch the behaviour.
- **4.C.3 Deformable surfaces.** **Two** deformable surfaces under wind and gravity, each **≥ 20×40 particles**, with different particle counts and different shapes. One side of each is fixed to a pole. Surfaces and poles have materials. Real time. Keys change the gravity or wind direction.
- **4.C.4 Character interaction with objects.** **≥ 100 obstacles** around a character controlled by mouse or keyboard. Obstacles are pushed away on contact, and there must be situations where **≥ 10 obstacles collide with each other**. Extra: obstacles vanish after 2 s and the same number respawn randomly without overlapping.

### 4.D Special effects
- **4.D.1 Environment effects on character.** Control a character. **≥ 3 places** that drastically change walking speed (faster or slower) or launch the character into the air, with **at least one jump place**. Objects are placed around those locations. Smooth animation.
- **4.D.2 Character combat effects.** Character A fights character B. **≥ 3 particle systems and ≥ 3 sound effects**, played at reasonable moments (when an action impacts the other character). B may stand still or move.
- **4.D.3 Character particle effects.** Control a character. **≥ 3 particle systems and sound effects**, played at reasonable moments (collecting items, running, jumping, ...). Each event triggers both its particle and its sound.

### 4.E Game menu/GUI
- **4.E.1 Save & Load.** A character plus **≥ 2 moving creatures**. Save and load the game state **at any time** to a save file. The state covers positions, walking speeds, orientations and actions of the character and all creatures, and loading restores them, at least the positions.
- **4.E.2 Function selection.** A menu with **≥ 3 labelled buttons**, each showing its content: (1) a game level (environment, avatar, monsters, meaningful objects), (2) game settings (≥ 2 items such as resolution, volume, brightness, gamma), (3) character selection (≥ 2 characters), (4) a demo video.
- **4.E.3 Game UI.** An interactive UI for character/gear selection or portraits. **Highlight the selected item with particle or sound effects.** Attribute bars (stamina, energy, XP, health, ...) update with events (damage, healing, ...). A **minimap** shows a top-down view.
