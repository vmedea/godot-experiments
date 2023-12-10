/* Based on code from http://bringerp.free.fr/RE/CaptainBlood/main.php5 by Kroah, which is a port
   of the fractal landscape renderer used in the Atari ST game Captain Blood. 
 */
using System;
using Godot;

public class Game {
	public bool _fast;
	public bool _descending;

	public byte [] _data;
	public ushort [] viewGfx;
	public View view;
	public long RandomXXXX_seed;
	public int Flying_distanceDone;
	public Delegate_Curve_RandomXXXX [] Curve_functionsRandom;
	public Delegate_Curve_RandomXXXX [] p5FunctionsRandomlyChosen;
	public Delegate_Curve_RandomXXXX Curve_pRandomFunction;
	public int [] Flying_finishLineReachedUnk;
	public long [] View_segmentsCurveOffsetFixed;
	public byte [] View_curves;
	public Delegate_Flying_InitCurveSegment View_pInitSegmentCurve;
	public int View_lastOffsetInCurve;

	//public int [] View_bufferUnk01;
	public int [] View_heights;
	public int viewHeightsOffsetBase = 128;
	public int [] Raytracer_heights;

	public int RandomXXXX_seed02;
	//public int Raytracer_groundColorUnk;
	public int word_1C770;
	public int word_1C772;
	public int word_1C76E;

	// Raytracer
	public ushort [] screen = new ushort [16000];

	public int Raytracer_segmentsBandInfoIndex;
	public int Raytracer_distanceBetween2Bands;
	public int Raytracer_nbBandsBetween2Segments;
	public byte [] Raytracer_curve = new byte [256 * 51 + 1];
	public int Raytracer_curveBaseOffset = 384;

	public int word_1CE80;
	public long dword_1CE82;
	public long dword_1CE86;
	public long dword_1CE8A;
	public long dword_1CE8E;
	public long dword_1CE92;
	public long dword_1CE96;

	public delegate int Delegate_Curve_RandomXXXX(ref bool X, int d2, int d4, int d6);
	public delegate void Delegate_Flying_InitCurveSegment(int d3);
	
	public bool fullRaytracer = false;
	public bool blueRaytracer = false;
	public bool imageRaytracer;
	public int [] Raytracer_segmentsSeed02;

	public string debugString = "";
	public int headingDeltaFixed = 0;

	public Game(bool fast) {
		_fast = fast;
		Curve_functionsRandom = new Delegate_Curve_RandomXXXX[] {
			Curve_RandomXXXX_1C676,
			Curve_RandomXXXX_1C68A,
			Curve_RandomXXXX_1C6A2,
			Curve_RandomXXXX_1C6BA,
			Curve_RandomXXXX_1C6C2,
			Curve_RandomXXXX_1C6DE,
			Curve_RandomXXXX_1C6E8,
			Curve_RandomXXXX_1C664,
			Curve_RandomXXXX_1C676,
			Curve_RandomXXXX_1C68A,
			Curve_RandomXXXX_1C6BA,
			Curve_RandomXXXX_1C6BE,
			Curve_RandomXXXX_1C6C2,
			Curve_RandomXXXX_1C6D0,
			Curve_RandomXXXX_1C6DE,
			Curve_RandomXXXX_1C6E8
		};

		Curve_pRandomFunction = Curve_RandomXXXX_1C6C2;

		View_curves = new byte [1024 * (16 + 1)]; // +1 for canyon
		//View_bufferUnk01 = new int [128];
		View_heights = new int [128 + 128];
		Raytracer_heights = new int [256];

		//2 bpp
		//256*128 pixels = 64*128 bytes = 8192 bytes
		//1 line = 256 pixels = 64 bytes
		//1 byte = 4 pixels 
		viewGfx = new ushort [256 * 128];

		_descending = false;
		Ship_Land_Init();
		if (_fast)
			Ship_LandFast();
		else
			Ship_Land();
	}

	public void Ship_Land_Init() {
		//_data = new byte [4 * 4608];
		for (int i = 0; i < View_curves.Length; i += 4) {
			View_curves[i + 0] = 0x01;
			View_curves[i + 1] = 0x34;
			View_curves[i + 2] = 0x42;
			View_curves[i + 3] = 0x24;
		}

		RandomXXXX_seed = 0x4D83;
		RandomXXXX_seed += 0x70000;
		dword_1CE96 = 0x0000A000; // A000 not initialized

		Flying_distanceDone = 0;
		p5FunctionsRandomlyChosen = new Delegate_Curve_RandomXXXX [4];
		for (int i = 0; i < 5; i++) {
			int index = (RandomXXXX_InitLong1A8EA() & 0x3C) >> 2;
			if (i != 4)
				p5FunctionsRandomlyChosen[i] = Curve_functionsRandom[index];
			else
				Curve_pRandomFunction = Curve_functionsRandom[index];
		}
		Flying_finishLineReachedUnk = new int [] {
			0, 0, 0, 0, 0,
			12, 8, 4, 0, -4, -8, -12, -14, -12, -8, -4, 0, 4, 8, 12, 14
		};
		view = new View();
		view.heading = 0;

		Raytracer_segmentsSeed02 = new int [64];
		View_segmentsCurveOffsetFixed = new long [64];
		for (int i = 0; i < 64; i++)
			View_segmentsCurveOffsetFixed[i] = -1;
		int d1 = 512;
		for (int i = 0; i < 16; i++) {
			View_segmentsCurveOffsetFixed[i] = d1 << 16;
			d1 += 1024;
		}
		view.unkDefense = 0xFF8;
		//Raytracer_groundColorUnk = 0x18;
		Flying_finishLineReachedUnk[0] = 0;
	}

	public void Ship_LandFast() {
		RandomXXXX_seed02 = 0x91A9;

		//view.cursorX = 110;
		view.cursorX = 94;
		view.cursorY = 56;

		view.firstSegmentIndex_x4 = 0;
		view.lastSegmentIndex_x4 = 0;
		view.distanceToFirstSegment = 2;
		view.distanceToLastSegment = 2;
		view.speed = 1;
		view.heightFixed = 34 << 16;
		view.heightGoal = 254;
		View_pInitSegmentCurve = Flying_InitCurveSegment_LandFast;
		int d3 = 0x40B0;
		for (int i = 0; i < 10; i++) {
			View_pInitSegmentCurve(d3);
			d3 += 1024;
		}
		Flying_finishLineReachedUnk[0] = 0;
		view.unk2C = 0;
		Flying_Iterate();
	}

	public void Ship_Land() {
		_descending = true;
		RandomXXXX_seed02 = 0x4FA8;
		View_lastOffsetInCurve = 0x400;
		view.cursorX = 110;
		view.cursorY = 56;

		view.firstSegmentIndex_x4 = 0;
		view.unk2C = 0;
		view.distanceToFirstSegment = 2;
		view.heightGoal = 1008;
		//view.speed = 22;
		view.speed = 11;

		Flying_Descending_Iterate();
	}

