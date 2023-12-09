extends Node2D

@onready var info_label: Label = $TextureRect/InfoLabel;
@onready var speed_gauge: TextureProgressBar = $TextureRect/Speed;


func _set_info(debug: String, distance: String, heading: String, speed: int):
	info_label.text = debug + "\n" + distance + "\n" + heading + "\nSpeed: "
	speed_gauge.value = speed


func _ready():
	$TextureRect.Info.connect(_set_info)
