#include "CanalesPwM.h"
#include "esp_attr.h"

uint32_t CanalesPwM::canal_rc[]= {0,0,0,0,0,0,0};
uint64_t CanalesPwM::canal_rc_last[]= {0,0,0,0,0,0,0};

CanalesPwM::CanalesPwM(uint8_t reqPin1,uint8_t reqPin2,uint8_t reqPin3,uint8_t reqPin4,uint8_t reqPin5,uint8_t reqPin6) : PIN1(reqPin1), PIN2(reqPin2), PIN3(reqPin3), PIN4(reqPin4), PIN5(reqPin5), PIN6(reqPin6){
	con_interrupt=false;
	_min=8000;
	_max=16000;
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


void CanalesPwM::initCanales_Interupt(){
	con_interrupt=true;
	
	
	rmt_config_t rmt_rx;
    
    rmt_rx.clk_div = 10; //division de reloj del micro. 10 ~ precision de decimas de microsegundo, 100 ~ microsegundos
    rmt_rx.mem_block_num = 1; //memoria interna reserbada, max 8
    rmt_rx.rmt_mode = RMT_MODE_RX;
    rmt_rx.rx_config.filter_en = true;
    rmt_rx.rx_config.filter_ticks_thresh = 100;
    rmt_rx.rx_config.idle_threshold = 50000u;

	rmt_isr_register(rmt_isr_handler, NULL,0, NULL);
	
   
   /***** CANAL 1 ******/
    rmt_rx.channel = RMT_CHANNEL_0; //canal(7 disponibles)
    rmt_rx.gpio_num = (gpio_num_t) PIN1; // pin
    rmt_config(&rmt_rx);
	
	rmt_set_rx_intr_en(rmt_rx.channel, 1);
    rmt_set_err_intr_en(rmt_rx.channel, 1);
    rmt_rx_start(rmt_rx.channel, 1);
	
	
	/***** CANAL 2 ******/
	rmt_rx.channel = RMT_CHANNEL_1; //canal(7 disponibles)
    rmt_rx.gpio_num =(gpio_num_t) PIN2; // pin
    rmt_config(&rmt_rx);
	
    rmt_set_rx_intr_en(rmt_rx.channel, 1);
    rmt_set_err_intr_en(rmt_rx.channel, 1);
    rmt_rx_start(rmt_rx.channel, 1);
	
	
	/***** CANAL 3 ******/
	rmt_rx.channel = RMT_CHANNEL_2; //canal(7 disponibles)
    rmt_rx.gpio_num =(gpio_num_t) PIN3; // pin
    rmt_config(&rmt_rx);
	
    rmt_set_rx_intr_en(rmt_rx.channel, 1);
    rmt_set_err_intr_en(rmt_rx.channel, 1);
    rmt_rx_start(rmt_rx.channel, 1);
	
	
	/***** CANAL 4 ******/
	rmt_rx.channel = RMT_CHANNEL_3; //canal(7 disponibles)
    rmt_rx.gpio_num =(gpio_num_t) PIN4; // pin
    rmt_config(&rmt_rx);
	
    rmt_set_rx_intr_en(rmt_rx.channel, 1);
    rmt_set_err_intr_en(rmt_rx.channel, 1);
    rmt_rx_start(rmt_rx.channel, 1);
	
	
	/***** CANAL 5 ******/
	rmt_rx.channel = RMT_CHANNEL_4; //canal(7 disponibles)
    rmt_rx.gpio_num =(gpio_num_t) PIN5; // pin
    rmt_config(&rmt_rx);
	
    rmt_set_rx_intr_en(rmt_rx.channel, 1);
    rmt_set_err_intr_en(rmt_rx.channel, 1);
    rmt_rx_start(rmt_rx.channel, 1);
	
	
	/***** CANAL 6 ******/
	rmt_rx.channel = RMT_CHANNEL_5; //canal(7 disponibles)
    rmt_rx.gpio_num =(gpio_num_t) PIN6; // pin
    rmt_config(&rmt_rx);
	
    rmt_set_rx_intr_en(rmt_rx.channel, 1);
    rmt_set_err_intr_en(rmt_rx.channel, 1);
    rmt_rx_start(rmt_rx.channel, 1);
	
}

uint64_t CanalesPwM::valor_int(uint8_t canal) {

	if(esp_timer_get_time()-CanalesPwM::canal_rc_last[canal] > 1000000) return 0; //demasiado tiempo(1s) desde ultimo update
	return CanalesPwM::canal_rc[canal];
}

uint64_t CanalesPwM::valor_noint(uint8_t canal) {
	
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

float CanalesPwM::valor(uint8_t canal) {
	uint64_t valorCanal=0;

	if (con_interrupt){
		canal--;
		valorCanal= valor_int(canal);
	}else{
		valorCanal= valor_noint(canal);
	}
	//Escalado entre -100 y 100
	
	if (valorCanal > 0) return ((valorCanal - _min) * (200.0f) / (_max - _min) - 100.0f);
	return 0; // si <=0 devolver 0
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

//test interrupcion
void IRAM_ATTR CanalesPwM::rmt_isr_handler(void* arg){
  //read RMT interrupt status.
    uint32_t intr_st = RMT.int_st.val;
    uint32_t i = 0;
    uint8_t channel;
	volatile rmt_item32_t* item ;
    
    for(i = 0; i < 32; i++) {
        if(i < 24) {
            if(intr_st & BIT(i)) {
                channel = i / 3;
                
                switch(i % 3) {
                    //TX END
                    case 0:
                        break;
                    //RX_END
                    case 1:
                        RMT.conf_ch[channel].conf1.rx_en = 0;
                        //change memory owner to protect data.
                        RMT.conf_ch[channel].conf1.mem_owner = RMT_MEM_OWNER_TX;
                        
					 	
						//item = RMTMEM.chan[channel].data32;
						item =(rmt_item32_t*) (RMT_CHANNEL_MEM(channel));
						if (item){
							canal_rc[channel]=(item)->duration0;
							canal_rc_last[channel]=esp_timer_get_time();
						}
						
                        RMT.conf_ch[channel].conf1.mem_wr_rst = 1;
                        RMT.conf_ch[channel].conf1.mem_owner = RMT_MEM_OWNER_RX;
                        RMT.conf_ch[channel].conf1.rx_en = 1;
                        break;
                        //ERR
                    case 2:
                        RMT.int_ena.val &= (~(BIT(i)));
                        break;
                    default:
                        break;
                }
                RMT.int_clr.val = BIT(i);
            }
        } else {
            if(intr_st & (BIT(i))) {
                channel = i - 24;
                RMT.int_clr.val = BIT(i);
                    
            }
        }
    }
}
