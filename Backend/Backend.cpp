#include "pch.h"
#include "Backend.h"

#include <WinBase.h>

HANDLE physics_mfile;
unsigned char* physics_buffer;

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
