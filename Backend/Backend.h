#pragma once


#define dllpub extern "C" __declspec(dllexport)

struct basics {
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

enum AC_STATUS {
	AC_OFF,
	AC_REPLAY,
	AC_LIVE,
	AC_PAUSE,
};

enum AC_SESSION_TYPE {
	AC_UNKNOWN = -1,
	AC_PRACTICE,
	AC_QUALIFY,
	AC_RACE,
	AC_HOTLAP,
	AC_TIME_ATTACK,
	AC_DRIFT,
	AC_DRAG
};

enum AC_FLAG_TYPE {
	AC_NO_FLAG,
	AC_BLUE_FLAG,
	AC_YELLOW_FLAG,
	AC_BLACK_FLAG,
	AC_WHITE_FLAG,
	AC_CHECKERED_FLAG,
	AC_PENALTY_FLAG
};

struct GraphicsMemoryMap {
	int packetId = 0;
	AC_STATUS status = AC_OFF;
	AC_SESSION_TYPE session = AC_PRACTICE;
	wchar_t currentTIme[15];
	wchar_t lastTime[15];
	wchar_t bestTime[15];
	wchar_t split[15];
	int completedLaps = 0;
	int position = 0;
	int iCurrentTime = 0;
	int iLastTime = 0;
	int iBestTime = 0;
	float sessionTimeLeft = 0;
	float distanceTraveled = 0;
	int isInPit = 0;
	int currentSectorIndex = 0;
	int lastSectorTime = 0;
	int numberOfLaps = 0;
	wchar_t tyreCompound[33];
	float replayTimeMultiplier = 0;
	float normalizedCarPosition = 0;
	float carCoordinates[3];
	float penaltyTime = 0;
	AC_FLAG_TYPE flag = AC_NO_FLAG;
	int idealLineOn = 0;
	int isInPitLane = 0;
	float surfaceGrip = 0;
	int mandatoryPitDone = 0;
	float windSpeed = 0;
	float windDirection = 0;
};

dllpub PhysicsMemoryMap* get_physics();
dllpub void initialize_physics();
dllpub void free_physics();

dllpub GraphicsMemoryMap* get_graphics();
dllpub void initialize_graphics();
dllpub void free_graphics();