	public void Iterate() {
		debugString = "";
		imageRaytracer = false;
		if (_descending) {
			Flying_Descending_Iterate();
		} else {
			if (Flying_finishLineReachedUnk[0] >= 0) {
				Flying_Iterate();
			} else if (Flying_finishLineReachedUnk[0] == -1) {
				Flying_finishLineReachedUnk[0] = -2;
				Flying_Stopping_Start();
			} else if (Flying_finishLineReachedUnk[0] == -2) {
				Flying_Stopping_Iterate();
			}
			//else if (Flying_finishLineReachedUnk[0] == -3) {
			//  Flying_finishLineReachedUnk[0] = -4;
			//  Raytracer_Start();
			//}
		}
	}

	public void Flying_Descending_Iterate() {
		View_InitHeights();

		view.heightFixed = view.heightGoal << 16;

		int d0 = view.distanceToFirstSegment - view.speed;

		if (d0 <= 1) {
			// NewSegment
			//int d1 = (int) (View_segmentsOffsetFixed[view.firstSegmentIndex_x4] >> 16);
			//int d2 = view.lastSegmentIndex_x4;
			//d2++;
			long curveOffset = View_segmentsCurveOffsetFixed[view.lastSegmentIndex_x4];
			curveOffset = ((((curveOffset >> 16) + 1024) % 16384) << 16) | (curveOffset & 0x0000FFFF);
			View_segmentsCurveOffsetFixed[(view.lastSegmentIndex_x4 + 1) % 64] = curveOffset;
			Raytracer_segmentsSeed02[(view.lastSegmentIndex_x4 + 1) % 64] = RandomXXXX_seed02;

			Flying_EndOfCanyonReached_SetCurveSegmentNormal((int) (curveOffset >> 16));
			view.firstSegmentIndex_x4 = (view.firstSegmentIndex_x4 + 1) % 64;
			d0 += 50;
		}
		// AfterNewSegment
		view.lastSegmentIndex_x4 = view.firstSegmentIndex_x4;
		view.distanceToFirstSegment = d0;
		view.distanceToLastSegment = d0;

		if (!fullRaytracer) {
			// 10 segments
			do {
				View_DrawNextSegment();
				view.distanceToLastSegment += 50;
			} while (view.distanceToLastSegment <= 500);
		} else
			Raytracer_Draw();

		Flying_MoveLeftOrRight();

		view.heightGoal -= 12;
		if (view.heightGoal <= 256) {
			_descending = false;
			Flying_distanceDone = 0;
		}
	}

	public void Flying_Stopping_Start() {
		int curveOffset = (int) (View_segmentsCurveOffsetFixed[view.lastSegmentIndex_x4] >> 16);
		int d1 = View_curves[curveOffset];
		if ((view.heightFixed >> 16) >= View_curves[curveOffset])
			d1 = view.heightFixed >> 16;
		curveOffset++;
		d1 += 4;
		if (d1 <= 63)
			d1 = 64;
		view.heightFixed = (d1 << 16) | (view.heightFixed & 0xFFFF);

		Flying_Stopping_Iterate();
	}

	public void Flying_Stopping_Iterate() {
		// 1CBB8
		if (view.distanceToFirstSegment == 2) {
			Flying_finishLineReachedUnk[0] = -3;
			Raytracer_InitDraw();
			return;
		}

		View_InitHeights();
		view.distanceToFirstSegment--;
		view.lastSegmentIndex_x4 = view.firstSegmentIndex_x4;
		view.distanceToLastSegment = view.distanceToFirstSegment;

		if (!fullRaytracer) {
			// 10 segments
			do {
				View_DrawNextSegment();
				view.distanceToLastSegment += 50;
			} while (view.distanceToLastSegment <= 500);
		} else
			Raytracer_Draw();
	}

	public void Flying_Iterate() {
		View_InitHeights();

		int d0 = view.distanceToFirstSegment - view.speed;
		if (d0 <= 1) {
			// Segment may disappear at bottom
			if (false) {
				// Check collision
				int d1 = (int) (View_segmentsCurveOffsetFixed[view.firstSegmentIndex_x4] >> 16);
				int d0Temp = View_curves[d1] + View_curves[d1 + 1];
				ROXR_b(1, ref d0Temp, false);
				d0Temp++;
			}

			// No collision => New curve
			// Vanilla
			//long curveOffset = View_segmentsCurveOffsetFixed[(view.lastSegmentIndex_x4 - 1 + 64) % 64];
			//curveOffset = ((((curveOffset >> 16) + 1024) % 16384) << 16) | (curveOffset & 0x0000FFFF);
			//View_segmentsCurveOffsetFixed[view.lastSegmentIndex_x4] = curveOffset;

			// Fixed for raytracer
			long curveOffset = View_segmentsCurveOffsetFixed[view.lastSegmentIndex_x4];
			curveOffset = ((((curveOffset >> 16) + 1024) % 16384) << 16) | (curveOffset & 0x0000FFFF);
			View_segmentsCurveOffsetFixed[(view.lastSegmentIndex_x4 + 1) % 64] = curveOffset;
			Raytracer_segmentsSeed02[(view.lastSegmentIndex_x4 + 1) % 64] = RandomXXXX_seed02;

			View_pInitSegmentCurve((int) (curveOffset >> 16));
			view.firstSegmentIndex_x4 = (view.firstSegmentIndex_x4 + 1) % 64;
			d0 += 50;
		}
		// AfterNewCurve
		view.distanceToFirstSegment = d0;
		view.lastSegmentIndex_x4 = view.firstSegmentIndex_x4;
		view.distanceToLastSegment = view.distanceToFirstSegment;
		if (!fullRaytracer) {
			// 10 segments
			do {
				View_DrawNextSegment();
				view.distanceToLastSegment += 50;
			} while (view.distanceToLastSegment <= 500);
		} else {
			Raytracer_Draw();
		}

		Flying_MoveLeftOrRight();
	}

	public void View_DrawNextSegment() {
		int curveOffset = (int) (View_segmentsCurveOffsetFixed[view.lastSegmentIndex_x4] >> 16);
		int curveOffsetFixedPart = (int) (View_segmentsCurveOffsetFixed[view.lastSegmentIndex_x4] & 0xFFFF);
		view.lastSegmentIndex_x4 = (view.lastSegmentIndex_x4 + 1) % 64;
		View_DrawSegment(View_curves, curveOffset, curveOffsetFixedPart);
	}

