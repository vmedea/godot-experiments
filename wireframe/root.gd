extends Node3D

@onready var tri: MeshInstance3D = $MeshInstance3D;


func _input(event: InputEvent):
	if event.is_action_pressed(&"ui_cancel"):
		get_tree().quit()


func _process(delta: float):
	tri.rotate_y(delta)
