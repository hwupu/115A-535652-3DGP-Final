"""Render the club-card icons (HUD tool picker) from the exported tool FBX files.

Run headless (Blender 5.3) from the repo root:
    blender -b --factory-startup --python Assets/_Project/Art/Tools/BlenderScripts~/tool_icons.py -- <repo root>

Reads Assets/_Project/Art/Tools/<Tool>/<Tool>.fbx (so it also checks the export) and writes
Assets/_Project/UI/ClubIcons/<Tool>_Icon.png: 256 x 256, transparent, the tool lying diagonally (grip top left,
head bottom right), soft studio light. Materials mirror the Unity ones in Editor/Magic/ToolModelsMenu.cs:
<Material>.jpg next to the FBX if it exists (+ <Material>_Normal.jpg), otherwise the colour in COLOURS.
"""
import math
import os
import sys

import bpy
import numpy as np
from mathutils import Euler, Matrix, Vector

# Tool -> extra tilt (degrees) on top of the 45-degree diagonal, if one reads better.
TOOLS = {"StandardClub": 0, "Sledgehammer": 0, "Mysterious": 0}
SIZE = 256
# Linear colours of the untextured materials (same numbers as ToolModelsMenu), (r, g, b, metallic, roughness, glow).
COLOURS = {
    "Club_Dark": (0.341, 0.341, 0.341, 0.5, 0.6, 0),
    "Wand_Pink": (0.8, 0.22, 0.382, 0, 0.4, 0),
    "Wand_Red": (0.8, 0.035, 0.138, 0, 0.3, 0),
    "Wand_Gold": (0.8, 0.441, 0.15, 1, 0.3, 0),
    "Wand_Cream": (0.8, 0.741, 0.636, 0, 0.5, 0),
    "Wand_Gem": (1.0, 0.237, 0.691, 0, 0.1, 0.6),
}
METAL = {"Club_Steel": (0.8, 0.4), "Club_Gold": (1.0, 0.4), "Hammer": (0.3, 0.65), "Club_Rubber": (0.0, 0.8),
         "GolfBall": (0.0, 0.5)}


def build_material(mat, folder):
    mat.use_nodes = True
    nodes, links = mat.node_tree.nodes, mat.node_tree.links
    nodes.clear()
    out = nodes.new("ShaderNodeOutputMaterial")
    bsdf = nodes.new("ShaderNodeBsdfPrincipled")
    links.new(bsdf.outputs["BSDF"], out.inputs["Surface"])
    tex_path = os.path.join(folder, mat.name + ".jpg")
    if os.path.exists(tex_path):
        tex = nodes.new("ShaderNodeTexImage")
        tex.image = bpy.data.images.load(tex_path)
        links.new(tex.outputs["Color"], bsdf.inputs["Base Color"])
        metallic, rough = METAL.get(mat.name, (0.0, 0.5))
        normal_path = os.path.join(folder, mat.name + "_Normal.jpg")
        if os.path.exists(normal_path):
            ntex = nodes.new("ShaderNodeTexImage")
            ntex.image = bpy.data.images.load(normal_path)
            ntex.image.colorspace_settings.name = "Non-Color"
            nmap = nodes.new("ShaderNodeNormalMap")
            links.new(ntex.outputs["Color"], nmap.inputs["Color"])
            links.new(nmap.outputs["Normal"], bsdf.inputs["Normal"])
    else:
        r, g, b, metallic, rough, glow = COLOURS.get(mat.name, (0.7, 0.7, 0.7, 0, 0.5, 0))
        bsdf.inputs["Base Color"].default_value = (r, g, b, 1)
        if glow:
            bsdf.inputs["Emission Color"].default_value = (r, g, b, 1)
            bsdf.inputs["Emission Strength"].default_value = glow * 2
    bsdf.inputs["Metallic"].default_value = metallic
    bsdf.inputs["Roughness"].default_value = rough