	public void View_DrawSegment(byte [] curve, int curveOffset, int curveOffsetFixedPart) {
		int viewHeightsOffset = 0;
		int pDstOffset = 28;
		int bitToSet = 1;
		int depthFactorFixed = 0xFF00 / view.distanceToLastSegment;

		if (depthFactorFixed <= 0x00FF) {
			View_FarLines(viewHeightsOffset, depthFactorFixed, pDstOffset, curve, curveOffset, bitToSet);
			return;
		}

		// Horizontal displacement from horizontal position (move left/right)
		int xPixelDelta = ((curveOffsetFixedPart * depthFactorFixed) >> 16) >> 8;
		viewHeightsOffset -= xPixelDelta;
		pDstOffset -= (xPixelDelta >> 2) & 0xFFFC;
		bitToSet <<= xPixelDelta & 0xF;

		// Set at middle of the view
		int nbPixelsWidthToDraw = xPixelDelta + 127;

		int yDeltaFixed = view.heightFixed - (curve[(curveOffset + 0x4000) & 0x3FFF] << 16);
		yDeltaFixed >>= 11;
		yDeltaFixed *= depthFactorFixed;
		int yFixed = 0x400000 + yDeltaFixed;
		// Right
		View_DrawSegment_Helper(viewHeightsOffset, yFixed, depthFactorFixed, pDstOffset, curve, curveOffset, bitToSet, nbPixelsWidthToDraw, 28, true);
		// Left
		nbPixelsWidthToDraw = 254 - nbPixelsWidthToDraw;
		View_DrawSegment_Helper(viewHeightsOffset, yFixed, depthFactorFixed, pDstOffset, curve, curveOffset, bitToSet, nbPixelsWidthToDraw, 28, false);
	}

	// Draw zoomed curve, larger than view width
	public void View_DrawSegment_Helper(int viewHeightsOffset, int yPixelFixed, int widthFixed, int pDstOffset, byte [] curve, int curveOffset, int bitToSet, int nbPixelsWidthToDraw, int waterLevel, bool doRight) {
		int remainingWidthFixed = -0x0100;

		for (; ; ) { // Loop curve
			remainingWidthFixed += widthFixed;
			int yPixelDelta = curve[(curveOffset + 0x4000) & 0x3FFF];
			if (doRight)
				curveOffset++;
			else
				curveOffset--;
			yPixelDelta -= curve[(curveOffset + 0x4000) & 0x3FFF];

			if (yPixelDelta != 0) { // Not horizontal
				int d1 = yPixelDelta << (16 - 3);
				int yPixelFixedTemp = yPixelFixed;
				for (; ; ) { // Loop write pixels
					int yPixel = yPixelFixedTemp >> 16;
					if (yPixel <= View_heights[viewHeightsOffsetBase + viewHeightsOffset]) {
						View_heights[viewHeightsOffsetBase + viewHeightsOffset] = yPixel;
						if (yPixel >= 0) {
							viewGfx[(pDstOffset + yPixel * 64) >> 1] |= (ushort)bitToSet;
						}
					}
					if (doRight) {
						viewHeightsOffset++;
						ROR_w (1, ref bitToSet);
						if ((bitToSet & 0x8000) != 0)
							pDstOffset += 4;
					} else {
						viewHeightsOffset--;
						ROL_w (1, ref bitToSet);
						if ((bitToSet & 0x0001) != 0)
							pDstOffset -= 4;
					}
					yPixelFixedTemp += d1;
					remainingWidthFixed -= 0x0100;
					if ((nbPixelsWidthToDraw == 0) || (remainingWidthFixed < 0))
						break;
					nbPixelsWidthToDraw--;
				}
				if ((nbPixelsWidthToDraw == 0) || (remainingWidthFixed >= 0))
					break;
				nbPixelsWidthToDraw--;
				yPixelFixed += (d1 >> 8) * widthFixed;
			} else { // Horizontal line
				int yPixel = yPixelFixed >> 16;
				int pixelOffset = yPixel * 64;
				if (curve [(curveOffset + 0x4000) & 0x3FFF] <= waterLevel) // 28
					pixelOffset += 2; // color light blue for "water"
				for (; ; ) {
					if (yPixel < View_heights[viewHeightsOffsetBase + viewHeightsOffset]) {
						View_heights[viewHeightsOffsetBase + viewHeightsOffset] = yPixel;
						if (yPixel >= 0) {
							viewGfx[(pDstOffset + pixelOffset) >> 1] |= (ushort) bitToSet;
						}
					}
					if (doRight) {
						viewHeightsOffset++;
						ROR_w (1, ref bitToSet);
						if ((bitToSet & 0x8000) != 0)
							pDstOffset += 4;
					} else {
						viewHeightsOffset--;
						ROL_w (1, ref bitToSet);
						if ((bitToSet & 0x0001) != 0)
							pDstOffset -= 4;
					}
					remainingWidthFixed -= 0x0100;
					if ((nbPixelsWidthToDraw == 0) || (remainingWidthFixed < 0))
						break;
					nbPixelsWidthToDraw--;
				}
				if ((nbPixelsWidthToDraw == 0) || (remainingWidthFixed >= 0))
					break;
				nbPixelsWidthToDraw--;
			}
		}
	}

	// Draw shrinked curve, smaller than view width
	public void View_FarLines(int viewHeightsOffset, int widthFixed, int pDstOffset, byte [] curve, int curveOffset, int bitToSet) {
		View_FarLines_Helper(viewHeightsOffset, widthFixed, pDstOffset, curve, curveOffset, bitToSet, 118, true);
		View_FarLines_Helper(viewHeightsOffset, widthFixed, pDstOffset, curve, curveOffset, bitToSet, 119, false);
	}

	public void View_FarLines_Helper(int viewHeightsOffset, int widthFixed, int pDstOffset, byte [] curve, int curveOffset, int bitToSet, int nbPixelsWidthToDraw, bool doRight) {
		int d0 = view.heightFixed >> (8 + 3);
		int remainingWidthFixed = widthFixed;
		bool first = true;
		for (; ; ) {
			if (!doRight || (doRight & !first)) {
				int yPixel = curve[(curveOffset + 0x4000) & 0x3FFF];
				yPixel = ((d0 - (yPixel << 5)) * widthFixed + 0x400000) >> 16;
				if (yPixel < View_heights[viewHeightsOffsetBase + viewHeightsOffset]) {
					View_heights[viewHeightsOffsetBase + viewHeightsOffset] = yPixel;
					if (yPixel >= 0) {
						int pixelOffset = yPixel * 64;
						if (curve[(curveOffset + 0x4000) & 0x3FFF] <= 28)
							pixelOffset += 2; // color light blue for "water"
						viewGfx[(pDstOffset + pixelOffset) >> 1] |= (ushort)bitToSet;
					}
				}
			}
			first = false;

			if (doRight) {
				viewHeightsOffset++;
				ROR_w (1, ref bitToSet);
				if ((bitToSet & 0x8000) != 0)
					pDstOffset += 4;
			} else {
				viewHeightsOffset--;
				ROL_w (1, ref bitToSet);
				if ((bitToSet & 0x0001) != 0)
					pDstOffset -= 4;
			}
			remainingWidthFixed -= 0x0100;

			for (; ; ) {
				if (doRight)
					curveOffset++;
				else
					curveOffset--;
				remainingWidthFixed += widthFixed;

				if ((nbPixelsWidthToDraw == 0) || (remainingWidthFixed > 255))
					break;
				nbPixelsWidthToDraw--;
			}

			if ((nbPixelsWidthToDraw == 0) || (remainingWidthFixed <= 255))
				break;
			nbPixelsWidthToDraw--;
		}
	}

