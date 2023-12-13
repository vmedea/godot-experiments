extends Node


func _ready() -> void:
	process_mode = PROCESS_MODE_ALWAYS


func _input(evt: InputEvent) -> void:
	if evt.is_action_pressed("pause"):
		get_tree().paused = !get_tree().paused
	if evt.is_action_pressed("ui_cancel"):
		get_tree().quit()
