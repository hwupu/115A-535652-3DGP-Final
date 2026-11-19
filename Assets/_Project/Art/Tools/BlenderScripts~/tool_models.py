"""Turn a downloaded tool model (Sketchfab) into a game tool: grip at the origin, hanging down, sized like the
placeholder in Prefabs/Tools. Used for the Sledgehammer and the Mysterious wand (the club has its own script,
golf_club_and_ball.py, because it also splits out the ball).

Run headless (Blender 5.3):
    blender -b --factory-startup --python tool_models.py -- <tool> <source .fbx/.blend> <out_dir>
    <tool> = Sledgehammer | Mysterious

Writes <out_dir>/<tool>.fbx, <tool>.blend and <tool>_preview.png.
How it is placed (conventions: Assets/_Project/Art/README.md):
  - the kept parts are joined into one mesh (modifiers applied), reduced to at most MAX_TRIS triangles;
  - the long axis (principal axis of the vertices) becomes vertical, the head (the end with the wider cross
    section) at the bottom, the grip end at the top;
  - the head's widest direction goes along Blender X (= across the swing in Unity, like the placeholders);
  - scaled to LENGTH, the top at TOP above the grip (origin), thickened by THICKEN across the long axis.
Materials keep their source names mapped by MATERIALS; the Magic tool (Use Tool and Ball Models) builds the
Unity materials with the same names.
"""
import os
import sys

import bmesh
import bpy
import numpy as np
from mathutils import Matrix, Vector

TOOLS = {
    # Placeholder: handle 0 .. -0.26, iron head 0.08 x 0.08 x 0.16 across the swing (to -0.30).
    "Sledgehammer": dict(keep=None, length=0.34, top=0.04, thicken=1.8, max_tris=2000,
                         materials={"Material": "Hammer"}),
    # Placeholder: wand 0 .. -0.28 with an orb at -0.30. Only the wand: the card and glow planes are dropped.
    "Mysterious": dict(keep=["Cylinder", "Cylinder.001", "Cylinder.003"], length=0.34, top=0.04, thicken=1.0,
                       max_tris=4000,
                       materials={"Material.002": "Wand_Cream", "Material.003": "Wand_Gem",
                                  "Material.004": "Wand_Pink", "Material.005": "Wand_Red",
                                  "Material.006": "Wand_Gold"}),
}
END_SLICE = 0.15   # fraction of the length looked at to tell the head end from the grip end


def args():
    a = sys.argv[sys.argv.index("--") + 1:]
    return a[0], a[1], a[2]


def load(path):
    if path.lower().endswith(".blend"):
        bpy.ops.wm.open_mainfile(filepath=path)
    else:
        bpy.ops.wm.read_factory_settings(use_empty=True)
        bpy.ops.import_scene.fbx(filepath=path)
    # The wand's source scene has unit scale 100: FBX_SCALE_ALL would export it 100x too big for Unity.
    bpy.context.scene.unit_settings.scale_length = 1.0


def joined_mesh(keep):
    """One new object holding the evaluated (modifiers applied), world-space meshes of the kept objects."""
    dg = bpy.context.evaluated_depsgraph_get()
    sources = [o for o in bpy.context.scene.objects if o.type == 'MESH' and (keep is None or o.name in keep)]
    bm = bmesh.new()
    materials = []
    for o in sources:
        ev = o.evaluated_get(dg)
        me = ev.to_mesh()
        me.transform(o.matrix_world)
        slot_map = []
        for slot in o.material_slots:
            if slot.material not in materials:
                materials.append(slot.material)
            slot_map.append(materials.index(slot.material))
        for p in me.polygons:
            p.material_index = slot_map[p.material_index] if slot_map else 0
        bm.from_mesh(me)
        ev.to_mesh_clear()
    mesh = bpy.data.meshes.new("Tool")
    bm.to_mesh(mesh)
    bm.free()
    for m in materials:
        mesh.materials.append(m)
    for o in list(bpy.context.scene.objects):
        bpy.data.objects.remove(o)
    obj = bpy.data.objects.new("Tool", mesh)
    bpy.context.scene.collection.objects.link(obj)
    return obj


def principal_axes(points):
    centred = points - points.mean(axis=0)
    _, vecs = np.linalg.eigh(np.cov(centred.T))
    return vecs[:, ::-1]   # columns, largest variance first


