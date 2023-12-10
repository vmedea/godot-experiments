# Godot Airball

An implementation of the Atari ST (among others) game Airball in the Godot game engine, version 4.

I've tried to use Godot 2D engine built-in functionality where possible.

- All game logic is implemented in GDscript.

- Uses an isometric TileMap and TileSet to draw the levels.

- Uses custom data layers to define tile attributes.

Uses game data and assets from original source:
http://oldschoolprg.x10.mx/projets.php

To write:

- y-sorting trick to approximate original drawing order (more or less), handling of player sprite

TODO:

- Fade in/out between levels
- Death handling
  - Death animation
  - Ball flying around
- Sound effects.
- Convert music from xm to .ogg.

    airball_1.ym e_YmMusic_Menu, (loops)
    airball_2.ym e_YmMusic_Game, (loops)
    airball_3.ym e_YmMusic_HighScore, (loops)
    airball_4.ym e_YmMusic_Death, (doesn't loop)

