#ifndef _rc_car_h
#define _rc_car_h

#include <Arduino.h>


class Rc_car
{
	


	public:
		/**
		 * @brief      Class constructor
		 */
		Rc_car(void);



	private: 

	float angulos[4];
	float pitch, roll, yaw;
	float rpmActual;
		
};

#endif
