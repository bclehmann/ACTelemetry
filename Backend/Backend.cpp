#include "pch.h"
#include "Backend.h"

#include <WinBase.h>

HANDLE physics_mfile;
HANDLE graphics_mfile;
unsigned char* physics_buffer;
unsigned char* graphics_buffer;

void initialize_physics() {
	physics_mfile = CreateFileMapping(INVALID_HANDLE_VALUE, NULL, PAGE_READONLY, 0, 10 * 1024, TEXT("Local\\acpmf_physics"));
	if (!physics_mfile) {
		throw;
	}
	physics_buffer = (unsigned char*)MapViewOfFile(physics_mfile, FILE_MAP_READ, 0, 0, sizeof(PhysicsMemoryMap));
}

PhysicsMemoryMap* get_physics()
{
	return (PhysicsMemoryMap*)physics_buffer;
}

void free_physics()
{
	UnmapViewOfFile(physics_buffer);
	CloseHandle(physics_mfile);
}

void initialize_graphics() {
	graphics_mfile = CreateFileMapping(INVALID_HANDLE_VALUE, NULL, PAGE_READONLY, 0, 10 * 1024, TEXT("Local\\acpmf_graphics"));
	if (!graphics_mfile) {
		throw;
	}
	graphics_buffer = (unsigned char*)MapViewOfFile(graphics_mfile, FILE_MAP_READ, 0, 0, sizeof(graphics_buffer));
}

GraphicsMemoryMap* get_graphics()
{
	return (GraphicsMemoryMap*)graphics_buffer;
}

void free_graphics()
{
	UnmapViewOfFile(graphics_buffer);
	CloseHandle(graphics_mfile);
}
