using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace LearningGodot;

public partial class MonkeyBusiness : PuzzleNode
{
    private int _rounds = 10_000;
    private readonly List<Monkey> _monkeys = new();
    
    public override void _Ready()
    {
        bool startOfMonkeyRecord = true;
        Monkey currentMonkey = null;
        
        // Parse and create monkeys
        foreach (string input in InputReader.ReadInput(11))
        {
            string line = input.ToLower();
            
            if (line == string.Empty)
            {
                startOfMonkeyRecord = true;
                continue;
            }
            
            if (startOfMonkeyRecord)
            {
                currentMonkey = new Monkey();
                _monkeys.Add(currentMonkey);
                startOfMonkeyRecord = false;
            }
            else if (line.Contains("starting"))
            {
                var items = line.Split(':')[1].Trim().Split(',');
                foreach (string item in items)
                {
                    currentMonkey.Items.Enqueue(long.Parse(item));
                }
            }
            else if (line.Contains("operation"))
            {
                var expression = line.Split('=')[1].Trim();
                currentMonkey.SetExpression(expression);
            }
            else if (line.Contains("test"))
            {
                var divisor = line.Split("by")[1].Trim();
                currentMonkey.Divisor = int.Parse(divisor);
            }
            else if (line.Contains("true"))
            {
                var monkeyToThrowTo = int.Parse(line.Split(' ')[^1]);
                currentMonkey.OnTrue = monkeyToThrowTo;
            }
            else if (line.Contains("false"))
            {
                var monkeyToThrowTo = int.Parse(line.Split(' ')[^1]);
                currentMonkey.OnFalse = monkeyToThrowTo;
            }
        }

        // Create a value that will be used to modulus with super large numbers.
        // It is the accumulated value of all of the test divisors, which creates a value that
        // after subsequent inspections, will always result in a divisible worry value.
        int mod = 1;
        foreach (Monkey monkey in _monkeys)
        {
            mod *= monkey.Divisor;
        }
        
        // Process monkey actions
        for (int i = 0; i < _rounds; i++)
        {
            foreach (Monkey monkey in _monkeys)
            {
                while (monkey.Items.TryDequeue(out long item))
                {
                    long moddedItem = item % mod;
                    long worry = monkey.Inspect(moddedItem);
                    int newMonkey = monkey.TestResponse(worry);

                    _monkeys[newMonkey].Items.Enqueue(worry);
                }
            }
        }
        
        _monkeys.Sort((a, b) =>
        {
            if (a.TotalInspections > b.TotalInspections)
            {
                return 1;
            }

            if (a.TotalInspections < b.TotalInspections)
            {
                return -1;
            }

            return 0;
        });

        var topMonkey = _monkeys[^1];
        var secondTopMonkey = _monkeys[^2];

        long totalInspections = topMonkey.TotalInspections * secondTopMonkey.TotalInspections;
        
        Print($"Total inspections: {totalInspections:N0}");
    }

    private class Monkey
    {
        private readonly Expression _expression = new();
        
        public long TotalInspections { get; private set; }

        public Queue<long> Items { get; } = new();
        
        public int Divisor { get; set; }
        
        public int OnTrue { get; set; }
        
        public int OnFalse { get; set; }

        public int TestResponse(long worry)
        {
            if (worry % Divisor == 0)
            {
                return OnTrue;
            }

            return OnFalse;
        }

        public long Inspect(long item)
        {
            TotalInspections++;

            // return _expression(item);
            return (long)_expression.Execute(new Godot.Collections.Array { item });
        }

        public void SetExpression(string expression)
        {
            var error = _expression.Parse(expression, new [] { "old" });
            if (error != Error.Ok)
            {
                throw new ArgumentException($"Expression {expression} is not valid.", nameof(expression));
            }
        }
    }
}