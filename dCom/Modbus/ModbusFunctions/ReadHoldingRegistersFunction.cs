using Common;
using Modbus.FunctionParameters;
using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;

namespace Modbus.ModbusFunctions
{
    /// <summary>
    /// Class containing logic for parsing and packing modbus read holding registers functions/requests.
    /// </summary>
    public class ReadHoldingRegistersFunction : ModbusFunction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReadHoldingRegistersFunction"/> class.
        /// </summary>
        /// <param name="commandParameters">The modbus command parameters.</param>
        public ReadHoldingRegistersFunction(ModbusCommandParameters commandParameters) : base(commandParameters)
        {
            CheckArguments(MethodBase.GetCurrentMethod(), typeof(ModbusReadCommandParameters));
        }

        /// <inheritdoc />
        public override byte[] PackRequest()
        {
            ModbusReadCommandParameters parameters = this.CommandParameters as ModbusReadCommandParameters;
            byte[] request = new byte[12];

            Buffer.BlockCopy(
                (Array)BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)parameters.TransactionId)),
                0, request, 0, 2
            );

            Buffer.BlockCopy(
                (Array)BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)parameters.ProtocolId)),
                0, request, 2, 2
            );

            Buffer.BlockCopy(
                (Array)BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)parameters.Length)),
                0, request, 4, 2
            );

            request[6] = parameters.UnitId;
            request[7] = parameters.FunctionCode;

            Buffer.BlockCopy(
                (Array)BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)parameters.StartAddress)),
                0, request, 8, 2
            );

            Buffer.BlockCopy(
                (Array)BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)parameters.Quantity)),
                0, request, 10, 2
            );

            return request;
        }

        /// <inheritdoc />
        public override Dictionary<Tuple<PointType, ushort>, ushort> ParseResponse(byte[] response)
        {
            Dictionary<Tuple<PointType, ushort>, ushort> dict = new Dictionary<Tuple<PointType, ushort>, ushort>();
            ModbusReadCommandParameters parameters = (ModbusReadCommandParameters)CommandParameters;

            if (response[7] == CommandParameters.FunctionCode + 0x80)
            {
                HandeException(response[8]);
            }

            ushort startAddress = parameters.StartAddress;
            int byteCount = response[8];

            for (int i = 0; i < byteCount / 2; ++i)
            {
                byte firstByte = response[9 + i * 2];
                byte secondByte = response[9 + 1 + i * 2];

                ushort value = BitConverter.ToUInt16(new byte[2] { secondByte, firstByte }, 0);

                dict.Add(new Tuple<PointType, ushort>(PointType.ANALOG_OUTPUT, startAddress++), value);
            }

            return dict;
        }
    }
}