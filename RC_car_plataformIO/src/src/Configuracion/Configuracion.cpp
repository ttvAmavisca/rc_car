#include "Configuracion.h"


Configuracion::Configuracion(void){
	
}


bool Configuracion::getFromEEPROM(void){
	
	EEPROM.begin(EEPROM_SIZE);
	EEPROM.get(DIRECCION_CONFIG, configActual );
  

  if (configActual.versionConfig_H !=VERSION_H || configActual.versionConfig_L !=VERSION_L)
  {
    //lectura inicial, o cambio de version guardar valores

    //Config de inicializacion, solo en cambios de version o inicial
    configActual.C1=31.6812;    // 
    configActual.C2=-5.9878;   // 
    configActual.C3=-0.2408;   // 
    configActual.C4=0.0;   // 
    configActual.C5=0.0;   // 
    configActual.C6=0.0;   // 
    configActual.C7=0.0;   // 
    configActual.C8=0.0;   // 
    configActual.C9=0.0;   // 
    configActual.C10=0.0;   // 
    configActual.C11=0.0;   // 
    configActual.C12=0.0;   // 
    configActual.C13=0.0;   // 
    configActual.C14=0.0;   // 
	
	
    
    configActual.versionConfig_H=VERSION_H;
    configActual.versionConfig_L=VERSION_L;
   
    //Config no encontrada, reinicializar
	  saveToEEPROM();
    
  } else {
    //lectura correcta existente
    
  }
}


void Configuracion::saveToEEPROM(void){
	 EEPROM.put(DIRECCION_CONFIG, configActual );
   EEPROM.commit();
}