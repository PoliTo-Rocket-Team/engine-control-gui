String data; //declares all variables
bool pinStatus = LOW;
int val;
int value_old;
int value_new;
bool sent = false;
String t = "";

const byte numChars = 32; //variables for receiving data
char receivedChars[numChars];
boolean newData = false;
String receivedString = "";
static boolean recvInProgress = false;

void setup() {
  pinMode(LED_BUILTIN, OUTPUT); //pin setup
  pinMode(A0, INPUT);
  Serial.begin(9600); //serial communication startup 
}

void send_data(String data, String header){ //function to send send data. Not very efficient. Data can be as long as needed
  int l = data.length();
  Serial.print(header); //prints the two control characters
  if (l < 7){
    for (int i = 0; i < 6 - l; i++){Serial.print(" ");} //prints spacing characters to make the data chunk 8 characters long
    Serial.print(data); //prins the data
  }
  else{
    Serial.print(data.substring(0, 6));
    Serial.print("C");
    data = data.substring(6);
    l = data.length();
    while (l > 7){
      Serial.print(data.substring(0, 7));
      data = data.substring(7);
      l = data.length();
      Serial.print("C");
    }
    for (int i = 0; i < 7 - l; i++){Serial.print(" ");} //prints spacing characters to make the data chunk 8 characters long
    Serial.print(data);
  }

}

String receive() { //function to read data if present in the buffer memory
    newData = false;
    static byte ndx = 0;
    char startMarker = '<';
    char endMarker = '>';
    char rc;
 
    while (Serial.available() > 0 && newData == false) {
        rc = Serial.read();

        if (recvInProgress == true) {
            if (rc != endMarker) {
                receivedChars[ndx] = rc;
                ndx++;
                if (ndx >= numChars) {
                    ndx = numChars - 1;
                }
            }
            else {
                receivedChars[ndx] = '\0'; // terminate the string
                recvInProgress = false;
                ndx = 0;
                newData = true;
                return String(receivedChars);
            }
        }

        else if (rc == startMarker) {
            recvInProgress = true;
        }
    }

    if (recvInProgress == true){
      return "";
    }
}

void loop() {
  t = String(millis()); //loop start time
  send_data(t, "S2");

  if(Serial.available()){ //if data is received
    data = receive(); //reads the received data

    if (data == "ToggleLED"){ //LED flash instruction (example)
      digitalWrite(LED_BUILTIN, HIGH);
      delay(5000);
      digitalWrite(LED_BUILTIN, LOW);
    }
    else if (data[0] == char('R')){ //if the first character is 'R' it repeats the same string back to the computer but starting with 'S'
      send_data(data.substring(1), "S1");
      send_data(String(receivedChars), "S4");
    }
  }


  String val = String(analogRead(A0)); // reads the analog pin, which not being connected to anything is just voltage fluctuations. Saved as a string

  send_data(val, "G0"); //sends data to graph
  send_data(val, "I0"); //and to progress bar
  String t = String(millis()); //sampling end time
  send_data(t, "S0");
  Serial.println(""); //new line concludes data transmission

  while (millis()%100 < 90){ //goes on hold for 90 ms out of every 100 ms. This way data is sent 10 times a second
    delay(9);
  }
  delay(12);
}
