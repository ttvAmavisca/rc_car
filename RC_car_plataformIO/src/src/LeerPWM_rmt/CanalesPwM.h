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
	void initCanales_Interupt();
	uint64_t valor_int(uint8_t canal);
	uint64_t valor_noint(uint8_t canal);
	float valor(uint8_t canal);
	void debugOutSerial(Stream* port);

    
	static void IRAM_ATTR rmt_isr_handler(void* arg);


	static uint32_t canal_rc[7];
	static uint64_t canal_rc_last[7];
private:
	uint64_t _min, _max; 
	const uint8_t PIN1 , PIN2, PIN3 ,PIN4 ,PIN5 ,PIN6;
	bool con_interrupt;
	static rmt_isr_handle_t _rmt_driver_intr_handle;
   
};


#endif // _CanalPwM_h