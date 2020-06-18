using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Xps.Packaging;

namespace AssettoCorsaTelemetryApp
{
	public class GraphicsData 
	{
		public enum AC_STATUS
		{
			AC_OFF,
			AC_REPLAY,
			AC_LIVE,
			AC_PAUSE,
		};

		public enum AC_SESSION_TYPE
		{
			AC_UNKNOWN = -1,
			AC_PRACTICE,
			AC_QUALIFY,
			AC_RACE,
			AC_HOTLAP,
			AC_TIME_ATTACK,
			AC_DRIFT,
			AC_DRAG
		};

		public enum AC_FLAG_TYPE
		{
			AC_NO_FLAG,
			AC_BLUE_FLAG,
			AC_YELLOW_FLAG,
			AC_BLACK_FLAG,
			AC_WHITE_FLAG,
			AC_CHECKERED_FLAG,
			AC_PENALTY_FLAG
		};

		public unsafe struct GraphicsMemoryMap
		{
			public int packetId;
			public AC_STATUS status;
			AC_SESSION_TYPE session;
			fixed char currentTIme[15];
			fixed char lastTime[15];
			fixed char bestTime[15];
			fixed char split[15];
			public int completedLaps;
			public int position;
			public int iCurrentTime;
			public int iLastTime;
			public int iBestTime;
			public float sessionTimeLeft;
			public float distanceTraveled;
			public int isInPit;
			public int currentSectorIndex;
			public int lastSectorTime;
			public int numberOfLaps;
			fixed char tyreCompound[33];
			public float replayTimeMultiplier;
			public float normalizedCarPosition;
			public fixed float carCoordinates[3];
			public float penaltyTime;
			AC_FLAG_TYPE flag;
			public int idealLineOn;
			public int isInPitLane;
			public float surfaceGrip;
			public int mandatoryPitDone;
			public float windSpeed;
			public float windDirection;
		};

		private static IntPtr graphicsPtr = IntPtr.Zero;

		//These will almost certainly need updating on your project. Sorry :/
#if RELEASE
		[DllImport(@"C:\Users\benny\source\repos\AssettoCorsaTelemetryApp\x64\Release\Backend.dll", EntryPoint = "initialize_graphics")]
#else
		[DllImport(@"C:\Users\benny\source\repos\AssettoCorsaTelemetryApp\x64\Debug\Backend.dll", EntryPoint = "initialize_graphics")]
#endif
		public static extern void InitializeGraphics();

#if RELEASE
		[DllImport(@"C:\Users\benny\source\repos\AssettoCorsaTelemetryApp\x64\Release\Backend.dll", EntryPoint = "free_graphics")]
#else
		[DllImport(@"C:\Users\benny\source\repos\AssettoCorsaTelemetryApp\x64\Debug\Backend.dll", EntryPoint = "free_graphics")]
#endif
		public static extern void FreeGraphics();

#if RELEASE
		[DllImport(@"C:\Users\benny\source\repos\AssettoCorsaTelemetryApp\x64\Release\Backend.dll", EntryPoint = "get_graphics")]
#else
		[DllImport(@"C:\Users\benny\source\repos\AssettoCorsaTelemetryApp\x64\Debug\Backend.dll", EntryPoint = "get_graphics")]
#endif
		private static extern IntPtr GetGraphicsPointer();

		public static GraphicsMemoryMap GetGraphics()
		{
			if(graphicsPtr == IntPtr.Zero)
			{
				graphicsPtr = GetGraphicsPointer();
			}

			GraphicsMemoryMap data = new GraphicsMemoryMap();
			data = Marshal.PtrToStructure<GraphicsMemoryMap>(graphicsPtr);
			return data;
		}
	}
}