	public void Raytracer_InitDraw() {
		//View_offsetToStartOfLine

		View_InitHeights();
		view.distanceToFirstSegment = 2;
		view.lastSegmentIndex_x4 = view.firstSegmentIndex_x4;
		view.distanceToLastSegment = view.distanceToFirstSegment;

		Raytracer_Draw();
		//Flying_finishLineReachedUnk[0] = -5;
	}

	public void Raytracer_Draw() {
		imageRaytracer = true;
		Raytracer_segmentsBandInfoIndex = 0;
		Raytracer_InitHeight();
		for (int i = 0; i < screen.Length; i++)
			screen[i] = 0;

		//int curveOffset = (int) (View_segmentsOffsetFixed[view.lastSegmentIndex_x4] >> 16);
		//int curveOffsetFixedPart = (int) (View_segmentsOffsetFixed [view.lastSegmentIndex_x4] & 0xFFFF);
		//view.lastSegmentIndex_x4 = (view.lastSegmentIndex_x4 + 1) % 64;
		//Raytracer_DoLineUnk01(View_heights, View_curvesY, curveOffset, curveOffsetFixedPart);

		int seedBackup = RandomXXXX_seed02;
		for (; ; ) { // Loop blue lines
			int curveOffsetFixedPart = (int) (View_segmentsCurveOffsetFixed[view.lastSegmentIndex_x4] & 0xFFFF);
			//if (view.lastSegmentIndex_x4 == view.firstSegmentIndex_x4)
			//  debugString += string.Format("{0:X4} ", curveOffsetFixedPart);
			Raytracer_NextBetween2Segments();

			// Classic
			int curveOffset = 0;

			//int curveOffset = (int) (View_segmentsOffsetFixed[view.lastSegmentIndex_x4] >> 16);

			for (; ; ) { // Loop between 2 segments
				// Classic
				//View_DrawSegment(Raytracer_curve, Raytracer_curveBaseOffset + curveOffset, 0);
				View_DrawSegment(Raytracer_curve, Raytracer_curveBaseOffset + curveOffset, curveOffsetFixedPart);

				if (!blueRaytracer)
					Raytracer_DoHoritontalBand();
				curveOffset += 256;
				view.distanceToLastSegment += Raytracer_distanceBetween2Bands;
				// Vanilla: 450, changed because switching from blue to raytracer caused bug
				if (view.distanceToLastSegment >= 500) {
					//Flying_finishLineReachedUnk[0] = -6;
					return;
				}
				Raytracer_nbBandsBetween2Segments--;
				if (Raytracer_nbBandsBetween2Segments == 0)
					break;
			}
		}
	}

	public int [] Raytracer_segmentsBandInfo = new int [] { 1, 50, 1, 50, 1, 50, 1, 50, 1, 50, 2, 25, 2, 25, 2, 25, 2, 25, 2, 25 };
	public void Raytracer_NextBetween2Segments() {
		int seedBackup = RandomXXXX_seed02;
		//RandomXXXX_seed02 = Raytracer_segmentsSeed02[view.lastSegmentIndex_x4];

		int curveOffset1 = (int) (-128 + (View_segmentsCurveOffsetFixed[view.lastSegmentIndex_x4] >> 16));
		int oldLastSegmentIndex_x4 = view.lastSegmentIndex_x4;
		view.lastSegmentIndex_x4 = (view.lastSegmentIndex_x4 + 1) % 64;
		int curveOffset2 = (int) (-128 + (View_segmentsCurveOffsetFixed[view.lastSegmentIndex_x4] >> 16));
		//if ((curveOffset1 < 0) || (curveOffset2 < 0))
		//  return;
		// distanceBetween2Bands * Raytracer_nbBandsBetween2Segments = always 50 = distance between 2 segments
		Raytracer_distanceBetween2Bands = Raytracer_segmentsBandInfo[Raytracer_segmentsBandInfoIndex * 2 + 0];
		Raytracer_nbBandsBetween2Segments = Raytracer_segmentsBandInfo[Raytracer_segmentsBandInfoIndex * 2 + 1];
		Raytracer_segmentsBandInfoIndex++;

		int d5 = Raytracer_nbBandsBetween2Segments * 256;
		int Raytracer_curveOffset = -384 + d5;

		//int [] seeds = new int [256];
		//RandomXXXX_seed02 = ((i) * 0x3171 - 1) & 0xFFFF;

		//if (oldLastSegmentIndex_x4 == view.firstSegmentIndex_x4)
		//  debugString = "";
		// curve from -128 to 127 (left to right)
		for (int i = 0; i < 256; i++) {
			//RandomXXXX_seed02 = ((Raytracer_segmentsSeed02[view.lastSegmentIndex_x4] * ((curveOffset2 + 0x4000) & 0x3FFF)) * 0x3171 - 1) & 0xFFFF;
			RandomXXXX_seed02 = ((Raytracer_segmentsSeed02[oldLastSegmentIndex_x4] * ((curveOffset1 + 0x4000) & 0x3FFF)) * 0x3171 - 1) & 0xFFFF;
			//if (oldLastSegmentIndex_x4 == view.firstSegmentIndex_x4)
			//  if (i < 10)
			//    debugString += string.Format ("{0:X4} ", RandomXXXX_seed02);
			//Console.Write (string.Format ("{0:X4} ", RandomXXXX_seed02));
			// 1A11 (1B11)
			long curveRaytracer = View_curves[(curveOffset2 + 0x4000) & 0x3FFF]; // start at curve2
			int d1 = View_curves[(curveOffset2 + 0x4000) & 0x3FFF] - View_curves[(curveOffset1 + 0x4000) & 0x3FFF];
			curveOffset1++;
			curveOffset2++;

			d1 = ((d1 << 8) / (Raytracer_nbBandsBetween2Segments - 1)) << 8;
			if (curveRaytracer <= 27)
				curveRaytracer = 28;

			// for each band between the 2 segments
			for (int j = 0; j < Raytracer_nbBandsBetween2Segments; j++) {
				Raytracer_curve[Raytracer_curveBaseOffset + Raytracer_curveOffset] = (byte) curveRaytracer;
				if ((curveRaytracer & 0xFFFF) != 28) {
					SWAP (ref curveRaytracer);
					curveRaytracer -= d1;
					SWAP (ref curveRaytracer);

					//RandomXXXX_seed02 = seeds[i];

					bool X = false;
					int d0 = Curve_RandomXXXX_1C664(ref X, -1, -1, -1);

					//seeds [i] = RandomXXXX_seed02;

					d0 = ((d0 & 0xFF) >> 5) - 2;
					//d0 = -2;
					curveRaytracer = (curveRaytracer & 0xFFFFFF00) + ((curveRaytracer - d0) & 0x000000FF);
					if ((curveRaytracer & 0xFFFF) < 28)
						curveRaytracer = 28;
					else if ((curveRaytracer & 0xFFFF) >= 255) {
						curveRaytracer = 255;
					}
				} else {
					bool X = false;
					Curve_RandomXXXX_1C664(ref X, -1, -1, -1);
				}

				Raytracer_curveOffset -= 256;
			}
			Raytracer_curveOffset += d5 + 1;
		}
		//if (oldLastSegmentIndex_x4 == view.firstSegmentIndex_x4)
		//  Console.WriteLine();

		RandomXXXX_seed02 = seedBackup;
	}

