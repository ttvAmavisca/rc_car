#ifndef _rc_car_h
#define _rc_car_h

#include <Arduino.h>


class Rc_car
{
	


	public:
		enum e_servo
		{
		enum_rueda_derecha = 0,
		enum_rueda_izquierda = 1,
		enum_marcha = 2,
		enum_motor = 3
		};

		enum e_modo
		{
		enum_manual = 0,
		enum_sistema_salida = 1,
		enum_semi_auto = 2,
		enum_full_auto = 3
		};

		/**
		 * @brief      Class constructor
		 */
		Rc_car(void);

		
		float angulos[4];
		float pitch, roll, yaw;
		float rpmActual;
		float consigna[4];
		float consigna_manual[4];
		
		float AnguloRuedaDerecha;
		float AnguloRuedaIzquierda;
		float velocidad[3];
		float aceleracion[3];
		uint8_t tipoControl;
		uint8_t modo_motor_actual;
		float  ESC_Dutycycle, ESC_avgInputCurrent,ESC_avgMotorCurrent,ESC_rpmActual,ESC_VoltajeEntrada;

	private: 
		bool none;
		
};

#endif