def setup_scene():
    scn = bpy.context.scene
    scn.render.engine = "BLENDER_EEVEE"
    scn.render.film_transparent = True
    scn.render.resolution_x = scn.render.resolution_y = SIZE
    scn.render.image_settings.file_format = "PNG"
    scn.render.image_settings.color_mode = "RGBA"
    scn.view_settings.view_transform = "Standard"
    world = bpy.data.worlds.new("Icon")
    world.use_nodes = True
    bg = world.node_tree.nodes["Background"]
    bg.inputs["Color"].default_value = (1, 1, 1, 1)
    bg.inputs["Strength"].default_value = 0.9
    scn.world = world
    key = bpy.data.lights.new("Key", "SUN")
    key.energy = 3.5
    key_obj = bpy.data.objects.new("Key", key)
    key_obj.rotation_euler = Euler((math.radians(40), math.radians(-25), math.radians(20)))
    scn.collection.objects.link(key_obj)
    cam = bpy.data.cameras.new("Cam")
    cam.type = "ORTHO"
    cam_obj = bpy.data.objects.new("Cam", cam)
    cam_obj.location = (0, -2, 0)
    cam_obj.rotation_euler = Euler((math.radians(90), 0, 0))   # looks along +Y at the XZ plane
    scn.collection.objects.link(cam_obj)
    scn.camera = cam_obj
    return cam


def render_tool(tool, spin, root, cam):
    folder = os.path.join(root, "Assets", "_Project", "Art", "Tools", tool)
    before = set(bpy.data.objects)
    bpy.ops.import_scene.fbx(filepath=os.path.join(folder, tool + ".fbx"))
    objs = [o for o in bpy.data.objects if o not in before and o.type == "MESH"]
    for o in objs:
        for slot in o.material_slots:
            if slot.material:
                build_material(slot.material, folder)
    # Whatever axes the FBX round trip gives: bake the transforms, then measure. The grip is the end nearest the
    # origin (the pivot), the long axis goes up (Z), the head's widest direction (club toe, hammer head, wand wings)
    # faces the camera across X. Then tilt 45 degrees: grip top left, head bottom right.
    for o in objs:
        o.data.transform(o.matrix_world)
        o.matrix_world = Matrix.Identity(4)
    pts = np.array([v.co[:] for o in objs for v in o.data.vertices])
    extent = pts.max(axis=0) - pts.min(axis=0)
    axis = int(np.argmax(extent))
    up = np.zeros(3)
    up[axis] = 1.0 if abs(pts[:, axis].max()) < abs(pts[:, axis].min()) else -1.0   # from the head to the grip
    t = pts @ up
    head = pts[t < t.min() + 0.15 * (t.max() - t.min())]
    others = [i for i in range(3) if i != axis]
    wide_i = others[int(np.argmax([np.ptp(head[:, i]) for i in others]))]
    wide = np.zeros(3)
    wide[wide_i] = 1.0 if head[:, wide_i].mean() >= 0 else -1.0   # the side the head sticks out to
    up_v, wide_v = Vector(up), Vector(wide)
    rot = Matrix((wide_v, up_v.cross(wide_v), up_v)).to_4x4()   # rows: (wide, side, up) -> (X, Y, Z)
    tilt = Matrix.Rotation(math.radians(-45 + spin), 4, "Y")
    for o in objs:
        o.data.transform(tilt @ rot)
    bpy.context.view_layer.update()
    pts = [o.matrix_world @ Vector(c) for o in objs for c in o.bound_box]
    lo = Vector((min(p.x for p in pts), min(p.y for p in pts), min(p.z for p in pts)))
    hi = Vector((max(p.x for p in pts), max(p.y for p in pts), max(p.z for p in pts)))
    centre = (lo + hi) / 2
    cam.ortho_scale = max(hi.x - lo.x, hi.z - lo.z) * 1.12
    scn = bpy.context.scene
    scn.camera.location = (centre.x, centre.y - 2, centre.z)
    out = os.path.join(root, "Assets", "_Project", "UI", "ClubIcons", tool + "_Icon.png")
    os.makedirs(os.path.dirname(out), exist_ok=True)
    scn.render.filepath = out
    bpy.ops.render.render(write_still=True)
    print("ICON", out)
    for o in objs:
        bpy.data.objects.remove(o)


def main():
    root = sys.argv[sys.argv.index("--") + 1]
    bpy.ops.wm.read_factory_settings(use_empty=True)
    cam = setup_scene()
    for tool, spin in TOOLS.items():
        render_tool(tool, spin, root, cam)


main()
