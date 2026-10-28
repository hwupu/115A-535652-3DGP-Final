using UnityEngine;
using UnityEngine.SceneManagement;

namespace Project3DGP.EditorTools
{
    /// <summary>
    /// The standard hierarchy of a level scene (#8, docs/architecture.md §2).
    /// Used by Magic > Level > Create Level Scene Skeleton and Validate Level Scene.
    /// Empty slots are filled with the shared prefabs once the owners' code exists (Sprint 3).
    /// </summary>
    internal static class LevelSkeleton
    {
        public enum Shape { Empty, Plane, Cylinder }

        public sealed class Slot
        {
            public string Path;      // hierarchy path from the scene root, e.g. "Managers/LevelManager"
            public Shape Shape;
            public Vector3 Position; // local position, used only when the slot is created
            public Vector3 Scale = Vector3.one;
            public bool Trigger;     // make the primitive's collider a trigger
            public string Purpose;   // shown by Validate
            public string SatisfiedBy; // another path that counts as this slot (e.g., Terrain instead of Ground)
        }

        // Parents come before their children. Order = order in the Hierarchy.
        public const string TerrainPath = "Environment/Course/Terrain";

        public static readonly Slot[] Slots =
        {
            new Slot { Path = "Systems", Purpose = "Level managers (no visuals)" },
            new Slot { Path = "Systems/LevelManager", Purpose = "LevelManager (Pongpat, #27)" },
            new Slot { Path = "Systems/DangerManager", Purpose = "DangerManager + DangerConfig (Pongpat, #7)" },
            new Slot { Path = "Systems/ScoreKeeper", Purpose = "ScoreKeeper (Pongpat, #43)" },
            new Slot { Path = "Systems/ShotController", Purpose = "ShotController (Henry, #13)" },

            new Slot { Path = "Cameras", Purpose = "Cameras" },
            new Slot { Path = "Cameras/CameraRig", Purpose = "CameraRig (Bahadir, #19)" },

            new Slot { Path = "Environment", Purpose = "Everything static: light, ground, walls, props" },
            new Slot { Path = "Environment/Lighting", Purpose = "Directional Light, Global Volume, sky" },
            new Slot { Path = "Environment/Course", Purpose = "Playable greybox: fairway, slopes, ramps" },
            new Slot
            {
                Path = "Environment/Course/Ground", Shape = Shape.Plane, Scale = new Vector3(5f, 1f, 5f),
                Purpose = "Ground plane (50 x 50 m), or a Terrain", SatisfiedBy = TerrainPath,
            },
            new Slot { Path = "Environment/Boundary", Purpose = "Walls or edges that keep the ball in" },
            new Slot { Path = "Environment/Props", Purpose = "Decoration (counts toward >= 100 objects, #74)" },

            new Slot { Path = "Gameplay", Purpose = "Things the ball and player interact with" },
            new Slot { Path = "Gameplay/BallSpawn", Position = new Vector3(0f, 0.1f, 0f), Purpose = "The tee: where the ball starts" },
            new Slot { Path = "Gameplay/Player", Purpose = "GolfBall + GolferAvatar (Henry, #14, #88)" },
            new Slot { Path = "Gameplay/Goal", Purpose = "LevelGoal group" },
            new Slot
            {
                Path = "Gameplay/Goal/Cup", Shape = Shape.Cylinder, Position = new Vector3(0f, 0.01f, 20f),
                Scale = new Vector3(0.5f, 0.01f, 0.5f), Trigger = true, Purpose = "Cup placeholder (Pongpat, #18)",
            },
            new Slot { Path = "Gameplay/Exit", Purpose = "LevelExit: the gate / portal to the next level (Henry, #38)" },
            new Slot { Path = "Gameplay/Hazards", Purpose = "Level hazards: cat, wind, ice, ..." },
            new Slot { Path = "Gameplay/InvalidZones", Purpose = "InvalidZone triggers: water, out of bounds (Henry, #14)" },

            new Slot { Path = "UI", Purpose = "HUD canvas and menu dialog (Henry, #20; Pongpat, #77)" },
        };

        /// <summary>Default scene objects that are moved into their group if they sit at the scene root.</summary>
        public static readonly (string Name, string Parent)[] Moves =
        {
            ("Main Camera", "Cameras"),
            ("Directional Light", "Environment/Lighting"),
            ("Global Volume", "Environment/Lighting"),
        };

        /// <summary>Finds a GameObject by hierarchy path in the scene, including inactive ones.</summary>
        public static GameObject Find(Scene scene, string path)
        {
            var parts = path.Split('/');
            foreach (var root in scene.GetRootGameObjects())
            {
                if (root.name != parts[0])
                    continue;
                if (parts.Length == 1)
                    return root;
                var child = root.transform.Find(string.Join("/", parts, 1, parts.Length - 1));
                if (child != null)
                    return child.gameObject;
            }
            return null;
        }

        /// <summary>True if the slot, or the object that can stand in for it, is in the scene.</summary>
        public static bool Exists(Scene scene, Slot slot) =>
            Find(scene, slot.Path) != null || (slot.SatisfiedBy != null && Find(scene, slot.SatisfiedBy) != null);

        public static string ParentPath(string path)
        {
            var slash = path.LastIndexOf('/');
            return slash < 0 ? null : path.Substring(0, slash);
        }

        public static string Name(string path) => path.Substring(path.LastIndexOf('/') + 1);
    }
}
