#ifndef _Configuracion_h
#define _Configuracion_h

#include <Arduino.h>
#include <EEPROM.h>


struct ConfiguracionDispositivo
{
	uint8_t versionConfig_H;  // version de la configuracion
	uint8_t versionConfig_L; // version de la configuracion
	float C1;
	float C2;
	float C3;
	float C4;
	float C5;
	float C6;
	float C7;
	float C8;
	float C9;
	float C10;
	float C11;
	float C12;
	float C13;
	float C14;
};
	
class Configuracion
{
	
	//******************************* VERSION *********************************
	const int DIRECCION_CONFIG = 0;
	


	//******************************* Datos *********************************
	public:
	
		const uint8_t VERSION_H = 1; // version del programa
		const uint8_t VERSION_L =5;  // version del programa
		
		ConfiguracionDispositivo configActual;
	

	
		/**
		 * @brief      Class constructor
		 */
		Configuracion(void);


	
		/**
		 * @brief      Recupera configuracion de la EEPROM
		 *
		 *
		 */
		bool getFromEEPROM(void);

		/**
		 * @brief      Guarda configuracion en la EEPROM
		 */
		void saveToEEPROM(void);

		

	private: 


		
};

#endif
