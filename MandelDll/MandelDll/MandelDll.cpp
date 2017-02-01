// MandelDll.cpp : Defines the exported functions for the DLL application.
//

#include "stdafx.h"
#include "Mandelbrot.h"


extern "C" __declspec(dllexport) void _stdcall Calculate(byte* scan0, int dataSize,
	double xMin, double xSize, double yMin, double ySize, int iterations)
{
	mandelbrot* M = new mandelbrot((unsigned*)scan0, dataSize*dataSize, iterations, DEFAULT_NUM_TILES);
	M->execute(xMin, xSize, yMin, ySize, -0.74543, 0.11301);
	M->~mandelbrot();
}