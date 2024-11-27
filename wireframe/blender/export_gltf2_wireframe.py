# Mara Huldra 2024
# SPDX-License-Identifier: MIT
# link or copy this to ~/.config/blender/X.X/scripts/startup for it to automatically run
import os

import bpy
import bmesh
from mathutils import Vector

TEMP_COLLECTION_NAME = 'TempExportCollection'
EPSILON = 1e-6
# Texture coordinate Y is reversed on GLTF export, somehow. Use 1.0-x.

def delete_collection(new_coll):
    # delete temporary collection and all objects in it
    for obj in new_coll.objects:
        bpy.data.objects.remove(obj, do_unlink=True)
    bpy.data.collections.remove(new_coll)


def hide_edge(uv_layer, loop):
    # to hide an edge, set .x on the current vertex and .y on the other vertex of the edge to 0
    loop[uv_layer].uv = Vector((0.0, loop[uv_layer].uv.y))
    loop.link_loop_next[uv_layer].uv = Vector((loop.link_loop_next[uv_layer].uv.x, 1.0 - 0.0))


def wireframe_export(context, filepath, obj):
    # make a copy, export the copy, delete it
    new_coll = bpy.data.collections.new(TEMP_COLLECTION_NAME)
    print(f'Created temporary collection {new_coll.name}')

    new_obj = obj.copy()
    new_obj.data = obj.data.copy() # deep copy, not linked object
    new_coll.objects.link(new_obj)
    
    # create editable mesh from new object
    me = new_obj.data
    #bm = bmesh.from_edit_mesh(me)
    bm = bmesh.new()
    bm.from_mesh(me)
    
    # first, triangulate, to make sure that our algorithm runs at the triangle level
    # that will be exported.
    bmesh.ops.triangulate(bm, faces=bm.faces[:], quad_method='BEAUTY', ngon_method='BEAUTY')
    # remove zero-area faces and edges
    bmesh.ops.dissolve_degenerate(bm, dist=EPSILON, edges=bm.edges[:])
    
    # need to make sure there are 3 UV layers, as layer 3 will be used for CUSTOM0
    # in godot
    while len(bm.loops.layers.uv) < 3:
        bm.loops.layers.uv.new()
    uv_layer = bm.loops.layers.uv[2]
    
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
        
        if face_a.normal.angle(face_b.normal) <= EPSILON:
            print(f'Hiding edge {edge.index}')
            hide_edge(uv_layer, loop_a)
            hide_edge(uv_layer, loop_b)

    bm.to_mesh(me)

    # link collection to scene (somehow, this is needed, or the exporter will not export anything)
    bpy.context.scene.collection.children.link(new_coll)
    
    print('Number of objects ', len(new_coll.objects))
    bpy.ops.export_scene.gltf(filepath=filepath, 
        check_existing=False, 
        export_format='GLB',
        collection=new_coll.name)
    # https://docs.blender.org/api/current/bpy.ops.export_scene.html#bpy.ops.export_scene.gltf
    
    # delete temporary collection and all objects in it
    delete_collection(new_coll)


def export_current_object(context, filepath):
    wireframe_export(context, filepath, context.active_object)
    # XX process all selected objects not just current one
    # bpy.context.selected_objects
    return {'FINISHED'}


# ExportHelper is a helper class, defines filename and
# invoke() function which calls the file selector.
from bpy_extras.io_utils import ExportHelper
from bpy.props import StringProperty, BoolProperty, EnumProperty
from bpy.types import Operator


class ExportSomeData(Operator, ExportHelper):
    """This appears in the tooltip of the operator and in the generated docs"""
    bl_idname = "export.gltf2_wireframe"
    bl_label = "Export GLTF2 with wireframe processing"

    # ExportHelper mix-in class uses this.
    filename_ext = ".glb"

    filter_glob: StringProperty(
        default="*.glb",
        options={'HIDDEN'},
        maxlen=255,  # Max internal buffer length, longer would be clamped.
    )

    def execute(self, context):
        return export_current_object(context, self.filepath)


# Only needed if you want to add into a dynamic menu
def menu_func_export(self, context):
    self.layout.operator(ExportSomeData.bl_idname, text="Export GLTF2 with wireframe processing")


# Register and add to the "file selector" menu (required to use F3 search "Text Export Operator" for quick access).
def register():
    bpy.utils.register_class(ExportSomeData)
    bpy.types.TOPBAR_MT_file_export.append(menu_func_export)


def unregister():
    bpy.utils.unregister_class(ExportSomeData)
    bpy.types.TOPBAR_MT_file_export.remove(menu_func_export)
