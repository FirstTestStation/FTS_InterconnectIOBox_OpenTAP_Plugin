# FTS_InterconnectIOBox OpenTAP Plugin 800-1020-xx

The `FTS_InterconnectIOBox` is a plugin for OpenTAP, designed to facilitate the testing and validation of interconnect IO boxes within the **First Test Station (FTS)** project. This plugin provides a set of sequences and tools to automate the testing process, ensuring the functionality and reliability of the interconnect IO boxes.

## Features

All the Test Steps required to control the **FTS_InterconnectIOBox** have been grouped into different categories for better clarity:

- **1-Wire**: Group of Test Steps to communicate with 1-Wire hardware devices (e.g., DS2502). These devices are used to identify connected hardware before applying power, reducing the risk of manipulation errors.
- **Analog**: Group of Test Steps to read voltage from the ADC, set voltage at the DAC, take measurements from the Power Monitor, control individual relays, and turn on/off open-collector transistors.
- **Communication**: Group of Test Steps to configure and perform read/write operations for the communication protocols **I2C, SPI, and Serial**.
- **Config**: Group of Test Steps to read/write configuration parameters stored in the configuration EEPROM.
- **Digital**: Group of Test Steps to configure the direction (**Input/Output**) and read/write by bit or byte on one of the two 8-bit IO ports.
- **GPIO**: Group of Test Steps to configure the direction of individual GPIO pins and read/write on designated GPIOs.
- **Route**: Group of Test Steps to control one of the four available relay banks.
- **SCPI**: Group of Test Steps to send predefined basic SCPI commands or manually send/query commands.
- **System**: Group of Test Steps to control or read various system or software versions available on the box.
- **ZModule**: Group of Test Steps to control the **Selftest Board**, facilitating command transmission and response between the box and the Selftest Board.

---

## Test Steps Reference

### 1-Wire

The 1-Wire Test Steps communicate with special 1-Wire hardware devices (e.g., DS2502). These devices are used to identify connected hardware before applying power, reducing the risk of manipulation errors.

| Step Name | Description |
|-----------|-------------|
| **1-Wire Check** | Check and validate the quantity of 1-Wire devices on the Fixture. The quantity and contents are both verified. |
| **1-Wire Read** | Read and validate the quantity of 1-Wire devices on the DUT. The quantity and contents are both verified. |
| **1-Wire validation** | Validate the part number of the Fixture or DUT by reading data from its 1-Wire device. |
| **1-Wire Write Data** | Write data (part number, serial number and connector refdes) on a 1-Wire DUT device. |

---

### Analog

| Step Name | Description |
|-----------|-------------|
| **ADC** | Read the voltage from the Pico Analog-to-Digital Converter (ADC) and verify if it falls within the expected range. |
| **DAC** | Output voltage 0–3.3 V from the Digital-to-Analog Converter (DAC). |
| **Open Collector Transistor Control** | SCPI command to control the three open-collector transistors and read back their status. |
| **PWR Monitor Read** | Read Load Voltage, Shunt voltage (0.1 Ω), Current (mA), or Power (mW) from the Power Monitor. |
| **Isolated Relay Control** | SCPI command to set the state of relays: LPR1, LPR2, HPR1, HPR2, and 5V_DUT. |

---

### Communication

| Step Name | Description |
|-----------|-------------|
| **I2C Config** | Enable and configure I2C communication, including baudrate, mode, chip select, and data bits. |
| **SPI Config** | Enable and configure SPI communication, including baudrate, mode, chip select, and data bits. |
| **Serial Config** | Enable and configure serial communication, including baud rate, protocol, handshake, and timeout. |
| **I2C/SPI Data W/R** | Write/read data on a register using the selected protocol (I2C or SPI). The communication protocol must be enabled before use. |
| **Serial Data Write/Read** | Write/read data on a Serial port. |
| **Data Analyze** | Analyze data from a device under test between a Low and High limit. Extracts a bitfield from raw I2C/SPI bytes, applies a math equation, and validates the result against configurable limits. |

---

### Config

