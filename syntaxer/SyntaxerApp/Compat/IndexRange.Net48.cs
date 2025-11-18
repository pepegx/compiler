namespace System
{
    public readonly struct Index
    {
        private readonly int _value; 
        public int Value { get { return (_value < 0) ? ~_value : _value; } }
        public bool IsFromEnd { get { return _value < 0; } }

        public Index(int value, bool fromEnd = false)
        {
            if (value < 0) throw new ArgumentOutOfRangeException("value");
            _value = fromEnd ? ~value : value;
        }

        public static Index Start { get { return new Index(0); } }
        public static Index End { get { return new Index(0, true); } }
        public static Index FromStart(int value) { return new Index(value); }
        public static Index FromEnd(int value) { return new Index(value, true); }

        public int GetOffset(int length)
        {
            int offset = IsFromEnd ? length - (~_value) : _value;
            return offset;
        }

        public override string ToString()
        {
            return IsFromEnd ? "^" + (~_value).ToString() : _value.ToString();
        }
    }

    public readonly struct Range
    {
        public Index Start { get; }
        public Index End { get; }

        public Range(Index start, Index end) { Start = start; End = end; }

        public static Range StartAt(Index start) { return new Range(start, Index.End); }
        public static Range EndAt(Index end) { return new Range(Index.Start, end); }
        public static Range All { get { return new Range(Index.Start, Index.End); } }

        public (int Start, int Length) GetOffsetAndLength(int length)
        {
            int start = Start.GetOffset(length);
            int end = End.GetOffset(length);
            return (start, end - start);
        }

        public override string ToString() { return Start.ToString() + ".." + End.ToString(); }
    }
}
