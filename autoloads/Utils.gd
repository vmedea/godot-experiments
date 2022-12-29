extends Node

class _Emitter:
	extends RefCounted
	signal emitted(object)
	
	func emit(object):
		emitted.emit(object)

## Await one of multiple signals. Takes an array of [signal, retval] arrays.
# Returns a proxy object with one signal "emitted".
# This signal will be emitted when one of the passed-in signals is emitted,
# with the associated retval value as argument.
# Note: this only works for signals without parameters.
func select(specs) -> _Emitter:
	var emitter = _Emitter.new()
	for spec in specs:
		spec[0].connect(emitter.emit.bind(spec[1]))
	return emitter

func _rot(angle, n):
	return fposmod(angle + n + 180.0, 360.0) - 180.0

## Animation (4-directional) from heading.
func animation4_from_heading(v: Vector2, angle_down: float, angle_up: float):
	# Angle with respect to +X axis.
	var deg: float = rad_to_deg(v.angle())
	
	# \       /
	#  \  U  /
	#   \   /
	#    \ /
	#  L  x  R
	#    / \
	#   /   \
	#  /  D  \
	# /       \
	
	if abs(_rot(deg, -90.0)) <= angle_down / 2.0:
		return "walk_down"
	if abs(_rot(deg, 90.0)) <= angle_up / 2.0:
		return "walk_up"
	
	if v.x < 0:
		return "walk_left"
	else:
		return "walk_right"
