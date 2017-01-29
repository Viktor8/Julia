// MandelDll.cpp : Defines the exported functions for the DLL application.
//

#include "stdafx.h"
#include "Mandelbrot.h"


extern "C" __declspec(dllexport) void _stdcall Calculate(byte* scan0, int dataSize, 
	double xMin, double xSize, double yMin, double ySize, int iterations)
{

	mandelbrot* M = new mandelbrot(dataSize*dataSize,iterations,DEFAULT_NUM_TILES);
	M->execute(xMin, xSize, yMin, ySize,-0.74543,0.11301);
	unsigned int* iterMap = M->getResult();
	int maxIt = 0;
	for (size_t i = 0; i < dataSize*dataSize; i++)
		if (iterMap[i] > maxIt && iterMap[i]!=iterations)
			maxIt = iterMap[i];
	float val = 0;
	

	for (size_t i = 0; i < dataSize * dataSize; ++i)
	{
		val = ((float)iterMap[i]) / maxIt;
		*(scan0 + i * 4) = 255 * val;								//B
		*(scan0 + i * 4 + 1) = 255 * val;							//G
		*(scan0 + i * 4 + 2) = 255 * val;							//R
		*(scan0 + i * 4 + 3) = 255;									//A
	}

	M->~mandelbrot();
}