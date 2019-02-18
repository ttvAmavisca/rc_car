#ifndef _CanalPwM_h
#define _CanalPwM_h

#include <Arduino.h>
#include <FunctionalInterrupt.h>





class CanalPwM
{
public:
  
	CanalPwM(uint8_t reqPin);
  
	~CanalPwM();

	void IRAM_ATTR isr();
 
 void IRAM_ATTR isr2();
 
	uint64_t valor();

private:
	const uint8_t PIN;
    volatile uint64_t startTIM;
    volatile uint64_t lastValue;
};


#endif // _CanalPwM_h