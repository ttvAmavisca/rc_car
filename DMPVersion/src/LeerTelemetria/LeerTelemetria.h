#ifndef _LeerTelemetria_h
#define _LeerTelemetria_h

#include <Arduino.h>


class LeerTelemetria
{
	
	public:
		/**
		 * @brief      Class constructor
		 */
		LeerTelemetria(void);

		/**
		 * @brief      Set the serial port for uart communication
		 * @param      port  - Reference to Serial port (pointer) 
		 */
		void setSerialPort(HardwareSerial* port);

		void processData();
		
		int16_t temperatura; // Temperature, Voltage, Current, used mAh, eRpM
		int16_t Voltage;
		int16_t Current;
		int16_t mAh;
		int16_t eRpM;
	private: 

		/** Variabel to hold the reference to the Serial object to use for UART */
		HardwareSerial* serialPort = NULL;

		uint8_t update_crc8(uint8_t crc, uint8_t crc_seed);
		uint8_t get_crc8(uint8_t *Buf, uint8_t BufLen);

		uint16_t b_counter = 0;
		uint16_t endMessage = 256;
		bool new_messageRead = false;
		bool message_error = false;
		uint8_t messageReceived[256];
		uint16_t lenPayload = 0;
		uint64_t timeEnd= 0;

		
};

#endif
