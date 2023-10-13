using System;
using System.Collections.Generic;
using Godot;

namespace LearningGodot;

public partial class DistressSignal : PuzzleNode
{
    public override void _Ready()
    {
        var pairs = new List<Packet>();
        
        // Parse input
        foreach (string line in InputReader.ReadInput(13))
        {
            if (line == string.Empty) continue;
            
            var packet = ParsePacket(line);

            pairs.Add(packet);
        }
        
        // Add divider packets
        var divider1 = ParsePacket("[[2]]");
        var divider2 = ParsePacket("[[6]]");
        
        pairs.Add(divider1);
        pairs.Add(divider2);
        
        pairs.Sort();

        int startIndex = pairs.IndexOf(divider1);
        int endIndex = pairs.IndexOf(divider2);

        int decoderKey = (startIndex + 1) * (endIndex + 1);
        
        Print($"Decoder Key: {decoderKey}");

        return;

        Packet ParsePacket(string line)
        {
            Packet root = null;
            Packet current = null;
            
            bool parsingValues = false;
            string currentValue = string.Empty;

            foreach (char symbol in line)
            {
                if (symbol == '[')
                {
                    // Opening new list
                    if (root == null)
                    {
                        root = new Packet();
                        current = root;
                        continue;
                    }

                    current = new Packet(current);
                }
                else if (symbol == ']')
                {
                    // Closing a list
                    if (parsingValues)
                    {
                        parsingValues = false;

                        // Add last value and then assign values
                        if (int.TryParse(currentValue, out int parsedValue))
                        {
                            current!.Add(parsedValue);
                        }

                        currentValue = string.Empty;
                    }

                    current = current!.Parent;
                }
                else if (symbol == ',')
                {
                    if (!parsingValues) continue;

                    // Compiling values
                    if (int.TryParse(currentValue, out int parsedValue))
                    {
                        current!.Add(parsedValue);
                    }

                    currentValue = string.Empty;
                }
                else
                {
                    parsingValues = true;

                    currentValue += symbol;
                }
            }

            return root;
        }
    }

    private class Packet : IComparable<Packet>, IComparer<Packet>
    {
        private readonly List<object> _data = new();

        public Packet(Packet parent = null)
        {
            Parent = parent;
            Parent?.Add(this);
        }
        
        public Packet Parent { get; }

        private bool TryGetValue(int i, out object value)
        {
            value = null;
            
            try
            {
                value = _data[i];
                return true;
            }
            catch (ArgumentOutOfRangeException) { }

            return false;
        }

        public void Add(object data)
        {
            _data.Add(data);
        }

        public int Compare(Packet a, Packet b)
        {
            return ComparePackets(a, b, 0);
        }
        
        private int ComparePackets(Packet a, Packet b, int index = 0)
        {
            int order = 0;
            while (true)
            {
                bool aHasValue = a.TryGetValue(index, out object value1);
                bool bHasValue = b.TryGetValue(index, out object value2);
                    
                if (aHasValue && bHasValue)
                {
                    if (value1 is int int1 && value2 is int int2)
                    {
                        // If both values are ints, than process them now.
                        order = int1.CompareTo(int2);
                    }
                    else
                    {
                        // If not, at least one is a packet, so convert the one that isn't to a packet.
                        if (value1 is not Packet packet1)
                        {
                            packet1 = new Packet();
                            packet1.Add((int)value1);
                        }

                        if (value2 is not Packet packet2)
                        {
                            packet2 = new Packet();
                            packet2.Add((int)value2);
                        }

                        order = ComparePackets(packet1, packet2);
                    }
                        
                    index++;

                    if (order != 0)
                    {
                        break;
                    }

                    continue;
                }

                if (!aHasValue && !bHasValue)
                {
                    return 0;
                }

                if (!aHasValue)
                {
                    return -1;
                }

                return 1;
            }

            return order;
        }

        public int CompareTo(Packet other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (ReferenceEquals(null, other)) return 1;
            
            return Compare(this, other);
        }
    }
}