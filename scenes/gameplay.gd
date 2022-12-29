extends Node2D

const FILE_MAGIC_NUMBER := 0xCC170206
const ROOM_MAX_X := 8
const ROOM_MAX_Y := 8
const ROOM_MAX_Z := 11

# north-west top coordinate of player, in tiles (16x16x16 pixels)
var player_coord: Vector3

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
		# right -Y   north
		# down  +X   east
		# left  +Y   south
		# up    -X   west
		room.exits = {'north': f.get_16(), 'east': f.get_16(), 'south': f.get_16(), 'west': f.get_16()}
		room.dims = {'x': f.get_8(), 'y': f.get_8(), 'z': ROOM_MAX_Z} # Height is always MAX_Z
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
		tilemaps[z].clear()

	for z in range(room.dims.z):
		var tilemap: TileMap = tilemaps[z]
		for y in range(room.dims.y):
			for x in range(room.dims.x):
				var tile_id = room.level[z][y][x]
				if tile_id != 0:
					tilemap.set_cell(0, Vector2i(x, y), 0, Vector2i(tile_id & 0xf, tile_id >> 4), 0)

func update_room_number():
	$CanvasLayer/RoomNumber.text = "Room %d" % [room_id]

var room_id = 0
var rooms = []

func place_player():
	var coord: Vector3 = player_coord + Vector3(0.0, -1.0, 0.0)
	$Level/Z9/Player.position = Vector2(coord.x - coord.y, coord.x*0.5 + coord.y*0.5 + coord.z) * Vector2(16.0, 16.0)

func _ready():
	rooms = load_rooms("res://rooms.bin")
	build_room(rooms[room_id])
	update_room_number()
	player_coord = Vector3(0, 0, 10)
	place_player()

func _process(delta):
	var speed = 0.125 * 60
	if Input.is_action_pressed('ui_up'):
		player_coord.x -= speed * delta
	if Input.is_action_pressed('ui_down'):
		player_coord.x += speed * delta
	if Input.is_action_pressed('ui_left'):
		player_coord.y += speed * delta
	if Input.is_action_pressed('ui_right'):
		player_coord.y -= speed * delta
	place_player()

func _input(event):
	var new_room_id = null
	#if event.is_action_pressed("ui_up"):
	#	new_room_id = rooms[room_id].exits.west
	#if event.is_action_pressed("ui_right"):
	#	new_room_id = rooms[room_id].exits.north
	#if event.is_action_pressed("ui_down"):
	#	new_room_id = rooms[room_id].exits.east
	#if event.is_action_pressed("ui_left"):
	#	new_room_id = rooms[room_id].exits.south
	if event.is_action_pressed("ui_page_up"):
		new_room_id = room_id - 1
	if event.is_action_pressed("ui_page_down"):
		new_room_id = room_id + 1
	if new_room_id != null:
		if new_room_id >= 0 and new_room_id < rooms.size():
			room_id = new_room_id
			build_room(rooms[room_id])
			update_room_number()

		
	if event.is_action_pressed("ui_cancel"):
		get_tree().quit()
