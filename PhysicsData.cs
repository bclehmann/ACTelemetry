using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Xps.Packaging;

namespace AssettoCorsaTelemetryApp
{
	public class PhysicsData
	{
		unsafe public struct PhysicsMemoryMap
		{
			public int packetId;
			public float gas;
			public float brake;
			public float fuel;
			public int gear;
			public int rpms;
			public float steerAngle;
			public float speedKmh;
			public fixed float velocity[3];
			public fixed float accG[3];
			public fixed float wheelSlip[4];
			public fixed float wheelLoad[4];
			public fixed float wheelsPressure[4];
			public fixed float wheelAngularSpeed[4];
			public fixed float tyreWear[4];
			public fixed float tyreDirtyLevel[4];
			public fixed float tyreCoreTemperature[4];
			public fixed float camberRAD[4];
			public fixed float suspensionTravel[4];
			public float drs;
			public float tc;
			public float heading;
			public float pitch;
			public float roll;
			public float cgHeight;
			public fixed float carDamage[5];
			public int numberOfTyresOut;
			public int pitLimiterOn;
			public float abs;
			public float kersCharge;
			public float kersInput;
			public int autoShifterOn;
			public fixed float righeHeight[2];
			public float turboBoost;
			public float ballast;
			public float airDensity;
			public float airTemp;
			public float roadTemp;
			public fixed float localAngularVel[2];
			public float finalFF;
			public float performanceMeter;
			public int engineBrake;
			public int ersRecoveryLevel;
			public int ersPowerLevel;
			public int ersHeatCharging;
			public int ersIsCharging;
			public float kersCurrentKJ;
			public int drsAvailable;
			public int drsEnabled;
			public fixed float brakeTemp[4];
			public float clutch;
			public fixed float tyreTempI[4];
			public fixed float tyreTempM[4];
			public fixed float tyreTempO[4];
			public int isAIControlled;
			public fixed float tyreContactPoint[12];
			public fixed float tyreContactNormal[12];
			public fixed float tyreContactHeading[12];
			public float brakeBias;
			public fixed float localVelocity[3];
		}

		private static IntPtr physicsptr = IntPtr.Zero;

		//These will almost certainly need updating on your project. Sorry :/
#if RELEASE
		[DllImport(@"C:\Users\benny\source\repos\AssettoCorsaTelemetryApp\x64\Release\Backend.dll", EntryPoint = "initialize_physics")]
#else
		[DllImport(@"C:\Users\benny\source\repos\AssettoCorsaTelemetryApp\x64\Debug\Backend.dll", EntryPoint = "initialize_physics")]
#endif
		public static extern void InitializePhysics();

#if RELEASE
		[DllImport(@"C:\Users\benny\source\repos\AssettoCorsaTelemetryApp\x64\Release\Backend.dll", EntryPoint = "free_physics")]
#else
		[DllImport(@"C:\Users\benny\source\repos\AssettoCorsaTelemetryApp\x64\Debug\Backend.dll", EntryPoint = "free_physics")]
#endif
		public static extern void FreePhysics();

#if RELEASE
		[DllImport(@"C:\Users\benny\source\repos\AssettoCorsaTelemetryApp\x64\Release\Backend.dll", EntryPoint = "get_physics")]
#else
		[DllImport(@"C:\Users\benny\source\repos\AssettoCorsaTelemetryApp\x64\Debug\Backend.dll", EntryPoint = "get_physics")]
#endif
		private static extern IntPtr GetPhysicsPointer();

		public static PhysicsMemoryMap GetPhysics()
		{
			if(physicsptr == IntPtr.Zero)
			{
				physicsptr = GetPhysicsPointer();
			}

			PhysicsMemoryMap data = new PhysicsMemoryMap();
			data = Marshal.PtrToStructure<PhysicsMemoryMap>(physicsptr);
			return data;
		}
	}
}
