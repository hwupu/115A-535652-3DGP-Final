# Blender 5.2 headless: add animation clips to <Name>_rig.blend, export FBX + base colour texture, render a contact sheet.
# Usage: blender -b --factory-startup --python anim.py -- <Name> <work_dir> <out_dir> [sheet]
import bpy, sys, os, math
import numpy as np
from mathutils import Vector, Quaternion

argv = sys.argv[sys.argv.index("--") + 1:]
name, work, out = argv[0], argv[1], argv[2]
want_sheet = len(argv) > 3 and argv[3] == "sheet"
bpy.ops.wm.open_mainfile(filepath=os.path.join(work, name + "_rig.blend"))
scene = bpy.context.scene
scene.render.fps = 24
rig = bpy.data.objects[name + "Rig"]
mesh = bpy.data.objects[name]
H = {"Hamster": 1.3, "Princess": 1.3, "Cat": 1.8}[name]   # metres; loc offsets below are in "height units"
AX = {"X": Vector((1, 0, 0)), "Y": Vector((0, 1, 0)), "Z": Vector((0, 0, 1))}

# Armature axes: X = character's left, -Y = forward, Z = up.
#   right arm down: Y-   left arm down: Y+    right arm forward: Z+   left arm forward: Z-
#   bend forward (spine, head): X+            leg forward: X-          turn head to its left: Z+
def world_q(spec):
    q = Quaternion()
    for axis, deg in spec:
        q = Quaternion(AX[axis], math.radians(deg)) @ q
    return q

def apply_pose(pose, frame, bones):
    for bn in bones:
        pb = rig.pose.bones[bn]
        rest = pb.bone.matrix_local.to_quaternion()
        spec = pose.get(bn, [])
        rot = [s for s in spec if s[0] != "loc"]
        loc = [s for s in spec if s[0] == "loc"]
        pb.rotation_mode = 'QUATERNION'
        pb.rotation_quaternion = rest.inverted() @ world_q(rot) @ rest
        offset = Vector(loc[0][1]) * H if loc else Vector()
        pb.location = rest.inverted() @ offset
        pb.keyframe_insert("rotation_quaternion", frame=frame)
        pb.keyframe_insert("location", frame=frame)

def merge(*dicts):
    out_ = {}
    for d in dicts:
        for k, v in d.items():
            out_[k] = list(out_.get(k, [])) + list(v) if k in out_ else list(v)
    return out_

CLIPS = []   # (clip name, loop, [(frame, pose)], events)

def clip(cname, keys, loop=True, events=None):
    CLIPS.append((cname, loop, keys, events or {}))

def mirror_arm(spec):
    """Left-arm version of a right-arm spec (Y and Z flip sign)."""
    return [(a, -d if a in ("Y", "Z") else d) for a, d in spec]

# ======================= poses =======================
if name in ("Hamster", "Princess"):
    def arms(r, l=None, rl=None, ll=None):
        p = {"UpperArm_R": r, "UpperArm_L": l if l is not None else mirror_arm(r)}
        if rl: p["LowerArm_R"] = rl
        if ll: p["LowerArm_L"] = ll
        return p
    ARM_DOWN = -72 if name == "Hamster" else -48   # the princess's wide dress: hands stay outside it
    DOWN = arms([("Y", ARM_DOWN), ("Z", 8)])
    def breathe(a):   # a = -1..1
        return {"Chest": [("X", 2 * a)], "Head": [("X", -1.5 * a)], "Hips": [("loc", (0, 0, 0.004 * a))]}
    clip("Idle", [(f, merge(DOWN, breathe(math.sin(f / 48 * 2 * math.pi)), arms([("Y", ARM_DOWN + 2 * math.sin(f / 48 * 2 * math.pi)), ("Z", 8)]))) for f in (1, 13, 25, 37, 49)])

    def walk_pose(phase):   # phase 0..1, one full cycle (two steps)
        s = math.sin(phase * 2 * math.pi)
        bob = abs(math.cos(phase * 2 * math.pi))
        return merge({"Leg_L": [("X", -28 * s)], "Leg_R": [("X", 28 * s)],
                      "Foot_L": [("X", 10 * s)], "Foot_R": [("X", -10 * s)],
                      "Hips": [("loc", (0, 0, 0.018 * (1 - bob))), ("Y", 3 * s)], "Chest": [("Z", 6 * s), ("X", 4)]},
                     arms([("Y", ARM_DOWN), ("Z", 25 * s)], [("Y", -ARM_DOWN), ("Z", 25 * s)]))
    clip("Walk", [(1 + i * 2, walk_pose(i / 8)) for i in range(9)])

    UP = arms([("Y", 65)], [("Y", -65)], [("Y", 15)], [("Y", -15)])
    clip("Celebrate", [
        (1, DOWN),
        (6, merge(DOWN, {"Hips": [("loc", (0, 0, -0.035))], "Leg_L": [("X", -15)], "Leg_R": [("X", -15)], "Chest": [("X", 10)]})),
        (12, merge(UP, {"Hips": [("loc", (0, 0, 0.14))], "Head": [("X", -15)], "Leg_L": [("X", 10)], "Leg_R": [("X", 10)]})),
        (18, merge(UP, {"Hips": [("loc", (0, 0, 0.0))], "Head": [("X", -10)]})),
        (22, merge(UP, {"Hips": [("loc", (0, 0, -0.025))], "Head": [("X", -5)], "Leg_L": [("X", -10)], "Leg_R": [("X", -10)]})),
        (30, merge(arms([("Y", 50)], [("Y", -50)], [("Y", 35)], [("Y", -35)]), {"Head": [("X", -12)]})),
        (37, DOWN)])

