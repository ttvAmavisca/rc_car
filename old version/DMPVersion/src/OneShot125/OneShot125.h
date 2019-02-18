#pragma once

#include <cstdint>

class OneShot125
{
	
	
public:
   
	OneShot125();
	
    void Init() ;

    void Arm() ;

    void Disarm() ;

    void Update(uint16_t motor_power);
};
