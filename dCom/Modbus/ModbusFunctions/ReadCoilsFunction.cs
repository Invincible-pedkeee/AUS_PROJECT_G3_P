using Common;
using Modbus.FunctionParameters;
using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;

namespace Modbus.ModbusFunctions
{
    /// <summary>
    /// Class containing logic for parsing and packing modbus read coil functions/requests.
    /// </summary>
    public class ReadCoilsFunction : ModbusFunction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReadCoilsFunction"/> class.
        /// </summary>
        /// <param name="commandParameters">The modbus command parameters.</param>
		public ReadCoilsFunction(ModbusCommandParameters commandParameters) : base(commandParameters)
        {
            CheckArguments(MethodBase.GetCurrentMethod(), typeof(ModbusReadCommandParameters));
        }

        /// <inheritdoc/>
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
            ModbusReadCommandParameters parameters = this.CommandParameters as ModbusReadCommandParameters;

            if (response[7] == CommandParameters.FunctionCode + 0x80)
            {
                HandeException(response[8]);
            }

            ushort startAddress = parameters.StartAddress;
            int byteCount = response[8];

            for (int i = 0; i < byteCount; ++i)
            {
                byte currentByte = response[9 + i];

                for (int j = 0; j < 8; ++j)
                {
                    if (parameters.Quantity <= 8 * i + j)
                        break;

                    ushort value = (ushort)(currentByte & 0x1);
                    currentByte >>= 1;

                    dict.Add(new Tuple<PointType, ushort>(PointType.DIGITAL_OUTPUT, startAddress++), value);
                }
            }

            return dict;
        }
     
    }
}