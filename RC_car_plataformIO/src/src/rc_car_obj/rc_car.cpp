#include "rc_car.h"


Rc_car::Rc_car(void){
	angulos[0]=1;
    angulos[1]=2;
    angulos[2]=3;
    angulos[3]=4;
    pitch=5;
    roll=6;
    yaw=7;
    rpmActual=8;
    consigna[enum_rueda_derecha]=9;
    consigna[enum_rueda_izquierda]=10;
    consigna[enum_marcha]=11;
    consigna[enum_motor]=12;
    
    velocidad[0]=13;
    velocidad[1]=14;
    velocidad[2]=15;
    aceleracion[0]=16;
    aceleracion[1]=17;
    aceleracion[2]=18;
    tipoControl=19;
   
    ESC_Dutycycle=21;
    ESC_avgInputCurrent=22;
    ESC_avgMotorCurrent=23;
    ESC_rpmActual=24;
    ESC_VoltajeEntrada=25;

    consigna_manual[enum_rueda_derecha]=26;
    consigna_manual[enum_rueda_izquierda]=27;
    consigna_manual[enum_marcha]=28;
    consigna_manual[enum_motor]=29;

}

