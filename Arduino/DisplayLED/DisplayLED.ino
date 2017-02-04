#include <LiquidCrystal.h>

String inputString = "";         // a string to hold incoming data
boolean stringComplete = false;  // whether the string is complete
int led = 7;

//Define os pinos que serão utilizados para ligação ao display
LiquidCrystal lcd(12, 11, 5, 4, 3, 2);
 

void setup() {
  // initialize serial:
  Serial.begin(9600);

  pinMode(led, OUTPUT);
  digitalWrite(led, HIGH);
  // reserve 200 bytes for the inputString:
  inputString.reserve(200);

  //Define o número de colunas e linhas do LCD
  lcd.begin(16, 2);
}

void loop() {
  if (stringComplete) {
    Serial.println(inputString);
    
    lcd.clear();
    lcd.setCursor(0, 0);
    lcd.print("Texto:");
    lcd.setCursor(0, 1);
    lcd.print(inputString);

    // clear the string:
    inputString = "";
    stringComplete = false;
  }
}

void serialEvent() {
  while (Serial.available()) {
    char inChar = (char)Serial.read();
    if (inChar == '\n') {
      stringComplete = true;
    }else{
      inputString += inChar;
    }
  }
}


