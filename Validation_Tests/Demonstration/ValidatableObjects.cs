using System;
using System.Collections.Generic;

namespace Validations_Tests.Demonstration
{
    public struct ValidationStruct
    {
        public int Integer { get; }
    }

    public class ValidationClass
    {
        public int Integer { get; }
    }

    public class BasicValidatableObject
    {
        public int Integer { get; set; } = 0;
        public string String { get; set; } = string.Empty;

        public List<int> IntegerList { get; set; } = new() { 1, 2, 3, 4, 5 };
        public List<ValidationClass> ItemList { get; set; } = new();
    }

    public class AdvancedValidatableObject
    {
        public int Integer { get; set; } = 0;
        public string String { get; set; } = string.Empty;
        public object Object { get; set; } = new object();
        public decimal Decimal { get; set; } = decimal.MaxValue;
        public double Double { get; set; } = double.MaxValue;
        public short Short { get; set; } = short.MaxValue;
        public long Long { get; set; } = long.MaxValue;
        public Type Type { get; set; } = typeof(AllFieldTypesDto);
        public Tuple<int, int> TwoComponentTupple { get; set; } = Tuple.Create(1, 2);
        public (int one, int two) TwoComponentNewTupple { get; set; } = (0, 0);

        public List<AllFieldTypesDto> AllFieldTypesList { get; set; } = new();
        public LinkedList<AllFieldTypesDto> AllFieldTypesLinkedList { get; set; } = new();
        public IEnumerable<AllFieldTypesDto> AllFieldTypesIEnumerable { get; set; } = new List<AllFieldTypesDto>();
        public Dictionary<string, AllFieldTypesDto> AllFieldTypesDictionary { get; set; } = new();
        public ValidationStruct Struct { get; set; } = new();
    }
}
