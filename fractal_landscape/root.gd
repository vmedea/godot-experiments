extends Node2D

@onready var info_label: Label = $TextureRect/InfoLabel;
@onready var speed_gauge: TextureProgressBar = $TextureRect/Speed;


func _set_info(debug: String, distance: String, heading: float, headingDelta: float, speed: int):
	var headingStr := "Heading: %+.3f %+.3f" % [heading, headingDelta];

	info_label.text = debug + "\n" + distance + "\n" + headingStr + "\nSpeed: "
	speed_gauge.value = speed


func _ready():
	$TextureRect.Info.connect(_set_info)
