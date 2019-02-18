#include "VescUart.h"
#include <HardwareSerial.h>

VescUart::VescUart(void){
	nunchuck.valueX         = 127;
	nunchuck.valueY         = 127;
	nunchuck.lowerButton  	= false;
	nunchuck.upperButton  	= false;
}

void VescUart::setSerialPort(HardwareSerial* port)
{
	serialPort = port;
}

void VescUart::setDebugPort(Stream* port)
{
	debugPort = port;
}


//Nota: reescrita completamenete para recepcion asincrona 40 veces mas rapido
int VescUart::receiveUartMessage(uint8_t * payloadReceived) {

	// Messages <= 255 starts with "2", 2nd byte is length
	// Messages > 255 starts with "3" 2nd and 3rd byte is length combined with 1st >>8 and then &0xFF

	if (serialPort->available()) {

			messageReceived[b_counter++] = serialPort->read();

			if (b_counter == 2) {

				switch (messageReceived[0])
				{
					case 2:
						if (debugPort != NULL) {
							debugPort->println("Msg reciebe start");
						}
						endMessage = messageReceived[1] + 5; //Payload size + 2 for sice + 3 for SRC and End.
						lenPayload = messageReceived[1];
					break;

					case 3:
						message_error=true;
						// ToDo: Add Message Handling > 255 (starting with 3)
						if( debugPort != NULL ){
							debugPort->println("Message is larger than 256 bytes - not supported");
						}
					break;

					default:
						if( debugPort != NULL ){
							debugPort->println("Unvalid start bit");
						}
					break;
				}
			}

			if (b_counter >= sizeof(messageReceived)) {
				message_error=true;
				
			}

			if (b_counter == endMessage && messageReceived[endMessage - 1] == 3) {
				messageReceived[endMessage] = 0;
				if (debugPort != NULL) {
					debugPort->println("End of message reached!");
				}
				new_messageRead = true;
				
			}
		}
	
	
	if (esp_timer_get_time() > timeEnd) { //timeout
		if (debugPort != NULL) {
			debugPort->println("timeoout");
		}
		message_error=true;
	}
	
	bool unpacked = false;

	if (new_messageRead) {
		unpacked = unpackPayload(messageReceived, endMessage, payloadReceived);
	}

	if (unpacked) {
		// Message was read
		return lenPayload; 
	}
	else {
		// No Message Read
		return 0;
	}
}


bool VescUart::unpackPayload(uint8_t * message, int lenMes, uint8_t * payload) {

	uint16_t crcMessage = 0;
	uint16_t crcPayload = 0;

	// Rebuild crc:
	crcMessage = message[lenMes - 3] << 8;
	crcMessage &= 0xFF00;
	crcMessage += message[lenMes - 2];

	if(debugPort!=NULL){
		debugPort->print("SRC received: "); debugPort->println(crcMessage);
	}

	// Extract payload:
	memcpy(payload, &message[2], message[1]);

	crcPayload = crc16(payload, message[1]);

	if( debugPort != NULL ){
		debugPort->print("SRC calc: "); debugPort->println(crcPayload);
	}
	
	if (crcPayload == crcMessage) {
		if( debugPort != NULL ) {
			debugPort->print("Received: "); 
			serialPrint(message, lenMes); debugPort->println();

			debugPort->print("Payload :      ");
			serialPrint(payload, message[1] - 1); debugPort->println();
		}

		return true;
	}else{
		return false;
	}
}


int VescUart::packSendPayload(uint8_t * payload, int lenPay) {

	uint16_t crcPayload = crc16(payload, lenPay);
	int count = 0;
	uint8_t messageSend[256];

	if (lenPay <= 256)
	{
		messageSend[count++] = 2;
		messageSend[count++] = lenPay;
	}
	else
	{
		messageSend[count++] = 3;
		messageSend[count++] = (uint8_t)(lenPay >> 8);
		messageSend[count++] = (uint8_t)(lenPay & 0xFF);
	}

	memcpy(&messageSend[count], payload, lenPay);

	count += lenPay;
	messageSend[count++] = (uint8_t)(crcPayload >> 8);
	messageSend[count++] = (uint8_t)(crcPayload & 0xFF);
	messageSend[count++] = 3;
	messageSend[count] = '\0';

	if(debugPort!=NULL){
		debugPort->print("UART package send: "); serialPrint(messageSend, count);
	}

	// Sending package
	serialPort->write(messageSend, count);

	// Returns number of send bytes
	return count;
}


bool VescUart::processReadPacket(uint8_t * message) {

	COMM_PACKET_ID packetId;
	int32_t ind = 0;

	packetId = (COMM_PACKET_ID)message[0];
	message++; // Removes the packetId from the actual message (payload)

	switch (packetId){
		case COMM_GET_VALUES: // Structure defined here: https://github.com/vedderb/bldc/blob/43c3bbaf91f5052a35b75c2ff17b5fe99fad94d1/commands.c#L164

			ind = 4; // Skip the first 4 bytes 
			data.avgMotorCurrent 	= buffer_get_float32(message, 100.0, &ind);
			data.avgInputCurrent 	= buffer_get_float32(message, 100.0, &ind);
			ind += 8; // Skip the next 8 bytes
			data.dutyCycleNow 		= buffer_get_float16(message, 1000.0, &ind);
			data.rpm 				= buffer_get_int32(message, &ind);
			data.inpVoltage 		= buffer_get_float16(message, 10.0, &ind);
			data.ampHours 			= buffer_get_float32(message, 10000.0, &ind);
			data.ampHoursCharged 	= buffer_get_float32(message, 10000.0, &ind);
			ind += 8; // Skip the next 8 bytes 
			data.tachometer 		= buffer_get_int32(message, &ind);
			data.tachometerAbs 		= buffer_get_int32(message, &ind);
			return true;

		break;

		default:
			return false;
		break;
	}
}