| Step Name | Description |
|-----------|-------------|
| **Read Full Configuration** | Read the full system power-up configuration stored in EEPROM using the SCPI command `CFG:READ:EEPROM:FULL?`. |
| **Write Default Configuration** | Write the default system power-up configuration to EEPROM using the SCPI command `CFG:WRITE:EEPROM:DEFAULT`. |
| **Write Configuration Parameter** | Write a specific system power-up configuration parameter to EEPROM using the SCPI command `CFG:WRITE:EEPROM:STR`. |
| **Read Configuration Parameter** | Read a specific system power-up configuration parameter from EEPROM using the SCPI command `CFG:READ:EEPROM:STR?`. |

---

### Digital

| Step Name | Description |
|-----------|-------------|
| **Dual Ports 8-bits Direction Write/Read** | Configure the direction of Port0 and/or Port1 as Input or Output. Configuration can be specified using a byte value in decimal, hexadecimal, or binary. |
| **Dual Ports 8-bits Write/Read** | Write or Read data of Port0 and/or Port1 configured as Input or Output. Set or Read data can be specified using a byte value in decimal, hexadecimal, or binary. |

---


### GPIO

| Step Name | Description |
|-----------|-------------|
| **GPIO Direction Write/Read** | Configure the direction of a GPIO as Input or Output. Direction can be set for a designated GPIO or for any GPIO on any Pico device. |
| **GPIO PAD Write/Read** | Low-level control of the different options available on each GPIO pin (PAD configuration). |
| **GPIO Input/Output Bit Write/Read** | Write or Read data of a GPIO pin on a dedicated GPIO or on a GPIO of any device. |

---

### Route

| Step Name | Description |
|-----------|-------------|
| **Single Bank Multiple Relay Close or Open** | Open or close one or multiple relay routes within a single bank. |
| **Exclusive Mode: Close Relay by Bank** | Closes one relay per bank in Exclusive mode, ensuring only one relay per bank is active at a time. |
| **Open Single Bank** | Open a single relay bank or all banks. |
| **Relays Banks Validation** | Read relay bank status for one or many banks (1 = Closed, 0 = Open). Optional status validation is available. |
| **Single Bank Multiple Relay Validation** | Read relay channel state (1 = Closed, 0 = Open) for one or multiple relays in a single bank. |

---

### SCPI

| Step Name | Description |
|-----------|-------------|
| **Basic Command** | Execute required SCPI basic commands (`*IDN?`, `*TST?`, `*OPC`, `*OPC?`, `*RST`, `*CLS`, `*WAI`). |
| **Manual Command** | Send a SCPI command entered manually. Supports both write commands (no response expected) and queries (response read and optionally validated against a numeric or string expected value). |
| **Register Command** | Read or write a SCPI register. Supports register write, read, and validation against expected data. |

---

### System

| Step Name | Description |
|-----------|-------------|
| **Beeper** | Generate a short (100 ms) beep pulse. |
| **Pico Firmware Version** | Read the firmware version for Pico Master, Slave1, Slave2, and Slave3 devices. |
| **Error Led** | Set or read the status of the Red Error LED. Each line of the test control is processed sequentially. |
| **Pico Slaves** | Enable, disable (reset), or read the status of Pico Slave devices. Each line of the test control is processed sequentially. |
| **System Version** | Read the SCPI version in use. The version can be validated and the result published. |
| **System Error** | Read the number of errors, read the error list, clear the error list, and optionally publish the result if it matches the expected value. |
| **TAP Settings Selector** | Adds TapSettings (DUT, Instruments, and Connections) to the Test Plan, reducing operator errors by ensuring the correct configuration is loaded before the test runs. |

---


### Fixture/DUT

| Step Name | Description |
|-----------|-------------|
| **Get Serial Number** | Retrieve the serial number of the fixture or DUT. |
| **Clear Serial Number** | Clear the serial number stored for the fixture or DUT. |
| **Setup Results Data** | Defines additional columns in CSV result tables and configures the file name and save location. Settings are processed by FTS OperatorGUI. **This step must be placed first in the Test Plan.** |

#### Setup Results Data — Parameters

**General**

