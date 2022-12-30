extends Node2D

const FILE_MAGIC_NUMBER := 0xCC170206
const ROOM_MAX_X := 8
const ROOM_MAX_Y := 8
const ROOM_MAX_Z := 11

## Special blocks.
enum Block {
	# Gates.
	EXIT_X0 = 20,
	EXIT_X1 = 22,
	EXIT_Y0 = 16,
	EXIT_Y1 = 18,

	# Light.
	CANDLE = 36,
	TORCH = 45,
	LAMPION = 178,

	# Air.
	PUMP = 24,
	
	# Initial quest.
	SPELLBOOK = 93,
	
	# Ingredients.
	BUDDHA = 92,
	CRUCIFIX = 46,
	PUMPKIN = 181,
	DRAGON = 191,
	FLASK = 157,
	BEANS = 200,
}

# north-west top coordinate of player, in tiles (16x16x16 pixels)
var player_coord: Vector3
var player_velocity: Vector3
var player_on_floor: bool

var room_id = 5
var rooms = []


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
		#print("Room %s: exits %s dims %s" % [n, room.exits, room.dims])

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
	var tilemap: TileMap = $Level
	tilemap.clear()
	
	for z in range(ROOM_MAX_Z):
		#tilemap.set_layer_z_index(z, z)
		tilemap.set_layer_y_sort_origin(z, z*16 + z)
		pass

	for z in range(room.dims.z):
		for y in range(room.dims.y):
			for x in range(room.dims.x):
				var tile_id = room.level[z][y][x]
				if tile_id != 0:
					tilemap.set_cell(10 - z, Vector2i(x + z, y + z), 0, Vector2i(tile_id & 0xf, tile_id >> 4), 0)

func update_room_number():
	$CanvasLayer/RoomNumber.text = "Room %d" % [room_id]

# Player size: somewhat smaller size than a tile.
var TOFS: float = 14.999/16

func place_player():
	# This is not perfect, but as close as we can get with y-sorting.
	var tilemap: TileMap = $Level
	# y-sorting grid offset.
	var grid_pos_s := Vector3i(floor(player_coord.x + TOFS), floor(player_coord.y + TOFS), floor(player_coord.z + TOFS))
	var pos_s = tilemap.map_to_local(Vector2i(grid_pos_s.x + grid_pos_s.z, grid_pos_s.y + grid_pos_s.z))
	# y-sorting layer.
	var layer_offset = Vector2(0.0, tilemap.get_layer_y_sort_origin(10 - grid_pos_s.z) - 1)
	# Display position of player.
	var local_pos = Vector2(player_coord.x - player_coord.y, player_coord.x*0.5 + player_coord.y*0.5 + player_coord.z) * Vector2(16.0, 16.0) + Vector2(16.0, 8.0)
	
	# Sorting position.
	$Level/Player.position = pos_s + layer_offset
	# Position of player sprite relative to sorting position.
	$Level/Player/Player.position = local_pos - $Level/Player.position
	
	# Show sorting position.
	#$Sprite2D.position = $Level.position + pos_s
	
	
func _ready():
	rooms = load_rooms("res://rooms.bin")
	build_room(rooms[room_id])
	update_room_number()
	player_coord = Vector3(3, 3, 9)
	place_player()

func collision_check(pos: Vector3):
	# Player box.
	var x0 := int(floor(pos.x))
	var x1 := int(floor(pos.x + TOFS))
	var y0 := int(floor(pos.y))
	var y1 := int(floor(pos.y + TOFS))
	var z0 := int(floor(pos.z))
	var z1 := int(floor(pos.z + TOFS))
	var room = rooms[room_id]
	for z in range(z0, z1 + 1):
		for y in range(y0, y1 + 1):
			for x in range(x0, x1 + 1):
				if x < 0 or y < 0 or z < 0 or x >= room.dims.x or y >= room.dims.y or z >= room.dims.z:
					return false
				if room.level[z][y][x] != 0:
					return false
	#print(x0, " ", x1, " ", y0, " ", y1, " ", z0, " ", z1)
	return true

func _physics_process(delta):
	var speed = 0.125 * 60
	var direction: Vector3
	if Input.is_action_pressed('ui_up'):
		direction.x = -1.0
	if Input.is_action_pressed('ui_down'):
		direction.x = 1.0
	if Input.is_action_pressed('ui_left'):
		direction.y = 1.0
	if Input.is_action_pressed('ui_right'):
		direction.y = -1.0
		
	#if direction.length_squared() != 0:
	#	$Level/Player.play("walk")
	#else:
	#	$Level/Player.stop()
	if player_on_floor:
		player_velocity.x = speed * direction.x
		player_velocity.y = speed * direction.y
	
	# Jumping
	if player_on_floor and Input.is_action_pressed('ui_accept'):
		player_velocity.z = -3.0
		player_on_floor = false
		
	# Gravity
	player_velocity += Vector3(0.0, 0.0, 3.0) * delta
	
	# Ideally, instead of doing this per coord, the collison would return a vector
	# maximum movement in each direction for slide.
	var new_coord = player_coord + Vector3(player_velocity.x, player_velocity.y, 0.0) * delta
	if collision_check(new_coord):
		player_coord = new_coord
	
	new_coord = player_coord + Vector3(0.0, 0.0, player_velocity.z) * delta
	if collision_check(new_coord):
		player_coord = new_coord
		player_on_floor = false
	else:
		player_on_floor = true
		
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
