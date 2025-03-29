# FTS_InterconnectIOBox OpenTAP Plugin

The `FTS_InterconnectIOBox` is a plugin for OpenTAP, designed to facilitate the testing and validation of interconnect IO boxes within the **First Test Station (FTS)** project. This plugin provides a set of sequences and tools to automate the testing process, ensuring the functionality and reliability of the interconnect IO boxes.

## Features

All the Test Steps required to control the **FTS_InterconnectIOBox** have been grouped into different categories for better clarity:

- **1-Wire**: Group of Test Steps to check, read, or write data from 1-Wire devices.
- **Analog**: Group of Test Steps to read voltage from the ADC, set voltage at the DAC, take measurements from the Power Monitor, control individual relays, and turn on/off open-collector transistors.
- **Communication**: Group of Test Steps to configure and perform read/write operations for the communication protocols **I2C, SPI, and Serial**.
- **Config**: Group of Test Steps to read/write configuration parameters stored in the configuration EEPROM.
- **Digital**: Group of Test Steps to configure the direction (**Input/Output**) and read/write by bit or byte on one of the two 8-bit IO ports.
- **GPIO**: Group of Test Steps to configure the direction of individual GPIO pins and read/write on designated GPIOs.
- **Route**: Group of Test Steps to control one of the four available relay banks.
- **SCPI**: Group of Test Steps to send predefined basic SCPI commands or manually send/query commands.
- **System**: Group of Test Steps to control or read various system or software versions available on the box.
- **ZModule**: Group of Test Steps to control the **Selftest Board**, facilitating command transmission and response between the box and the Selftest Board.

## Installation

To install the `FTS_InterconnectIOBox` plugin, two methods are available: you can directly use the TapPackage available in the repository, or if modifications are required, you can clone and build it.

If clone methos is selected, Follow these steps:

1. **Clone the Repository**:
    ```sh
    git clone https://github.com/FirstTestStation-Project/FTS_InterconnectIOBox_OpenTAP_Plugin
    ```

2. **Build the Plugin**:
    - Open the solution file in your preferred IDE (e.g., Visual Studio).
    - Build the solution to generate the TapPackage.

3. **Install the Package using the OpenTAP Package Manager**:
    - Launch the OpenTAP Package Manager.
    - Click on the **`+`** button to open the file explorer, navigate to the cloned directory, and select the **TapPackage** to install.
    - Once the installation is complete, close the Package Manager.


## Contributing

Contributions to the `FTS_InterconnectIOBox` plugin are welcome! To contribute, follow these steps:

1. **Fork the Repository**:
    - Fork the repository on GitHub to your own account.

2. **Create a New Branch**:
    - Create a new branch for your feature or bugfix.
    ```sh
    git checkout -b your-feature-branch
    ```

3. **Commit Your Changes**:
    - Commit your changes with a descriptive message.
    ```sh
    git commit -m "Description of your changes"
    ```

4. **Push to Your Branch**:
    - Push your changes to your forked repository.
    ```sh
    git push origin your-feature-branch
    ```

5. **Open a Pull Request**:
    - Open a pull request on the original repository to merge your changes.

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.

## Support

For support and questions, please open an issue on the GitHub repository or contact the maintainers.

---
