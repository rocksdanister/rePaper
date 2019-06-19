<h1 align="center">
<img src="docs/favicon.ico" alt="rePaper" width="128"//>
<br/>
rePaper - A window to the the outside world
</h1>

[![GitHub release](https://img.shields.io/github/release/rocksdanister/rePaper/all.svg)](https://github.com/rocksdanister/rePaper/releases)
[![Github all releases](https://img.shields.io/github/downloads/rocksdanister/rePaper/total.svg)](https://github.com/rocksdanister/rePaper/releases)

## Contents

- [About](#about)
- [Download](#download)
- [Features](#features)
- [Screenshots](#screenshots)
- [Support](#support)

## About
![demo-gif](./resources/htmlbanner_gif.gif?raw=true "demo")

Try to imagine your desktop as a window, as it rains outside you see water droplets dripping down..as it snows you see it get frosted.. as sun rises & sets scenery changes realtime.

Thats the idea behind this software; What started out as a simple idea, after many months of work I present to you rePaper.

When running fullscreen applications or games rePaper will go to sleep (~0% cpu & gpu usage); no performance cost.

## Download
##### Latest version: v0.1 (Windows 10, 8.1, 7)
- [`rePaper.zip`][direct-win64]  
   _(SHA-256: 7c9e8506c98cb003628c427f9f257b6f474a77c84b1575771b3b69db213f2404)_

[direct-win64]: https://github.com/rocksdanister/rePaper/releases/download/v0.1/rePaper.zip


Unzip the file, select Start.exe to get started. First run will be slow since the application is unsigned windows defender will do multiple scans, make sure to uncheck "always ask before opening this file".

To update the program, just delete the old folder & extract the new version; Run it once to fix the new startup path if run at startup enabled.

Save files & settings are stored in <username>\Saved Games\rePaper.

## Features

* Video file as wallpaper support (Hardware Accelerated Playback optional)
* Most of the processing is done via gpu shaders, low cpu usage.
* Openweathermap API used to gather weather information. 
  (most of these weather conditions are supported: [https://openweathermap.org/weather-conditions](https://openweathermap.org/weather-conditions))
 * Real-time day/night cycle based on sunrise-sunset time.  
* rePaper will pause when running fullscreen application or games (~0% cpu & gpu usage; main execution thread is stopped, background thread used for monitoring).
* rePaper will pause when application focus change (optional).
* Ultrawide resolution support, multimonitor systems partially supported.
* Toggle windows 10 Light/Darkmode automatically based on time (optional).
* Automatically detect clock, text color based on wallpaper (picture wallpaper only).
* Your areas weather might not always be exciting, all the weather effects are user selectable.
* Customizable -  adjustable performance settings (fps, blur quality) , custom UI color, different clock styles & more.
* Appears behind desktop icons.
* Brightness change based on time of day & weather - less bright when clowdy, raining.
* Runs at system startup (optional).

## Screenshots
![desktop](./docs/images/preview/1_2x.jpg?raw=true "desktop")
![desktop2](./docs/images/preview/2_2x.jpg?raw=true "desktop2")
![desktop3](./docs/images/preview/6_2x.jpg?raw=true "desktop3")
![desktop4](./docs/images/preview/5_2x.jpg?raw=true "desktop4")

## Support
If you like what I do & want to support me:

[![ko-fi](https://www.ko-fi.com/img/githubbutton_sm.svg)](https://ko-fi.com/P5P1U8NQ)
