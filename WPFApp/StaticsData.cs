using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace AssettoCorsaTelemetryApp
{
	class StaticsData : IMemoryMappedFile<StaticsMemoryMap>
	{
		public StaticsData()
		{
			InitializeStatics();
			staticsPtr = GetGraphicsPointer();
		}

		private IntPtr staticsPtr = IntPtr.Zero;

		//These will almost certainly need updating on your project. Sorry :/
#if RELEASE
		[DllImport(@"C:\Program Files\ACTelemetry\Backend.dll", EntryPoint = "initialize_statics")]
#else
		[DllImport(@"C:\Users\benny\source\repos\AssettoCorsaTelemetryApp\x64\Debug\Backend.dll", EntryPoint = "initialize_statics")]
#endif
		private static extern void InitializeStatics();

#if RELEASE
		[DllImport(@"C:\Program Files\ACTelemetry\Backend.dll", EntryPoint = "free_statics")]
#else
		[DllImport(@"C:\Users\benny\source\repos\AssettoCorsaTelemetryApp\x64\Debug\Backend.dll", EntryPoint = "free_statics")]
#endif
		private static extern void FreeStatics();

#if RELEASE
		[DllImport(@"C:\Program Files\ACTelemetry\Backend.dll", EntryPoint = "get_statics")]
#else
		[DllImport(@"C:\Users\benny\source\repos\AssettoCorsaTelemetryApp\x64\Debug\Backend.dll", EntryPoint = "get_statics")]
#endif
		private static extern IntPtr GetGraphicsPointer();

		public StaticsMemoryMap GetData()
		{
			StaticsMemoryMap data = new StaticsMemoryMap();
			data = Marshal.PtrToStructure<StaticsMemoryMap>(staticsPtr);
			return data;
		}

		public void Dispose()
		{
			FreeStatics();
		}
	}

	unsafe public struct StaticsMemoryMap
	{
		public fixed char smVersion[15];
		public fixed char acVersion[15];
		public int numberOfSessions;
		public int numCars;
		public fixed char carModel[33];
		public fixed char track[33];
		public fixed char playerName[33];
		public fixed char playerSurname[33];
		public fixed char playerNick[33];
		public int sectorCount;
		public float maxTorque;
		public float maxPower;
		public int maxRpm;
		public float maxFuel;
		public fixed float suspensionMaxTravel[4];
		public fixed float tyreRadius[4];
		public float maxTurboBoost;
		public float deprecated_1;
		public float deprecated_2;
		public int penaltiesEnabled;
		public float aidFuelRate;
		public float aidTireRate;
		public float aidMechanicalDamage;
		public int aidAllowTyreBlankets;
		public float aidStability;
		public int aidAutoClutch;
		public int aidAutoBlip;
		public int hasDRS;
		public int hasERS;
		public int hasKERS;
		public float kersMaxJ;
		public int engineBrakeSettingsCount;
		public int ersPowerControllerCount;
		public float trackSPlineLength;
		public fixed char trackConfiguration[33];
		public float ersMaxJ;
		public int isTimeRace;
		public int hasExtraLap;
		public fixed char carSkin[33];
		public int reversedGridPositions;
		public int PitWindowStart;
		public int PitWindowEnd;
	}
}
