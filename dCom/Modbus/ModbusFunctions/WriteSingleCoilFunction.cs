using Common;
using Modbus.FunctionParameters;
using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;

namespace Modbus.ModbusFunctions
{
    /// <summary>
    /// Class containing logic for parsing and packing modbus write coil functions/requests.
    /// </summary>
    public class WriteSingleCoilFunction : ModbusFunction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WriteSingleCoilFunction"/> class.
        /// </summary>
        /// <param name="commandParameters">The modbus command parameters.</param>
        public WriteSingleCoilFunction(ModbusCommandParameters commandParameters) : base(commandParameters)
        {
            CheckArguments(MethodBase.GetCurrentMethod(), typeof(ModbusWriteCommandParameters));
        }

        /// <inheritdoc />
        public override byte[] PackRequest()
        {
            ModbusWriteCommandParameters parameters = this.CommandParameters as ModbusWriteCommandParameters;
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
                (Array)BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)parameters.OutputAddress)),
                0, request, 8, 2
            );
            ushort coilValue = parameters.Value == 0 ? (ushort)0x0000 : (ushort)0xFF00;

            Buffer.BlockCopy(
                (Array)BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)coilValue)),
                0, request, 10, 2
            );

            return request;
        }

        /// <inheritdoc />
        public override Dictionary<Tuple<PointType, ushort>, ushort> ParseResponse(byte[] response)
        {
            Dictionary<Tuple<PointType, ushort>, ushort> dict = new Dictionary<Tuple<PointType, ushort>, ushort>();
            ModbusWriteCommandParameters parameters = this.CommandParameters as ModbusWriteCommandParameters;

            if (response[7] == CommandParameters.FunctionCode + 0x80)
            {
                HandeException(response[8]);
            }

            ushort address = BitConverter.ToUInt16(new byte[2] { response[9], response[8] }, 0);
            ushort rawValue = BitConverter.ToUInt16(new byte[2] { response[11], response[10] }, 0);

            ushort normalizedValue = rawValue == 0xFF00 ? (ushort)1 : (ushort)0;

            dict.Add(new Tuple<PointType, ushort>(PointType.DIGITAL_OUTPUT, address), normalizedValue);
            return dict;
        }
    }
}