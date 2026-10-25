using System.Linq;

namespace Project3DGP.EditorTools
{
    /// <summary>
    /// The course's mini-exercise catalogue (docs/references/course/mini-exercises.md)
    /// and our assignment plan (docs/mini-exercises.md). Data only.
    /// </summary>
    public static class MiniExerciseCatalog
    {
        public sealed class Exercise
        {
            public readonly string Code;        // "4.C.4"
            public readonly string FolderName;  // "ObjectInteraction"
            public readonly string Title;
            public readonly string[] Checklist;

            public Exercise(string code, string folderName, string title, params string[] checklist)
            {
                Code = code;
                FolderName = folderName;
                Title = title;
                Checklist = checklist;
            }

            /// <summary>"4.C.4" -> "4C4", used in folder and scene names.</summary>
            public string ShortCode => Code.Replace(".", "");
        }

        public sealed class Assignment
        {
            public readonly string Member;
            public readonly string Code;
            public readonly int Issue;
            public readonly string DemoDate;

            public Assignment(string member, string code, int issue, string demoDate)
            {
                Member = member;
                Code = code;
                Issue = issue;
                DemoDate = demoDate;
            }
        }

        public static readonly string[] Members = { "Henry", "Bahadir", "Pongpat" };

        public static readonly Exercise[] Exercises =
        {
            new Exercise("4.A.1", "HierarchicalModel", "Animation of a hierarchical model",
                "Hierarchical model with >= 2 joints and 3 objects (A, B, C)",
                "At least one model with >= 5 components",
                "A rotates about one axis, B about a different axis",
                "C moves periodically (sin/cos) along B's local y-axis",
                "Keys change the joint rotation directions"),
            new Exercise("4.A.2", "CharacterNavigation", "Character navigation",
                "Mouse and/or keyboard control of a human-like character",
                ">= 10 obstacles",
                "Environment has another character with >= 2 legs, a house, a tree, or a water stream",
                "Legs animate properly while moving and while standing"),
            new Exercise("4.A.3", "GamePortals", "Game portals",
                "Controllable character",
                ">= 3 portals teleport the character to other locations",
                ">= 1 particle system at each portal",
                "Particle burst around the character after teleporting"),
            new Exercise("4.B.1", "AStarPathFinding", "A* path finding",
                "Environment with obstacles",
                "Click the floor to set a target",
                "Path shown with small spheres (count proportional to path length)",
                "Character walks continuously along the path with animation",
                "New target can be set at any time; orientation changes smoothly; no position jumps"),
            new Exercise("4.B.2", "AICharacters", "AI characters",
                "System-controlled characters follow the player",
                "Maze with multiple paths to the player",
                "Smooth animation",
                "They attack when near the player"),
            new Exercise("4.C.1", "MassSpring", "Mass-spring system",
                ">= 10 threads of >= 10 particles connected by springs",
                "Each thread has one particle fixed to an obstacle",
                "Simulated under gravity",
                "Keys change the gravity direction"),
            new Exercise("4.C.2", "GroupSteering", "Group steering",
                ">= 3 steering behaviours (wander, pursuit, arrival, evasion, escape)",
                ">= 30 agents, all clearly visible",
                "Agents get close but never overlap",
                "Keys switch the behaviour"),
            new Exercise("4.C.3", "DeformableSurfaces", "Deformable surfaces",
                "Two surfaces under wind and gravity, each >= 20x40 particles",
                "Different particle counts and different shapes",
                "One side of each fixed to a pole; surfaces and poles have materials",
                "Real time; keys change the gravity or wind direction"),
            new Exercise("4.C.4", "ObjectInteraction", "Character interaction with objects",
                ">= 100 obstacles around a mouse/keyboard-controlled character",
                "Obstacles are pushed away on contact",
                "Situations where >= 10 obstacles collide with each other",
                "Extra: obstacles vanish after 2 s and respawn randomly without overlapping"),
            new Exercise("4.D.1", "EnvironmentEffects", "Environment effects on character",
                "Controllable character",
                ">= 3 places that drastically change walking speed or launch the character",
                "At least one jump place",
                "Objects placed around those locations; smooth animation"),
            new Exercise("4.D.2", "CombatEffects", "Character combat effects",
                "Character A fights character B",
                ">= 3 particle systems played when an action impacts the other character",
                ">= 3 sound effects played at reasonable moments"),
            new Exercise("4.D.3", "CharacterParticleEffects", "Character particle effects",
                "Controllable character",
                ">= 3 particle systems and sound effects at reasonable moments (collect, run, jump, ...)",
                "Each event triggers both its particle and its sound"),
            new Exercise("4.E.1", "SaveLoad", "Save & Load",
                "A character plus >= 2 moving creatures",
                "Save and load at any time to a save file",
                "State covers positions, speeds, orientations and actions of all characters",
                "Loading restores the state (at least positions)"),
            new Exercise("4.E.2", "FunctionSelection", "Function selection",
                "Menu with >= 3 labelled buttons, each showing its content",
                "(1) A game level: environment, avatar, monsters, meaningful objects",
                "(2) Settings with >= 2 items (resolution, volume, brightness, gamma)",
                "(3) Character selection (>= 2 characters)",
                "(4) A demo video"),
            new Exercise("4.E.3", "GameUI", "Game UI",
                "Interactive UI for character/gear selection or portraits",
                "Selected item highlighted with particle or sound effects",
                "Attribute bars (stamina, energy, XP, health) update with events",
                "Minimap with a top-down view"),
        };

        /// <summary>Our plan (proposal section 12). Order within a member is provisional.</summary>
        public static readonly Assignment[] Plan =
        {
            new Assignment("Henry", "4.C.4", 57, "Nov 16"),
            new Assignment("Henry", "4.E.3", 58, "Nov 16"),
            new Assignment("Henry", "4.A.1", 59, "Dec 21"),
            new Assignment("Henry", "4.A.3", 60, "Dec 21"),
            new Assignment("Bahadir", "4.A.2", 61, "Nov 16"),
            new Assignment("Bahadir", "4.D.1", 62, "Nov 16"),
            new Assignment("Bahadir", "4.B.1", 63, "Dec 21"),
            new Assignment("Bahadir", "4.B.2", 64, "Dec 21"),
            new Assignment("Pongpat", "4.E.2", 65, "Nov 16"),
            new Assignment("Pongpat", "4.C.4", 66, "Nov 16"),
            new Assignment("Pongpat", "4.D.3", 67, "Dec 21"),
            new Assignment("Pongpat", "4.E.1", 68, "Dec 21"),
        };

        public static Exercise Find(string code) => Exercises.First(e => e.Code == code);
    }
}
