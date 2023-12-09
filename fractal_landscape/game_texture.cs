using Godot;
using System;

public partial class game_texture : TextureRect
{
	const int WIDTH = 256;
	const int HEIGHT = 128;
	
	Image img = Image.Create(WIDTH, HEIGHT, false, Image.Format.L8);
	byte [] buffer = new byte [WIDTH * HEIGHT];
	Game _game;
	double _frame;
	double _speedFloat;
		
	[Signal]
	public delegate void InfoEventHandler(string debug, int state, int distance, float heading, float headingDelta, int speed);
	
	public game_texture()
	{
		_game = new Game(false);
		_frame = 0.0;
		_game.fullRaytracer = false;
		_speedFloat = (double)_game.view.speed;
	}
	
	public override void _Ready()
	{
		img.Fill(Color.Color8(255, 0, 255, 255));
		//GD.Print("Hello from C# ", img.GetWidth(), " ", img.GetHeight(), " ", Color.Color8(255, 0, 255, 255));
		ImageTexture t = this.Texture as ImageTexture;
		t.SetImage(img);
	}
	
	void UpdateTexture() {
		bool blue = true;
		if (_game.imageRaytracer && !_game.blueRaytracer)
			blue = false;

		// XXX show both?
		// would need to factor this out to a GameModel class that provides two textures, or make one big buffer, of say, 256x256
		if (blue) {
			// Two interpeaved bitplanes, 256x128
			const int planes = 2;
			const int stride = WIDTH / 8 * planes;
			int ptr = 0;
			for (int y=0; y < HEIGHT; ++y) {
				for (int x=0; x < WIDTH; ++x) {
					int in_ofs = y * stride + (x >> 4) * 2 * planes + ((x >> 3) & 1);
					int in_bit = 7 - (x & 0x7);
					int idx = ((_game.viewGfx[in_ofs + 0] >> (in_bit)) & 1) | 
							  (((_game.viewGfx[in_ofs + 2] >> (in_bit)) & 1) << 1);
					buffer[ptr] = (byte)idx;
					ptr += 1;
				}
			}
		} else {
			// Four interleaved bitplanes, 320x200
			const int xofs = 32;
			const int yofs = 18;
			const int planes = 4;
			const int stride = 320 / 8 * planes;
			int ptr = 0;
			for (int y=yofs; y < (yofs + HEIGHT); ++y) {
				for (int x=xofs; x < (xofs + WIDTH); ++x) {
					int in_ofs = y * stride + (x >> 4) * 2 * planes + ((x >> 3) & 1);
					int in_bit = 7 - (x & 0x7);
					int idx = ((_game.screen[in_ofs + 0] >> (in_bit)) & 1) | 
							  (((_game.screen[in_ofs + 2] >> (in_bit)) & 1) << 1) |
							  (((_game.screen[in_ofs + 4] >> (in_bit)) & 1) << 2) |
							  (((_game.screen[in_ofs + 6] >> (in_bit)) & 1) << 3);
					buffer[ptr] = (byte) idx;
					ptr += 1;
				}
			}
		}

		img.SetData(WIDTH, HEIGHT, false, Image.Format.L8, buffer);
		ImageTexture t = this.Texture as ImageTexture;
		t.Update(img);
	}
	
	public void Iterate() {
		if (_game.Flying_finishLineReachedUnk [0] != -3) {
			_game.Iterate();
			UpdateTexture();
		}
		
		// Information for HUD
		string debug = _game.debugString;
		int state = 0;
		int distance = 0;
		int speed = 0;
		
		if (_game.Flying_finishLineReachedUnk[0] >= 0) {
			if (_game.Flying_finishLineReachedUnk[0] == 0) {
				state = 1;
				distance = 0x8000 - _game.Flying_distanceDone;
			} else {
				state = 2;
				distance = _game.Flying_finishLineReachedUnk [0];
			}
			speed = _game.view.speed;
		}
		EmitSignal(SignalName.Info, debug, state, distance, _game.view.heading / 65536.0f, (_game.headingDeltaFixed >> 8) / 256.0f, speed);
	}

	public override void _Process(double delta)
	{
		_frame -= delta;
		if (_frame < 0) {
			Iterate();
			_frame += 0.030;
		}
	}
	
	public override void _Input(InputEvent evt)
	{
		if (evt is InputEventKey kev) {
			if (kev.Pressed) {
				switch (kev.Keycode) {
					case Key.Space:
						_game.fullRaytracer = !_game.fullRaytracer;
						break;
					case Key.V:
						_game.view.cursorX = 110;
						_game.view.cursorY = 56;
						break;
					case Key.B:
						_game.blueRaytracer = !_game.blueRaytracer;
						break;
					case Key.Right:
						_game.view.cursorX += 1;
						break;
					case Key.Left:
						_game.view.cursorX -= 1;
						break;
					case Key.Up:
						_game.view.heightFixed -= 0x30000;
						break;
					case Key.Down:
						_game.view.heightFixed += 0x30000;
						break;
				}
			}
		} else if (evt is InputEventMouseMotion mev) {
			Vector2 diff = mev.Relative;
			if ((mev.ButtonMask & MouseButtonMask.Left) != 0) {
				// Heading control.
				_game.view.cursorX += (int)diff.X / 2;
				_game.view.cursorY += (int)diff.Y / 2;
			} else if ((mev.ButtonMask & MouseButtonMask.Right) != 0) {
				// Speed control.
				_speedFloat += diff.Y / 32.0;
				if (_speedFloat < 0) {
					_speedFloat = 0;
				}
				if (_speedFloat > 11) {
					_speedFloat = 11;
				}
				_game.view.speed = (int)_speedFloat;
			}
		}
	}
}
