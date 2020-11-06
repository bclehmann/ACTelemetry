using System;
using System.Collections.Generic;
using System.Text;

namespace AssettoCorsaTelemetryApp
{
	public interface IMemoryMappedFile<T> : IDisposable
	{
		T GetData();
	}
}