if name == "Hamster":
    # Aim: bent over the ball (front right), right hand low, left hand reaching across to it.
    AIM = merge(arms([("Y", -75), ("Z", 28)], [("Y", 68), ("Z", -55)]), {"Spine": [("X", 10)], "Head": [("X", 12), ("Z", -8)]})
    clip("AimIdle", [(f, merge(AIM, {"UpperArm_R": [("Z", 3 * math.sin(f / 48 * 2 * math.pi))], "Chest": [("X", 1.5 * math.sin(f / 48 * 2 * math.pi))]})) for f in (1, 13, 25, 37, 49)])
    BACK = merge(arms([("Y", -15), ("Z", -40)], [("Y", 35), ("Z", -10)], [("Z", -15)]), {"Chest": [("Z", -22)], "Spine": [("X", 8)], "Head": [("X", 12), ("Z", 10)]})
    HIT = merge(arms([("Y", -80), ("Z", 30)], [("Y", 72), ("Z", -50)]), {"Chest": [("Z", 6)], "Spine": [("X", 10)], "Head": [("X", 14)]})
    THRU = merge(arms([("Y", -15), ("Z", 100)], [("Y", 25), ("Z", -100)], [("Z", 25)]), {"Chest": [("Z", 32)], "Spine": [("X", 2)], "Head": [("X", -5), ("Z", 25)]})
    clip("Swing_Club", [(1, AIM), (7, BACK), (10, HIT), (15, THRU), (24, THRU), (36, AIM)], loop=False, events={"OnSwingImpact": 10})
    OVER = merge(arms([("Y", 70), ("Z", -20)], [("Y", -70), ("Z", 20)], [("Y", 30)], [("Y", -30)]), {"Spine": [("X", -12)], "Head": [("X", -10)], "Hips": [("loc", (0, 0, 0.03))]})
    SMASH = merge(arms([("Y", -55), ("Z", 70)], [("Y", 55), ("Z", -70)]), {"Spine": [("X", 28)], "Head": [("X", 15)], "Hips": [("loc", (0, 0, -0.035))], "Leg_L": [("X", -12)], "Leg_R": [("X", -12)]})
    clip("Swing_Hammer", [(1, AIM), (11, OVER), (13, OVER), (16, SMASH), (20, merge(SMASH, {"Hips": [("loc", (0, 0, -0.02))]})), (28, SMASH), (40, AIM)], loop=False, events={"OnSwingImpact": 16})
    TAPBACK = merge(AIM, {"UpperArm_R": [("Z", -18)]})
    TAP = merge(AIM, {"UpperArm_R": [("Z", 22)], "Head": [("Z", 6)]})
    clip("Swing_Mysterious", [(1, AIM), (6, TAPBACK), (9, TAP), (14, TAP), (22, AIM)], loop=False, events={"OnSwingImpact": 9})
    clip("Equip", [(1, AIM), (6, merge(arms([("Y", -40), ("Z", 75)], [("Y", 68), ("Z", -55)], [("Z", 60)]), {"Head": [("X", 8), ("Z", -20)]})), (13, AIM)], loop=False)
    SCARED = merge(arms([("Y", -25), ("Z", 75)], [("Y", 25), ("Z", -75)], [("Z", 70)], [("Z", -70)]),
                   {"Spine": [("X", -16)], "Head": [("X", -8)], "Hips": [("loc", (0, 0.03, -0.015))], "Leg_L": [("X", -10)], "Leg_R": [("X", 8)]})
    clip("Fail", [(1, DOWN), (8, SCARED), (12, merge(SCARED, {"Chest": [("Z", 3)]})), (16, merge(SCARED, {"Chest": [("Z", -3)]})), (20, merge(SCARED, {"Chest": [("Z", 3)]})), (24, SCARED)], loop=False)
    clip("Flinch", [(1, DOWN), (4, merge(DOWN, {"Spine": [("X", -10)], "Head": [("X", -12)], "UpperArm_R": [("Z", 25)], "UpperArm_L": [("Z", -25)]})), (12, DOWN)], loop=False)

