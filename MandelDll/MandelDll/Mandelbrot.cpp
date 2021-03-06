//////////////////////////////////////////////////////////////////////////////
//// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF
//// ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO
//// THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
//// PARTICULAR PURPOSE.
////
//// Copyright (c) Microsoft Corporation. All rights reserved
//////////////////////////////////////////////////////////////////////////////

//----------------------------------------------------------------------------
// File: MandelBrot.cpp
// 
// Implements Mandel Brot sample in C++ AMP
//----------------------------------------------------------------------------


#include "MandelBrot.h"
#include <math.h>
#include <assert.h>
#include <iostream>



mandelbrot::mandelbrot(unsigned * _output, int _data_size, int _iterations)
{
	result_out = _output;
	iterations = _iterations;
	num_of_tiles = DEFAULT_NUM_TILES;
	data_size_1d = sqrt(_data_size);
	accelerator::set_default((accelerator::get_all())[1].get_device_path());
}



mandelbrot::~mandelbrot()
{
}

unsigned mandelbrot::mandelbrot_calc(int iterations, double y0, double x0) restrict(cpu, amp)
{

	double y = y0;
	double x = x0;
	double xt = x0;

	int iteration = 0;
	while ((x <= 2.0)&(y <= 2.0)&(x >= -2.0)&(y >= -2.0) & (iteration < iterations))
	{
		xt = x;
		x = x * x - y*y - 0.74543;
		y = 2 * xt * y + 0.11301;
		iteration++;
	}
	unsigned t = iteration * 255 / iterations;;
	unsigned result = 255;
	result <<= 8;
	result |= t;
	result <<= 8;
	result |= t;
	result <<= 8;
	result |= t;
	return (unsigned)result;
}
unsigned mandelbrot::mandelbrot_calc(int iterations, float y0, float x0) restrict(cpu, amp)
{

	float y = y0;
	float x = x0;
	float xt = x0;

	int iteration = 0;
	while ((x <= 2.0)&(y <= 2.0)&(x >= -2.0)&(y >= -2.0) & (iteration < iterations))
	{
		xt = x;
		x = x * x - y*y - 0.74543;
		y = 2 * xt * y + 0.11301;
		iteration++;
	}
	unsigned t = iteration * 255 / iterations;;
	unsigned result = 255;
	result <<= 8;
	result |= t;
	result <<= 8;
	result |= t;
	result <<= 8;
	result |= t;
	return (unsigned)result;
}


void mandelbrot::execute(double xMin, double xSize, double yMin, double ySize)
{
	int iters = iterations;
	int tiles = num_of_tiles;

	int size_1d = (int)data_size_1d;

	array<int, 1> count(1);
	array<unsigned, 2> a_data(size_1d, size_1d);
	//int zero = 0;

	int max_chunks = (size_1d * size_1d) / (TILE_SIZE * TILE_SIZE);

	double yscale = ySize / (double)size_1d;
	double xscale = xSize / (double)size_1d;

	parallel_for_each(extent<2>(TILE_SIZE, tiles*TILE_SIZE).tile<TILE_SIZE, TILE_SIZE>(),
		[=, &a_data, &count](tiled_index<TILE_SIZE, TILE_SIZE> tidx) restrict(amp)
	{
		tile_static int chunk_id;
		tile_static int global_y;
		tile_static int global_x;

		// Here each tile will process a chuck of data and pick next block to process
		// This is like load balancing computation, a tile will pick next available chunk to process
		// "chunk_id" value in a tile will determine which chunk of data is being processed by this tile
		while (1)
		{
			// All threads from previous iteration sync here
			tidx.barrier.wait();
			if (tidx.local[1] == 0 && tidx.local[0] == 0)
			{
				// Sync-ing chuck to be processed between tiles
				// get chunk to process for this tile
				chunk_id = atomic_fetch_add(&count[0], 1);
				global_y = chunk_id / (size_1d / TILE_SIZE) * TILE_SIZE;
				global_x = chunk_id % (size_1d / TILE_SIZE) * TILE_SIZE;
			}
			// Sync within a tile.
			// Now threads have tile specific chunk_id, global_y, and global_x
			tidx.barrier.wait();

			if (chunk_id >= max_chunks) break;

			// calculate Mandelbrot for scaled coordinate of pixel 
			double y0 = (global_y + tidx.local[0]) * yscale + yMin;
			double x0 = (global_x + tidx.local[1]) * xscale + xMin;
			a_data(global_y + tidx.local[0], global_x + tidx.local[1]) =
				mandelbrot_calc(iters, y0, x0);
		}
	});

	copy(a_data, result_out);
}
void mandelbrot::execute(float xMin, float xSize, float yMin, float ySize)
{
	int iters = iterations;
	int tiles = num_of_tiles;

	int size_1d = (int)data_size_1d;

	array<int, 1> count(1);
	array<unsigned, 2> a_data(size_1d, size_1d);
	//int zero = 0;

	int max_chunks = (size_1d * size_1d) / (TILE_SIZE * TILE_SIZE);

	float yscale = ySize / (float)size_1d;
	float xscale = xSize / (float)size_1d;

	parallel_for_each(extent<2>(TILE_SIZE, tiles*TILE_SIZE).tile<TILE_SIZE, TILE_SIZE>(),
		[=, &a_data, &count](tiled_index<TILE_SIZE, TILE_SIZE> tidx) restrict(amp)
	{
		tile_static int chunk_id;
		tile_static int global_y;
		tile_static int global_x;

		while (1)
		{
			// All threads from previous iteration sync here
			tidx.barrier.wait();
			if (tidx.local[1] == 0 && tidx.local[0] == 0)
			{
				// Sync-ing chuck to be processed between tiles
				// get chunk to process for this tile
				chunk_id = atomic_fetch_add(&count[0], 1);
				global_y = chunk_id / (size_1d / TILE_SIZE) * TILE_SIZE;
				global_x = chunk_id % (size_1d / TILE_SIZE) * TILE_SIZE;
			}
			// Sync within a tile.
			// Now threads have tile specific chunk_id, global_y, and global_x
			tidx.barrier.wait();

			if (chunk_id >= max_chunks) break;

			// calculate Mandelbrot for scaled coordinate of pixel 
			float y0 = (global_y + tidx.local[0]) * yscale + yMin;
			float x0 = (global_x + tidx.local[1]) * xscale + xMin;
			a_data(global_y + tidx.local[0], global_x + tidx.local[1]) =
				mandelbrot_calc(iters, y0, x0);
		}
	});

	copy(a_data, result_out);
}

