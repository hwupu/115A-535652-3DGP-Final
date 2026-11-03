# Blender 5.2 headless: import a Tripo3D FBX, decimate, scale, build a simple biped armature, skin it.
# Usage: blender -b --factory-startup --python rig.py -- <Name> <src.fbx> <out_dir>
import bpy, bmesh, sys, os, math
from mathutils import Vector, Matrix

# Landmarks in normalized units (height 1, feet at z 0, faces -Y, character's left = +X).
CONFIGS = {
    "Hamster": dict(height=1.3, tris=15000, hip=0.20, neck=0.50, head_top=0.90, leg_top=0.18, foot_x=0.10,
                    arm_start=0.27, shoulder_x=0.21, arm_zmax=0.52, tail=None, ears=None),
    "Princess": dict(height=1.3, tris=15000, hip=0.17, neck=0.46, head_top=0.90, leg_top=0.15, foot_x=0.13,
                     arm_start=0.31, shoulder_x=0.24, arm_zmax=0.48, tail=dict(ymin=0.10, zmax=0.26, xabs=0.08),
                     ears=None),
    "Cat": dict(height=1.8, tris=15000, hip=0.22, neck=0.52, head_top=0.92, leg_top=0.20, foot_x=0.13,
                arm_start=0.31, shoulder_x=0.24, arm_zmax=0.50, tail=dict(ymin=0.15, zmax=0.27, xabs=1.0),
                ears=dict(base=(0.19, 0.87), tip=(0.25, 0.99))),
}

argv = sys.argv[sys.argv.index("--") + 1:]
name, src, out_dir = argv[0], argv[1], argv[2]
cfg = CONFIGS[name]
os.makedirs(out_dir, exist_ok=True)

bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.ops.import_scene.fbx(filepath=src)
mesh = [o for o in bpy.data.objects if o.type == 'MESH'][0]
for o in list(bpy.data.objects):
    if o is not mesh:
        bpy.data.objects.remove(o)
mesh.name = name
mesh.data.name = name + "Mesh"
bpy.context.view_layer.objects.active = mesh
mesh.select_set(True)
bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)

# ---- decimate ----
tris = sum(len(p.vertices) - 2 for p in mesh.data.polygons)
if tris > cfg["tris"]:
    mod = mesh.modifiers.new("Decimate", 'DECIMATE')
    mod.ratio = cfg["tris"] / tris
    mod.use_collapse_triangulate = True
    bpy.ops.object.modifier_apply(modifier=mod.name)
for p in mesh.data.polygons:
    p.use_smooth = False   # faceted low-poly look

# ---- normalize: height 1, feet centred at the origin ----
vs = [v.co for v in mesh.data.vertices]
zmin = min(v.z for v in vs); h = max(v.z for v in vs) - zmin
feet = [v for v in vs if v.z < zmin + 0.06 * h]
cx = (min(v.x for v in feet) + max(v.x for v in feet)) / 2
cy = (min(v.y for v in feet) + max(v.y for v in feet)) / 2
for v in mesh.data.vertices:
    v.co = Vector(((v.co.x - cx) / h, (v.co.y - cy) / h, (v.co.z - zmin) / h))
vs = [v.co.copy() for v in mesh.data.vertices]

def row_center_y(z, band=0.03, xabs=0.15, ymax=9.0):
    s = [v.y for v in vs if abs(v.z - z) < band and abs(v.x) < xabs and v.y < ymax]
    return (min(s) + max(s)) / 2 if s else 0.0

def centroid(sel):
    n = len(sel)
    return sum(sel, Vector()) / n if n else None

# ---- landmarks ----
hip, neck, top = cfg["hip"], cfg["neck"], cfg["head_top"]
chest = hip + (neck - hip) * 0.55
spine_pts = {z: Vector((0, row_center_y(z), z)) for z in (hip, chest, neck)}
spine_pts[hip].y = spine_pts[chest].y   # skirts / capes / tails pull the hip row back
head_top = Vector((0, row_center_y(top - 0.1), top))

arms = {}
for side in (1, -1):   # +1 = left (+X), -1 = right
    sel = [v for v in vs if v.x * side > cfg["arm_start"] and v.z < cfg["arm_zmax"] and v.z > 0.28 and v.y < 0.15]
    tip_v = max(sel, key=lambda v: v.x * side)
    near = [v for v in sel if v.x * side < cfg["arm_start"] + 0.04]
    root_c = centroid(near)
    tipc = centroid([v for v in sel if v.x * side > tip_v.x * side - 0.03])
    tip = Vector((tip_v.x, tipc.y, tipc.z))
    # extend the arm line back to the shoulder (works for T- and A-pose)
    d = root_c - tip
    k = (cfg["shoulder_x"] * side - root_c.x) / d.x
    shoulder = root_c + d * k
    palm = shoulder.lerp(tip, 0.78)
    elbow = shoulder.lerp(palm, 0.5)
    arms[side] = (shoulder, elbow, palm, tip)

legs = {}
for side in (1, -1):
    fx = cfg["foot_x"] * side
    foot = [v for v in vs if v.z < 0.05 and v.x * side > 0]
    fy = (min(v.y for v in foot) + max(v.y for v in foot)) / 2
    toe_y = min(v.y for v in foot)
    legs[side] = (Vector((fx, fy, cfg["leg_top"])), Vector((fx, fy, 0.045)), Vector((fx, toe_y + 0.02, 0.02)))

