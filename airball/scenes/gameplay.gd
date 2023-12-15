extends Node2D

# Player size: somewhat smaller size than a tile.
const TOFS: float = 14.999/16


# north-west top coordinate of player, in tiles (16x16x16 pixels)
var player_coord: Vector3
var player_velocity: Vector3
var player_on_floor: bool

var room_id = 0
var rooms = []
var room: GameRoom
## Flags from Types.BlockAttributes for each block type.
var block_attributes: Array[int]

const Blocks = Types.Blocks
const Compass = Types.Compass

func get_block_attributes(tileset: TileSet) -> Array[int]:
	var source: TileSetAtlasSource = tileset.get_source(0)
	var blocks: Array[int] = []
	var size: Vector2i = source.get_atlas_grid_size()
	for y in range(size.y):
		for x in range(size.x):
			var data: TileData = source.get_tile_data(Vector2i(x, y), 0)
			var flags: int = 0
			for bit in 8:
				if data.get_custom_data_by_layer_id(bit):
					flags |= 1 << bit
			blocks.append(flags)
	return blocks


func build_room(room):
	var tilemap: TileMap = $Level
	tilemap.clear()
	
	for z in range(GameRoom.ROOM_MAX_Z):
		tilemap.set_layer_y_sort_origin(z, z*16 + z)

	for z in range(room.dims.z):
		for y in range(room.dims.y):
			for x in range(room.dims.x):
				var tile_id = room.get_block(Vector3(x, y, z))
				if tile_id > 0:
					tilemap.set_cell(10 - z, Vector2i(x + z, y + z), 0, Vector2i(tile_id & 0xf, tile_id >> 4), 0)


func enter_room(new_room_id: int, exit_id: int):
	if new_room_id >= 0 and new_room_id < rooms.size():
		room_id = new_room_id
		room = rooms[room_id]
		if exit_id >= 0:
			player_coord = room.find_entrance_from(exit_id)
		else:
			player_coord = Vector3(3, 3, 7)
		player_velocity = Vector3(0.0, 0.0, 0.0)
		build_room(room)
		# Need to find out where to enter in new room.
		update_room_number()


func update_room_number():
	$CanvasLayer/RoomNumber.text = "Room %d" % [room_id]
	print('Entrance from north: ', room.find_entrance_from(Types.Compass.NORTH))
	print('Entrance from south: ', room.find_entrance_from(Types.Compass.SOUTH))
	print('Entrance from west: ', room.find_entrance_from(Types.Compass.WEST))
	print('Entrance from east: ', room.find_entrance_from(Types.Compass.EAST))


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
	block_attributes = get_block_attributes(load("res://tilesets/game.tres"))
	rooms = GameRoom.load_rooms("res://rooms.bin")
	enter_room(0, -1)


## Check new position for collisions.
func collision_check(room, pos: Vector3):
	# Player box.
	var x0 := int(floor(pos.x))
	var x1 := int(floor(pos.x + TOFS))
	var y0 := int(floor(pos.y))
	var y1 := int(floor(pos.y + TOFS))
	var z0 := int(floor(pos.z))
	var z1 := int(floor(pos.z + TOFS))
	
	for z in range(z0, z1 + 1):
		for y in range(y0, y1 + 1):
			for x in range(x0, x1 + 1):
				if room.get_block(Vector3i(x, y, z)) != 0:
					return false # XXX spiky/pump

	return true


## Check player position, velocity to see if player is entering a portal (one of
## EXIT_*), and from which side. The direction is important, and the player
## has to be correctly in front of the portal. It is possible to diagonally enter a portal.
func exit_check(room, pos: Vector3, velocity: Vector3):
	var x0 := int(floor(pos.x + velocity.x))
	var x1 := int(floor(pos.x + velocity.x + TOFS))
	var y0 := int(floor(pos.y + velocity.y))
	var y1 := int(floor(pos.y + velocity.y + TOFS))
	var x
	var y
	var z := int(floor(pos.z))
	if velocity.x < 0:
		x = x0
	if velocity.x > 0:
		x = x1
	if velocity.y < 0:
		y = y0
	if velocity.y > 0:
		y = y1
		
	if x != null:
		var block_types = [Blocks.EXIT_X0, Blocks.EXIT_X1]
		if room.get_block(Vector3i(x, y0, z)) in block_types and room.get_block(Vector3i(x, y1, z)) in block_types:
			if velocity.x < 0:
				return Compass.WEST
			else:
				return Compass.EAST
	if y != null:
		var block_types = [Blocks.EXIT_Y0, Blocks.EXIT_Y1]
		if room.get_block(Vector3i(x0, y, z)) in block_types and room.get_block(Vector3i(x1, y, z)) in block_types:
			if velocity.y < 0:
				return Compass.NORTH
			else:
				return Compass.SOUTH
	return -1		

## Called every physics frame.
func _physics_process(delta):
	var room = rooms[room_id]
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
		
	if direction.length_squared() != 0:
		$Level/Player/Player.play("walk")
	else:
		$Level/Player/Player.stop()
	if player_on_floor:
		player_velocity.x = speed * direction.x
		player_velocity.y = speed * direction.y

	# Jumping (XXX break off after maximum number of tiles traveled)
	if player_on_floor and Input.is_action_pressed('ui_accept'):
		player_velocity.z = -0.2 * 60
		player_on_floor = false
		
	# Gravity
	player_velocity += Vector3(0.0, 0.0, 60.0) * delta
	
	# Ideally, instead of doing this per coord, the collison would return a vector
	# maximum movement in each direction for slide.
	var new_coord = player_coord + Vector3(player_velocity.x, player_velocity.y, 0.0) * delta
	if collision_check(room, new_coord):
		player_coord = new_coord
	
	new_coord = player_coord + Vector3(0.0, 0.0, player_velocity.z) * delta
	if collision_check(room, new_coord):
		player_coord = new_coord
		player_on_floor = false
	else:
		# XXX fall to death
		player_on_floor = true
		player_velocity.z = 0.0

	var exit_id = exit_check(room, player_coord, player_velocity*delta)
	if exit_id != -1:
		print('Detected exit in direction %d' % [exit_id])
		var new_room_id = room.exits[exit_id]
		if new_room_id >= 0 and new_room_id < rooms.size():
			enter_room(new_room_id, exit_id)

	place_player()


func _input(event):
	var new_room_id = null
	#if event.is_action_pressed("ui_up"):
	#	new_room_id = rooms[room_id].exits[Compass.WEST]
	#if event.is_action_pressed("ui_right"):
	#	new_room_id = rooms[room_id].exits[Compass.NORTH]
	#if event.is_action_pressed("ui_down"):
	#	new_room_id = rooms[room_id].exits[Compass.EAST]
	#if event.is_action_pressed("ui_left"):
	#	new_room_id = rooms[room_id].exits[Compass.SOUTH]
	if event.is_action_pressed("ui_page_up"):
		new_room_id = room_id - 1
	if event.is_action_pressed("ui_page_down"):
		new_room_id = room_id + 1
	if new_room_id != null:
		enter_room(new_room_id, -1)
		
	if event.is_action_pressed("ui_cancel"):
		get_tree().quit()
