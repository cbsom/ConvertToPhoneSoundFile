# Convert To Phone Sound File

A very simple and straightforward .net core 3.1 desktop application that uses ffmpeg to convert any sound file to an 8 bit mono u-law WAV file.

The generated files are relatively very small for WAV files.

Obviously, this application is not meant to generate high quality music files and the like.

The generated files ARE good for speech files which are meant to be played over a telephone line.

We use this application to generate small WAV files to be used as recordings for Twilio's Programmable Voice "Play" verb.


IMPORTANT NOTE: The app expects to find ffmpeg.exe in the root folder.

Ffmpeg can be acquired at https://www.ffmpeg.org/

The latest ffmpeg build for Windows can be downloaded from https://www.gyan.dev/ffmpeg/builds/ffmpeg-release-essentials.7z
