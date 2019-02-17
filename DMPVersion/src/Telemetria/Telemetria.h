#ifndef _telemetria_h
#define _telemetria_h

#include <Arduino.h>
#include "../MPU9250_DMP/MPU9250-DMP.h"
#include "../rc_car_obj/Rc_car.h"
#include "../Configuracion/Configuracion.h"


//enum ComandosBluetooth : int { Iniciar_Regulacion = 1, Parar_Regulacion, peticion_calibracion, Valores_Calibracion, Mover_Posicion, Cambiar_Tipo_Regula, PedirEstado };
		#define COMANDOS_BLUETOOTH_INICIAR 1
		#define COMANDOS_BLUETOOTH_PARAR 2
		#define COMANDOS_BLUETOOTH_PETICION_CALIBRA 3
		#define COMANDOS_BLUETOOTH_VALORES_CALIBRA 4
		#define COMANDOS_BLUETOOTH_MOVER_POSICION 5
		#define COMANDOS_BLUETOOTH_CAMBIAR_TIPO_REGULA 5
		#define COMANDOS_BLUETOOTH_PEDIR_ESTADO 7
		
		
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

		void enviarDatosBluetooth();
		
		void enviarConfiguracionActual();
		void serialEvent2();
		void NuevosValoresImu();
		void setDebug(bool estado);
		
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

		
		
};

#endif