if name == "Princess":
    def sway(f, n=72):
        a = math.sin(f / n * 2 * math.pi)
        return merge(DOWN, {"Spine": [("Z", 3 * a)], "Head": [("Y", 4 * a)], "Tail_1": [("Z", 10 * a)], "Tail_2": [("Z", 12 * a)], "Chest": [("X", 1.5 * a)]})
    CLIPS[:] = [c for c in CLIPS if c[0] != "Idle"]
    clip("Idle", [(f, sway(f - 1)) for f in (1, 19, 37, 55, 73)])
    WAVE_UP = arms([("Y", 55), ("Z", 15)], [("Y", -ARM_DOWN), ("Z", -8)], [("Y", 50)])
    clip("Wave", [(1, DOWN), (8, WAVE_UP)] + [(8 + i * 6, merge(WAVE_UP, {"LowerArm_R": [("Z", 25 if i % 2 else -25)], "Head": [("Y", -6)]})) for i in range(1, 6)] + [(48, WAVE_UP), (56, DOWN)], loop=False)
    clip("Cheer", [(f, p) for f, p in next(c for c in CLIPS if c[0] == "Celebrate")[2]])
    CLIPS[:] = [c for c in CLIPS if c[0] != "Celebrate"]
    SCARED = merge(arms([("Y", -25), ("Z", 75)], [("Y", 25), ("Z", -75)], [("Z", 70)], [("Z", -70)]), {"Spine": [("X", -6)], "Head": [("X", 8)], "Leg_L": [("Z", -6)], "Leg_R": [("Z", 6)]})
    clip("Scared", [(1 + i * 2, merge(SCARED, {"Chest": [("Z", 2.5 if i % 2 else -2.5)], "Tail_1": [("X", -20)]})) for i in range(13)])

