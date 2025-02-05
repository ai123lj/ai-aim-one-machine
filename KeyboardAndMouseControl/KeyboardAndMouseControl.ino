
#include "USB.h"
#include "USBHIDMouse.h"
#include "USBHIDKeyboard.h"
USBHIDMouse Mouse;
USBHIDKeyboard Keyboard;

// #include <WiFi.h>
// #include <Udp.h>
// WiFiUDP Udp;
void setup() {
  Serial.begin(921600);

  Mouse.begin();
  Keyboard.begin();
  USB.begin();
  // WiFi.mode(WIFI_MODE_STA);
  // WiFi.begin("CMCC-gFgv", "WNgHCcd2");
  // while (WiFi.status() != WL_CONNECTED) {
  //   Serial.print('.');
  //   delay(1000);
  // }
  // Serial.println();
  // Serial.print(WiFi.localIP().toString());
  // Udp.begin(9000);
}
char buf[50];
void loop() {
  // ////////////////////////////////////////////////////////////////////////////////接收app地址
  // int packetSize = Udp.parsePacket();
  // if (packetSize) {
  //   int n = Udp.read((char*)buf, sizeof(buf));  //把数据存在数组里
  //   if (buf[0] == '{') {
  //     buf[n] = 0;
  //     int mode = 0, x = 0, y = 0;
  //     sscanf(buf, "{%d,%d,%d}", &mode, &x, &y);
  //     switch (mode) {
  //       case 0:
  //         moveMouse3(x, y);
  //         break;
  //       case 1:
  //         moveMouse(x, y, MOUSE_LEFT);
  //         break;
  //       case 2:
  //         Mouse.press(MOUSE_LEFT);
  //         break;
  //       case 3:
  //         Mouse.release(MOUSE_LEFT);
  //         break;
  //       case 4:
  //         moveMouse(x, y, MOUSE_LEFT);
  //         delay(35);
  //         Keyboard.press('2');
  //         delay(30);
  //         Keyboard.release('2');
  //         break;
  //     }
  //   }
  // }

  // use serial input to control the mouse:
  if (Serial.available() > 0) {
    //验证头
    if (Serial.read() != '{')
      return;
    //头正确，开始接收
    delayMicroseconds(300);
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
    switch (mode) {
      case 0:
        moveMouse3(x, y);
        break;
      case 1:
        moveMouse(x, y, MOUSE_LEFT);
        break;
      case 2:
        Mouse.press(MOUSE_LEFT);
        break;
      case 3:
        Mouse.release(MOUSE_LEFT);
        break;
      case 4:
        moveMouse(x, y, MOUSE_LEFT);
        delay(35);
        Keyboard.press('2');
        delay(30);
        Keyboard.release('2');
        break;
    }

    // moveMouse(x, y);
    // switch (mode) {
    //   case 1:
    //     Mouse.click(MOUSE_LEFT);
    //     break;
    //   case 2:
    //     Mouse.press(MOUSE_LEFT);
    //     break;
    //   case 3:
    //     Mouse.release(MOUSE_LEFT);
    //     break;
    //   case 4:
    //     Mouse.click(MOUSE_LEFT);
    //     delay(30);
    //     Keyboard.press('2');
    //     delay(30);
    //     Keyboard.release('2');
    //     break;
    // }
  }
}
void moveMouse3(int x, int y) {
  int num;
  if (x > y) {
    num = x;
  } else {
    num = y;
  }
  if (num <= 120) {
    Mouse.move(x, y);
  } else if (num <= 240) {
    Mouse.move(x / 2, y / 2);
    Mouse.move(x / 2 + (x % 2), y / 2 + (y % 2));
  } else if (num <= 360) {
    Mouse.move(x / 3, y / 3);
    Mouse.move(x / 3, y / 3);
    Mouse.move(x / 3 + (x % 3), y / 3 + (y % 3));
  } else if (num <= 480) {
    Mouse.move(x / 4, y / 4);
    Mouse.move(x / 4, y / 4);
    Mouse.move(x / 4, y / 4);
    Mouse.move(x / 4 + (x % 4), y / 4 + (y % 4));
  }
}
void moveMouse(int x, int y, int button) {
  int num;
  if (abs(x) > abs(y)) {
    num = abs(x);
  } else {
    num = abs(y);
  }

  hid_mouse_report_t report;
  if (num <= 120) {  //一次移动加点击
    report = { .buttons = button, .x = x, .y = y, .wheel = 0, .pan = 0 };
    Mouse.sendReport(report);
    report = { .buttons = 0, .x = 0, .y = 0, .wheel = 0, .pan = 0 };
    Mouse.sendReport(report);
  } else if (num <= 240) {
    report = { .buttons = 0, .x = x / 2, .y = y / 2, .wheel = 0, .pan = 0 };
    Mouse.sendReport(report);
    report = { .buttons = button, .x = x / 2 + (x % 2), .y = y / 2 + (y % 2), .wheel = 0, .pan = 0 };
    Mouse.sendReport(report);
    report = { .buttons = 0, .x = 0, .y = 0, .wheel = 0, .pan = 0 };
    Mouse.sendReport(report);
  } else if (num <= 360) {
    report = { .buttons = 0, .x = x / 3, .y = y / 3, .wheel = 0, .pan = 0 };
    Mouse.sendReport(report);
    Mouse.sendReport(report);
    report = { .buttons = button, .x = x / 3 + (x % 3), .y = y / 3 + (y % 3), .wheel = 0, .pan = 0 };
    Mouse.sendReport(report);
    report = { .buttons = 0, .x = 0, .y = 0, .wheel = 0, .pan = 0 };
    Mouse.sendReport(report);
  } else if (num <= 480) {
    report = { .buttons = 0, .x = x / 4, .y = y / 4, .wheel = 0, .pan = 0 };
    Mouse.sendReport(report);
    Mouse.sendReport(report);
    Mouse.sendReport(report);
    report = { .buttons = button, .x = x / 4 + (x % 4), .y = y / 4 + (y % 4), .wheel = 0, .pan = 0 };
    Mouse.sendReport(report);
    report = { .buttons = 0, .x = 0, .y = 0, .wheel = 0, .pan = 0 };
    Mouse.sendReport(report);
  }
}
