#include "LeerTelemetriaBlHeli.h"
#include <HardwareSerial.h>

LeerTelemetriaBlHeli::LeerTelemetriaBlHeli(void){
	temperatura =0; 
	Voltage =0; 
	Current =0; 
	mAh =0; 
	eRpM =0; 
	b_counter=0;
	timeEnd=0;
}

void LeerTelemetriaBlHeli::setSerialPort(HardwareSerial* port)
{
	serialPort = port;
}



void LeerTelemetriaBlHeli::processData() {
	//Nota: para recibir telemetria, habilitar la auto telemetria o enviar PWM de 30us
    new_messageRead=false;
	while (serialPort->available() &&( ! new_messageRead)) {
			messageReceived[b_counter++] = serialPort->read();

			if (b_counter >= 10) {

			 uint8_t crc8 = get_crc8(messageReceived, 9); // get the 8 bit CRC
      
			  temperatura = messageReceived[0]; // temperature
			  Voltage = ((int16_t)messageReceived[1]<<8)|messageReceived[2]; // voltage *100
			  Current = ((int16_t)messageReceived[3]<<8)|messageReceived[4]; // Current *100
			  mAh = ((int16_t)messageReceived[5]<<8)|messageReceived[6]; // used mA/h   
			  eRpM = (((int16_t)messageReceived[7]<<8)|messageReceived[8]); // eRpM /100
					
			//Serial.print("blheli ");Serial.print(Voltage); Serial.print("V ");Serial.print(Current); Serial.print("A ");Serial.print(eRpM);Serial.print("RPM ");
			 if(crc8 != messageReceived[9]) message_error=true;
				new_messageRead = true;
				b_counter=0;
				timeEnd=esp_timer_get_time()+100000;
				
			}
		}
		
		if (esp_timer_get_time() > timeEnd && b_counter > 0) {
	
			message_error=true;
			b_counter=0;
			timeEnd=esp_timer_get_time()+100000;
		}
		
}

// 8-Bit CRC
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
uint8_t LeerTelemetriaBlHeli::update_crc8(uint8_t crc, uint8_t crc_seed){
	uint8_t crc_u, i;
	crc_u = crc;
	crc_u ^= crc_seed;
	for ( i=0; i<8; i++) crc_u = ( crc_u & 0x80 ) ? 0x7 ^ ( crc_u << 1 ) : ( crc_u << 1 );
	return (crc_u);
}

uint8_t LeerTelemetriaBlHeli::get_crc8(uint8_t *Buf, uint8_t BufLen){
	uint8_t crc = 0, i;
	for( i=0; i<BufLen; i++) crc = update_crc8(Buf[i], crc);
	return (crc);
}