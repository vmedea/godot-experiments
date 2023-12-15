extends Node2D

@onready var info_label: Label = $TextureRect/InfoLabel;
@onready var speed_gauge: TextureProgressBar = $TextureRect/Speed;


func _set_info(debug: String, state: int, distance: int, heading: float, headingDelta: float, speed: int) -> void:
	if state >= 1:
		var headingStr := "Heading: %+.3f %+.3f" % [heading, headingDelta]
		var distanceStr := ""
		if state == 1:
			distanceStr = "Distance to start of canyon: %d" % [distance]
		elif state == 2:
			distanceStr = "Distance to end of canyon: %d" % [distance]

		info_label.text = debug + "\n" + distanceStr + "\n" + headingStr + "\nSpeed: "
		speed_gauge.value = speed
		speed_gauge.show()
	else:
		info_label.text = ""
		speed_gauge.hide()


func _ready() -> void:
	$TextureRect.Info.connect(_set_info)
