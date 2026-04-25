using Common;
using Modbus.FunctionParameters;
using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;

namespace Modbus.ModbusFunctions
{
    /// <summary>
    /// Class containing logic for parsing and packing modbus write single register functions/requests.
    /// </summary>
    public class WriteSingleRegisterFunction : ModbusFunction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WriteSingleRegisterFunction"/> class.
        /// </summary>
        /// <param name="commandParameters">The modbus command parameters.</param>
        public WriteSingleRegisterFunction(ModbusCommandParameters commandParameters) : base(commandParameters)
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
            Buffer.BlockCopy(
                (Array)BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)parameters.Value)),
                0, request, 10, 2
            );

            return request;


        }

        /// <inheritdoc />
        public override Dictionary<Tuple<PointType, ushort>, ushort> ParseResponse(byte[] response)
        {
            Dictionary<Tuple<PointType, ushort>, ushort> dictionary = new Dictionary<Tuple<PointType, ushort>, ushort>();

            if (response[7] == CommandParameters.FunctionCode + 0x80)
            {
                HandeException(response[8]);
            }

            ushort address = BitConverter.ToUInt16(new byte[2] { response[9], response[8] }, 0);
            ushort value = BitConverter.ToUInt16(new byte[2] { response[11], response[10] }, 0);

            dictionary.Add(new Tuple<PointType, ushort>(PointType.ANALOG_OUTPUT, address), value);

            return dictionary;
        }
    }
}