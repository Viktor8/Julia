#pragma once
#include <amp.h>
#include "stdafx.h"


#define TILE_SIZE       16

#define DEFAULT_ITERATIONS   1000
#define DEFAULT_NUM_TILES   512

using namespace concurrency;

class mandelbrot
{
public:
	mandelbrot(int _data_size, int _iterations, int _num_tiles);
	void execute(double xMin, double xSize, double yMin, double ySize, double cRe , double cIm);
	unsigned int* getResult();
	~mandelbrot();


private:
	static unsigned mandelbrot_calc(int iterations, double y0, double x0) restrict(cpu, amp);

	std::vector<unsigned> data;
	unsigned iterations;
	unsigned num_of_tiles;
	float cIm;
	float cRe;
};
