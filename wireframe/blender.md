blender notes
--------------

in the off chance, does anyone know about #blender #blender3d scripting?

i need a processing step, before export, that marks tri edges in the mesh that are planar (the inside edges of triangulated polygons), then converts these (after seperating out the triangles) to custom vertex attribute flags

these are used by a shader to hide them in wireframe rendering

i honestly have no idea where to even start

===

https://blenderartists.org/t/labelling-colouring-edges-based-on-concave-convex-angle/1454778/2

https://blender.stackexchange.com/questions/60889/values-of-the-normal-of-a-selected-face
https://docs.blender.org/api/current/mathutils.html#mathutils.Vector.angle



https://blender.stackexchange.com/questions/114736/finding-neighboring-mesh-faces
https://blenderartists.org/t/how-to-get-face-adjacency-information/505885/6



	import bpy
	import bmesh

	ob = bpy.context.edit_object
	me = ob.data
	bm = bmesh.from_edit_mesh(me)
	# have a face selected and active
	face = bm.select_history.active

	link_faces = [f for e in face.edges
				  for f in e.link_faces if f is not face]
	# select them              
	for f in link_faces:
		f.select = True
	# update mesh    
	bmesh.update_edit_mesh(me)
	me.update()

https://docs.blender.org/api/current/bmesh.types.html#bmesh.types.BMFace
  normal
https://docs.blender.org/api/current/bmesh.types.html#bmesh.types.BMEdge

AI suggestions:
https://venice.ai/chat/SbaAzM4fjDKXy8BHX3Xhp

	import bpy
	from bpy import context
	import bmesh
	from mathutils import Vector

	def mark_planar_tri_edges(obj):
		# Prepare the mesh
		mesh = obj.data
		
		# Create a BMesh representation of the mesh
		bm = bmesh.from_edit_mesh(mesh)
		
		# Create a vertex group to store the planar edges
		planar_edges_group = obj.vertex_groups.new(name='PlanarEdges')
		
		# Iterate through the mesh triangles
		for tri in bm.faces:
			if tri.type == bmesh.types.BM_FACE_TYPE_TRI:
				# Get the three edges of the triangle
				edge1, edge2, edge3 = tri.edges
				
				# Check if the triangle edges are planar
				edge1_vec = edge1.verts[1].co - edge1.verts[0].co
				edge2_vec = edge2.verts[1].co - edge2.verts[0].co
				edge3_vec = edge3.verts[1].co - edge3.verts[0].co
				plane_normal = edge1_vec.cross(edge2_vec)
				plane = edge1_vec.project(plane_normal)
				edge_dist = plane.distance_to_point(edge3_vec)
				
				# Check if the distance is close enough to be considered planar (adjust the threshold as needed)
				if abs(edge_dist) < 0.001:
					# Mark the planar triangle edges
					for edge in [edge1, edge2, edge3]:
						for vert in edge.verts:
							planar_edges_group.add([vert.index], 1.0, 'ADD')
		
		# Write the BMesh data back to the mesh
		bmesh.update_edit_mesh(mesh)

	# Get the current active object
	obj = bpy.context.active_object

	# Enter edit mode
	bpy.ops.object.mode_set(mode='EDIT')

	# Call the function to mark planar tri edges
	mark_planar_tri_edges(obj)

	# Exit edit mode
	bpy.ops.object.mode_set(mode='OBJECT')


https://blenderartists.org/t/custom-vertex-attributes-data/1311915
https://projects.blender.org/blender/blender/commit/565510b
https://docs.blender.org/api/current/bpy.types.AttributeGroup.html#bpy.types.AttributeGroup


https://blender.stackexchange.com/questions/266991/manually-setting-custom-attributes-per-edge-vertex-etc

with blender api:

	mesh = bpy.context.collection.objects["Cube"].data
	attribute = mesh.attributes.new(name="new attribute", type="FLOAT", domain="POINT")
	attribute_values = [i for i in range(len(mesh.vertices))]
	attribute.data.foreach_set("value", attribute_values)

with bmesh:

	import bpy
	import bmesh

	mesh = bpy.data.objects["Cube"].data
	attribute = mesh.attributes.new(name="new attribute", type="FLOAT", domain="POINT")
	attribute_values = [i for i in range(len(mesh.vertices))]

	bm = bmesh.from_edit_mesh(mesh)
	layer = bm.verts.layers.float.get(attribute.name)

	for vert in bm.verts:
		print(f"Previous value for {vert} : {vert[layer]}")
		vert[layer] = attribute_values[vert.index]
		print(f"New value for {vert} : {vert[layer]}")

	bmesh.update_edit_mesh(mesh)

===

i'm exporting to GLTF for godot; the shader in godot is working for manually entered mesh data

i think i mostly figured it out; 

- first, triangulate the mesh
- look up the object in bpy.data, then pass it to bmesh to get connectivity information
- for each edge look up the two adjacent triangles through edge.link_faces
  - using this, compute the angle between them with facea.normal.angle(faceb.normal), if this is ~0, they're planar with each other
- then, set the custom attributes for the two vertices comprising the edge, based on this result 
  - bmesh gives access to custom vertex attributes through `bm.verts.layers.float.get(attribute.name)`
- then, export the resulting mesh with the usual gltf exporter

i still need to see how it all fits together and that the data ends up in the right place in godot, will look into it further tomorrow

## How to get attributes to godot?

Use UV2 and up, this will end up in CUSTOM0 and up:

https://www.reddit.com/r/godot/comments/1embc6h/example_of_setting_attributes_in_blender_to_be/
https://github.com/godotengine/godot/pull/52504

UVs are vector2s and the CUSTOMs are vector4s, so UV3 and UV4 are packed into CUSTOM0, UV5 and UV6 are packed into CUSTOM1 etc.

## Triangulating a mesh

	import bpy
	import bmesh

	def triangulate_object(obj):
		me = obj.data
		# Get a BMesh representation
		bm = bmesh.new()
		bm.from_mesh(me)

		bmesh.ops.triangulate(bm, faces=bm.faces[:])
		# V2.79 : bmesh.ops.triangulate(bm, faces=bm.faces[:], quad_method=0, ngon_method=0)

		# Finish up, write the bmesh back to the mesh
		bm.to_mesh(me)
		bm.free()

	# Get the active object (could be any mesh object)
	triangulate_object(bpy.context.active_object)
