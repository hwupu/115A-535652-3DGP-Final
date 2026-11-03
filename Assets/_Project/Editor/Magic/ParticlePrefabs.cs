using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Project3DGP.EditorTools
{
    /// <summary>
    /// Placeholder particle prefabs (#39) in Prefabs/FX, made once and never overwritten: replace a prefab's content
    /// (or tweak it) and every scene instance follows. Two URP particle materials in Materials/FX: a soft round dot
    /// (transparent) and plain opaque squares (confetti, leaves).
    ///   FX_ImpactSparks   one burst, the ball on each shot         (shared)
    ///   FX_Confetti       one burst, the cup                       (shared)
    ///   FX_WaterSplash    one burst, the ball in the pond          (Garden)
    ///   FX_CatZzz         looping, above the sleeping cat          (Garden)
    ///   FX_FallingLeaves  looping, under a tree's crown            (Garden)
    /// </summary>
    internal static class ParticlePrefabs
    {
        public const string Folder = "Assets/_Project/Prefabs/FX";
        const string MaterialFolder = "Assets/_Project/Materials/FX";

        public static GameObject ImpactSparks => Ensure("FX_ImpactSparks", BuildSparks);
        public static GameObject Confetti => Ensure("FX_Confetti", BuildConfetti);
        public static GameObject WaterSplash => Ensure("FX_WaterSplash", BuildSplash);
        public static GameObject CatZzz => Ensure("FX_CatZzz", BuildZzz);
        public static GameObject FallingLeaves => Ensure("FX_FallingLeaves", BuildLeaves);

        static GameObject Ensure(string name, System.Action<ParticleSystem> build)
        {
            var path = $"{Folder}/{name}.prefab";
            if (File.Exists(path))
                return AssetDatabase.LoadAssetAtPath<GameObject>(path);

            var go = new GameObject(name);
            try
            {
                var particles = go.AddComponent<ParticleSystem>();
                particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                build(particles);
                MagicMenuUtil.EnsureFolder(Folder);
                var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
                MagicMenuUtil.Log("Created " + path);
                return prefab;
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        // ---------- the five effects ----------

        static void BuildSparks(ParticleSystem p)
        {
            OneShot(p, count: 24);
            var main = p.main;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.25f, 0.5f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(2.5f, 5f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.04f, 0.08f);
            main.startColor = new ParticleSystem.MinMaxGradient(new Color(1f, 0.85f, 0.4f), new Color(1f, 0.55f, 0.2f));
            main.gravityModifier = 1f;
            Shape(p, ParticleSystemShapeType.Hemisphere, 0.05f, Quaternion.Euler(-90f, 0f, 0f));
            FadeOut(p);
            Material(p, soft: true);
        }

        static void BuildConfetti(ParticleSystem p)
        {
            OneShot(p, count: 70);
            var main = p.main;
            main.startLifetime = new ParticleSystem.MinMaxCurve(1.5f, 2.5f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(4f, 7f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.08f, 0.14f);
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
            main.startColor = RandomColors(new Color(0.95f, 0.35f, 0.4f), new Color(0.35f, 0.65f, 0.95f),
                new Color(0.95f, 0.85f, 0.3f), new Color(0.45f, 0.85f, 0.45f));
            main.gravityModifier = 0.5f;
            Shape(p, ParticleSystemShapeType.Cone, 0.2f, Quaternion.Euler(-90f, 0f, 0f), angle: 25f);
            Spin(p);
            var drag = p.limitVelocityOverLifetime;
            drag.enabled = true;
            drag.drag = 1.5f;
            Material(p, soft: false);
        }

        static void BuildSplash(ParticleSystem p)
        {
            OneShot(p, count: 40);
            var main = p.main;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.6f, 1f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(2f, 4f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.08f, 0.18f);
            main.startColor = new ParticleSystem.MinMaxGradient(new Color(0.75f, 0.9f, 1f, 0.9f), new Color(0.45f, 0.7f, 0.95f, 0.8f));
            main.gravityModifier = 1.2f;
            Shape(p, ParticleSystemShapeType.Cone, 0.15f, Quaternion.Euler(-90f, 0f, 0f), angle: 30f);
            FadeOut(p);
            Material(p, soft: true);
        }

        static void BuildZzz(ParticleSystem p)
        {
            Looping(p, rate: 0.8f);
            var main = p.main;
            main.startLifetime = 2.5f;
            main.startSpeed = 0.3f;
            main.startSize = new ParticleSystem.MinMaxCurve(0.12f, 0.2f);
            main.startColor = new Color(1f, 1f, 1f, 0.85f);
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            Shape(p, ParticleSystemShapeType.Cone, 0.05f, Quaternion.Euler(-90f, 0f, 0f), angle: 10f);
            var sway = p.velocityOverLifetime;
            sway.enabled = true;
            sway.x = new ParticleSystem.MinMaxCurve(0.25f, AnimationCurve.EaseInOut(0f, -1f, 1f, 1f));
            // All three axes must use the same mode (curve), so y and z are flat curves.
            sway.y = new ParticleSystem.MinMaxCurve(0f, AnimationCurve.Linear(0f, 0f, 1f, 0f));
            sway.z = new ParticleSystem.MinMaxCurve(0f, AnimationCurve.Linear(0f, 0f, 1f, 0f));
            var size = p.sizeOverLifetime;
            size.enabled = true;
            size.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 0.6f, 1f, 1.6f));
            FadeOut(p);
            Material(p, soft: true);
        }

        static void BuildLeaves(ParticleSystem p)
        {
            Looping(p, rate: 1.5f);
            var main = p.main;
            main.prewarm = true;
            main.startLifetime = new ParticleSystem.MinMaxCurve(5f, 7f);
            main.startSpeed = 0f;
            main.startSize = new ParticleSystem.MinMaxCurve(0.1f, 0.18f);
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
            main.startColor = RandomColors(new Color(0.45f, 0.7f, 0.3f), new Color(0.85f, 0.6f, 0.25f), new Color(0.6f, 0.75f, 0.3f));
            main.gravityModifier = 0.04f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            var shape = p.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(3f, 0.5f, 3f);
            Spin(p);
            var noise = p.noise;
            noise.enabled = true;
            noise.strength = 0.4f;
            noise.frequency = 0.3f;
            Material(p, soft: false);
        }

        // ---------- helpers ----------

        // A single burst, not looping, not on wake: a script moves it and calls Play.
        static void OneShot(ParticleSystem p, short count)
        {
            var main = p.main;
            main.loop = false;
            main.playOnAwake = false;
            main.duration = 1f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = count * 2;
            var emission = p.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, count) });
        }

        static void Looping(ParticleSystem p, float rate)
        {
            var main = p.main;
            main.loop = true;
            main.playOnAwake = true;
            main.maxParticles = 50;
            var emission = p.emission;
            emission.rateOverTime = rate;
        }

        static void Shape(ParticleSystem p, ParticleSystemShapeType type, float radius, Quaternion rotation, float angle = 25f)
        {
            var shape = p.shape;
            shape.enabled = true;
            shape.shapeType = type;
            shape.radius = radius;
            shape.angle = angle;
            shape.rotation = rotation.eulerAngles;
        }

        static void FadeOut(ParticleSystem p)
        {
            var color = p.colorOverLifetime;
            color.enabled = true;
            var gradient = new Gradient();
            gradient.SetKeys(new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 0.6f), new GradientAlphaKey(0f, 1f) });
            color.color = gradient;
        }

        static void Spin(ParticleSystem p)
        {
            var rotation = p.rotationOverLifetime;
            rotation.enabled = true;
            rotation.z = new ParticleSystem.MinMaxCurve(-3f, 3f);
        }

        // Each particle picks one of the colours (a gradient with hard steps, sampled at random).
        static ParticleSystem.MinMaxGradient RandomColors(params Color[] colors)
        {
            var keys = new GradientColorKey[colors.Length];
            for (var i = 0; i < colors.Length; i++)
                keys[i] = new GradientColorKey(colors[i], (i + 1f) / colors.Length);
            var gradient = new Gradient { mode = GradientMode.Fixed };
            gradient.SetKeys(keys, new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) });
            return new ParticleSystem.MinMaxGradient(gradient) { mode = ParticleSystemGradientMode.RandomColor };
        }

        static void Material(ParticleSystem p, bool soft)
        {
            var renderer = p.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial = soft ? SoftMaterial() : SolidMaterial();
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
        }

        static Material SoftMaterial()
        {
            var path = MaterialFolder + "/ParticleSoft.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null)
                return material;
            material = new Material(ParticleShader);
            // URP transparent, alpha blended (what the Inspector sets for Surface Type = Transparent).
            material.SetFloat("_Surface", 1f);
            material.SetFloat("_Blend", 0f);
            material.SetOverrideTag("RenderType", "Transparent");
            material.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
            material.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
            material.SetFloat("_ZWrite", 0f);
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.renderQueue = (int)RenderQueue.Transparent;
            var dot = AssetDatabase.GetBuiltinExtraResource<Texture2D>("Default-Particle.psd");
            if (dot != null)
                material.SetTexture("_BaseMap", dot);
            return Save(material, path);
        }

        static Material SolidMaterial()
        {
            var path = MaterialFolder + "/ParticleSolid.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            return material != null ? material : Save(new Material(ParticleShader), path);
        }

        static Shader ParticleShader => Shader.Find("Universal Render Pipeline/Particles/Unlit") ?? Shader.Find("Particles/Standard Unlit");

        static Material Save(Material material, string path)
        {
            MagicMenuUtil.EnsureFolder(MaterialFolder);
            AssetDatabase.CreateAsset(material, path);
            MagicMenuUtil.Log("Created " + path);
            return material;
        }
    }
}
