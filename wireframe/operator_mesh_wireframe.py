import bpy
import bmesh

EPSILON = 1e-6
# Texture coordinate Y is reversed on GLTF export, somehow. Use 1.0-x.

def hide_edge(uv_layer, loop):
    # to hide an edge, set .x on the current vertex and .y on the other vertex of the edge to 0
    loop[uv_layer].uv = Vector((0.0, loop[uv_layer].uv.y))
    loop.link_loop_next[uv_layer].uv = Vector((loop.link_loop_next[uv_layer].uv.x, 1.0 - 0.0))


def main(context):
    bpy.ops.object.mode_set(mode='EDIT')
    obj = context.active_object
    me = obj.data
    bm = bmesh.from_edit_mesh(me)
    
    # first, triangulate, to make sure that our algorithm runs at the triangle level
    # that will be exported.
    bmesh.ops.triangulate(bm, faces=bm.faces[:], quad_method='BEAUTY', ngon_method='BEAUTY')
    # remove zero-area faces and edges
    bmesh.ops.dissolve_degenerate(bm, dist=EPSILON, edges=bm.edges[:])
    
    # need to add 3 UV layers, as layer 3 will be used for CUSTOM0
    while len(bm.loops.layers.uv) < 3:
        bm.loops.layers.uv.new()
    uv_layer = bm.loops.layers.uv[2]
    
    for edge in bm.edges:
        edge.select = False
    for face in bm.faces:
        face.select = False
        
    # Set flags to visible by default
    for face in bm.faces:
        for loop in face.loops:
            loop[uv_layer].uv = Vector((1.0, 0.0))
    
    for edge in bm.edges:
        if len(edge.link_faces) != 2:
            raise ValueError(f"Edge {edge.index} doesn't have two linked faces")
        if len(edge.link_loops) != 2:
            raise ValueError(f"Edge {edge.index} doesn't have two linked loops")
        face_a = edge.link_faces[0]
        face_b = edge.link_faces[1]
        loop_a = edge.link_loops[0]
        loop_b = edge.link_loops[1]
        if loop_a.face != face_a or loop_b.face != face_b:
            raise ValueError(f"Edge {edge.index} has face-loop inconsistency")
        
        # detect degenerate faces
        if face_a.normal.length < EPSILON or face_b.normal.length < EPSILON:
            #edge.select = True
            if face_a.normal.length < EPSILON:
                print(f'Face {face_a.index} has s')
                face_a.select = True
            if face_b.normal.length < EPSILON:
                face_b.select = True
            print(f"Edge {edge.index} has a face with zero-length normal")

        if face_a.normal.angle(face_b.normal) <= EPSILON:
        #if False:
            print(f'Hiding edge {edge.index}')
            hide_edge(uv_layer, loop_a)
            hide_edge(uv_layer, loop_b)

    bmesh.update_edit_mesh(me)


class UvOperator(bpy.types.Operator):
    """UV Operator description"""
    bl_idname = "uv.wireframe_uv_edges"
    bl_label = "Wireframe UV edges"

    @classmethod
    def poll(cls, context):
        obj = context.active_object
        return obj and obj.type == 'MESH' and obj.mode == 'EDIT'

    def execute(self, context):
        main(context)
        return {'FINISHED'}


def menu_func(self, context):
    self.layout.operator(UvOperator.bl_idname, text="Wireframe UV edges")


# Register and add to the "UV" menu (required to also use F3 search "Simple UV Operator" for quick access).
def register():
    bpy.utils.register_class(UvOperator)
    bpy.types.IMAGE_MT_uvs.append(menu_func)


def unregister():
    bpy.utils.unregister_class(UvOperator)
    bpy.types.IMAGE_MT_uvs.remove(menu_func)


if __name__ == "__main__":
    #register()

    # test call
    #bpy.ops.uv.simple_operator()
    main(bpy.context)
