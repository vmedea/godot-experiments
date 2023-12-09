extends Node2D

# TODO:
#   overlay debug messages (distance to canyon etc)
# Would be nice:
#   nicer speed bar (gradient bar)

# Called when the node enters the scene tree for the first time.
func _ready():
	pass # Replace with function body.

func _input(evt):
	if evt.is_action_pressed("ui_cancel"):
		get_tree().quit()
