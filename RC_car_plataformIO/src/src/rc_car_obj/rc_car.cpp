#include "rc_car.h"


Rc_car::Rc_car(void){
	angulos[0]=1;
    angulos[1]=2;
    angulos[2]=3;
    angulos[3]=4;
    pitch=5;
    roll=6;
    yaw=7;
    rpmActual=8;
    
    
    velocidad[0]=13;
    velocidad[1]=14;
    velocidad[2]=15;
    aceleracion[0]=16;
    aceleracion[1]=17;
    aceleracion[2]=18;
    tipoControl=19;
   
    ESC_Dutycycle=21;
    ESC_avgInputCurrent=22;
    ESC_avgMotorCurrent=23;
    ESC_rpmActual=24;
    ESC_VoltajeEntrada=25;

    consigna[enum_rueda_derecha]=9;
    consigna[enum_rueda_izquierda]=10;
    consigna[enum_marcha]=11;
    consigna[enum_motor]=12;

    consigna_manual[enum_rueda_derecha]=26;
    consigna_manual[enum_rueda_izquierda]=27;
    consigna_manual[enum_marcha]=28;
    consigna_manual[enum_motor]=29;

    consigna_rc[enum_rueda_derecha]=26;
    consigna_rc[enum_rueda_izquierda]=27;
    consigna_rc[enum_marcha]=28;
    consigna_rc[enum_motor]=29;

    estado_arranque=0;
    estado_auto=0;
	estado_semi_auto=0;

  arranque_aceleracion_fase_1 =1;
  arranque_tmax_fase_1 =1000000;
  arranque_Vmax_fase_1 =59;
	arranque_aceleracion_fase_2 =1;
  arranque_tmax_fase_2 =1000000;
  arranque_Vmax_fase_2 =59;
	arranque_aceleracion_fase_3 =1;
  arranque_tmax_fase_3 =1000000;
  arranque_Vmax_fase_3 =1;

}

void Rc_car::calcular_consignas()
{

  //motor
  switch (modo_motor_actual)
  {
  case Rc_car::enum_manual:
    consigna[Rc_car::enum_rueda_derecha] = 1000+  consigna_manual[Rc_car::enum_rueda_derecha]/10;
    consigna[Rc_car::enum_rueda_izquierda] = 1000  +  consigna_manual[Rc_car::enum_rueda_izquierda]/10;
    consigna[Rc_car::enum_marcha] =1000 +  consigna_manual[Rc_car::enum_marcha]/10;
    consigna[Rc_car::enum_motor] = 1000 + consigna_manual[Rc_car::enum_motor]/10 ;
    break;
  case Rc_car::enum_sistema_salida:
    consigna[Rc_car::enum_rueda_derecha] = consigna_rc[Rc_car::enum_rc_canal_1];
    consigna[Rc_car::enum_rueda_izquierda] = consigna_rc[Rc_car::enum_rc_canal_1];
    Modo_salida();
    break;
  case Rc_car::enum_semi_auto:
    consigna[Rc_car::enum_rueda_derecha] = consigna_rc[Rc_car::enum_rc_canal_1];
    consigna[Rc_car::enum_rueda_izquierda] = consigna_rc[Rc_car::enum_rc_canal_1];
    consigna[Rc_car::enum_marcha] = consigna_rc[Rc_car::enum_rc_canal_3];
    consigna[Rc_car::enum_motor] = consigna_rc[Rc_car::enum_rc_canal_2];
    break;
  case Rc_car::enum_full_auto:
    consigna[Rc_car::enum_rueda_derecha] = consigna_rc[Rc_car::enum_rc_canal_1];
    consigna[Rc_car::enum_rueda_izquierda] = consigna_rc[Rc_car::enum_rc_canal_1];
    consigna[Rc_car::enum_marcha] = consigna_rc[Rc_car::enum_rc_canal_3];
    consigna[Rc_car::enum_motor] = consigna_rc[Rc_car::enum_rc_canal_2];
    break;
  }
}


bool Rc_car::Cambiar_modo(int nuevo_modo )
{

  switch (modo_motor_actual)
  {
  case Rc_car::enum_manual:
    
    break;
  case Rc_car::enum_sistema_salida:
    
    break;
  case Rc_car::enum_semi_auto:
    
    break;
  case Rc_car::enum_full_auto:
    
    break;
  }
  
  modo_motor_actual =nuevo_modo;
  return true;
}

void Rc_car::Modo_salida()
{
  int64_t tiempo_Fase;

  //motor
  switch (estado_arranque)
  {

    //si se suelta el acelerador parar
    if (consigna_rc[Rc_car::enum_rc_canal_2] < 0.1){
        estado_arranque =0; //cambiar a etapa 1 al detectar acelerador
    }
    

      //estado inicial
  case 0:
    //Esperando acelerador
    consigna[Rc_car::enum_marcha] = -100.0;
    consigna[Rc_car::enum_motor] = 0.0;
    if (consigna_rc[Rc_car::enum_rc_canal_2] > 0.5) {
        estado_arranque =1; //cambiar a etapa 1 al detectar acelerador
        t_arranque=esp_timer_get_time();
    }
    break;
  case 1:
    consigna[Rc_car::enum_marcha] = 0.0; //enum_marcha
   
     tiempo_Fase = esp_timer_get_time() -t_arranque;
    consigna[Rc_car::enum_motor] += (tiempo_Fase /1000000.f) * arranque_aceleracion_fase_1 ;

    
    if ( ESC_rpmActual > arranque_Vmax_fase_1 || tiempo_Fase > arranque_tmax_fase_1) {
        estado_arranque =2; //cambiar a etapa 
        t_arranque=esp_timer_get_time();
    }

    break;
  case 2:
    consigna[Rc_car::enum_marcha] = 100.0; 
   
     tiempo_Fase = esp_timer_get_time() -t_arranque;
    consigna[Rc_car::enum_motor] += (tiempo_Fase /1000000.f) * arranque_aceleracion_fase_2 ;

    
    if ( ESC_rpmActual > arranque_Vmax_fase_2 || tiempo_Fase > arranque_tmax_fase_2) {
        estado_arranque =3; //cambiar a etapa 
        t_arranque=esp_timer_get_time();
    }
    break;
  case 3:
    consigna[Rc_car::enum_marcha] = 100.0; 
   
     tiempo_Fase = esp_timer_get_time() -t_arranque;
    consigna[Rc_car::enum_motor] += (tiempo_Fase /1000000.f) * arranque_aceleracion_fase_3 ;

    
    if ( ESC_rpmActual > arranque_Vmax_fase_3 || tiempo_Fase > arranque_tmax_fase_3) {
        Cambiar_modo(Rc_car::enum_semi_auto);
    }
    break;
    
  }
}

