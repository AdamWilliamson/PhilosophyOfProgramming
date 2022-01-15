using System;
using System.Collections.Generic;

namespace Validations_Tests
{
    public struct FieldTestStruct
    {
        public int Integer { get; }
    }

    public class AllFieldTypesDto : IComparable
    {
        public int Integer { get; set; } = 0;
        public string String { get; set; } = String.Empty;
        public object Object { get; set; } = new object();
        public decimal Decimal { get; set; } = decimal.MaxValue;
        public double Double { get; set; } = double.MaxValue;
        public short Short { get; set; } = short.MaxValue;
        public long Long { get; set; } = long.MaxValue;
        public Type Type { get; set; } = typeof(AllFieldTypesDto);
        public Tuple<int, int> TwoComponentTupple { get; set; } = Tuple.Create(1, 2);
        public (int one, int two) TwoComponentNewTupple { get; set; } = (0, 0);
        //public AllFieldTypesDto AllFieldTypesDtoChild { get; set; } = new();

        public List<AllFieldTypesDto> AllFieldTypesList { get; set; } = new();
        public LinkedList<AllFieldTypesDto> AllFieldTypesLinkedList { get; set; } = new ();
        public IEnumerable<AllFieldTypesDto> AllFieldTypesIEnumerable { get; set; } = new List<AllFieldTypesDto>();
        public Dictionary<string, AllFieldTypesDto> AllFieldTypesDictionary { get; set; } = new();
        public FieldTestStruct Struct { get; set; } = new();

        private int compareToReturnValue = -1;
        public void CompareToValue(int value) 
        {
            compareToReturnValue = value;
        }

        public int CompareTo(object? obj)
        {
            return compareToReturnValue;
        }
    }
}
