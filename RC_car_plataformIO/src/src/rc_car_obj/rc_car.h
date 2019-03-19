#ifndef _rc_car_h
#define _rc_car_h

#include <Arduino.h>

class Rc_car
{

  public:
	enum E_servo
	{
		e_servo_direccion = 0,
		e_servo_rueda_derecha = 0,
		e_servo_rueda_izquierda = 1,
		e_servo_marcha = 2,
		e_servo_motor = 3
	};

	enum E_modo
	{
		e_modo_manual = 0,
		e_modo_sistema_salida = 1,
		e_modo_semi_auto = 2,
		e_modo_full_auto = 3
	};

	enum E_tipo_control
	{
		e_control_RC = 0,
		e_control_BT = 1
	};

	enum E_regulacion_potencia
	{
		e_reg_pot_incremental = 0,
		e_reg_pot_max = 1,
		e_reg_pot_agresiva = 2
	};

	enum E_regulacion_direccion
	{
		e_reg_dir_directa = 0,
		e_reg_dir_Ackermann = 1
	};

	enum E_canal_rc
	{
		e_ch_rc_canal_1 = 0,
		e_ch_rc_canal_2,
		e_ch_rc_canal_3,
		e_ch_rc_canal_4,
		e_ch_rc_canal_5,
		e_ch_rc_canal_6
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
	bool Cambiar_modo(int nuevo_modo);

	float angulos[4];
	float pitch, roll, yaw;
	float rpmActual;

	float consigna[4];		   //consigna enviada
	float consigna_manual[4];  //consigna de control BT
	float consigna_rc[6];	  //consigna de entrada
	float consigna_control[4]; //consigna de entrada

	int64_t last_manual_received;
	int64_t last_rc_received;

	float AnguloRuedaDerecha;
	float AnguloRuedaIzquierda;
	int giro_val[3];
	int aceleracion[3];
	float imu_temp;
	uint8_t tipo_regulacion_potencia;  //algoritmo de regulacion de potencia
	uint8_t tipo_regulacion_direccion; //algoritmo de regulacion de direccion
	uint8_t modo_motor_actual;		   // tipo de control manual, semi, auto, y sistema de salida
	uint8_t tipo_control_actual;	   //procedencia de control
	float ESC_Dutycycle, ESC_avgInputCurrent, ESC_avgMotorCurrent, ESC_rpmActual, ESC_VoltajeEntrada, ESC_mah, ESC_temp;
	float bar_temperatura, bar_presion;
	float ina_shuntvoltage,ina_busvoltage,ina_current_mA,ina_loadvoltage,ina_power_mW;


	float arranque_aceleracion_fase_1, arranque_tmax_fase_1, arranque_Vmax_fase_1;
	float arranque_aceleracion_fase_2, arranque_tmax_fase_2, arranque_Vmax_fase_2;
	float arranque_aceleracion_fase_3, arranque_tmax_fase_3, arranque_Vmax_fase_3;

  private:
	int estado_arranque;
	int estado_auto;
	int estado_semi_auto;
	bool none;

	void modo_semi_auto();
	void modo_auto();
	void calculateAckerman();

	int64_t t_arranque;

	/**
		 * @brief      Calculo de consigna del motor para el modo salida
		 */
	void Modo_salida();
};

#endif
