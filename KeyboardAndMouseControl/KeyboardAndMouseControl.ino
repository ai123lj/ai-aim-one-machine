#ifndef ARDUINO_USB_MODE
#error This ESP32 SoC has no Native USB interface
#elif ARDUINO_USB_MODE == 1
#warning This sketch should be used when USB is in OTG mode
void setup() {}
void loop() {}
#else

#include "USB.h"
#include "USBHIDMouse.h"
#include "USBHIDKeyboard.h"
USBHIDMouse Mouse;
USBHIDKeyboard Keyboard;

void setup() {
  Serial.begin(921600);
  Mouse.begin();
  Keyboard.begin();
  USB.begin();
}
char buf[50];
void loop() {
  // use serial input to control the mouse:
  if (Serial.available() > 0) {
    //验证头
    if (Serial.read() != '{')
      return;
    delay(1);
    //头正确，开始接收
    
    int len;
    for (len = 0; Serial.available() > 0; len++) {
      buf[len] = Serial.read();
    }
    //验证尾
    if (buf[len - 1] != '}')
      return;
    buf[len - 1] = 0;

    int mode = 0, x = 0, y = 0;
    sscanf(buf, "%d,%d,%d", &mode, &x, &y);

    moveMouse(x, y);
    switch (mode) {
      case 1:
        Mouse.click(MOUSE_LEFT);
        break;
      case 2:
        Mouse.press(MOUSE_LEFT);
        break;
      case 3:
        Mouse.release(MOUSE_LEFT);
        break;
      case 4:
        Mouse.click(MOUSE_LEFT);
        delay(30);
        Keyboard.press('2');
        delay(30);
        Keyboard.release('2');
        break;
    }
  }
}
void moveMouse(int x, int y) {
  for (int i = 0; i < 3; i++) {
    Mouse.move(x / 3, y / 3);
  }
  Mouse.move(x % 3, y % 3);
}
#endif /* ARDUINO_USB_MODE */
