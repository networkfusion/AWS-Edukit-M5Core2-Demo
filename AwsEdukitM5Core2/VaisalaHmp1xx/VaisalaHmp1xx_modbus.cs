using Iot.Device.Modbus.Client;
using System;
using System.Collections;
using System.Device.Model;
using System.Diagnostics;
using System.Threading;
using UnitsNet;

namespace VaisalaHmp1xx
{
    /// <summary>
    ///  Base class for common functions of the Vaisala HMP1xx sensors.
    /// </summary>
    [Interface("Vaisala HMP1xx temperature and humidity sensor")]
    public class VaisalaHmp1xx_modbus : IDisposable
    {
        // TODO: should be an abstract class
        private readonly ModbusClient _sensor;
        private static double _humidity;
        private static double _temperature;
        private static double _probeTemperature;
        // Derived parameters
        private static double _frostPointTemperature;
        private static double _dewPointTemperature;
        private static double _mixingRatio;
        private static double _wetbulbTemperature;


        public VaisalaHmp1xx_modbus(string port)
        {
            _sensor = new ModbusClient(port, 19200, System.IO.Ports.Parity.None, 8, System.IO.Ports.StopBits.Two, System.IO.Ports.SerialMode.RS485)
            {
                ReadTimeout = 5000,
                WriteTimeout = 5000
            };

        }

        public void Open()
        {
            //Close();

            //_sensor.Open();
            // Debug.WriteLine("HMP1xx serial port opened!");
            Thread.Sleep(5000); // Give the sensor a second to settle.
            ReadAllRegisters();
        }

        public void Close()
        {
            //if (_sensor.IsOpen)
            //{
            //    _sensor.Close();
            //}
            //_sensor.DataReceived -= Port_DataReceived;
        }


        /// <summary>
        /// Retrive the error information from the device.
        /// </summary>
        public void GetDeviceErrors()
        {
            Debug.WriteLine("Attempting to get device error info!");

        }

        /// <summary>
        /// Retrive the device information.
        /// </summary>
        public Hashtable GetDeviceInformation()
        {

            Hashtable infoFields = new(); // TODO : use DeviceInformation class
            Debug.WriteLine("Attempting to get sensor info!");
            return infoFields;
        }

        private float ConvertToFloat(ushort high, ushort low)
        {
            byte[] bytes = new byte[4];
            bytes[0] = (byte)(low & 0xFF);
            bytes[1] = (byte)(low >> 8);
            bytes[2] = (byte)(high & 0xFF);
            bytes[3] = (byte)(high >> 8);
            return BitConverter.ToSingle(bytes, 0);
        }

        public void ReadAllRegisters()
        {
            Debug.WriteLine("Attempting to read all modbus registers!");
            // Read all the registers in one go.
            ushort startAddress = 1; // TODO: check this value 0 or 1?
            ushort numRegisters = 2; // TODO: check this value 26 or 27?
            short[] inputRegistersRead = _sensor.ReadInputRegisters(0xF0, startAddress, numRegisters);
            //Debug.WriteLine($"Read {registers.Length} registers from sensor.");
            //// Parse the registers
            //_humidity = ConvertToFloat((ushort)registers[0], (ushort)registers[1]); // Humidity in %
            //_temperature = ConvertToFloat((ushort)registers[2], (ushort)registers[3]); // Temperature in °C
            //// Derived parameters (placeholders for now)
            //// _frostPointTemperature = _temperature - 5; // Placeholder calculation
            //_dewPointTemperature = ConvertToFloat((ushort)registers[8], (ushort)registers[9]);
            //_mixingRatio = ConvertToFloat((ushort)registers[16], (ushort)registers[17]);
            //_wetbulbTemperature = ConvertToFloat((ushort)registers[18], (ushort)registers[19]);
            //Debug.WriteLine($"Read Registers: Humidity={_humidity}%, Temperature={_temperature}°C, ProbeTemp={_probeTemperature}°C");
        }

        /// <summary>
        /// Perform initialization command sequence.
        /// </summary>
        private void Initialize()
        {

            // We should possibily disable the transmit line now (unless we are in poll mode)?!
        }

        /// <summary>
        /// Gets the last relative humidity reading from the sensor.
        /// </summary>
        /// <remarks>
        /// Received every 1-2 seconds.
        /// </remarks>
        /// <returns>Relative humidity reading.</returns>
        [Telemetry("RelativeHumidity")]
        public RelativeHumidity GetRelativeHumidity()
        {
            return RelativeHumidity.FromPercent(_humidity);
        }

        /// <summary>
        /// Gets the last temperature reading from the sensor.
        /// </summary>
        /// <remarks>
        /// Received every 1-2 seconds.
        /// </remarks>
        /// <returns>Temperature reading.</returns>
        [Telemetry("Temperature")]
        public Temperature GetTemperature()
        {
            return Temperature.FromDegreesCelsius(_temperature);
        }

        /// <summary>
        /// Gets the last probe temperature reading from the sensor.
        /// </summary>
        /// <remarks>
        /// Received every 1-2 seconds.
        /// </remarks>
        /// <returns>Temperature reading.</returns>
        [Telemetry("ProbeTemperature")]
        public Temperature GetProbeTemperature()
        {
            return Temperature.FromDegreesCelsius(_probeTemperature);
        }

        /// <summary>
        /// Gets the last derived frost point temperature reading from the sensor.
        /// </summary>
        /// <remarks>
        /// Received every 1-2 seconds.
        /// </remarks>
        /// <returns>Temperature reading.</returns>
        [Telemetry("FrostPointTemperature")]
        public Temperature GetFrostPointTemperature()
        {
            return Temperature.FromDegreesCelsius(_frostPointTemperature);
        }

        /// <summary>
        /// Gets the last derived dew point temperature reading from the sensor.
        /// </summary>
        /// <remarks>
        /// Received every 1-2 seconds.
        /// </remarks>
        /// <returns>Temperature reading.</returns>
        [Telemetry("DewPointTemperature")]
        public Temperature GetDewPointTemperature()
        {
            return Temperature.FromDegreesCelsius(_dewPointTemperature);
        }


        /// <summary>
        /// Gets the derived mixing ratio reading from the sensor.
        /// </summary>
        /// <remarks>
        /// The mixing ratio of water vapour in air is the weight of water vapour mixed into a given weight of dry air.
        /// </remarks>
        /// <returns>Mass reading.</returns>
        [Telemetry("MixingRatio")]
        public MassConcentration GetMixingRatio()
        {
            return MassConcentration.FromGramsPerLiter(_mixingRatio); //fixme: this is a a strange unit (g/kg)
        }

        /// <summary>
        /// Gets the derived wet bulb temperature reading from the sensor.
        /// </summary>
        /// <remarks>
        /// Received every 1-2 seconds.
        /// </remarks>
        /// <returns>Temperature reading.</returns>
        [Telemetry("WetBulbTemperature")]
        public Temperature GetWetBulbTemperature()
        {
            return Temperature.FromDegreesCelsius(_wetbulbTemperature);
        }



        /// <inheritdoc cref="IDisposable" />
        public void Dispose()
        {
            if (_sensor != null)
            {
                _sensor?.Dispose();
                //_sensor = null; // FIXME: causes readings to always be null!
            }
        }
    }
}
