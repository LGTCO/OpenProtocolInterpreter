using System;
using System.Collections.Generic;
using OpenProtocolInterpreter.Converters;
using OpenProtocolInterpreter.Result;
using System.Linq;
using System.Text;

namespace OpenProtocolInterpreter.TighteningResults
{
    public class Mid0900 : Mid, ITighteningResults, IController
    {
        private readonly IValueConverter<int> _intConverter;
        private readonly IValueConverter<IEnumerable<VariableDataField>> _variableDataFieldListConverter;
        private readonly IValueConverter<DateTime> _dateConverter;
        private readonly IValueConverter<IEnumerable<ResolutionField>> _resolusionConverter;


        private const int LAST_REVISION = 1;
        public const int MID = 900;

        public Mid0900() : base(MID, LAST_REVISION)
        {
            _intConverter = new Int32Converter();
            _dateConverter = new DateConverter();
            _variableDataFieldListConverter = new VariableDataFieldListConverter(_intConverter);
            _resolusionConverter = new ResolusionConverter(_intConverter);

            PidDatas = new List<VariableDataField>();
            ParameterDatas = new List<VariableDataField>();
            ResolutionDatas = new List<ResolutionField>();
        }

        // DEV NOTE: All fields with strings are left adjusted and padded with spaces. All numerical fields are right adjusted and padded with 0's
        protected override Dictionary<int, List<DataField>> RegisterDatafields()
        {
            return new Dictionary<int, List<DataField>>()
            {
                {
                    1, new List<DataField>()
                    {
                        new DataField((int) DataFields.RESULT_DATA_IDENTIFIER, 20, 10, '0', DataField.PaddingOrientations.LEFT_PADDED, false),
                        new DataField((int) DataFields.TIME_STAMP, 30, 19, '0', DataField.PaddingOrientations.LEFT_PADDED, false),
                        new DataField((int) DataFields.NUMBER_OF_PID_DATA_FIELDS, 49, 3, '0', DataField.PaddingOrientations.LEFT_PADDED, false),
                        new DataField((int)DataFields.PID_DATA,0,0,false),
                        new DataField((int) DataFields.TRACE_TYPE, 52, 2, '0', DataField.PaddingOrientations.LEFT_PADDED, false), // potential will be a problem as index 52 might be correct is datafields isnt 000 and is defined
                        new DataField((int) DataFields.TRANSDUCER_TYPE, 54, 2, '0', DataField.PaddingOrientations.LEFT_PADDED, false),
                        new DataField((int) DataFields.Unit, 56, 3, '0', DataField.PaddingOrientations.LEFT_PADDED, false),
                        new DataField((int) DataFields.NUMBER_OF_PARAMETER_DATA_FIELDS, 59, 3, '0', DataField.PaddingOrientations.LEFT_PADDED, false),
                        new DataField((int)DataFields.PARAMETER_DATA, 0, 0, false),
                        new DataField((int) DataFields.NUMBER_OF_RESOLUTION_FIELDS, 0, 3, '0', DataField.PaddingOrientations.LEFT_PADDED, false), // defined at runtime because unknown data field length that before it
                        new DataField((int)DataFields.RESOLUTION_DATA, 0, 0, false), //defined at runtime
                        new DataField((int) DataFields.NUMBER_OF_TRACE_SAMPLES, 0, 5, '0', DataField.PaddingOrientations.LEFT_PADDED, false), // defined at rrunetime because unknown resolution field length before it
                        new DataField((int) DataFields.NUL_CHAR, 0, 1, false), // delimits ascii data from binary. // defined at runtime
                        new DataField((int)DataFields.TRACE_SAMPLE, 0, 0, false),
                    }
                }
            };
        }

