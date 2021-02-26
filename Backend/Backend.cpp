#include "pch.h"
#include "Backend.h"

#include <memory>
#include <WinBase.h>

HANDLE physics_mfile;
HANDLE graphics_mfile;
HANDLE statics_mfile;
physics_mmap* physics_buffer;
graphics_mmap* graphics_buffer;
statics_mmap* statics_buffer;

template<class T>
void initialize_mmap(HANDLE& handle, T*& buffer, LPCWSTR path) {
	handle = CreateFileMappingW(INVALID_HANDLE_VALUE, NULL, PAGE_READONLY, 0, 10 * 1024, path);
	if (handle == nullptr) {
		throw;
	}

	buffer = reinterpret_cast<T*>(MapViewOfFile(handle, FILE_MAP_READ, 0, 0, sizeof(T)));
}

template<class T>
void free_mmap(HANDLE& handle, T*& buffer) {
	UnmapViewOfFile(buffer);
	CloseHandle(handle);
}

void initialize_physics() {
	initialize_mmap<physics_mmap>(physics_mfile, physics_buffer, TEXT("Local\\acpmf_physics"));
}

physics_mmap* get_physics()
{
	return physics_buffer;
}

void free_physics()
{
	free_mmap<physics_mmap>(physics_mfile, physics_buffer);
}

void initialize_graphics() {
	initialize_mmap<graphics_mmap>(graphics_mfile, graphics_buffer, TEXT("Local\\acpmf_graphics"));
}

graphics_mmap* get_graphics()
{
	return graphics_buffer;
}

void free_graphics()
{
	free_mmap<graphics_mmap>(graphics_mfile, graphics_buffer);
}


void initialize_statics() {
	initialize_mmap<statics_mmap>(statics_mfile, statics_buffer, TEXT("Local\\acpmf_static"));
}

statics_mmap* get_statics()
{
	return statics_buffer;
}

void free_statics()
{
	free_mmap<statics_mmap>(statics_mfile, statics_buffer);
}
