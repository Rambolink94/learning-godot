using System;
using System.Collections.Generic;
using System.Linq;

namespace LearningGodot;

public partial class NoSpaceLeft : PuzzleNode
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Directory root = new Directory("root", null);
		Directory current = root;
		
		const string baseDir = "/";
		
		bool inListDir = false;
		
		// Build directories based on input
		foreach (string line in InputReader.ReadInput(7))
		{
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

		const int diskSize = 70_000_000;
		const int updateSize = 30_000_000;
		
		int usedSpace = root.TotalSize;
		
		int smallestSize = int.MaxValue;
		Directory smallestValidDirectory = null;
		
		GetSmallestPossibleDirectoryToDelete(root);
		
		Print($"Best Directory: {smallestValidDirectory.Name} of size {smallestValidDirectory.TotalSize}");

		void GetSmallestPossibleDirectoryToDelete(Directory currentDir)
		{
			int currentSize = currentDir.TotalSize;
			if (usedSpace - currentSize <= diskSize - updateSize)
			{
				// Found a deletion that will allow for update
				if (currentSize < smallestSize)
				{
					// Get the directory that would result in the smallest delete
					smallestSize = currentSize;
					smallestValidDirectory = currentDir;
				}
			}

			foreach (Directory directory in currentDir.Directories)
			{
				GetSmallestPossibleDirectoryToDelete(directory);
			}
		}
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

		// TODO: Add fileAdded event that parents can subscribe to cache size changes.
		public int TotalSize => _directories.Values.Sum(directory => directory.TotalSize) + Size;

		public IEnumerable<Directory> Directories
		{
			get
			{
				foreach (Directory directory in _directories.Values)
				{
					yield return directory;
				}
			}
		}
		
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