        public override string Pack()
        {
            NumberOfPIDDataFields = PidDatas.Count;
            var pidSize = 0;
            foreach (var item in PidDatas)
            {
                pidSize = pidSize + item.Length + 17;
            }

            NumberOfParameterDataFields = ParameterDatas.Count;
            var parameterSize = 0;
            foreach (var item in ParameterDatas)
            {
                parameterSize = parameterSize + item.Length + 17;
            }

            NumberOfResolutionFields = ResolutionDatas.Count;
            var resolutionSize = 0;
            foreach (var item in ResolutionDatas)
            {
                resolutionSize = resolutionSize + item.Length + 18;
            }

            NumberOfTraceSamples = RawTraceSamples.Length;
            var endIndex = 49;

            var pidFields = GetField(1, (int)DataFields.PID_DATA);
            pidFields.Index = endIndex;
            pidFields.Size = pidSize;
            pidFields.SetValue(_variableDataFieldListConverter.Convert(PidDatas));

            endIndex += pidFields.Size;

            var traceTypeFields = GetField(1, (int)DataFields.TRACE_TYPE);
            traceTypeFields.Index = endIndex;
            endIndex += traceTypeFields.Size;

            var transducerType = GetField(1, (int)DataFields.TRANSDUCER_TYPE);
            transducerType.Index = endIndex;
            endIndex += transducerType.Size;

            var unit = GetField(1, (int)DataFields.Unit);
            unit.Index = endIndex;
            endIndex += unit.Size;


            var parameterField = GetField(1, (int)DataFields.PARAMETER_DATA);
            parameterField.Index += endIndex;
            parameterField.Size = parameterSize;
            parameterField.SetValue(_variableDataFieldListConverter.Convert(ParameterDatas));
            endIndex += parameterSize;


            var resolusionField = GetField(1, (int)DataFields.RESOLUTION_DATA);
            resolusionField.Index = endIndex;
            resolusionField.Size = resolutionSize;
            resolusionField.SetValue(_resolusionConverter.Convert(ResolutionDatas));
            endIndex += resolutionSize;

            var traceSample = GetField(1, (int)DataFields.NUMBER_OF_TRACE_SAMPLES);
            traceSample.Index = endIndex;
            endIndex += traceSample.Size;

            var nul = GetField(1, (int)DataFields.NUL_CHAR);
            nul.Index = endIndex;
            endIndex += nul.Size;



            ConvertToBinary();


            var traceField = GetField(1, (int)DataFields.TRACE_SAMPLE);
            traceField.Index = endIndex;
            if (BinaryTraceSamples != null)
            {
                traceField.Size = BinaryTraceSamples.Length;
            }

            if (!RevisionsByFields.Any())
                return BuildHeader();

            StringBuilder package = new StringBuilder();
            package.Append(BuildHeader());

            int prefixIndex = 1;
            for (int i = 1; i <= (HeaderData.Revision > 0 ? HeaderData.Revision : 1); i++)
                package.Append(Pack(RevisionsByFields[i], ref prefixIndex));

            return package.ToString();
        }


        public override Mid Parse(byte[] package)
        {
            var asciiLength = GetAsciiLength(package);
            var asciiMessage = System.Text.Encoding.ASCII.GetString(package, 0, asciiLength);

            // Parse the 20 byte header
            HeaderData = ProcessHeader(asciiMessage);

            var endIndex = 49;
            ProcessAsciiData(endIndex, asciiMessage);


            ProcessDataFields(asciiMessage);
            // END OF ASCII DATA. 

            // Get Coeffiecient
            GetCoefficient();
            // START OF BINARY DATA

            // Store the raw binary data
            BinaryTraceSamples = new byte[package.Length - asciiLength];
            Array.Copy(package, asciiLength, BinaryTraceSamples, 0, package.Length - asciiLength);

            // Convert raw binary into 2 byte trace samples
            ProcessBinaryData(BinaryTraceSamples);

            return this;
        }

        public override Mid Parse(string package)
        {
            var asciiLength = GetAsciiLength(package);

            //前半部分
            var asciiMessage = package.Substring(0, asciiLength); //(package, 0, asciiLength);

            // Parse the 20 byte header
            HeaderData = ProcessHeader(asciiMessage);

            int endIndex = 49;
            ProcessAsciiData(endIndex, asciiMessage);

            ProcessDataFields(asciiMessage);
            // END OF ASCII DATA. 

            // Get Coeffiecient
            GetCoefficient();
            // START OF BINARY DATA

            // Store the raw binary data
            BinaryTraceSamples = ToBytes(package.Substring(asciiLength, package.Length - asciiLength));

            // Convert raw binary into 2 byte trace samples
            ProcessBinaryData(BinaryTraceSamples);

            return this;
        }

        private int GetAsciiLength(dynamic package)
        {
            var asciiLength = 0;
            for (int i = 0; i < package.Length; i++)
            {
                // found ASCII delimiter
                if (package[i] == 0x00)
                {
                    asciiLength = i + 1; // plus 1 to include null terminator/delimiter
                    break;
                }
            }

            return asciiLength;
        }

