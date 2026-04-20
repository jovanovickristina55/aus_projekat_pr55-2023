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
            ModbusWriteCommandParameters p = (ModbusWriteCommandParameters)CommandParameters;

            byte[] request = new byte[12];

            request[0] = (byte)(p.TransactionId >> 8);
            request[1] = (byte)(p.TransactionId & 0xFF);

            request[2] = (byte)(p.ProtocolId >> 8);
            request[3] = (byte)(p.ProtocolId & 0xFF);

            request[4] = (byte)(p.Length >> 8);
            request[5] = (byte)(p.Length & 0xFF);

            request[6] = p.UnitId;

            request[7] = p.FunctionCode;

            request[8] = (byte)(p.OutputAddress >> 8);
            request[9] = (byte)(p.OutputAddress & 0xFF);

            ushort value = p.Value == 1 ? (ushort)0xFF00 : (ushort)0x0000;

            request[10] = (byte)(value >> 8);
            request[11] = (byte)(value & 0xFF);

            return request;
        }

        /// <inheritdoc />
        public override Dictionary<Tuple<PointType, ushort>, ushort> ParseResponse(byte[] response)
        {
            if (response[7] == (byte)(CommandParameters.FunctionCode + 0x80))
            {
                HandeException(response[8]);
            }

            ushort address = (ushort)((response[8] << 8) | response[9]);
            ushort raw = (ushort)((response[10] << 8) | response[11]);

            ushort value = raw == 0xFF00 ? (ushort)1 : (ushort)0;

            return new Dictionary<Tuple<PointType, ushort>, ushort>
            {
                { Tuple.Create(PointType.DIGITAL_OUTPUT, address), value }
            };
        }
    }
}