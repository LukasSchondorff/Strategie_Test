import bpy
from mathutils import Matrix, Vector


def origin_to_bottom(ob, matrix=Matrix()):
    me = ob.data
    mw = ob.matrix_world
    local_verts = [matrix @ Vector(v[:]) for v in ob.bound_box]
    o = sum(local_verts, Vector()) / 8
    o.z = min(v.z for v in local_verts)
    o = matrix.inverted() @ o
    me.transform(Matrix.Translation(-o))

    mw.translation = mw @ o

for o in bpy.context.scene.objects:
    if o.type == 'MESH':
        origin_to_bottom(o)
        #origin_to_bottom(o, matrix=o.matrix_world) # global