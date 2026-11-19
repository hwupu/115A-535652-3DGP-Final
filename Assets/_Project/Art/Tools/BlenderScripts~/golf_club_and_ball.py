"""Split the Sketchfab "Golf Club and Ball" FBX into a club tool and a golf ball for Unity.

Run headless (Blender 5.3):
    blender -b --factory-startup --python golf_club_and_ball.py -- "<Golf Club and Ball.FBX>" <out_dir>

Writes to <out_dir>:
    StandardClub.fbx, StandardClub.blend   club, grip at the origin, hanging down, head toe toward Unity +Z
    GolfBall.fbx, GolfBall.blend           unit sphere (diameter 1) centred at the origin
    preview.png                            contact sheet next to the placeholder sizes
The textures are not embedded; the Magic tool (Use Tool and Ball Models) builds URP materials from the JPGs
copied next to each FBX. Conventions: Assets/_Project/Art/README.md.
"""
import math
import os
import sys

import bpy
from mathutils import Matrix, Vector

# ---- Sizes (metres, in the tool prefab's space: the hamster's hand scales it) ----
CLUB_LENGTH = 0.34   # placeholder: shaft from the grip (0) down to the sole (-0.30)
CLUB_TOP = 0.04      # the hand holds the middle of the rubber grip, not its very end
CLUB_THICKEN = 2.5   # chibi proportions: a real shaft would be ~2 mm here and vanish; head ends up ~9.5 cm
BALL_DIAMETER = 1.0  # GolfBall.prefab scales its root to 0.2 and keeps the SphereCollider radius 0.5

# Source material name -> our name (also the Unity material and texture name)
CLUB_MATERIALS = {"07 - Default": "Club_Steel", "08 - Default": "Club_Rubber",
                  "09 - Default": "Club_Dark", "13 - Default": "Club_Gold"}
BALL_MATERIALS = {"01 - Default": "GolfBall"}


def args():
    a = sys.argv[sys.argv.index("--") + 1:]
    return a[0], a[1]


def apply_transforms(obj):
    obj.data = obj.data.copy()
    obj.data.transform(obj.matrix_world)
    obj.matrix_world = Matrix.Identity(4)


def world_verts(obj):
    return [v.co.copy() for v in obj.data.vertices]


def rename_materials(obj, table):
    for slot in obj.material_slots:
        if slot.material and slot.material.name in table:
            slot.material.name = table[slot.material.name]


def prepare_club(club):
    apply_transforms(club)
    vs = world_verts(club)
    zmin, zmax = min(v.z for v in vs), max(v.z for v in vs)
    length = zmax - zmin
    top = [v for v in vs if v.z > zmax - 0.1 * length]
    bottom = [v for v in vs if v.z < zmin + 0.1 * length]
    shaft = Vector((sum(v.x for v in top) / len(top), sum(v.y for v in top) / len(top), 0))
    head = Vector((sum(v.x for v in bottom) / len(bottom), sum(v.y for v in bottom) / len(bottom), 0))
    # Head at the bottom (the source already has it there), shaft axis through the origin.
    m = Matrix.Translation(Vector((-shaft.x, -shaft.y, -zmax)))
    # Turn so the head's toe points +Y in Blender (= +Z in Unity after the FBX axis conversion).
    toe = (head - shaft).normalized()
    angle = math.atan2(toe.y, toe.x)
    m = Matrix.Rotation(math.radians(90) - angle, 4, 'Z') @ m
    s = CLUB_LENGTH / length
    m = Matrix.Diagonal((s * CLUB_THICKEN, s * CLUB_THICKEN, s, 1)) @ m
    m = Matrix.Translation(Vector((0, 0, CLUB_TOP))) @ m
    club.data.transform(m)
    club.data.update()
    club.name = club.data.name = "StandardClub"
    rename_materials(club, CLUB_MATERIALS)


def prepare_ball(ball):
    apply_transforms(ball)
    vs = world_verts(ball)
    lo = Vector((min(v.x for v in vs), min(v.y for v in vs), min(v.z for v in vs)))
    hi = Vector((max(v.x for v in vs), max(v.y for v in vs), max(v.z for v in vs)))
    centre = (lo + hi) / 2
    s = BALL_DIAMETER / max(hi - lo)
    ball.data.transform(Matrix.Scale(s, 4) @ Matrix.Translation(-centre))
    ball.data.update()
    ball.name = ball.data.name = "GolfBall"
    rename_materials(ball, BALL_MATERIALS)