        private void ProcessAsciiData(int index, string asciiMessage)
        {
            var endIndex = index;
            var numberOfPidDataFields = GetField(1, (int)DataFields.NUMBER_OF_PID_DATA_FIELDS);
            endIndex += numberOfPidDataFields.Size;

            var numPids = _intConverter.Convert(GetValue(numberOfPidDataFields, asciiMessage));


            if (numPids > 0)
            {
                var dataFieldListField = GetField(1, (int)DataFields.PID_DATA);
                dataFieldListField.Index = numberOfPidDataFields.Index + numberOfPidDataFields.Size;

                var fieldLen = _intConverter.Convert(asciiMessage.Substring(dataFieldListField.Index + 5, 3));
                endIndex = numPids * fieldLen;

                dataFieldListField.Size = endIndex;

                PidDatas = _variableDataFieldListConverter.Convert(GetValue(dataFieldListField, asciiMessage)).ToList();
            }

            var traceTypeField = GetField(1, (int)DataFields.TRACE_TYPE);
            traceTypeField.Index = endIndex;
            endIndex += traceTypeField.Size;


            var transducerField = GetField(1, (int)DataFields.TRANSDUCER_TYPE);
            transducerField.Index = endIndex;
            endIndex += transducerField.Size;

            var unitField = GetField(1, (int)DataFields.Unit);
            unitField.Index = endIndex;
            endIndex += unitField.Size;

            var numberofParameters = GetField(1, (int)DataFields.NUMBER_OF_PARAMETER_DATA_FIELDS);
            numberofParameters.Index = endIndex;
            endIndex += numberofParameters.Size;

            var numParameters = _intConverter.Convert(GetValue(numberofParameters, asciiMessage));

            if (numParameters > 0)
            {
                var parameterListField = GetField(1, (int)DataFields.PARAMETER_DATA);
                parameterListField.Index = endIndex;

                var fieldLen = _intConverter.Convert(asciiMessage.Substring(parameterListField.Index + 5, 3)) + 17;
                endIndex += numParameters * fieldLen;
                parameterListField.Size = numParameters * fieldLen;

                ParameterDatas = _variableDataFieldListConverter.Convert(GetValue(parameterListField, asciiMessage)).ToList();
            }

            var numberofResolutionFields = GetField(1, (int)DataFields.NUMBER_OF_RESOLUTION_FIELDS);
            numberofParameters.Index = endIndex;
            endIndex += numberofResolutionFields.Size;

            var numResolutions = _intConverter.Convert(GetValue(numberofResolutionFields, asciiMessage));
            if (numResolutions > 0)
            {
                var resolutionListFiled = GetField(1, (int)DataFields.RESOLUTION_DATA);
                resolutionListFiled.Index = endIndex;

                var fieldLen = _intConverter.Convert(asciiMessage.Substring(resolutionListFiled.Index + 10, 3)) + 18;
                endIndex += numParameters * fieldLen;
                resolutionListFiled.Size = numParameters * fieldLen;

                ResolutionDatas = _resolusionConverter.Convert(GetValue(resolutionListFiled, asciiMessage)).ToList();
            }

            var numberofTraceSampleField = GetField(1, (int)DataFields.NUMBER_OF_TRACE_SAMPLES);
            numberofTraceSampleField.Index = endIndex;

            endIndex += numberofTraceSampleField.Size;

            var nulCharacterField = GetField(1, (int)DataFields.NUL_CHAR);
            nulCharacterField.Index = endIndex;
            endIndex += nulCharacterField.Size;
        }

        private void ProcessBinaryData(byte[] traceDatas)
        {
            RawTraceSamples = new int[traceDatas.Length / 2];
            for (int i = 0, j = 0; j < RawTraceSamples.Length; i += 2, j++)
            {
                RawTraceSamples[j] = (traceDatas[i] << 8 | traceDatas[i + 1]);
            }
        }


        private void ConvertToBinary()
        {
            if (RawTraceSamples == null)
            {
                return;
            }

            BinaryTraceSamples = new byte[RawTraceSamples.Length * 2];
            var index = 0;
            for (int i = 0; i < RawTraceSamples.Length; i++)
            {
                BinaryTraceSamples[index++] = (byte)(RawTraceSamples[i] >> 8);
                BinaryTraceSamples[index++] = (byte)RawTraceSamples[i];
            }
        }



        private void GetCoefficient()
        {
            var coefficientPID = ParameterDatas.Where(field => field.ParameterId == 02213 || field.ParameterId == 02214).FirstOrDefault();
            if (coefficientPID == null)
            {
                return;
            }

            if (!Double.TryParse(coefficientPID.RealValue, out double coeff))
                throw new Exception("Coefficient not found!");

            if (!Enum.TryParse(coefficientPID.ParameterId.ToString(), out CoefficientOperation op))
                throw new Exception("Coefficient operation type not found!");

            Coefficient = coeff;
            OperationType = op;
        }

