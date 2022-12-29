extends Node2D

const FILE_MAGIC_NUMBER := 0xCC170206
const ROOM_MAX_X := 8
const ROOM_MAX_Y := 8
const ROOM_MAX_Z := 11

func load_rooms(filename):
	var f = FileAccess.open(filename, FileAccess.READ)
	var magic := f.get_32()
	if magic != FILE_MAGIC_NUMBER:
		printerr("load_rooms: Wrong magic number")
		return null
	var num_rooms := f.get_32()
	print("Number of rooms: %s" % [num_rooms])
	var rooms_data = []
	for n in num_rooms:
		var room = {}
		room.exits = [f.get_16(), f.get_16(), f.get_16(), f.get_16()]
		room.dims = [f.get_8(), f.get_8(), ROOM_MAX_Z] # Height is always MAX_Z
		print("Room %s: exits %s dims %s" % [n, room.exits, room.dims])

		room.wall_type = f.get_8()
		room.light_default = f.get_8()
		room.level = []
		for z in ROOM_MAX_Z:
			var slice = []
			for y in ROOM_MAX_Y:
				slice.append(f.get_buffer(ROOM_MAX_X))
			room.level.append(slice)
				
		rooms_data.append(room)
	return rooms_data

func build_room(room):
	var tilemaps = [$Level/Z0, $Level/Z1, $Level/Z2, $Level/Z3, $Level/Z4, $Level/Z5, $Level/Z6, $Level/Z7, $Level/Z8, $Level/Z9, $Level/Z10]
	for z in range(ROOM_MAX_Z):
		var tilemap: TileMap = tilemaps[z]
		for y in range(ROOM_MAX_Y):
			for x in range(ROOM_MAX_X):
				var tile_id = room.level[z][y][x]
				if tile_id != 0:
					tilemap.set_cell(0, Vector2i(x, y), 0, Vector2i(tile_id & 0xf, tile_id >> 4), 0)
				else:
					tilemap.set_cell(0, Vector2i(x, y))

var room_id = 0
var rooms = []

func _ready():
	rooms = load_rooms("res://rooms.bin")
	build_room(rooms[room_id])

func _input(event):
	if event.is_action_pressed("ui_left"):
		room_id -= 1
		if room_id < 0:
			room_id = 0
		build_room(rooms[room_id])
	if event.is_action_pressed("ui_right"):
		room_id += 1
		if room_id >= rooms.size():
			room_id = rooms.size() - 1
		build_room(rooms[room_id])
