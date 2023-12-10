extends RefCounted
class_name GameRoom

const FILE_MAGIC_NUMBER := 0xCC170206
const ROOM_MAX_X := 8
const ROOM_MAX_Y := 8
const ROOM_MAX_Z := 11

var dims
var exits
var room

# Imported constants
const Compass = Types.Compass
const Blocks = Types.Blocks
const DIR_OFFSET = Types.DIR_OFFSET


static func load_rooms(filename):
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
		# North, East, South, West
		room.exits = [f.get_16(), f.get_16(), f.get_16(), f.get_16()]
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
				
		rooms_data.append(new(room))
	return rooms_data


func _init(data):
	room = data
	dims = data.dims
	exits = data.exits

## Get block by x,y,z integer coordinate.
func get_block(grid_pos: Vector3i) -> int:
	if grid_pos.x < 0 or grid_pos.y < 0 or grid_pos.z < 0 or grid_pos.x >= room.dims.x or grid_pos.y >= room.dims.y or grid_pos.z >= room.dims.z:
		return -1
	return room.level[grid_pos.z][grid_pos.y][grid_pos.x]


func find_entrance_from(direction: Compass):
	# Scan along wall:
	# NORTH -Y   XZ y==ymax   Blocks.EXIT_Y0 Blocks.EXIT_Y1
	# EAST  +X   YZ x==0      Blocks.EXIT_X0 Blocks.EXIT_X1
	# SOUTH +Y   XZ y==0      Blocks.EXIT_Y0 Blocks.EXIT_Y1
	# WEST  -X   YZ x==xmax   Blocks.EXIT_X0 Blocks.EXIT_X1
	if direction == Compass.NORTH or direction == Compass.SOUTH:
		# Scan north or south wall for Blocks.EXIT_Y0 Blocks.EXIT_Y1
		var y = room.dims.y - 1 if direction == Compass.NORTH else 0
		for x in room.dims.x - 1:
			for z in room.dims.z:
				if get_block(Vector3i(x, y, z)) == Blocks.EXIT_Y0 and get_block(Vector3i(x + 1, y, z)) == Blocks.EXIT_Y1:
					return Vector3(x + 0.5, y, z) + Vector3(DIR_OFFSET[direction])
	else:
		# Scan west or east wall for Blocks.EXIT_X0 Blocks.EXIT_X1
		var x = room.dims.x - 1 if direction == Compass.WEST else 0
		for y in room.dims.y - 1:
			for z in room.dims.z:
				if get_block(Vector3i(x, y, z)) == Blocks.EXIT_X1 and get_block(Vector3i(x, y + 1, z)) == Blocks.EXIT_X0:
					return Vector3(x, y + 0.5, z) + Vector3(DIR_OFFSET[direction])