        public int ResultDataIdentifier
        {
            get => GetField(1, (int)DataFields.RESULT_DATA_IDENTIFIER).GetValue(_intConverter.Convert);
            set => GetField(1, (int)DataFields.RESULT_DATA_IDENTIFIER).SetValue(_intConverter.Convert, value);
        }
        public DateTime TimeStamp
        {
            get => GetField(1, (int)DataFields.TIME_STAMP).GetValue(_dateConverter.Convert);
            set => GetField(1, (int)DataFields.TIME_STAMP).SetValue(_dateConverter.Convert, value);
        }

        public int NumberOfPIDDataFields
        {
            get => GetField(1, (int)DataFields.NUMBER_OF_PID_DATA_FIELDS).GetValue(_intConverter.Convert);
            set => GetField(1, (int)DataFields.NUMBER_OF_PID_DATA_FIELDS).SetValue(_intConverter.Convert, value);
        }

        public int NumberOfParameterDataFields
        {
            get => GetField(1, (int)DataFields.NUMBER_OF_PARAMETER_DATA_FIELDS).GetValue(_intConverter.Convert);
            set => GetField(1, (int)DataFields.NUMBER_OF_PARAMETER_DATA_FIELDS).SetValue(_intConverter.Convert, value);
        }

        public List<VariableDataField> PidDatas { get; set; }
        public List<VariableDataField> ParameterDatas { get; set; }

        public List<ResolutionField> ResolutionDatas { get; set; }

        public int TraceType
        {
            get => GetField(1, (int)DataFields.TRACE_TYPE).GetValue(_intConverter.Convert);
            set => GetField(1, (int)DataFields.TRACE_TYPE).SetValue(_intConverter.Convert, value);
        }

        public int TransducerType
        {
            get => GetField(1, (int)DataFields.TRANSDUCER_TYPE).GetValue(_intConverter.Convert);
            set => GetField(1, (int)DataFields.TRANSDUCER_TYPE).SetValue(_intConverter.Convert, value);
        }

        public int Unit
        {
            get => GetField(1, (int)DataFields.Unit).GetValue(_intConverter.Convert);
            set => GetField(1, (int)DataFields.Unit).SetValue(_intConverter.Convert, value);
        }

        public int NumberOfResolutionFields
        {
            get => GetField(1, (int)DataFields.NUMBER_OF_RESOLUTION_FIELDS).GetValue(_intConverter.Convert);
            set => GetField(1, (int)DataFields.NUMBER_OF_RESOLUTION_FIELDS).SetValue(_intConverter.Convert, value);
        }

        public int NumberOfTraceSamples
        {
            get => GetField(1, (int)DataFields.NUMBER_OF_TRACE_SAMPLES).GetValue(_intConverter.Convert);
            set => GetField(1, (int)DataFields.NUMBER_OF_TRACE_SAMPLES).SetValue(_intConverter.Convert, value);
        }

        public int NullChar
        {
            get => GetField(1, (int)DataFields.NUL_CHAR).GetValue(_intConverter.Convert);
            set => GetField(1, (int)DataFields.NUL_CHAR).SetValue(_intConverter.Convert, value);
        }

        public double Coefficient { get; private set; }


        public byte[] BinaryTraceSamples { get; private set; }

        public float[] TraceSamples { get; private set; }

        public int[] RawTraceSamples { get; set; }

        public CoefficientOperation OperationType { get; private set; }

        public enum CoefficientOperation
        {
            NONE = 0,
            DIVISION = 02213,
            MULTIPLICATION = 02214
        }

        public enum DataFields
        {
            //rev 1
            RESULT_DATA_IDENTIFIER = 0,
            TIME_STAMP = 1,
            NUMBER_OF_PID_DATA_FIELDS = 2,
            PID_DATA = 3,
            TRACE_TYPE = 4,
            TRANSDUCER_TYPE = 5,
            Unit = 6,
            NUMBER_OF_PARAMETER_DATA_FIELDS = 7,
            PARAMETER_DATA = 8,
            NUMBER_OF_RESOLUTION_FIELDS = 9,
            RESOLUTION_DATA = 10,
            NUMBER_OF_TRACE_SAMPLES = 11,
            NUL_CHAR = 12,
            TRACE_SAMPLE = 13,
        }
    }
}