	public void Raytracer_DoHoritontalBand() {
		int a5 = sub_1CEF8(24); // ou 32 si defenses
		int viewHeightsOffsets = 127;
		int bufferUnk02Offset = 256;
		int screenOffset = 0xBC8;

		word_1CE80 = 5;
		dword_1CE82 = 0x007F0000;
		dword_1CE86 = 0x007F0000;
		dword_1CE8A = 0x007F0000;
		dword_1CE8E = 0x007F0000;
		dword_1CE92 = 0x007F007F;
		// Vanilla
		//dword_1CE96 &= 0x0000FFFF; // right not initialized
		dword_1CE96 = 0x00000000;

		int d5 = sub_1CE9E(View_heights[viewHeightsOffsetBase + viewHeightsOffsets], a5);
		viewHeightsOffsets++;

		int d6 = 2;
		// 1CDB6
		for (int i = 0; i < 255; i++) {
			viewHeightsOffsets--;
			int d0 = View_heights[viewHeightsOffsetBase + viewHeightsOffsets];
			d5 = sub_1CE9E(d0, a5);

			bufferUnk02Offset--;
			if (d0 < Raytracer_heights[bufferUnk02Offset]) {
				// If1
				if (d0 > 128)
					throw new Exception();
				if (d0 < 0)
					d0 = 0;
				int offsetToStartOfLine = d0 * 160;
				int pDst = screenOffset + offsetToStartOfLine;
				int height = Raytracer_heights[bufferUnk02Offset] - View_heights[viewHeightsOffsetBase + viewHeightsOffsets];
				//height--;
				if ((height >= 1) && (height <= 127)) {
					// If2
					if ((d5 >= 0) && (word_1CE80 >= 0)) {
						//d5 += ((word_1CE80 - d5) >> 1) + (((word_1CE80 - d5) < 0) ? 0x8000 : 0);
						d5 += ((word_1CE80 - d5) >> 1);
					}
					word_1CE80 = d5;
					int d1 = d5;

					for (int j = 0; j < height; j++) {
						d6 ^= 0xFFFF;
						screen[(pDst + 0) >> 1] = (ushort)(screen[(pDst + 0) >> 1] & d6);
						screen[(pDst + 2) >> 1] = (ushort)(screen[(pDst + 2) >> 1] & d6);
						screen[(pDst + 4) >> 1] = (ushort)(screen[(pDst + 4) >> 1] & d6);
						screen[(pDst + 6) >> 1] = (ushort)(screen[(pDst + 6) >> 1] & d6);
						d6 ^= 0xFFFF;

						if (d5 != 0) {
							// If3
							screen[(pDst + 6) >> 1] = (ushort)(screen[(pDst + 6) >> 1] | d6);
							if ((d5 & 0x01) != 0)
								screen[(pDst + 0) >> 1] = (ushort)(screen[(pDst + 0) >> 1] | d6);
							if ((d5 & 0x02) != 0)
								screen[(pDst + 2) >> 1] = (ushort)(screen[(pDst + 2) >> 1] | d6);
							if ((d5 & 0x04) != 0)
								screen[(pDst + 4) >> 1] = (ushort)(screen[(pDst + 4) >> 1] | d6);
						} // Endif3

						d1--;
						if (d1 < 0) {
							if (d5 > 0) {
								d5--;
								d1 = d5;
							}
						}
						pDst += 160;
					}
				} // Endif2
				Raytracer_heights[bufferUnk02Offset] = View_heights [viewHeightsOffsetBase + viewHeightsOffsets];
			} // Endif1

			ROL_w (1, ref d6);
			if ((d6 & 0x0001) != 0)
				screenOffset -= 8;

			d0 = 0;
			dword_1CE82 += 0xB000;
			dword_1CE86 += 0x9000;
			dword_1CE8A += 0x7000;
			dword_1CE8E += 0x6000;
			dword_1CE92 += 0x4000;
			dword_1CE96 += 0x2F000;

			View_heights[viewHeightsOffsetBase + viewHeightsOffsets] = 127;
		}
	}

	public int sub_1CEF8(int d0) {
		d0 <<= 16;
		int d6 = 0xFF00 / view.distanceToLastSegment;
		int d1 = ((view.heightFixed - d0) >> 11) * d6 + 0x400000;
		d1 >>= 16;
		return (d1);
	}

	public int sub_1CE9E(int d0, int a5) {
		int d5 = 6;
		if (d0 <= (dword_1CE82 >> 16))
			dword_1CE82 = (d0 << 16) + (dword_1CE82 & 0xFFFF);
		else
			d5--;
		if (d0 <= (dword_1CE86 >> 16))
			dword_1CE86 = (d0 << 16) + (dword_1CE86 & 0xFFFF);
		else
			d5--;
		if (d0 <= (dword_1CE8A >> 16))
			dword_1CE8A = (d0 << 16) + (dword_1CE8A & 0xFFFF);
		else
			d5--;
		if (d0 <= (dword_1CE8E >> 16))
			dword_1CE8E = (d0 << 16) + (dword_1CE8E & 0xFFFF);
		else
			d5--;
		if (d0 <= (dword_1CE92 >> 16))
			dword_1CE92 = (d0 << 16) + (dword_1CE92 & 0xFFFF);
		else
			d5--;
		if (d0 <= (dword_1CE96 >> 16))
			dword_1CE96 = (d0 << 16) + (dword_1CE96 & 0xFFFF);
		else
			d5--;
		if (d0 >= a5) {
			d5 = -256;
		}
		return (d5);
	}
	
	public void Flying_InitCurveSegment_LandFast(int d3) {
		view.heightGoal = 240;
		view.heading = 0;
		View_pInitSegmentCurve = Flying_InitCurveSegment_Canyon;
		Flying_finishLineReachedUnk[0] = 128;
		Flying_finishLineReachedUnk[3] = 256;
		Flying_finishLineReachedUnk[1] = 7;
		Flying_finishLineReachedUnk[4] = 6;
		Flying_finishLineReachedUnk[2] = 0;
		Flying_InitCurveSegment_Canyon(d3);
	}

