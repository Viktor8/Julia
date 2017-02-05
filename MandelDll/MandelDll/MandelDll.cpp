// MandelDll.cpp : Defines the exported functions for the DLL application.
//

#include "Mandelbrot.h"


extern "C" __declspec(dllexport) void _stdcall Calculate(int* scan0, int dataSize,
	double xMin, double xSize, double yMin, double ySize, int iterations)
{
	mandelbrot* M = new mandelbrot((unsigned*)scan0, dataSize*dataSize, iterations);

		M->execute(xMin, xSize, yMin, ySize);
	
}