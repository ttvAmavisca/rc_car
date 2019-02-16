#include "LeerTelemetria.h"
#include <HardwareSerial.h>

LeerTelemetria::LeerTelemetria(void){
	temperatura =0; 
	Voltage =0; 
	Current =0; 
	mAh =0; 
	eRpM =0; 
	b_counter=0;
}

void LeerTelemetria::setSerialPort(HardwareSerial* port)
{
	serialPort = port;
}



void LeerTelemetria::processData() {

	// Messages > 255 starts with "3" 2nd and 3rd byte is length combined with 1st >>8 and then &0xFF

	if (serialPort->available()) {

			messageReceived[b_counter++] = serialPort->read();

			if (b_counter >= 10) {

			 uint8_t crc8 = get_crc8(messageReceived, 9); // get the 8 bit CRC
      
			  temperatura = messageReceived[0]; // temperature
			  Voltage = (messageReceived[1]<<8)|messageReceived[2]; // voltage
			  Current = (messageReceived[3]<<8)|messageReceived[4]; // Current
			  mAh = (messageReceived[5]<<8)|messageReceived[6]; // used mA/h
			  eRpM = (messageReceived[7]<<8)|messageReceived[8]; // eRpM *100
					

			 if(crc8 != messageReceived[9]) message_error=true;
			
			 new_messageRead = true;
				
			}
		}
}

// 8-Bit CRC
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
uint8_t LeerTelemetria::update_crc8(uint8_t crc, uint8_t crc_seed){
	uint8_t crc_u, i;
	crc_u = crc;
	crc_u ^= crc_seed;
	for ( i=0; i<8; i++) crc_u = ( crc_u & 0x80 ) ? 0x7 ^ ( crc_u << 1 ) : ( crc_u << 1 );
	return (crc_u);
}

uint8_t LeerTelemetria::get_crc8(uint8_t *Buf, uint8_t BufLen){
	uint8_t crc = 0, i;
	for( i=0; i<BufLen; i++) crc = update_crc8(Buf[i], crc);
	return (crc);
}