def export(obj, out_dir):
    for o in bpy.context.scene.objects:
        o.select_set(o == obj)
    bpy.context.view_layer.objects.active = obj
    path = os.path.join(out_dir, obj.name + ".fbx")
    bpy.ops.export_scene.fbx(filepath=path, use_selection=True, object_types={'MESH'},
                             axis_forward='-Z', axis_up='Y', apply_scale_options='FBX_SCALE_ALL',
                             bake_space_transform=True, path_mode='STRIP', embed_textures=False,
                             use_mesh_modifiers=True, mesh_smooth_type='FACE', add_leaf_bones=False)
    print("EXPORTED", path)


def save_blend(keep, out_dir):
    """Save a .blend holding only `keep` (the other object is unlinked for the save, then restored)."""
    others = [o for o in bpy.context.scene.objects if o != keep]
    for o in others:
        bpy.context.scene.collection.objects.unlink(o) if o.name in bpy.context.scene.collection.objects else \
            [c.objects.unlink(o) for c in o.users_collection]
    bpy.ops.wm.save_as_mainfile(filepath=os.path.join(out_dir, keep.name + ".blend"), copy=True)
    for o in others:
        bpy.context.scene.collection.objects.link(o)


def stats(obj):
    obj.data.calc_loop_triangles()
    vs = world_verts(obj)
    lo = Vector((min(v.x for v in vs), min(v.y for v in vs), min(v.z for v in vs)))
    hi = Vector((max(v.x for v in vs), max(v.y for v in vs), max(v.z for v in vs)))
    print(f"STATS {obj.name}: tris={len(obj.data.loop_triangles)} "
          f"min=({lo.x:.3f},{lo.y:.3f},{lo.z:.3f}) max=({hi.x:.3f},{hi.y:.3f},{hi.z:.3f}) "
          f"materials={[s.material.name for s in obj.material_slots]}")


def render_preview(club, ball, out_dir):
    scn = bpy.context.scene
    # Ball shown at its in-game size (0.2 m) beside the club head, for scale.
    ball.scale = (0.2, 0.2, 0.2)
    ball.location = (0.0, 0.25, -0.20)
    # Placeholder outline: a 0.3 m wire box for the old shaft + head.
    bpy.ops.mesh.primitive_cube_add(size=1, location=(0, 0.03, -0.15))
    ref = bpy.context.object
    ref.scale = (0.04, 0.1, 0.3)
    ref.display_type = 'WIRE'
    ref.hide_render = False
    ref.modifiers.new("w", 'WIREFRAME').thickness = 0.002
    bpy.ops.object.camera_add(location=(0.9, 0.35, -0.08))
    cam = bpy.context.object
    target = Vector((0, 0.08, -0.12))
    cam.rotation_euler = (target - cam.location).to_track_quat('-Z', 'Y').to_euler()
    cam.data.type = 'ORTHO'
    cam.data.ortho_scale = 0.55
    scn.camera = cam
    world = bpy.data.worlds.new("Preview")
    world.color = (0.55, 0.6, 0.55)
    scn.world = world
    scn.render.engine = 'BLENDER_WORKBENCH'
    scn.display.shading.light = 'STUDIO'
    scn.display.shading.color_type = 'TEXTURE'
    scn.render.resolution_x = 700
    scn.render.resolution_y = 700
    scn.render.filepath = os.path.join(out_dir, "preview.png")
    bpy.ops.render.render(write_still=True)


def main():
    src, out_dir = args()
    os.makedirs(out_dir, exist_ok=True)
    bpy.ops.wm.read_factory_settings(use_empty=True)
    bpy.ops.import_scene.fbx(filepath=src)
    club, ball = bpy.data.objects["Club"], bpy.data.objects["ball"]
    prepare_club(club)
    prepare_ball(ball)
    for obj in (club, ball):
        stats(obj)
        export(obj, out_dir)
        save_blend(obj, out_dir)
    render_preview(club, ball, out_dir)


main()