def place(obj, cfg):
    me = obj.data
    pts = np.array([v.co[:] for v in me.vertices])
    axes = principal_axes(pts)
    long_axis = Vector(axes[:, 0])
    t = pts @ axes[:, 0]
    lo, hi = t.min(), t.max()
    length = hi - lo
    low_end = pts[t < lo + END_SLICE * length]
    high_end = pts[t > hi - END_SLICE * length]

    def spread(end):
        flat = end - np.outer(end @ axes[:, 0], axes[:, 0])
        return np.ptp(flat @ axes[:, 1]) + np.ptp(flat @ axes[:, 2])

    head_is_high = spread(high_end) > spread(low_end)
    up = -long_axis if head_is_high else long_axis   # from the head toward the grip
    head = high_end if head_is_high else low_end
    grip = low_end if head_is_high else high_end

    # Head's widest direction, perpendicular to the long axis.
    head_flat = head - np.outer(head @ np.array(up), np.array(up))
    head_axes = principal_axes(head_flat)
    wide = Vector(head_axes[:, 0])
    wide = (wide - wide.dot(up) * up).normalized()
    side = up.cross(wide).normalized()
    # Rotation taking (wide, side, up) to (X, Y, Z).
    rot = Matrix((wide, side, up)).to_4x4()

    grip_top = max(Vector(p).dot(up) for p in grip)
    grip_centre = Vector(grip.mean(axis=0))
    offset = rot @ grip_centre
    m = Matrix.Translation(Vector((-offset.x, -offset.y, -grip_top))) @ rot
    s = cfg["length"] / length
    m = Matrix.Diagonal((s * cfg["thicken"], s * cfg["thicken"], s, 1)) @ m
    m = Matrix.Translation(Vector((0, 0, cfg["top"]))) @ m
    me.transform(m)
    me.update()


def reduce(obj, max_tris):
    obj.data.calc_loop_triangles()
    tris = len(obj.data.loop_triangles)
    if tris <= max_tris:
        return
    mod = obj.modifiers.new("Decimate", 'DECIMATE')
    mod.ratio = max_tris / tris
    bpy.context.view_layer.objects.active = obj
    bpy.ops.object.modifier_apply(modifier=mod.name)


def finish(obj, name, cfg):
    obj.name = obj.data.name = name
    for slot in obj.material_slots:
        if slot.material and slot.material.name in cfg["materials"]:
            slot.material.name = cfg["materials"][slot.material.name]
        mat = slot.material
        if mat and mat.use_nodes:   # viewport colour = shader colour, for the preview (and Blender's solid view)
            for node in mat.node_tree.nodes:
                if node.type in {'BSDF_DIFFUSE', 'BSDF_GLASS', 'BSDF_PRINCIPLED'} and 'Color' in node.inputs:
                    mat.diffuse_color = node.inputs['Color'].default_value
                    break
                if node.type == 'BSDF_PRINCIPLED':
                    mat.diffuse_color = node.inputs['Base Color'].default_value
                    break
    for poly in obj.data.polygons:
        poly.use_smooth = True


def stats(obj):
    obj.data.calc_loop_triangles()
    vs = [v.co for v in obj.data.vertices]
    lo = Vector((min(v.x for v in vs), min(v.y for v in vs), min(v.z for v in vs)))
    hi = Vector((max(v.x for v in vs), max(v.y for v in vs), max(v.z for v in vs)))
    print(f"STATS {obj.name}: tris={len(obj.data.loop_triangles)} "
          f"min=({lo.x:.3f},{lo.y:.3f},{lo.z:.3f}) max=({hi.x:.3f},{hi.y:.3f},{hi.z:.3f}) "
          f"materials={[s.material.name for s in obj.material_slots]}")


def export(obj, out_dir):
    bpy.ops.object.select_all(action='DESELECT')
    obj.select_set(True)
    bpy.context.view_layer.objects.active = obj
    path = os.path.join(out_dir, obj.name + ".fbx")
    bpy.ops.export_scene.fbx(filepath=path, use_selection=True, object_types={'MESH'},
                             axis_forward='-Z', axis_up='Y', apply_scale_options='FBX_SCALE_ALL',
                             bake_space_transform=True, path_mode='STRIP', embed_textures=False,
                             mesh_smooth_type='FACE', add_leaf_bones=False)
    bpy.ops.wm.save_as_mainfile(filepath=os.path.join(out_dir, obj.name + ".blend"), copy=True)
    print("EXPORTED", path)


def render_preview(obj, out_dir):
    scn = bpy.context.scene
    bpy.ops.object.camera_add(location=(0.9, 0.5, -0.05))
    cam = bpy.context.object
    target = Vector((0, 0, -0.13))
    cam.rotation_euler = (target - cam.location).to_track_quat('-Z', 'Y').to_euler()
    cam.data.type = 'ORTHO'
    cam.data.ortho_scale = 0.5
    scn.camera = cam
    world = bpy.data.worlds.new("Preview")
    world.color = (0.55, 0.6, 0.55)
    scn.world = world
    scn.render.engine = 'BLENDER_WORKBENCH'
    scn.display.shading.light = 'STUDIO'
    scn.display.shading.color_type = 'MATERIAL'
    scn.render.resolution_x = scn.render.resolution_y = 600
    scn.render.filepath = os.path.join(out_dir, obj.name + "_preview.png")
    bpy.ops.render.render(write_still=True)


def main():
    tool, src, out_dir = args()
    cfg = TOOLS[tool]
    os.makedirs(out_dir, exist_ok=True)
    load(src)
    obj = joined_mesh(cfg["keep"])
    place(obj, cfg)
    reduce(obj, cfg["max_tris"])
    finish(obj, tool, cfg)
    stats(obj)
    export(obj, out_dir)
    render_preview(obj, out_dir)


if __name__ == "__main__":
    main()
