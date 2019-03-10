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

		enum e_canal_rc
		{
		enum_rc_canal_1=0,
		enum_rc_canal_2,
		enum_rc_canal_3,
		enum_rc_canal_4,
		enum_rc_canal_5,
		enum_rc_canal_6
		};

		/**
		 * @brief      Class constructor
		 */
		Rc_car(void);

		/**
		 * @brief      calculo de consignas dependiendo de las entradas y modo actual
		 */
		void calcular_consignas();

		/**
		 * @brief      cambio de modo de control
		 */
		bool Cambiar_modo(int nuevo_modo );
		
		float angulos[4];
		float pitch, roll, yaw;
		float rpmActual;
		float consigna[4];
		float consigna_manual[4];
		float consigna_rc[6];
		
		float AnguloRuedaDerecha;
		float AnguloRuedaIzquierda;
		float giro_val[3];
		float aceleracion[3];
		uint8_t tipoControl;
		uint8_t modo_motor_actual;
		float  ESC_Dutycycle, ESC_avgInputCurrent,ESC_avgMotorCurrent,ESC_rpmActual,ESC_VoltajeEntrada;


		float arranque_aceleracion_fase_1, arranque_tmax_fase_1, arranque_Vmax_fase_1;
		float arranque_aceleracion_fase_2, arranque_tmax_fase_2, arranque_Vmax_fase_2;
		float arranque_aceleracion_fase_3, arranque_tmax_fase_3, arranque_Vmax_fase_3;

	private: 
	    int estado_arranque;
		int estado_auto;
		int estado_semi_auto;
		bool none;


		int64_t t_arranque;
		
		

		/**
		 * @brief      Calculo de consigna del motor para el modo salida
		 */
		void Modo_salida();

		
};

#endif
