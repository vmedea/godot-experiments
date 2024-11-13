@tool
extends EditorScript

const OUT_PATH: String = "res://wireframes/"

func do_convert(filename: String) -> void:
	var filename_out := OUT_PATH + filename.get_file().trim_suffix(".glb") + ".res"
	print(filename, " -> ", filename_out)
	
	var packedscene_in: PackedScene = load(filename)
	var scene_in := packedscene_in.instantiate()
	
	var children := scene_in.get_children()
	
	var mesh_in: ArrayMesh = children[0].mesh

	assert(mesh_in.surface_get_primitive_type(0) == Mesh.PRIMITIVE_TRIANGLES)
	var arr_mesh := MeshUtils.unroll_vertices(mesh_in, [Mesh.ARRAY_VERTEX, Mesh.ARRAY_CUSTOM0])
	assert(arr_mesh.surface_get_primitive_type(0) == Mesh.PRIMITIVE_TRIANGLES)
	
	var material := ShaderMaterial.new()
	material.shader = preload("res://shader.gdshader")
	arr_mesh.surface_set_material(0, material)
	

	ResourceSaver.save(arr_mesh, filename_out)

	
func _run() -> void:
	var dir = DirAccess.open("res://")
	for filename in dir.get_files():
		#if filename.ends_with(".res"):
		#	do_convert(filename)
		if filename.ends_with(".glb"):
			do_convert(filename)
