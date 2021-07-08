using OpenProtocolInterpreter.Result;
using OpenProtocolInterpreter.TighteningResults;
using System.Collections.Generic;

namespace OpenProtocolInterpreter.Converters
{
    public class VariableDataFieldListConverter : AsciiConverter<IEnumerable<VariableDataField>>
    {
        private readonly IValueConverter<int> _intConverter;

        public VariableDataFieldListConverter(IValueConverter<int> intConverter)
        {
            _intConverter = intConverter;
        }

        public override IEnumerable<VariableDataField> Convert(string value)
        {
            int length = 0;
            for (int i = 0; i < value.Length; i += 17 + length)
            {
                length = _intConverter.Convert(value.Substring(i + 5, 3));
                yield return new VariableDataField()
                {
                    ParameterId = _intConverter.Convert(value.Substring(i, 5)),
                    Length = length,
                    DataType = _intConverter.Convert(value.Substring(i + 8, 2)),
                    Unit = _intConverter.Convert(value.Substring(i + 10, 3)),
                    StepNumber = _intConverter.Convert(value.Substring(i + 13, 4)),
                    RealValue = value.Substring(i + 17, length)
                };
            }
        }

        public override string Convert(IEnumerable<VariableDataField> value)
        {
            string pack = string.Empty;
            foreach (var v in value)
            {
                pack += _intConverter.Convert('0', 5, DataField.PaddingOrientations.LEFT_PADDED, v.ParameterId);
                pack += _intConverter.Convert('0', 3, DataField.PaddingOrientations.LEFT_PADDED, v.Length);
                pack += _intConverter.Convert('0', 2, DataField.PaddingOrientations.LEFT_PADDED, v.DataType);
                pack += _intConverter.Convert('0', 3, DataField.PaddingOrientations.LEFT_PADDED, v.Unit);
                pack += _intConverter.Convert('0', 4, DataField.PaddingOrientations.LEFT_PADDED, v.StepNumber);
                pack += GetPadded(' ', 1, DataField.PaddingOrientations.RIGHT_PADDED, v.RealValue);
            }
            
            return pack;
        }

        public override string Convert(char paddingChar, int size, DataField.PaddingOrientations orientation, IEnumerable<VariableDataField> value) => Convert(value);
    }

    public class ResolusionConverter: AsciiConverter<IEnumerable<ResolutionField>>
    {
        public ResolusionConverter(IValueConverter<int> intConverter)
        {
            _intConverter = intConverter;
        }


        public override IEnumerable<ResolutionField> Convert(string value)
        {
            int length = 0;
            for (int j = 0; j < value.Length; j += 18 + length)
            {
                length = _intConverter.Convert(value.Substring(j + 10, 3));
                var rf = new ResolutionField();
                rf.FirstIndex = _intConverter.Convert(value.Substring(j, 5));
                rf.LastIndex = _intConverter.Convert(value.Substring(j + 5, 5));
                rf.Length = length;
                rf.DataType = _intConverter.Convert(value.Substring(j + 13, 2));
                rf.Unit = _intConverter.Convert(value.Substring(j + 15, 3));
                rf.TimeValue = double.Parse(value.Substring(j + 18, length));

                yield return rf;
            }
        }


        public override string Convert(IEnumerable<ResolutionField> value)
        {
            string pack = string.Empty;
            foreach (var v in value)
            {
                pack += _intConverter.Convert('0', 5, DataField.PaddingOrientations.LEFT_PADDED, v.FirstIndex);
                pack += _intConverter.Convert('0', 3, DataField.PaddingOrientations.LEFT_PADDED, v.LastIndex);
                pack += _intConverter.Convert('0', 2, DataField.PaddingOrientations.LEFT_PADDED, v.Length);
                pack += _intConverter.Convert('0', 3, DataField.PaddingOrientations.LEFT_PADDED, v.Unit);
                //pack += _intConverter.Convert('0', v.Length, DataField.PaddingOrientations.LEFT_PADDED, v.TimeValue);
                pack += v.TimeValue.ToString().PadLeft(v.Length, '0');
            }
            return pack;
        }

        public override string Convert(char paddingChar, int size, DataField.PaddingOrientations orientation, IEnumerable<ResolutionField> value) => Convert(value);

        private readonly IValueConverter<int> _intConverter;
    }
}
