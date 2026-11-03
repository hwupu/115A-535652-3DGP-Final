import bpy, sys
p = sys.argv[sys.argv.index("--")+1]
bpy.ops.wm.open_mainfile(filepath=p)
for img in list(bpy.data.images):
    bpy.data.images.remove(img)
bpy.ops.wm.save_as_mainfile(filepath=p, compress=True)
