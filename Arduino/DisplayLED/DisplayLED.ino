//Biblioteca padrão de LCD (16x2)
#include <LiquidCrystal.h>

//String para guardar os dados que chegam pela Serial
String inputString = "";

//Controle de recebimento
boolean stringComplete = false;

//Define os pinos que serão utilizados para ligação ao display
LiquidCrystal lcd(12, 11, 5, 4, 3, 2);
 
//Rotina inicial
void setup() {
  //Inicializa a Serial
  Serial.begin(9600);

  //Pré-aloca 200 bytes para inputString
  inputString.reserve(200);

  //Define o número de colunas e linhas do LCD
  lcd.begin(16, 2);
}

//Rotina de repetição
void loop() {
  //Apenas mostra o texto ao receber o caracter '\n' 
  if (stringComplete) {
	//Imprime o texto em modo de depuração
    Serial.println(inputString);
    
	//Limpa mensagens anteriores
    lcd.clear();
    lcd.setCursor(0, 0);
    lcd.print("Texto:");
    lcd.setCursor(0, 1);
	//Escreve a mensagem no LCD
    lcd.print(inputString);

    // Limpa a string:
    inputString = "";
    stringComplete = false;
  }
}

//Rotina de interrupção da Serial
void serialEvent() {
  //Enquanto houverem caracteres a leitura ocorre
  while (Serial.available()) {
	//Guarda o caracter recebido
    char inChar = (char)Serial.read();
	
	//Apenas sinaliza o fim da recepeção ao receber o caracter '\n' 
    if (inChar == '\n') {
      stringComplete = true;
    }else{
      inputString += inChar;
    }
  }
}