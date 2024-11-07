extends MeshInstance3D


func _ready() -> void:
	var vertices := PackedVector3Array()
	vertices.push_back(Vector3(-1, -1, 0))
	vertices.push_back(Vector3(-1, 1, 0))
	vertices.push_back(Vector3(1, -1, 0))
	
	vertices.push_back(Vector3(1, -1, 0))
	vertices.push_back(Vector3(-1, 1, 0))
	vertices.push_back(Vector3(1, 1, 0))
	
	var flags := PackedByteArray()
	flags.append_array(PackedByteArray([255, 255, 0, 0]))
	flags.append_array(PackedByteArray([255, 0,   0, 0]))
	flags.append_array(PackedByteArray([0,   255, 0, 0]))
	
	flags.append_array(PackedByteArray([255, 0,   0, 0]))
	flags.append_array(PackedByteArray([0,   255, 0, 0]))
	flags.append_array(PackedByteArray([255, 255, 0, 0]))
	
	var arr_mesh := ArrayMesh.new()
	var arrays = []
	arrays.resize(Mesh.ARRAY_MAX)
	arrays[Mesh.ARRAY_VERTEX] = vertices
	arrays[Mesh.ARRAY_CUSTOM0] = flags
	arr_mesh.add_surface_from_arrays(Mesh.PRIMITIVE_TRIANGLES, arrays)
	
	var material := ShaderMaterial.new()
	material.shader = load("res://shader.gdshader")
	arr_mesh.surface_set_material(0, material)

	mesh = arr_mesh


func _process(delta: float) -> void:
	pass
