#include "LeerPWM.h"


CanalPwM::CanalPwM(uint8_t reqPin) : PIN(reqPin){
	pinMode(PIN, INPUT_PULLUP);
	attachInterrupt(PIN, std::bind(&CanalPwM::isr,this), RISING);
};

CanalPwM::~CanalPwM() {
	detachInterrupt(PIN);
}

void IRAM_ATTR CanalPwM::isr() {
	startTIM=esp_timer_get_time();
	attachInterrupt(PIN, std::bind(&CanalPwM::isr2,this), FALLING);
}

void IRAM_ATTR CanalPwM::isr2() {
	lastValue=esp_timer_get_time() - startTIM;
	attachInterrupt(PIN, std::bind(&CanalPwM::isr,this), RISING);
}

uint64_t CanalPwM::valor() {
	return lastValue;
}