bool VescUart::getVescValues(void) {

	uint8_t command[1] = { COMM_GET_VALUES };
	uint8_t payload[256];
	
    if (message_error || new_messageRead) {
		b_counter = 0;
		endMessage = 256;
		new_messageRead = false;
		message_error = false;
		lenPayload = 0;
		timeEnd=esp_timer_get_time()+ 100000; //100ms
	
		if (debugPort != NULL) {
			debugPort->println("Send request");
		}
		packSendPayload(command, 1);
	}
	

	int lenPayload = receiveUartMessage(payload);

	if (lenPayload > 55) {
		bool read = processReadPacket(payload); //returns true if sucessful
		return read;
	}
	else
	{
		return false;
	}
}

void VescUart::setNunchuckValues() {
	int32_t ind = 0;
	uint8_t payload[11];

	
	if (esp_timer_get_time() < timSend4)  return;
	
	timSend4 =esp_timer_get_time() + sendinterValMax4;
	
	payload[ind++] = COMM_SET_CHUCK_DATA;
	payload[ind++] = nunchuck.valueX;
	payload[ind++] = nunchuck.valueY;
	buffer_append_bool(payload, nunchuck.lowerButton, &ind);
	buffer_append_bool(payload, nunchuck.upperButton, &ind);
	
	// Acceleration Data. Not used, Int16 (2 byte)
	payload[ind++] = 0;
	payload[ind++] = 0;
	payload[ind++] = 0;
	payload[ind++] = 0;
	payload[ind++] = 0;
	payload[ind++] = 0;

	if(debugPort != NULL){
		debugPort->println("Data reached at setNunchuckValues:");
		debugPort->print("valueX = "); debugPort->print(nunchuck.valueX); debugPort->print(" valueY = "); debugPort->println(nunchuck.valueY);
		debugPort->print("LowerButton = "); debugPort->print(nunchuck.lowerButton); debugPort->print(" UpperButton = "); debugPort->println(nunchuck.upperButton);
	}

	packSendPayload(payload, 11);
}

void VescUart::setCurrent(float current) {
	int32_t index = 0;
	uint8_t payload[5];
	
	if (esp_timer_get_time() < timSend3)  return;
	
	timSend3 =esp_timer_get_time() + sendinterValMax3;

	payload[index++] = COMM_SET_CURRENT;
	buffer_append_int32(payload, (int32_t)(current * 1000), &index);
	

	packSendPayload(payload, 5);
}

void VescUart::setBrakeCurrent(float brakeCurrent) {
	int32_t index = 0;
	uint8_t payload[5];

	if (esp_timer_get_time() < timSend5)  return;
	
	timSend5 =esp_timer_get_time() + sendinterValMax5;
	
	payload[index++] = COMM_SET_CURRENT_BRAKE;
	buffer_append_int32(payload, (int32_t)(brakeCurrent * 1000), &index);

	packSendPayload(payload, 5);
}

void VescUart::setRPM(float rpm) {
	int32_t index = 0;
	uint8_t payload[5];
	
	if (esp_timer_get_time() < timSend2)  return;
	
	timSend2 =esp_timer_get_time() + sendinterValMax2;

	payload[index++] = COMM_SET_RPM ;
	buffer_append_int32(payload, (int32_t)(rpm), &index);

	packSendPayload(payload, 5);
}

void VescUart::setDuty(float duty) {
	int32_t index = 0;
	uint8_t payload[5];

	if (esp_timer_get_time() < timSend1)  return;
	
	timSend1 =esp_timer_get_time() + sendinterValMax1;
	payload[index++] = COMM_SET_DUTY;
	buffer_append_int32(payload, (int32_t)(duty * 100000), &index);

	packSendPayload(payload, 5);
}

void VescUart::serialPrint(uint8_t * data, int len) {
	if(debugPort != NULL){
		for (int i = 0; i <= len; i++)
		{
			debugPort->print(data[i]);
			debugPort->print(" ");
		}

		debugPort->println("");
	}
}

void VescUart::printVescValues() {
	if(debugPort != NULL){
		debugPort->print("avgMotorCurrent: "); 	debugPort->println(data.avgMotorCurrent);
		debugPort->print("avgInputCurrent: "); 	debugPort->println(data.avgInputCurrent);
		debugPort->print("dutyCycleNow: "); 	debugPort->println(data.dutyCycleNow);
		debugPort->print("rpm: "); 				debugPort->println(data.rpm);
		debugPort->print("inputVoltage: "); 	debugPort->println(data.inpVoltage);
		debugPort->print("ampHours: "); 		debugPort->println(data.ampHours);
		debugPort->print("ampHoursCharges: "); 	debugPort->println(data.ampHoursCharged);
		debugPort->print("tachometer: "); 		debugPort->println(data.tachometer);
		debugPort->print("tachometerAbs: "); 	debugPort->println(data.tachometerAbs);
	}
}