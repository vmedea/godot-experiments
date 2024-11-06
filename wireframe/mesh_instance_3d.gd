extends MeshInstance3D


func _ready() -> void:
	var vertices := PackedVector3Array()
	vertices.push_back(Vector3(0, 1, 0))
	vertices.push_back(Vector3(1, 0, 0))
	vertices.push_back(Vector3(0, 0, 1))
	
	var color0 := PackedByteArray()
	color0.append_array(PackedByteArray([255, 0, 0, 255]))
	color0.append_array(PackedByteArray([0, 255, 0, 255]))
	color0.append_array(PackedByteArray([0, 0, 255, 255]))
	var color1 := PackedByteArray()
	color1.append_array(PackedByteArray([255, 255, 0, 255]))
	color1.append_array(PackedByteArray([0, 255, 255, 255]))
	color1.append_array(PackedByteArray([255, 0, 255, 255]))
	
	var arr_mesh := ArrayMesh.new()
	var arrays = []
	arrays.resize(Mesh.ARRAY_MAX)
	arrays[Mesh.ARRAY_VERTEX] = vertices
	arrays[Mesh.ARRAY_CUSTOM0] = color0
	arrays[Mesh.ARRAY_CUSTOM1] = color1
	arr_mesh.add_surface_from_arrays(Mesh.PRIMITIVE_TRIANGLES, arrays)
	
	var material := ShaderMaterial.new()
	material.shader = load("res://shader.gdshader")
	arr_mesh.surface_set_material(0, material)

	mesh = arr_mesh


func _process(delta: float) -> void:
	pass
