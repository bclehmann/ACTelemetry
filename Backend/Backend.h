#pragma once


#define export extern "C" __declspec(dllexport)

struct basics{
	float gas;
	float brake;
	int gear;
	float steering_angle;
};

struct PhysicsMemoryMap
{
	int packetId = 0;
	float gas = 0;
	float brake = 0;
	float fuel = 0;
	int gear = 0;
	int rpms = 0;
	float steerAngle = 0;
	float speedKmh = 0;
	float velocity[3];
	float accG[3];
	float wheelSlip[4];
	float wheelLoad[4];
	float wheelsPressure[4];
	float wheelAngularSpeed[4];
	float tyreWear[4];
	float tyreDirtyLevel[4];
	float tyreCoreTemperature[4];
	float camberRAD[4];
	float suspensionTravel[4];
	float drs = 0;
	float tc = 0;
	float heading = 0;
	float pitch = 0;
	float roll = 0;
	float cgHeight;
	float carDamage[5];
	int numberOfTyresOut = 0;
	int pitLimiterOn = 0;
	float abs = 0;
	float kersCharge = 0;
	float kersInput = 0;
	int autoShifterOn = 0;
	float righeHeight[2];
	float turboBoost = 0;
	float ballast = 0;
	float airDensity = 0;
	float airTemp = 0;
	float roadTemp = 0;
	float localAngularVel[3];
	float finalFF = 0;
	float performanceMeter = 0;
	int engineBrake = 0;
	int ersRecoveryLevel = 0;
	int ersPowerLevel = 0;
	int ersHeatCharging = 0;
	int ersIsCharging = 0;
	float kersCurrentKJ = 0;
	int drsAvailable = 0;
	int drsEnabled = 0;
	float brakeTemp[4];
	float clutch = 0;
	float tyreTempI[4];
	float tyreTempM[4];
	float tyreTempO[4];
	int isAIControlled;
	float tyreContactPoint[4][3];
	float tyreContactNormal[4][3];
	float tyreContactHeading[4][3];
	float brakeBias;
	float localVelocity[3];
};

export PhysicsMemoryMap* get_physics();
export void initialize_physics();
export void free_physics();
