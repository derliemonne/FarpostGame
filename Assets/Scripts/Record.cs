using System;

public struct Record : IComparable<Record>
{
    public string Name;
    public int Place;
    public int MaxAltitude;
    public int CompareTo(Record obj) => Place.CompareTo(obj.Place);
}
