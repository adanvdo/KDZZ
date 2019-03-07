# KDZZ

LG KDZ Zip Package Creation Tool - For TWRP Packages

## Latest Version 

  Download Now: [KDZZ v 1.0.1.0](https://github.com/adanvdo/KDZZ/releases)

*Features will continue to be added/updated over time.*

## Requirements
	
  - H872 or H932 KDZ File
  - Windows LG Firmware Extract tool
    - [Download v1.2.6.1](https://sourceforge.net/projects/lgtools/files/KDZTools/WindowsLGFirmwareExtract_v1.2.6.1.zip/download)
	
## Installation

  - Extract contents of KDZZ.zip to the directory of your choice.

## Usage Instructions
---
#### 1. Extract KDZ

  - Using Windows LG Firmware Extract
    - Set your working directory (i.e. C:\Users\JDOE\Desktop\KDZFiles)

		- ![set working directory](./KDZZ/files/readme/lgfe1.jpg)
	- Extract the DZ file from the KDZ
		- ![open KDZ and extract](./KDZZ/files/readme/lgfe2.jpg)
	- Extract the bins from the extracted DZ file
		- ![open DZ and extract](./KDZZ/files/readme/lgfe3.jpg)
	- Merge the extracted System bins
		- ![merge system bins](./KDZZ/files/readme/lgfe4.jpg)

#### 2. Run KDZZ

  - **Open CMD in Windows**
  - **cd to KDZZ.exe directory or type** 
    - `[path to KDZZ.exe] -p "[destination project directory path]" -b "[path to extracted kdz files]"`
    - `example: C:\Users\JDOE\KDZZ\KDZZ.exe -p "C:\Users\JDOE\20fproject" -b "C:\Users\JDOE\h87220f"`
    - ![run KDZZ](./KDZZ/files/readme/kdzz1.jpg)		  

  - **Press enter to run the utility. Follow the prompts.**

    - ![run KDZZ](./KDZZ/files/readme/kdzz2.jpg)		

  - **KDZZ will copy all of the relevant, extracted KDZ bins to the project directory.  When complete, you will be prompted to choose a package type.**

    - ![choose package](./KDZZ/files/readme/kdzz4.jpg)

  - **Specify your package name**

    - ![specify name](./KDZZ/files/readme/kdzz5.jpg)

  - **Wait for Zip file to be created**

    - ![wait](./DZZ/files/readme/kdzz6.jpg)

  - **Example Bootloader Package** 

    - ![zip contents](./KDZZ/files/readme/kdzz7.jpg)

    - ![updater-script](./KDZZ/files/readme/kdzz8.jpg)
	

## License
---
ISC License (ISC)
