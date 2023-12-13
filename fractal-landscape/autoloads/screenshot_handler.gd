# Godot screenshot handler.
# Part of "Xenomusa" (C) Mara Huldra 2022-2023
# SPDX-License-Identifier: MIT
#
# To configure in project settings:
# - Add script as autoload.
# - Assign a key (say, F12) to action "screenshot".
extends Node

# Screenshot target setup.
@export var root_path: String = "user://"
@export var prefix: String = "screenshot"
@export var suffix: String = ".png"
@export var digits: int = 4


## Escape for regular expression. From https://github.com/godotengine/godot-proposals/issues/7995.
func _re_escape(input: String) -> String:
	input = input.replace("\\", "\\\\")
	input = input.replace(".", "\\.")
	input = input.replace("^", "\\^")
	input = input.replace("$", "\\$")
	input = input.replace("*", "\\*")
	input = input.replace("+", "\\+")
	input = input.replace("?", "\\?")
	input = input.replace("(", "\\(")
	input = input.replace(")", "\\)")
	input = input.replace("[", "\\[")
	input = input.replace("]", "\\]")
	input = input.replace("{", "\\{")
	input = input.replace("}", "\\}")
	input = input.replace("|", "\\|")
	return input


# Save an arbitrary Image.
func save_image(image: Image) -> void:
	# List the directory and find highest id used.
	var spec := RegEx.create_from_string("^" + _re_escape(prefix) + "([0-9]{" + str(digits) + "})" + _re_escape(suffix) + "$")
	var highest := 0
	for filename in DirAccess.open(root_path).get_files():
		var m := spec.search(filename)
		if m:
			highest = max(highest, int(m.get_string(1)))
		
	var path := root_path + prefix + str(highest + 1).pad_zeros(digits) + suffix
		
	image.save_png(path)
	print("Saved screenshot to ", path)


# Save an arbitrary viewport.
func save_viewport(viewport: Viewport) -> void:
	save_image(viewport.get_texture().get_image())


# Always process, even when paused.
func _ready() -> void:
	process_mode = PROCESS_MODE_ALWAYS


# Input handler for saving the main viewport.
func _input(event: InputEvent) -> void:
	if event.is_action_pressed(&"screenshot"):
		save_viewport(get_viewport())
