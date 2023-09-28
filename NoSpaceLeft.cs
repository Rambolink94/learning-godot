using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace LearningGodot;

public partial class NoSpaceLeft : Node
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Directory root = new Directory("root", null);
		Directory current = root;
		
		const string baseDir = "/";
		int maxSize = 100_000;
		int runningTotal = 0;
		
		bool inListDir = false;
		int lineNumber = 0;
		foreach (string line in InputReader.ReadInput(7))
		{
			++lineNumber;
			
			// Split into command parts
			var commandParts = line.Split(' ');
			if (commandParts[0] == "$")
			{
				inListDir = false;
				
				// Get Command
				string commandToken = commandParts[1];
				if (commandToken == "cd")
				{
					// Change Directory
					string changeToken = commandParts[2];
					if (changeToken == baseDir)
					{
						// Return to base directory
						current = root;
					}
					else if (changeToken == "..")
					{
						// Traversed all sub-directories, so safe to get max size
						int totalSize = current.TotalSize;
						if (totalSize <= maxSize)
						{
							runningTotal += totalSize;
						}
						
						// Back one directory
						current = current.Parent;
					}
					else
					{
						// Going into directory
						current = current.ChangeDirectory(changeToken);
					}
				}
				else if (commandToken == "ls")
				{
					// List Directory
					inListDir = true;
				}
				else
				{
					throw new Exception($"Unknown command {commandToken}");
				}
			}
			else if (inListDir)
			{
				// This is always true if not a command, but it's good to be explicit.
				if (commandParts[0] == "dir")
				{
					current.AddDirectory(commandParts[1]);
					continue;
				}
				
				int size = int.Parse(commandParts[0]);
				string fileName = commandParts[1];

				current.AddFile(fileName, size);
			}
		}
		
		GD.Print(runningTotal);
	}

	private class Directory
	{
		private readonly Dictionary<string, Directory> _directories = new();
		private readonly Dictionary<string, int> _files = new();

		public Directory(string name, Directory parent)
		{
			this.Name = name;
			this.Parent = parent;
		}
		
		public Directory Parent { get; }
		
		public string Name { get; private set; }

		public int Size => _files.Values.Sum();

		public int TotalSize => _directories.Values.Sum(directory => directory.TotalSize) + Size;

		public bool AddFile(string fileName, int size)
		{
			return _files.TryAdd(fileName, size);
		}

		public bool AddDirectory(string directoryName)
		{
			return _directories.TryAdd(directoryName, new Directory(directoryName, this));
		}
		
		public Directory ChangeDirectory(string directoryName)
		{
			if (_directories.TryGetValue(directoryName, out Directory value))
			{
				return value;
			}

			throw new ArgumentException("Directory doesn't exist.", nameof(directoryName));
		}
	}
}