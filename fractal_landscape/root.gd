extends Node2D

# Would be nice:
#   nicer speed bar (gradient bar)

@onready var info_label: Label = $TextureRect/InfoLabel;
@onready var speed_gauge: TextureProgressBar = $TextureRect/Speed;

func _set_info(debug: String, distance: String, heading: String, speed: int):
	info_label.text = debug + "\n" + distance + "\n" + heading + "\nSpeed: ";
	speed_gauge.value = speed;


func _ready():
	$TextureRect.Info.connect(_set_info)


func _input(evt):
	if evt.is_action_pressed("ui_cancel"):
		get_tree().quit()