	public void Flying_InitCurveSegment_Canyon(int d3) {
		d3 &= 0x3C00;
		int pCurveCurrentOffset = d3;
		Flying_finishLineReachedUnk[0]--;
		if (Flying_finishLineReachedUnk[0] < 0) {
			Flying_EndOfCanyonReached_SetCurveSegmentNormal(d3);
			return;
		}
		int d4 = Flying_finishLineReachedUnk[2];
		Flying_finishLineReachedUnk[1]--;
		if (Flying_finishLineReachedUnk[1] < 0) {
			// 1C89A
			int d1 = Flying_finishLineReachedUnk[4];
			if ((d1 % 2) != 0)
				throw new Exception ();
			d4 = Flying_finishLineReachedUnk[5 + (d1 >> 1)];
			Flying_finishLineReachedUnk[2] = d4;
			d4 += Flying_finishLineReachedUnk[3];
			d1 = (d1 + 2) % 32;
			Flying_finishLineReachedUnk[4] = d1;
			if ((d1 == 0) || (d1 == 16)) {
				bool X = false;
				Flying_finishLineReachedUnk[1] = Curve_RandomXXXX_1C664(ref X, -1, -1, -1) % 32;
			}
		} else {
			d4 += Flying_finishLineReachedUnk[3];
			if (d4 < 0) {
				d4 += 2;
				Flying_finishLineReachedUnk[4] = 16;
				Flying_finishLineReachedUnk[1] = 0;
			} else {
				if (d4 >= 640) {
					d4 -= 2;
					Flying_finishLineReachedUnk[4] = 0;
					Flying_finishLineReachedUnk[1] = 0;
				}
			}
		}

		// 1C8EE
		Flying_finishLineReachedUnk[3] = d4;
		pCurveCurrentOffset += d4;
		pCurveCurrentOffset += 128;
		for (int i = 0; i < 193; i++)
			View_curves[pCurveCurrentOffset + 32 + i] = 0xFF;
		Flying_InitCurve_Helper(3, 1, 16, pCurveCurrentOffset, Curve_pRandomFunction);
		pCurveCurrentOffset += 224;
		Flying_InitCurve_Helper(3, 1, 16, pCurveCurrentOffset, Curve_pRandomFunction);
		pCurveCurrentOffset -= 128;
		View_curves[pCurveCurrentOffset + 32] = 0;
		bool X2 = false;
		View_curves[pCurveCurrentOffset + 48] = (byte) (Curve_RandomXXXX_1C664(ref X2, -1, -1, -1) & 0xFF);
		View_curves[pCurveCurrentOffset + 16] = (byte) (Curve_RandomXXXX_1C664(ref X2, -1, -1, -1) & 0xFF);
		Flying_InitCurve_Helper(3, 4, 8, pCurveCurrentOffset, Curve_pRandomFunction);
		pCurveCurrentOffset -= 96;
		Flying_MinBoundCurve(pCurveCurrentOffset);
	}

	public void Flying_EndOfCanyonReached_SetCurveSegmentNormal(int d3) {
		View_pInitSegmentCurve = Flying_InitCurveSegment_Normal;
		Flying_InitCurveSegment_Normal(d3);
	}

	public void Flying_InitCurveSegment_Normal(int d3) {
		int distanceDelta = ((int) (view.heading)) >> 16;
		if (distanceDelta >= 0)
			distanceDelta = -distanceDelta;
		distanceDelta += 64;
		Flying_distanceDone += distanceDelta << 2;
		if (Flying_distanceDone >= 0x8000) {
			Flying_CanyonReached(distanceDelta, d3);
			return;
		}

		int d0 = Flying_distanceDone;
		ROL_w(5, ref d0);
		d0 &= 0x000C; // 1100
		Delegate_Curve_RandomXXXX a5 = p5FunctionsRandomlyChosen[d0 >> 2];
		bool X = false;
		d0 = Curve_RandomXXXX_1C664(ref X, -1, -1, -1) % 64;

		int pCurveLast = d0;
		int pCurveLastPrevious = pCurveLast;
		int previousLastOffsetInCurve = View_lastOffsetInCurve;
		View_lastOffsetInCurve = d3;
		pCurveLast += d3;
		pCurveLastPrevious += previousLastOffsetInCurve;
		pCurveLast -= 256;
		pCurveLastPrevious -= 256;
		pCurveLast = (pCurveLast + 16384) % 16384;

		Flying_InitCurve(pCurveLast, a5);
		Flying_MinBoundCurve(pCurveLast);
	}

	public void Flying_CanyonReached(int distanceDelta, int d3) {
		Flying_distanceDone -= 0x8000;
		word_1C770 = distanceDelta;
		Flying_InitCanyon(d3);
	}

	public void Flying_InitCanyon(int d3) {
	  View_pInitSegmentCurve = Flying_InitCurveSegment_CanyonTransition;
	  word_1C772 = 112;
	  d3 &= 0x3C00;
	  d3 += 0x200;
	  long temp = (d3 << 16) + (View_segmentsCurveOffsetFixed[view.lastSegmentIndex_x4] & 0xFFFF);
	  View_segmentsCurveOffsetFixed[view.lastSegmentIndex_x4] = temp;
	  word_1C76E = d3;
	  Flying_InitCurveSegment_CanyonTransition(d3);
	}

	public void Flying_InitCurveSegment_CanyonTransition(int d3) {
		if (word_1C772 <= 32) {
			Flying_CanyonOnly (d3);
			return;
		}
		d3 &= 0x3C00;
		int pCurveCurrentOffset = d3 + 256;
		int pCurveCurrentOffsetTemp = pCurveCurrentOffset;
		for (int i = 0; i < 449; i++) {
			View_curves[pCurveCurrentOffset + i + 32] = 240;
		}
		Delegate_Curve_RandomXXXX a5 = Curve_pRandomFunction;
		Flying_InitCurve_CanyonTransition_Helper(pCurveCurrentOffset, a5);
		pCurveCurrentOffset += 480;
		Flying_InitCurve_CanyonTransition_Helper(pCurveCurrentOffset, a5);
		pCurveCurrentOffset -= 224;
		pCurveCurrentOffset -= word_1C772;
		for (int i = 0; i <= word_1C772 * 2; i++) {
			View_curves[pCurveCurrentOffset + i + 1] = 28;
		}
		Flying_InitCurve_CanyonTransition_Helper(pCurveCurrentOffset, a5);
		pCurveCurrentOffset += word_1C772 * 2;
		word_1C772 -= 8;
		Flying_InitCurve_CanyonTransition_Helper(pCurveCurrentOffset, a5);

		pCurveCurrentOffset = pCurveCurrentOffsetTemp;
		Flying_MinBoundCurve(pCurveCurrentOffset);
	}

