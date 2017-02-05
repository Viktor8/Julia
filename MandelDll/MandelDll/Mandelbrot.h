#pragma once
#include <amp.h>



#define TILE_SIZE       16
#define DEFAULT_ITERATIONS   1000
#define DEFAULT_NUM_TILES   512

using namespace concurrency;

class mandelbrot
{
public:
	mandelbrot(unsigned* _output,int _data_size, int _iterations);
	void execute(double xMin, double xSize, double yMin, double ySize);
	void execute(float xMin, float xSize, float yMin, float ySize);
	~mandelbrot();


private:
	static unsigned mandelbrot_calc(int iterations, double y0, double x0) restrict(cpu, amp);
	static unsigned mandelbrot_calc(int iterations, float y0, float x0) restrict(cpu, amp);

	unsigned* result_out;
	unsigned iterations;
	unsigned num_of_tiles;
	unsigned data_size_1d;
	float cIm;
	float cRe;
};

