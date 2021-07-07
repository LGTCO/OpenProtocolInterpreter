using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using OpenProtocolInterpreter.Converters;
using OpenProtocolInterpreter.Result;

namespace OpenProtocolInterpreter.TighteningResults
{
    //public class Mid0901 : Mid, ITighteningResults, IController
    //{

    //    public Mid0901():base(MID, LAST_REVISION)
    //    {
    //    }

    //    protected override Dictionary<int, List<DataField>> RegisterDatafields()
    //    {
    //        return new Dictionary<int, List<DataField>>()
    //        {
    //            {
    //              1, new List<DataField>()
    //              {
    //                 new DataField((int)DataFields.RESULT_DATA_IDENTIFIER,20,10,'0',DataField.PaddingOrientations.LEFT_PADDED,false),
    //                 new DataField((int)DataFields.TIME_STAMP,30,19,'0',DataField.PaddingOrientations.LEFT_PADDED,false),
    //                 new DataField((int)DataFields.NUMBER_OF_PIDS,42,3,'0',DataField.PaddingOrientations.LEFT_PADDED,false),
    //              }            
    //            },
    //        };
    //    }


    //    public override string Pack()
    //    {
    //        return base.Pack();
    //    }

    //    public override Mid Parse(byte[] package)
    //    {
    //        var asciiLength = 0;
    //        for (int i = 0; i < package.Length; i++)
    //        {
    //            // found ASCII delimiter
    //            if (package[i] == 0x00)
    //            {
    //                asciiLength = i + 1; // plus 1 to include null terminator/delimiter
    //                break;
    //            }
    //        }

    //        var asciiMessage = System.Text.Encoding.ASCII.GetString(package, 0, asciiLength);

    //        // Parse the 20 byte header
    //        HeaderData = ProcessHeader(asciiMessage);

    //        var numberOfPidDataFields = GetField(1, (int)DataFields.NUMBER_OF_PIDS);
    //        var numPids = GetValue(numberOfPidDataFields, asciiMessage);

    //        if ((Convert.ToInt32(numPids) > 0))
    //        {
    //            dataFieldListField.Index = numberOfPidDataFields.Index + numberOfPidDataFields.Size;
    //            dataFieldListField.Size = asciiMessage.Length - dataFieldListField.Index;
    //        }


    //        var numberOfPidDataFields2 = GetField(1, (int)DataFields.NUMBER_OF_PID_DATA_FIELDS2);
    //        var dataFieldListField2 = GetField(1, (int)DataFields.VARIABLE_DATA_FIELDS);

    //        if (!Int32.TryParse(GetValue(numberOfPidDataFields2, asciiMessage), out int numPids2))
    //            throw new Exception("Cannot parse Variable DataField (2)");

    //        // We don't know the total length of the all datafields so GetDataFieldsByFieldCount loops through 
    //        // n datafields and accumalates the total length
    //        if (numPids2 > 0)
    //        {
    //            dataFieldListField2.Index = numberOfPidDataFields2.Index + numberOfPidDataFields2.Size;
    //            VariableDataFields = GetDataFieldsByFieldCount(asciiMessage.Substring(dataFieldListField2.Index), Convert.ToInt32(numPids2), out int endLength);
    //            dataFieldListField2.Size = endLength;

    //            // Parse the datafieldList. Look up coefficient 
    //            var coefficientPID = VariableDataFields.Where(field => field.ParameterId == 02213 || field.ParameterId == 02214).FirstOrDefault(); ;
    //            if (!Double.TryParse(coefficientPID.RealValue, out double coeff))
    //                throw new Exception("Coefficient not found!");

    //            if (!Enum.TryParse(coefficientPID.ParameterId.ToString(), out CoefficientOperation op))
    //                throw new Exception("Coefficient operation type not found!");

    //            Coefficient = coeff;
    //            OperationType = op;
    //        }

    //        // defining this at runtime
    //        var numberOfResolutionFields = GetField(1, (int)DataFields.NUMBER_OF_RESOLUTION_FIELDS);
    //        numberOfResolutionFields.Index = dataFieldListField2.Index + dataFieldListField2.Size;
    //        if (!Int32.TryParse(GetValue(numberOfResolutionFields, asciiMessage), out int numberOfResolutionValue))
    //            throw new Exception("Cannot parse Number of Resolutions DataField");


    //        // There is a resolution field
    //        var resolutionField = GetField(1, (int)DataFields.RESOLUTION_FIELDS);
    //        if (numberOfResolutionValue > 0)
    //        {
    //            // Get field, set index to end of datafieldList2. Set size to end length of the resolutionfields
    //            resolutionField.Index = numberOfResolutionFields.Index + numberOfResolutionFields.Size;
    //            ResolutionFields = GetResolutionFieldsByFieldCount(asciiMessage.Substring(resolutionField.Index), numberOfResolutionValue, out int endLength);
    //            resolutionField.Size = endLength;
    //        }
    //        else
    //        {
    //            throw new Exception("No resolution datafield found!");
    //        }

    //        var numberOfTraceSamples = GetField(1, (int)DataFields.NUMBER_OF_TRACE_SAMPLES);
    //        numberOfTraceSamples.Index = resolutionField.Index + resolutionField.Size;

    //        var nullCharacter = GetField(1, (int)DataFields.NUL_CHAR);
    //        nullCharacter.Index = numberOfTraceSamples.Index + numberOfTraceSamples.Size;

    //        // Substring all the fields into their correct property. Once this is called. All the properties are set based on the index and size given
    //        ProcessDataFields(asciiMessage);
    //        // END OF ASCII DATA. 

    //        // START OF BINARY DATA

    //        // Store the raw binary data
    //        RawTraceSamples = new byte[package.Length - asciiLength];
    //        Array.Copy(package, asciiLength, RawTraceSamples, 0, package.Length - asciiLength);

    //        // Convert raw binary into 2 byte trace samples
    //        TraceSamples = new float[RawTraceSamples.Length / 2];
    //        for (int i = 0, j = 0; j < TraceSamples.Length; i += 2, j++)
    //        {
    //            TraceSamples[j] = ((short)RawTraceSamples[i] << 8 | (ushort)RawTraceSamples[i + 1]);
    //        }

    //        return this;
    //    }




    //    public List<VariableDataField> VariableDataFields { get; set; }

    //    public enum DataFields
    //    {
    //        RESULT_DATA_IDENTIFIER,
    //        TIME_STAMP,
    //        NUMBER_OF_PIDS,
    //    }

    //    private readonly Int32Converter _int32Converter;
    //    private readonly DateConverter _dateConverter;


    //    private const int LAST_REVISION = 1;
    //    public const int MID = 901;
    //}

}