	public void Flying_InitCurve_CanyonTransition_Helper(int pCurveCurrentOffset, Delegate_Curve_RandomXXXX pFunctionRandom) {
		Flying_InitCurve_Helper(2, 1, 16, pCurveCurrentOffset, pFunctionRandom);
	}

	public void Flying_CanyonOnly(int d3) {
		if (word_1C770 >= 0x8000) {
			Flying_EndOfCanyonReached_SetCurveSegmentNormal(d3);
			return;
		}
		//int d0 = word_1C76E - word_1C770;
		int d0 = (int) (word_1C76E - (View_segmentsCurveOffsetFixed[view.firstSegmentIndex_x4] >> 16));

		if (d0 < 0)
			d0 = -d0;
		if (d0 > 72)
			Flying_InitCanyon(d3);
		else
			Flying_InitCurveSegment_LandFast(d3);
	}

	public void Flying_InitCurve(int pCurveCurrentOffset, Delegate_Curve_RandomXXXX pFunctionRandom) {
		bool X = false;
		View_curves[pCurveCurrentOffset + 128] = (byte) (Curve_RandomXXXX_1C664(ref X, -1, -1, -1) & 0xFF);
		View_curves[pCurveCurrentOffset + 192] = (byte) (Curve_RandomXXXX_1C664(ref X, -1, -1, -1) & 0xFF);
		View_curves[pCurveCurrentOffset + 256] = (byte) (Curve_RandomXXXX_1C664(ref X, -1, -1, -1) & 0xFF);
		View_curves[pCurveCurrentOffset + 320] = (byte) (Curve_RandomXXXX_1C664(ref X, -1, -1, -1) & 0xFF);
		View_curves[pCurveCurrentOffset + 384] = (byte) (Curve_RandomXXXX_1C664(ref X, -1, -1, -1) & 0xFF);
		Flying_InitCurve_Helper(2, 7, 32, pCurveCurrentOffset, pFunctionRandom);
	}

	public void Flying_InitCurve_Helper(int variation, int nbVertices, int bandWidth, int pCurveCurrentOffset, Delegate_Curve_RandomXXXX pFunctionRandom) {
		for (; ; ) {
			if (variation == 7) {
				Flying_InitCurve_MidPoints_NoVariation(nbVertices, bandWidth, pCurveCurrentOffset);
				return;
			}
			int pCurveCurrentOffsetTemp = pCurveCurrentOffset;
			for (int i = 0; i < nbVertices; i++) {
				int d2 = View_curves[pCurveCurrentOffsetTemp];
				pCurveCurrentOffsetTemp += bandWidth;
				int d1 = View_curves[pCurveCurrentOffsetTemp + bandWidth];
				//d2 = (d1 + d2) / 2;
				//bool X = false;
				d2 = d1 + d2;
				bool X = LSR_w(1, ref d2);
				int d0 = pFunctionRandom(ref X, d2, variation, bandWidth);
				ROXL_w(2, ref d0, X);
				d0 = (int) ((sbyte) d0);
				d0 += d2;
				if (d0 < 0)
					View_curves[pCurveCurrentOffsetTemp] = 0;
				else {
					if (d0 >= 255)
						d0 = d2;
					View_curves[pCurveCurrentOffsetTemp] = (byte) (d0 & 0xFF);
				}
				pCurveCurrentOffsetTemp += bandWidth;
			}
			variation++;
			nbVertices *= 2;
			bandWidth >>= 1;
			if(bandWidth == 0)
				break;
		}
	}

	public void Flying_InitCurve_MidPoints_NoVariation(int nbVertices, int bandWidth, int pCurveCurrentOffset) {
		for (; ; ) {
			for (int i = 0; i < nbVertices; i++) {
				int d2 = View_curves[pCurveCurrentOffset];
				pCurveCurrentOffset += bandWidth;
				int d1 = View_curves[pCurveCurrentOffset + bandWidth];
				d2 = (d1 + d2) >> 1;
				View_curves[pCurveCurrentOffset] = (byte) (d2 & 0xFF);
				pCurveCurrentOffset += bandWidth;
			}
			nbVertices *= 2;
			bandWidth >>= 1;
			if (bandWidth == 0)
				break;
		}
	}

	#region Misc
	public void Flying_MoveLeftOrRight() {
		//if (!fullRaytracer) {
		int d0 = (view.cursorX - 110) << 16;
		// Vanilla
		//if (view.speed == 0)
		//  return;
		if (view.speed <= 1)
			d0 >>= 2;
		d0 >>= 7;
		headingDeltaFixed = d0 * 2;
		view.heading += headingDeltaFixed;
		int d1 = view.firstSegmentIndex_x4;
		for (int i = 0; i < 13; i++) {
			View_segmentsCurveOffsetFixed[(d1 + i) % 64] += (d0 * (i + 1));
		}
		//}
		//else
		//  view.cursorX = 110;

		int deltaY = (view.cursorY - 56);
		view.heightFixed -= deltaY << 14;
		if ((view.heightFixed >> 16) < 64) {
			view.heightFixed = 64 << 16;
			//view.cursorY = 56;
		}
		if ((view.heightFixed >> 16) > 400) {
			view.heightFixed = 400 << 16;
			//view.cursorY = 56;
		}

		//int curveOffset = (int) (View_segmentsCurveOffsetFixed[view.firstSegmentIndex_x4] >> 16);
		//view.heightFixed = (View_curves[curveOffset]+50) << 16;
	}

	public void Flying_MinBoundCurve(int curveOffset) {
		byte d1 = 28;
		for (int i = 0; i < 512; i++) {
			if (View_curves[curveOffset] <= d1)
				View_curves[curveOffset] = d1;
			curveOffset++;
		}
	}

	public void View_InitHeights() {
		for (int i = 0; i < 256; i++) {
			//View_bufferUnk01[i] = 127;
			View_heights[i] = 127;
		}
		for (int i = 0; i < viewGfx.Length; i++)
			viewGfx[i] = 0;
	}

	public void Raytracer_InitHeight() {
		for (int i = 0; i < 256; i++) {
			Raytracer_heights[i] = 127;
		}
	}
	#endregion

	#region Curve_RandomXXXX
	public int RandomXXXX_InitLong1A8EA() {
		long d1 = 0xE62D;
		long d0 = RandomXXXX_seed & 0xFFFF;
		d0 = (d0 * d1) & 0xFFFFFFFF;
		d1 = (d1 * ((RandomXXXX_seed >> 16) & 0xFFFF)) & 0xFFFFFFFF;
		SWAP(ref d1);
		d0 = (d0 + d1) & 0xFFFFFFFF;
		d1 = (d1 & 0xFFFF0000) | 0xBB40;
		d1 = ((d1 & 0xFFFF) * (RandomXXXX_seed & 0xFFFF)) & 0xFFFFFFFF;
		SWAP(ref d1);
		d0 = (d0 + d1) & 0xFFFFFFFF;
		d0 = (d0 + 1) & 0xFFFFFFFF;
		RandomXXXX_seed = d0;
		//int result = ((int) d0) >> 2;
		ASR_l(2, ref d0);
		return ((int) d0);
	}