| Parameter | Description |
|-----------|-------------|
| DUT | Reference to the `FTS_DUT` instrument used in this test step. Provides all product and fixture metadata. |

**Columns to Add on CSV**

These toggles control which metadata fields from `FTS_DUT` are injected as extra columns into the CSV result file.

| Parameter | Default | Description |
|-----------|---------|-------------|
| Product Name | ✅ Enabled | Adds the Product Name defined in `FTS_DUT`. |
| Product Number | ✅ Enabled | Adds the Product Number (Part Number) defined in `FTS_DUT`. |
| Fixture Name | ✅ Enabled | Adds the Fixture Name defined in `FTS_DUT`. |
| Fixture Number | ✅ Enabled | Adds the Fixture Number defined in `FTS_DUT`. |
| Fixture Serial | ✅ Enabled | Adds the Fixture Serial Number defined in `FTS_DUT`. |

> **Note:** Serial Number is always added automatically and cannot be disabled.

**CSV File Name**

Controls the `<ResultName>` portion of the output file, which follows the pattern `<ResultName>-<Date>-<Verdict>.csv`.

| Parameter | Description |
|-----------|-------------|
| File Name Mode | Selects how the result file name is built. Available modes: |
| | `Fixed` — uses the text entered in **Fixed Name**. |
| | `SerialNumber` — uses the DUT serial number. |
| | `ProductName` — uses the product name. |
| | `Id` — uses the DUT ID. |
| | `ProductNumber` — uses the part number. |
| | `ProductName_SerialNumber` *(default)* — combines product name and serial number. |
| | `ProductNumber_SerialNumber` — combines part number and serial number. |
| | `ProductName_ProductNumber_SerialNumber` — combines product name, part number, and serial number. |
| Fixed Name | Static file name to use when **File Name Mode** is set to `Fixed`. Default: `TestResults`. |

**CSV Save Location**

| Parameter | Description |
|-----------|-------------|
| Save Mode | Where FTS OperatorGUI saves the CSV result files. Options: `Default`, `Local`, `Network`, `LocalAndNetwork`. |
| Local Folder | Local folder path for saving CSV files. Also used as a temporary staging location when **Save Mode** is `Network` and the network is unreachable. Visible when Save Mode is `Local` or `LocalAndNetwork`. Default: `C:\TestResults`. |
| Network Folder | UNC network path where OperatorGUI copies the CSV after each run. If the network is down, the file is queued and retried automatically. Visible when Save Mode is `Network` or `LocalAndNetwork`. Default: `\\Server\TestResults`. |

---


### ZModule — Selftest DUT

| Step Name | Description |
|-----------|-------------|
| **Selftest COM Command** | Group of I2C commands used to communicate with the Selftest Board. |
| **Selftest GPIO Command** | Group of I2C commands used to control GPIOs on the Selftest Board. |





---

## Installation

To install the `FTS_InterconnectIOBox` plugin, two methods are available: you can directly use the TapPackage available in the repository, or if modifications are required, you can clone and build it.

If the clone method is selected, follow these steps:

1. **Clone the Repository**:
    ```sh
    git clone https://github.com/FirstTestStation/FTS_InterconnectIOBox_OpenTAP_Plugin.git
    ```

2. **Install the Package using the OpenTAP Package Manager**:
    - Launch the OpenTAP Package Manager.
    - Click on the **`+`** button to open the file explorer, navigate to the cloned directory (`bin/release/*.TapPackage`), and select the **TapPackage** to install.
    - Once the installation is complete, close the Package Manager.

## Plugin Modification

- If modifications are required, follow the instructions from the [OpenTAP SDK YouTube tutorial](https://www.youtube.com/watch?v=BXTiCLEXcwI&list=PLdeXOCgDt7jhuDA_tVp3joJG0cV3Lzvo2&index=1) to set up the necessary environment.
- Clone the plugin repository.
- Load the `.csproj` file from the GitHub repository.

## License

This project is licensed under the [Mozilla Public License 2.0](https://www.mozilla.org/MPL/2.0/).

## Support

For support and questions, please open an issue on the GitHub repository or contact the maintainers.

---