if name == "Cat":
    # rest: A-pose arms ~35 deg down. SIT lowers the hips onto the ground, legs forward.
    def arms(r, l=None):
        return {"UpperArm_R": r, "UpperArm_L": l if l is not None else mirror_arm(r)}
    SIT = {"Hips": [("loc", (0, 0.02, -0.16))], "Leg_L": [("X", -82), ("Z", 8)], "Leg_R": [("X", -82), ("Z", -8)], "Foot_L": [("X", 20)], "Foot_R": [("X", 20)]}
    LAP = arms([("Y", -45), ("Z", 30)])
    TAIL_REST = {"Tail_1": [("Z", 0)]}
    def asleep(f):
        a = math.sin(f / 96 * 2 * math.pi)
        return merge(SIT, LAP, {"Spine": [("X", 18)], "Chest": [("X", 8 + 2 * a)], "Head": [("X", 28 - 2 * a), ("Y", 8)],
                                "Ear_L": [("Y", -10)], "Ear_R": [("Y", 10)], "Tail_2": [("Z", 4 * a)]})
    clip("Asleep", [(f, asleep(f - 1)) for f in (1, 25, 49, 73, 97)])
    def twitch(f):
        a = math.sin(f / 48 * 2 * math.pi)
        flick = 30 if 6 <= f <= 9 or 30 <= f <= 32 else 0
        jerk = 6 if 24 <= f <= 27 else 0
        return merge(SIT, LAP, {"Spine": [("X", 15)], "Chest": [("X", 6)], "Head": [("X", 20 - jerk), ("Y", 5)],
                                "Ear_L": [("Y", -10 + flick)], "Ear_R": [("Y", 10 - flick * 0.4)],
                                "Tail_1": [("Z", 15 * a)], "Tail_2": [("Z", 18 * math.sin(f / 48 * 2 * math.pi - 0.8))], "Tail_3": [("Z", 20 * math.sin(f / 48 * 2 * math.pi - 1.6))]})
    clip("Twitching", [(f, twitch(f - 1)) for f in (1, 6, 7, 9, 10, 13, 24, 25, 27, 28, 30, 32, 33, 37, 49)])
    def suspicious(f):
        look = 32 * math.sin(f / 96 * 2 * math.pi)
        a = math.sin(f / 48 * 2 * math.pi)
        return merge(SIT, arms([("Y", -35), ("Z", 25)]), {"Spine": [("X", 2)], "Head": [("X", 0), ("Z", look)],
                                "Tail_1": [("Z", 10 * a)], "Tail_2": [("Z", 14 * a)]})
    clip("Suspicious", [(f, suspicious(f - 1)) for f in range(1, 98, 12)])
    def searching(f):
        look = 40 * math.sin(f / 96 * 2 * math.pi)
        a = math.sin(f / 48 * 2 * math.pi)
        return merge(arms([("Y", 20), ("Z", 50)]), {"Spine": [("X", 14)], "Head": [("X", -10), ("Z", look)],
                     "Hips": [("loc", (0.02 * a, 0, -0.02))], "Leg_L": [("X", -6)], "Leg_R": [("X", 6)],
                     "Tail_1": [("X", 30), ("Z", 15 * a)], "Tail_2": [("X", 10), ("Z", 20 * a)]})
    clip("Searching", [(f, searching(f - 1)) for f in range(1, 98, 12)])
    def awake(f):
        shake = 8 if (f // 6) % 2 else -8
        a = math.sin(f / 24 * 2 * math.pi)
        return merge(arms([("Y", 95), ("Z", 15 + shake)]), {"Spine": [("X", -8)], "Head": [("X", -4)],
                     "Hips": [("loc", (0, 0, 0.015 * abs(a)))], "Ear_L": [("Y", 25)], "Ear_R": [("Y", -25)],
                     "Tail_1": [("X", 55)], "Tail_2": [("X", 25)], "Tail_3": [("X", 15)]})
    clip("Awake", [(f, awake(f - 1)) for f in range(1, 50, 6)])

# ======================= build actions =======================
if rig.animation_data is None:
    rig.animation_data_create()
all_bones = [b.name for b in rig.data.bones]
for cname, loop, keys, events in CLIPS:
    act = bpy.data.actions.new(cname)
    act.use_fake_user = True
    rig.animation_data.action = act
    used = sorted({b for _, p in keys for b in p} | {"Hips"})
    for frame, pose in keys:
        apply_pose(pose, frame, used)
    act.frame_range = (1, max(f for f, _ in keys))
    act["loop"] = loop
    for ev, fr in events.items():
        act["event_" + ev] = fr

# ---- export: rest pose as the default pose ----
# The FBX exporter writes the pose of the last baked action as the default node pose, so
# the T-pose action "_Rest" is baked last (actions bake alphabetically); its take is unused in Unity.
rest_act = bpy.data.actions.new("_Rest")   # "_" sorts after the clip names: baked last
rest_act.use_fake_user = True
rig.animation_data.action = rest_act
for pb in rig.pose.bones:
    pb.rotation_mode = 'QUATERNION'
    pb.rotation_quaternion = Quaternion(); pb.location = Vector()
    pb.keyframe_insert("rotation_quaternion", frame=1)
    pb.keyframe_insert("location", frame=1)
scene.frame_set(1)

# base colour texture, 2048 JPEG
mat = mesh.data.materials[0]
mat.name = name + "_Mat"
img = None
bsdf = next(n for n in mat.node_tree.nodes if n.type == 'BSDF_PRINCIPLED')
link = bsdf.inputs["Base Color"].links
if link:
    n = link[0].from_node
    while n.type != 'TEX_IMAGE' and n.inputs and n.inputs[0].links:
        n = n.inputs[0].links[0].from_node
    img = n.image if n.type == 'TEX_IMAGE' else None
if img is None:
    img = next(i for i in bpy.data.images if "base" in i.filepath.lower())
os.makedirs(out, exist_ok=True)
img.scale(2048, 2048)
scene.render.image_settings.file_format = 'JPEG'
scene.render.image_settings.quality = 88
tex_path = os.path.join(out, name + "_BaseColor.jpg")
img.save_render(tex_path, scene=scene)
print("TEX", tex_path, os.path.getsize(tex_path))

bpy.ops.object.select_all(action='DESELECT')
rig.select_set(True); mesh.select_set(True)
bpy.context.view_layer.objects.active = rig
fbx = os.path.join(out, name + ".fbx")
bpy.ops.export_scene.fbx(filepath=fbx, use_selection=True, object_types={'ARMATURE', 'MESH'},
                         axis_forward='-Z', axis_up='Y', apply_scale_options='FBX_SCALE_ALL',
                         add_leaf_bones=False, use_armature_deform_only=False, armature_nodetype='NULL',
                         mesh_smooth_type='FACE', path_mode='STRIP', embed_textures=False,
                         bake_anim=True, bake_anim_use_all_actions=True, bake_anim_use_nla_strips=False,
                         bake_anim_force_startend_keying=True, bake_anim_simplify_factor=0.0)
print("FBX", fbx, os.path.getsize(fbx))
# clip table for the Unity importer / handoff doc
with open(os.path.join(out, name + "_clips.txt"), "w") as f:
    for cname, loop, keys, events in CLIPS:
        end = max(fr for fr, _ in keys)
        f.write(f"{cname}\t{1}\t{end}\t{'loop' if loop else 'once'}\t" + ",".join(f"{k}@{v}" for k, v in events.items()) + "\n")
for img in list(bpy.data.images):   # the texture lives next to the FBX; keep the .blend small
    bpy.data.images.remove(img)
bpy.ops.wm.save_as_mainfile(filepath=os.path.join(out, name + ".blend"), compress=True)

# ======================= contact sheet =======================
if want_sheet:
    scene.render.engine = 'BLENDER_EEVEE'
    scene.render.resolution_x, scene.render.resolution_y = 260, 320
    world = bpy.data.worlds.new("w"); scene.world = world; world.use_nodes = True
    world.node_tree.nodes["Background"].inputs[0].default_value = (0.85, 0.85, 0.85, 1)
    sun = bpy.data.objects.new("sun", bpy.data.lights.new("sun", 'SUN')); scene.collection.objects.link(sun)
    sun.rotation_euler = (0.7, 0.2, -0.6); sun.data.energy = 3
    cam = bpy.data.objects.new("cam", bpy.data.cameras.new("cam")); scene.collection.objects.link(cam); scene.camera = cam
    cam.data.type = 'ORTHO'; cam.data.ortho_scale = H * 1.7
    d = 6
    cam.location = (d * 0.6, -d * 0.8, H * 0.6); cam.rotation_euler = (math.radians(88), 0, math.radians(37))
    tiles = []
    for cname, loop, keys, events in CLIPS:
        rig.animation_data.action = bpy.data.actions[cname]
        frames = [fr for fr, _ in keys]
        picks = sorted(set([frames[0], frames[len(frames) // 3], frames[2 * len(frames) // 3], frames[-1]] + list(events.values())))[:4]
        row = []
        for fr in picks:
            scene.frame_set(fr)
            p = f"/tmp/tripo/sheet_tmp.png"
            scene.render.filepath = p
            scene.render.image_settings.file_format = 'PNG'
            bpy.ops.render.render(write_still=True)
            im = bpy.data.images.load(p, check_existing=False)
            a = np.array(im.pixels[:]).reshape(im.size[1], im.size[0], 4)
            bpy.data.images.remove(im)
            row.append(a)
        while len(row) < 4:
            row.append(np.ones_like(row[0]))
        tiles.append(np.concatenate(row, axis=1))
    sheet = np.concatenate(tiles[::-1], axis=0)   # pixels are bottom-up
    img = bpy.data.images.new("sheet", sheet.shape[1], sheet.shape[0], alpha=True)
    img.pixels = sheet.ravel()
    img.filepath_raw = f"/tmp/tripo/sheet_{name}.png"; img.file_format = 'PNG'; img.save()
    print("SHEET", img.filepath_raw, [c[0] for c in CLIPS])
