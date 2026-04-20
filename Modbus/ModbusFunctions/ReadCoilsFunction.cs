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
            ModbusReadCommandParameters p = (ModbusReadCommandParameters)CommandParameters;

            byte[] request = new byte[12];

            request[0] = (byte)(p.TransactionId >> 8);
            request[1] = (byte)(p.TransactionId & 0xFF);

            request[2] = (byte)(p.ProtocolId >> 8);
            request[3] = (byte)(p.ProtocolId & 0xFF);

            request[4] = (byte)(p.Length >> 8);
            request[5] = (byte)(p.Length & 0xFF);

            request[6] = p.UnitId;

            request[7] = p.FunctionCode;

            request[8] = (byte)(p.StartAddress >> 8);
            request[9] = (byte)(p.StartAddress & 0xFF);

            request[10] = (byte)(p.Quantity >> 8);
            request[11] = (byte)(p.Quantity & 0xFF);

            return request;
        }

        /// <inheritdoc />
        public override Dictionary<Tuple<PointType, ushort>, ushort> ParseResponse(byte[] response)
        {
            if (response[7] == (byte)(CommandParameters.FunctionCode + 0x80))
            {
                HandeException(response[8]);
            }

            ModbusReadCommandParameters p = (ModbusReadCommandParameters)CommandParameters;
            Dictionary<Tuple<PointType, ushort>, ushort> result = new Dictionary<Tuple<PointType, ushort>, ushort>();

            byte byteCount = response[8];

            for (int i = 0; i < p.Quantity; i++)
            {
                int dataByteIndex = 9 + (i / 8);
                int bitIndex = i % 8;

                if (dataByteIndex >= 9 + byteCount)
                    break;

                ushort value = (ushort)((response[dataByteIndex] >> bitIndex) & 0x01);
                ushort address = (ushort)(p.StartAddress + i);

                result.Add(Tuple.Create(PointType.DIGITAL_OUTPUT, address), value);
            }

            return result;
        }
    }
}