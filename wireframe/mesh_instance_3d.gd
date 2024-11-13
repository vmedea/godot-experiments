extends MeshInstance3D


func set_wireframe_mesh(mesh_in: ArrayMesh) -> void:
	assert(mesh_in.surface_get_primitive_type(0) == Mesh.PRIMITIVE_TRIANGLES)
	var arr_mesh := MeshUtils.unroll_vertices(mesh_in, [Mesh.ARRAY_VERTEX, Mesh.ARRAY_CUSTOM0])
	assert(arr_mesh.surface_get_primitive_type(0) == Mesh.PRIMITIVE_TRIANGLES)
	
	var material := ShaderMaterial.new()
	material.shader = preload("res://shader.gdshader")
	arr_mesh.surface_set_material(0, material)

	mesh = arr_mesh


func _ready() -> void:
	pass


func _process(delta: float) -> void:
	pass
