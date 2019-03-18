#ifndef _telemetria_h
#define _telemetria_h

#include <Arduino.h>
#include "../MPU9250_DMP/MPU9250-DMP.h"
#include "../rc_car_obj/Rc_car.h"
#include "../Configuracion/Configuracion.h"
#include "../BMP280/BMP280.h"

//enum ComandosBluetooth : int { Iniciar_Regulacion = 1, Parar_Regulacion, peticion_calibracion, Valores_Calibracion, Mover_Posicion, Cambiar_Tipo_Regula, PedirEstado };
/*
		#define COMANDOS_BLUETOOTH_ENVIO_PARAMETROS 1
		#define COMANDOS_BLUETOOTH_ENVIO_IMU 2
		#define COMANDOS_BLUETOOTH_PEDIR_DATOS_COCHE 3
		#define COMANDOS_BLUETOOTH_PEDIR_DATOS_IMU 4
		#define COMANDOS_BLUETOOTH_PETICION_CALIBRA 5
		#define COMANDOS_BLUETOOTH_VALORES_CALIBRA 6
		#define COMANDOS_BLUETOOTH_CAMBIAR_MODO 7
		#define COMANDOS_BLUETOOTH_CONTROL_REMOTO 8
		#define COMANDOS_BLUETOOTH_PEDIR_ESTADO 9
		#define COMANDOS_BLUETOOTH_PEDIR_VALORESMANUAL 10
		#define COMANDOS_BLUETOOTH_NUEVOS_VALORESMANUAL 11

		#define RESPUESTAS_BLUETOOTH_AUTO_PARAMETROS 1
		#define RESPUESTAS_BLUETOOTH_AUTO_IMU 2
		#define RESPUESTAS_BLUETOOTH_DATOS_COCHE 3
		#define RESPUESTAS_BLUETOOTH_DATOS_IMU 4
		#define RESPUESTAS_BLUETOOTH_VALORES_CALIBRA 5
		#define RESPUESTAS_BLUETOOTH_VALORES_CAMBIADOS 6
		#define RESPUESTAS_BLUETOOTH_MODO 7
		#define RESPUESTAS_BLUETOOTH_CONTROL_REMOTO 8
		#define RESPUESTAS_BLUETOOTH_ESTADO 9
		#define RESPUESTAS_BLUETOOTH_MSG 10
		#define RESPUESTAS_BLUETOOTH_VALORESMANUAL 11
*/

#define LENGH_RESPUESTAS_BLUETOOTH_DATOS_COCHE 26
#define LENGH_RESPUESTAS_BLUETOOTH_DATOS_IMU 65
#define LENGH_RESPUESTAS_BLUETOOTH_VALORES_CALIBRA 63
#define LENGH_RESPUESTAS_BLUETOOTH_VALORESMANUAL 13
#define LENGH_RESPUESTAS_BLUETOOTH_ESTADO 9

#define LONGITUD_BUFFER_BLUETOOTH 80

class Telemetria
{

  public:
	/**
		 * @brief      Class constructor
		 */
	Telemetria(void);

	enum ComandosBluetooth : int
	{
		e_com_ping = 1,
		e_com_auto_parametros,
		e_com_auto_imu,
		e_com_peticion_coche,
		e_com_peticion_imu,
		e_com_peticion_calibracion,
		e_com_nueva_calibracion,
		e_com_cambiar_modo,
		e_com_control_remoto,
		e_com_PedirEstado,
		e_com_pedir_valoresmanual,
		e_com_nuevos_valoresmanual
	};

	enum RespuestasBluetooth : int
	{
		e_res_pong = 1,
		e_res_auto_parametros,
		e_res_auto_imu,
		e_res_datos_coche,
		e_res_datosn_imu,
		e_res_valores_calibracion,
		e_res_cambio_ok,
		e_res_cambiar_modo,
		e_res_control_remoto,
		e_res_estado,
		e_res_msg,
		e_res_valores_manual
	};

	/**
		 * @brief      Set the serial port for uart communication
		 * @param      port  - Reference to Serial port (pointer) 
		 */
	void setSerialPort(Stream *port);
	void setImu(MPU9250_DMP *imu_obj);
	void setCar(Rc_car *p_rc_car);
	void setConfig(Configuracion *p_rc_Configuracion);
	void setBar(BMP280 *p_rc_car);
	void setObjects(Stream *port, MPU9250_DMP *imu_obj, Rc_car *p_rc_car, Configuracion *p_rc_Configuracion, BMP280 *p_bar);

	void enviarDatosCoche();
	void enviarDatosEstado();
	void enviarDatosImu();

	void enviarConfiguracionActual();
	void enviarDatosManual();
	void serialEvent2();
	void NuevosValoresImu();
	void NuevosValoresBar();
	void setDebug(bool estado);
	void autoTelemetria();

  private:
	Stream *serialPort = NULL;

	bool combrobarFinCadena();
	int reconocerComando();
	void enviarMensaje(uint8_t idMensaje);
	void pong();
	void procesaComando();

	MPU9250_DMP *imu;
	Rc_car *rc_car;
	Configuracion *rc_Configuracion;
	BMP280 *barometro;

	bool tel_Debug;

	// ================================================================
	// ======             Variables Comunicaciones               ======
	// ================================================================

	uint8_t datosRecibidosSerial[LONGITUD_BUFFER_BLUETOOTH];
	byte bytesRecibidos;
	bool conexionCorrecta;
	bool nuevoComando;
	bool envio_parametros_coche;
	bool envio_parametros_imu;
	uint64_t _timAutoTelemetria_coche;
	uint64_t _timAutoTelemetria_imu;
};

#endif