tail = None
if cfg["tail"]:
    t = cfg["tail"]
    sel = [v for v in vs if v.y > t["ymin"] and v.z < t["zmax"] and abs(v.x) < t["xabs"]]
    body = Vector((0, row_center_y(hip), hip))
    root_v = min(sel, key=lambda v: (v - body).length)
    tip_v = max(sel, key=lambda v: (v - root_v).length)
    d = (tip_v - root_v)
    length = d.length
    pts = []
    for i in range(4):
        f = i / 3
        target = root_v + d * f
        nb = [v for v in sel if abs((v - root_v).dot(d.normalized()) - f * length) < length / 8]
        c = centroid(nb) or target
        pts.append(c)
    pts[0] = pts[0].lerp(body, 0.3)
    tail = pts

# ---- game size (metres): landmarks stay normalized, bone() scales them ----
for v in mesh.data.vertices:
    v.co = v.co * cfg["height"]

# ---- armature ----
arm_data = bpy.data.armatures.new(name + "Armature")
rig = bpy.data.objects.new(name + "Rig", arm_data)
bpy.context.scene.collection.objects.link(rig)
bpy.context.view_layer.objects.active = rig
bpy.ops.object.mode_set(mode='EDIT')
eb = arm_data.edit_bones

S = cfg["height"]
def bone(n, head, tail_, parent=None, connect=False, roll_up=None):
    b = eb.new(n)
    b.head, b.tail = head * S, tail_ * S
    if parent:
        b.parent = eb[parent]
        b.use_connect = connect
    if roll_up is not None:
        b.align_roll(roll_up)
    return b

bone("Root", Vector((0, 0, 0)), Vector((0, 0.15, 0)), roll_up=Vector((0, 0, 1)))
bone("Hips", spine_pts[hip] - Vector((0, 0, 0.04)), spine_pts[hip], "Root", roll_up=Vector((0, -1, 0)))
bone("Spine", spine_pts[hip], spine_pts[chest], "Hips", True, roll_up=Vector((0, -1, 0)))
bone("Chest", spine_pts[chest], spine_pts[neck], "Spine", True, roll_up=Vector((0, -1, 0)))
bone("Head", spine_pts[neck], head_top, "Chest", True, roll_up=Vector((0, -1, 0)))
for side, s in ((1, "L"), (-1, "R")):
    sh, el, pa, ti = arms[side]
    bone("UpperArm_" + s, sh, el, "Chest", roll_up=Vector((0, 0, 1)))
    bone("LowerArm_" + s, el, pa, "UpperArm_" + s, True, roll_up=Vector((0, 0, 1)))
    bone("Hand_" + s, pa, ti, "LowerArm_" + s, True, roll_up=Vector((0, 0, 1)))
    top_, ankle, toe = legs[side]
    bone("Leg_" + s, top_, ankle, "Hips", roll_up=Vector((0, -1, 0)))
    bone("Foot_" + s, ankle, toe, "Leg_" + s, True, roll_up=Vector((0, 0, 1)))
if cfg["ears"]:
    (bx, bz), (tx, tz) = cfg["ears"]["base"], cfg["ears"]["tip"]
    ey = row_center_y(bz, xabs=0.3) + 0.03
    for side, s in ((1, "L"), (-1, "R")):
        bone("Ear_" + s, Vector((bx * side, ey, bz)), Vector((tx * side, ey + 0.01, tz)), "Head", roll_up=Vector((0, -1, 0)))
if tail:
    parent = "Hips"
    for i in range(3):
        bone(f"Tail_{i+1}", tail[i], tail[i + 1], parent, i > 0, roll_up=Vector((0, 0, 1)))
        parent = f"Tail_{i+1}"
bpy.ops.object.mode_set(mode='OBJECT')

# ---- skin: heat weights, then nearest-bone fill for anything left unweighted ----
bpy.ops.object.select_all(action='DESELECT')
mesh.select_set(True); rig.select_set(True)
bpy.context.view_layer.objects.active = rig
bpy.ops.object.parent_set(type='ARMATURE_AUTO')
deform = [b for b in arm_data.bones if b.name != "Root"]
for vg in list(mesh.vertex_groups):
    if vg.name == "Root":
        mesh.vertex_groups.remove(vg)
groups = {vg.index: vg.name for vg in mesh.vertex_groups}

def seg_dist(p, a, b):
    ab = b - a
    t = max(0.0, min(1.0, (p - a).dot(ab) / ab.length_squared))
    return (a + ab * t - p).length

empty = 0
for v in mesh.data.vertices:
    total = sum(g.weight for g in v.groups if g.group in groups)
    if total < 1e-4:
        empty += 1
        best = min(deform, key=lambda b: seg_dist(v.co, b.head_local, b.tail_local))  # both in metres now
        vg = mesh.vertex_groups.get(best.name) or mesh.vertex_groups.new(name=best.name)
        vg.add([v.index], 1.0, 'REPLACE')
# keep at most 4 influences, normalized (Unity's default skin weights)
bpy.context.view_layer.objects.active = mesh
bpy.ops.object.mode_set(mode='WEIGHT_PAINT')
bpy.ops.object.vertex_group_limit_total(group_select_mode='ALL', limit=4)
bpy.ops.object.vertex_group_normalize_all(lock_active=False)
bpy.ops.object.mode_set(mode='OBJECT')

tris = sum(len(p.vertices) - 2 for p in mesh.data.polygons)
print(f"RIG {name}: tris={tris} verts={len(mesh.data.vertices)} bones={len(arm_data.bones)} unweighted_filled={empty}")
for b in arm_data.bones:
    print(f"BONE {b.name:12s} head={tuple(round(c,3) for c in b.head_local)} tail={tuple(round(c,3) for c in b.tail_local)}")
bpy.ops.wm.save_as_mainfile(filepath=os.path.join(out_dir, name + "_rig.blend"))
