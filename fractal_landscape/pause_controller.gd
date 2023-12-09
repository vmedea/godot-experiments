extends Node


func _ready():
	process_mode = PROCESS_MODE_ALWAYS


func _input(evt):
	if evt.is_action_pressed("pause"):
		get_tree().paused = !get_tree().paused
	if evt.is_action_pressed("ui_cancel"):
		get_tree().quit()
