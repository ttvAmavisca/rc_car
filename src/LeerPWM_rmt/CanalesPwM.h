#ifndef _CanalesPwM_h
#define _CanalesPwM_h

#include <Arduino.h>
#include <driver/rmt.h>





class CanalesPwM
{
public:
  
	CanalesPwM(uint8_t reqPin1,uint8_t reqPin2,uint8_t reqPin3,uint8_t reqPin4,uint8_t reqPin5,uint8_t reqPin6);
  
	~CanalesPwM();

    void initCanales();
	uint64_t valor(uint8_t canal);

private:
	const uint8_t PIN1 , PIN2, PIN3 ,PIN4 ,PIN5 ,PIN6;
   
};


#endif // _CanalPwM_h