	public int Curve_RandomXXXX_1C664(ref bool X, int d2, int d4, int d6) {
		int d0 = (RandomXXXX_seed02 * 0x3171 - 1) & 0xFFFF;
		RandomXXXX_seed02 = d0;
		//X = false;
		return (d0);
	}

	public int Curve_RandomXXXX_1C676(ref bool X, int d2, int d4, int d6) {
		int d0 = (RandomXXXX_seed02 * 0x3171 - 1) & 0xFFFF;
		RandomXXXX_seed02 = d0;
		X = ASR_b(d4, ref d0);
		return (d0);
	}

	public int Curve_RandomXXXX_1C68A(ref bool X, int d2, int d4, int d6) {
		int d0 = (RandomXXXX_seed02 * 0x3171 - 1) & 0xFFFF;
		RandomXXXX_seed02 = d0;
		d0 |= 0xFF80;
		X = ASR_w(d4, ref d0);
		return (d0);
	}

	public int Curve_RandomXXXX_1C6A2(ref bool X, int d2, int d4, int d6) {
		int d0 = (RandomXXXX_seed02 * 0x3171 - 1) & 0xFFFF;
		RandomXXXX_seed02 = d0;
		d0 &= 0x7F;
		X = LSR_b(d4, ref d0);
		return (d0);
	}

	public int Curve_RandomXXXX_1C6BA(ref bool X, int d2, int d4, int d6) {
		return (-4);
	}

	public int Curve_RandomXXXX_1C6BE(ref bool X, int d2, int d4, int d6) {
		return (3);
	}

	public int Curve_RandomXXXX_1C6C2(ref bool X, int d2, int d4, int d6) {
		int d0 = (RandomXXXX_seed02 + 1) & 0xFFFF;
		RandomXXXX_seed02 = d0;
		X = ASR_b(d4, ref d0);
		return (d0);
	}

	public int Curve_RandomXXXX_1C6D0(ref bool X, int d2, int d4, int d6) {
		int d0 = (RandomXXXX_seed02 - 1) & 0xFFFF;
		RandomXXXX_seed02 = d0;
		X = ASR_b(d4, ref d0);
		return (d0);
	}

	public int Curve_RandomXXXX_1C6DE(ref bool X, int d2, int d4, int d6) {
		int d0 = d6 - d2 * 2;
		X = ASR_b(d4, ref d0);
		return (d0);
	}

	public int Curve_RandomXXXX_1C6E8(ref bool X, int d2, int d4, int d6) {
		return (0);
	}
	#endregion

	#region ASM68k
	public void SWAP(ref long a) {
		a = ((a & 0xFFFF) << 16) | ((a >> 16) & 0xFFFF);
	}

	// No X IN
	public void ROL_w(int rot, ref int reg) {
		int backup = (int) (reg & 0xFFFF0000);
		reg &= 0xFFFF;
		for (int i = 0; i < rot; i++) {
			reg = ((reg << 1) | ((reg >> 15) & 0x0001)) & 0xFFFF;
		}
		reg |= backup;
	}

	// No X IN
	public void ROR_w(int rot, ref int reg) {
		int backup = (int) (reg & 0xFFFF0000);
		reg &= 0xFFFF;
		for (int i = 0; i < rot; i++) {
			reg = ((reg >> 1) | ((reg & 0x0001) << 15)) & 0xFFFF;
		}
		reg |= backup;
	}

	public bool ROXL_w(int rot, ref int reg, bool X) {
		int backup = (int) (reg & 0xFFFF0000);
		reg &= 0xFFFF;
		for (int i = 0; i < rot; i++) {
			bool C = (reg & 0x8000) != 0;
			reg = ((reg << 1) | (X ? 0x0001 : 0x0000)) & 0xFFFF;
			X = C;
		}
		reg |= backup;
		return (X);
	}

	public bool ROXR_b(int rot, ref int reg, bool X) {
		int backup = (int) (reg & 0xFFFFFF00);
		reg &= 0xFF;
		for (int i = 0; i < rot; i++) {
			bool C = (reg & 0x01) != 0;
			reg = ((reg >> 1) | (X ? 0x80 : 0x00)) & 0xFF;
			X = C;
		}
		reg |= backup;
		return (X);
	}

	// No X IN
	public bool LSR_b(int rot, ref int reg) {
		int backup = (int) (reg & 0xFFFFFF00);
		reg &= 0xFF;
		bool X = false;
		for (int i = 0; i < rot; i++) {
			bool C = (reg & 0x01) != 0;
			reg = (reg >> 1) & 0xFF;
			X = C;
		}
		reg |= backup;
		return (X);
	}

	// No X IN
	public bool LSR_w(int rot, ref int reg) {
		int backup = (int) (reg & 0xFFFF0000);
		reg &= 0xFFFF;
		bool X = false;
		for (int i = 0; i < rot; i++) {
			bool C = (reg & 0x0001) != 0;
			reg = (reg >> 1) & 0xFFFF;
			X = C;
		}
		reg |= backup;
		return (X);
	}

	// No X IN
	public bool ASR_b(int rot, ref int reg) {
		int backup = (int) (reg & 0xFFFFFF00);
		reg &= 0xFF;
		bool X = false;
		for (int i = 0; i < rot; i++) {
			bool MSB = (reg & 0x80) != 0;
			bool C = (reg & 0x01) != 0;
			reg = ((reg >> 1) | (MSB ? 0x80 : 0x00)) & 0xFF;
			X = C;
		}
		reg |= backup;
		return (X);
	}

	// No X IN
	public bool ASR_w(int rot, ref int reg) {
		int backup = (int) (reg & 0xFFFF0000);
		reg &= 0xFFFF;
		bool X = false;
		for (int i = 0; i < rot; i++) {
			bool MSB = (reg & 0x8000) != 0;
			bool C = (reg & 0x0001) != 0;
			reg = ((reg >> 1) | (MSB ? 0x8000 : 0x0000)) & 0xFFFF;
			X = C;
		}
		reg |= backup;
		return (X);
	}

	// No X IN
	public bool ASR_l(int rot, ref long reg) {
		bool X = false;
		reg &= 0xFFFFFFFF;
		for (int i = 0; i < rot; i++) {
			bool MSB = (reg & 0x80000000) != 0;
			bool C = (reg & 0x00000001) != 0;
			reg = ((reg >> 1) | (MSB ? 0x80000000 : 0x00000000)) & 0xFFFFFFFF;
			X = C;
		}
		return (X);
	}
	#endregion
}
