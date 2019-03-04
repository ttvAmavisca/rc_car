#include "CanalesPwM.h"



CanalesPwM::CanalesPwM(uint8_t reqPin1,uint8_t reqPin2,uint8_t reqPin3,uint8_t reqPin4,uint8_t reqPin5,uint8_t reqPin6) : PIN1(reqPin1), PIN2(reqPin2), PIN3(reqPin3), PIN4(reqPin4), PIN5(reqPin5), PIN6(reqPin6){
	
};

CanalesPwM::~CanalesPwM() {
	
}



	

	
void CanalesPwM::initCanales(){
	rmt_config_t rmt_rx;
    
    rmt_rx.clk_div = 10; //division de reloj del micro. 10 ~ precision de decimas de microsegundo, 100 ~ microsegundos
    rmt_rx.mem_block_num = 1; //memoria interna reserbada, max 8
    rmt_rx.rmt_mode = RMT_MODE_RX;
    rmt_rx.rx_config.filter_en = true;
    rmt_rx.rx_config.filter_ticks_thresh = 100;
    rmt_rx.rx_config.idle_threshold = 50000u;
	
   
   /***** CANAL 1 ******/
    rmt_rx.channel = RMT_CHANNEL_0; //canal(7 disponibles)
    rmt_rx.gpio_num = (gpio_num_t) PIN1; // pin
    rmt_config(&rmt_rx);
	
    rmt_driver_install(rmt_rx.channel, 0, 0);
    rmt_rx_start(RMT_CHANNEL_0, 1);
	
	
	/***** CANAL 2 ******/
	rmt_rx.channel = RMT_CHANNEL_1; //canal(7 disponibles)
    rmt_rx.gpio_num =(gpio_num_t) PIN2; // pin
    rmt_config(&rmt_rx);
	
    rmt_driver_install(rmt_rx.channel, 0, 0);
    rmt_rx_start(RMT_CHANNEL_1, 1);
	
	
	/***** CANAL 3 ******/
	rmt_rx.channel = RMT_CHANNEL_2; //canal(7 disponibles)
    rmt_rx.gpio_num =(gpio_num_t) PIN3; // pin
    rmt_config(&rmt_rx);
	
    rmt_driver_install(rmt_rx.channel, 0, 0);
    rmt_rx_start(RMT_CHANNEL_2, 1);
	
	
	/***** CANAL 4 ******/
	rmt_rx.channel = RMT_CHANNEL_3; //canal(7 disponibles)
    rmt_rx.gpio_num =(gpio_num_t) PIN4; // pin
    rmt_config(&rmt_rx);
	
    rmt_driver_install(rmt_rx.channel, 0, 0);
    rmt_rx_start(RMT_CHANNEL_3, 1);
	
	
	/***** CANAL 5 ******/
	rmt_rx.channel = RMT_CHANNEL_4; //canal(7 disponibles)
    rmt_rx.gpio_num =(gpio_num_t) PIN5; // pin
    rmt_config(&rmt_rx);
	
    rmt_driver_install(rmt_rx.channel, 0, 0);
    rmt_rx_start(RMT_CHANNEL_4, 1);
	
	
	/***** CANAL 6 ******/
	rmt_rx.channel = RMT_CHANNEL_5; //canal(7 disponibles)
    rmt_rx.gpio_num =(gpio_num_t) PIN6; // pin
    rmt_config(&rmt_rx);
	
    rmt_driver_install(rmt_rx.channel, 0, 0);
    rmt_rx_start(RMT_CHANNEL_5, 1);
	
}

uint64_t CanalesPwM::valor(uint8_t canal) {
	
	uint64_t returnvalue=0;
	rmt_item32_t* item;

	//TODO: Leer interrupcion para asegurar que se esta recibiendo nuevos datos
	//if (REG_GET_BIT(RMT_INT_RAW_REG ,RMT_CH0_RX_END_INT_RAW) != 0 ) ; //leer bit de interrupcion de recepcion y borrarlo

	switch(canal){
	case 1:
		item = (rmt_item32_t*) (RMT_CHANNEL_MEM(RMT_CHANNEL_0));
		returnvalue=item->duration0;
		//returnvalue=item->duration1;
		rmt_rx_start(RMT_CHANNEL_0, 1);
		break;
	case 2:
		item = (rmt_item32_t*) (RMT_CHANNEL_MEM(RMT_CHANNEL_1));
		returnvalue=item->duration0;
		//returnvalue=item->duration1;
		rmt_rx_start(RMT_CHANNEL_1, 1);
		break;
	case 3:
		item = (rmt_item32_t*) (RMT_CHANNEL_MEM(RMT_CHANNEL_2));
		returnvalue=item->duration0;
		//returnvalue=item->duration1;
		rmt_rx_start(RMT_CHANNEL_2, 1);
		break;
	case 4:
		item = (rmt_item32_t*) (RMT_CHANNEL_MEM(RMT_CHANNEL_3));
		returnvalue=item->duration0;
		//returnvalue=item->duration1;
		rmt_rx_start(RMT_CHANNEL_3, 1);
		break;
	case 5:
		 item = (rmt_item32_t*) (RMT_CHANNEL_MEM(RMT_CHANNEL_4));
		returnvalue=item->duration0;
		//returnvalue=item->duration1;
		rmt_rx_start(RMT_CHANNEL_4, 1);
		break;
	case 6:
		item = (rmt_item32_t*) (RMT_CHANNEL_MEM(RMT_CHANNEL_5));
		returnvalue=item->duration0;
		//returnvalue=item->duration1;
		rmt_rx_start(RMT_CHANNEL_5, 1);
		break;
	}
	return returnvalue;
}

void CanalesPwM::debugOutSerial(Stream* debugPort) {
	rmt_item32_t* item;
	
	debugPort->print("Canales: 1 (");
	item = (rmt_item32_t*) (RMT_CHANNEL_MEM(RMT_CHANNEL_0));
	rmt_rx_start(RMT_CHANNEL_0, 1);
	debugPort->print((int)item->duration0);
	
	debugPort->print(") 2(");
	item = (rmt_item32_t*) (RMT_CHANNEL_MEM(RMT_CHANNEL_1));
	rmt_rx_start(RMT_CHANNEL_1, 1);
	debugPort->print((int)item->duration0);
	
	debugPort->print(") 3(");
	item = (rmt_item32_t*) (RMT_CHANNEL_MEM(RMT_CHANNEL_2));
	rmt_rx_start(RMT_CHANNEL_2, 1);
	debugPort->print((int)item->duration0);
	
	debugPort->print(") 4(");
	item = (rmt_item32_t*) (RMT_CHANNEL_MEM(RMT_CHANNEL_3));
	rmt_rx_start(RMT_CHANNEL_3, 1);
	debugPort->print((int)item->duration0);
	
	debugPort->print(") 5(");
	item = (rmt_item32_t*) (RMT_CHANNEL_MEM(RMT_CHANNEL_4));
	rmt_rx_start(RMT_CHANNEL_4, 1);
	debugPort->print((int)item->duration0);
	
	debugPort->print(") 6(");
	item = (rmt_item32_t*) (RMT_CHANNEL_MEM(RMT_CHANNEL_5));
	rmt_rx_start(RMT_CHANNEL_5, 1);
	debugPort->print((int)item->duration0);
	debugPort->println(") .");
	
}
