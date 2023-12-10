# Godot screenshot handler.
# Part of "Xenomusa" (C) Mara Huldra 2022
# SPDX-License-Identifier: MIT
#
# To configure in project settings:
# - Add script as autoload.
# - Assign a key to action "screenshot".
extends Node

# Screenshot target setup.
@export var root_path: String = "user://"
@export var prefix: String = "screenshot"
@export var suffix: String = ".png"
@export var digits: int = 4

func _re_escape(m):
	"Esacape for regular expression. Currently only handles \".\"."
	return m.replace(".", "\\.")

func _input(event) -> void:
	if event.is_action_pressed("screenshot"):
		var image = get_viewport().get_texture().get_image()
		
		# List the directory and find highest id used.
		var spec = RegEx.create_from_string("^" + _re_escape(prefix) + "([0-9]{" + str(digits) + "})" + _re_escape(suffix) + "$")
		var highest = 0
		for filename in DirAccess.open(root_path).get_files():
			var m = spec.search(filename)
			if m:
				highest = max(highest, int(m.get_string(1)))
			
		var path = root_path + prefix + str(highest + 1).pad_zeros(digits) + suffix
			
		image.save_png(path)
		print("Saved screenshot to ", path)
