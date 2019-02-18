#ifndef _telemetria_h
#define _telemetria_h

#include <Arduino.h>
#include "../MPU9250_DMP/MPU9250-DMP.h"
#include "../rc_car_obj/Rc_car.h"
#include "../Configuracion/Configuracion.h"


//enum ComandosBluetooth : int { Iniciar_Regulacion = 1, Parar_Regulacion, peticion_calibracion, Valores_Calibracion, Mover_Posicion, Cambiar_Tipo_Regula, PedirEstado };
		#define COMANDOS_BLUETOOTH_ENVIO_PARAMETROS 1
		#define COMANDOS_BLUETOOTH_ENVIO_IMU 2
		#define COMANDOS_BLUETOOTH_PEDIR_DATOS_COCHE 3
		#define COMANDOS_BLUETOOTH_PEDIR_DATOS_IMU 4
		#define COMANDOS_BLUETOOTH_PETICION_CALIBRA 5
		#define COMANDOS_BLUETOOTH_VALORES_CALIBRA 6
		#define COMANDOS_BLUETOOTH_CAMBIAR_MODO 7
		#define COMANDOS_BLUETOOTH_CONTROL_REMOTO 8
		#define COMANDOS_BLUETOOTH_PEDIR_ESTADO 9


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
		
		#define LONGITUD_BUFFER_BLUETOOTH 25  
		
class Telemetria
{
	


	public:
		/**
		 * @brief      Class constructor
		 */
		Telemetria(void);


		/**
		 * @brief      Set the serial port for uart communication
		 * @param      port  - Reference to Serial port (pointer) 
		 */
		void setSerialPort(Stream* port);
		void setImu(MPU9250_DMP * imu_obj);
		void setCar(Rc_car * p_rc_car);
		void setConfig(Configuracion * p_rc_Configuracion);

		void enviarDatosCoche();
		void enviarDatosImu();
		
		void enviarConfiguracionActual();
		void serialEvent2();
		void NuevosValoresImu();
		void setDebug(bool estado);
		void autoTelemetria();
		
	private: 

		Stream* serialPort = NULL;
        
		bool combrobarFinCadena();
		int reconocerComando();
		void enviarMensaje(uint8_t idMensaje);
		void procesaComando();
		float angulos[4];
		float pitch, roll, yaw;
		float rpmActual;
		float ConsignaRPMActual;
		float ConsignaDireccionActual;
		float AnguloRuedaDerecha;
		float AnguloRuedaIzquierda;
		float velocidad[3];
		float aceleracion[3];
		uint8_t tipoControl;
		uint8_t modo_motor_actual;
		float  ESC_Dutycycle, ESC_avgInputCurrent,ESC_avgMotorCurrent,ESC_rpmActual,ESC_VoltajeEntrada;
 
		
		MPU9250_DMP * imu;
		Rc_car * rc_car;
		Configuracion * rc_Configuracion;
		
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
		
		
};

#endif
