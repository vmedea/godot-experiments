extends Node

## Compass (cardinal) directions.
enum Compass {
	NORTH = 0, ## -Y (right key)
	EAST = 1,  ## +X (down key)
	SOUTH = 2, ## +Y (left key)
	WEST = 3,  ## -X (up key)
	UP = 4,    ## -Z
	DOWN = 5,  ## Z
}

## Compass (cardinal) directions, bitflags type.
enum CompassFlags {
	NORTH = 1 << 0,
	EAST = 1 << 1,
	SOUTH = 1 << 2,
	WEST = 1 << 3,
	UP = 1 << 4,
	DOWN = 1 << 5,
}

## Block attributes. Lower 6 bits match CompassFlags.
enum BlockAttributes {
	PAIN_NORTH = 1 << 0,
	PAIN_EAST = 1 << 1,
	PAIN_SOUTH = 1 << 2,
	PAIN_WEST = 1 << 3,
	PAIN_UP = 1 << 4,
	PAIN_DOWN = 1 << 5,
	PORTABLE = 1 << 6,
	DROPPABLE = 1 << 7,
}

## Offset, per compass direction.
const DIR_OFFSET = [
	Vector3i(0, -1, 0),
	Vector3i(1, 0, 0),
	Vector3i(0, 1, 0),
	Vector3i(-1, 0, 0),
	Vector3i(0, 0, -1),
	Vector3i(0, 0, 1),
]

## Special blocks.
enum Blocks {
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
