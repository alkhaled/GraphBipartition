using System;
using System.Collections.Generic;
using System.Linq;

/*
 * A Bipartition of a graph splits the graph into two subsets that are connected by a single edge.
 * Given a complete set of bipartitions this program attempts to reconstruct an undirected graph using the following observation:
 *	If A and B are subsets resulting from partitioning a graph G, then there cannot exist a subset A' smaller than A and B that contains elements from both A and B
 *	because if it did then A,B would not be a valid bipartition.
 * 
 * Using this information we sort the partitions by Min(A,B) where A and B are the two sides of the partition. Then starting with the largest partition as our base, 
 * we recursively find the smallest subset that contains A, and pull out the elements of A into a new child subset. When this process is done, each subset should be of size one.
 * 
 * 
 * members is used to find all the elements that are part of, or once were (moved to a child), part of the current subset.
 * localMembers contains all the elements that still haven't been moved to a child subset.
 */
namespace TreeBipartition
{
	class TreeBipartition
	{
		class Set
		{
			public Set()
			{
				localMembers = new List<string>();
				members = new Dictionary<string, bool>();
			}

			// All nodes that exist in Set and any of its subsets - Note: Members will never be removed, only added
			private Dictionary<String, bool> members { get; set; }

			// Nodes that reside only in the set (not in the children) - Members can be removed only when placing them into a child set
			public List<String> localMembers { get; set; }

			// All subsets that share an edge with an element of localMembers
			List<Set> children = new List<Set>();

			public void SetLocalMember(String a)
			{
				localMembers.Add(a);
				members.Add(a, true);
			}

			private void RemoveFromLocalMembers(Set goingToChild)
			{
				// Assuming linq is optimized so this is not an O(n^2) op.
				var filtered = localMembers
									 .Where(x => !goingToChild.localMembers.Any(y => y == x));
				localMembers = filtered.ToList();
			}

			public bool IsSubset(Set candidateSubset)
			{
				var subset = candidateSubset.localMembers;
				// If an element exists in the candidate Subset but not our current subset then return false
				foreach (var member in subset)
				{
					if (!members.ContainsKey(member))
						return false;
				}
				return true;
			}

			public void MoveSubset(Set newChild)
			{
				// At this point newChild is already known to be a subset of the current set, we need to check if it is also a subset of any of the child subsets
				// if not just push this child onto the children list of the current set and remove newChilds members from this set's localMembers.

				foreach (var child in children)
				{
					if (child.IsSubset(newChild))
					{
						child.MoveSubset(newChild);
						RemoveFromLocalMembers(newChild);
						return;
					}
				}

				// New child is not a subset of any of the current subset's children.
				children.Add(newChild);
				RemoveFromLocalMembers(newChild);
			}

			public void PrettyPrint(string indent, bool last)
			{
				Console.Write(indent);
				if (last)
				{
					Console.Write("\\-");
					indent += "  ";
				}
				else
				{
					Console.Write("|-");
					indent += "| ";
				}
				Console.WriteLine(localMembers.First());

				for (int i = 0; i < children.Count; i++)
					children[i].PrettyPrint(indent, i == children.Count - 1);
			}
		}

		class Partition : IComparable
		{
			public Set a;
			public Set b;

			public Partition(Set a, Set b)
			{
				this.a = a;
				this.b = b;
			}

			public Set GetMin()
			{
				if (a.localMembers.Count() < b.localMembers.Count())
					return a;
				else
					return b;
			}

			int IComparable.CompareTo(object obj)
			{
				Partition toCompare = (Partition)obj;
				int toCompareMin = Math.Min(toCompare.a.localMembers.Count, toCompare.b.localMembers.Count);
				int thisMin = Math.Min(this.a.localMembers.Count, this.b.localMembers.Count);
				return thisMin - toCompareMin;
			}
		}


		static void Main(string[] args)
		{
			var partitions = TestCase1();
			RetrieveTreeFromBipartition(partitions);
			Console.WriteLine();
			partitions = TestCase2();
			RetrieveTreeFromBipartition(partitions);
		}

		static void RetrieveTreeFromBipartition(List<Partition> partitions)
		{
			// Sort the partitions so that we can visit them in decreasing size
			var sortedPartitions = partitions.ToArray();
			Array.Sort(sortedPartitions);

			// Set the root partition to be the most evenly divided one.
			var arbitraryRootPartition = sortedPartitions[sortedPartitions.Length - 1];

			//Use the partition information to divide subsets
			for (int nextPartition = sortedPartitions.Length - 2; nextPartition >= 0; nextPartition--)
			{
				// I don't think we need to consider the larger of the two subsets, as its size may be larger than a previously visited subset.
				var nextSubTree = sortedPartitions[nextPartition].GetMin();

				if (arbitraryRootPartition.a.IsSubset(nextSubTree))
					arbitraryRootPartition.a.MoveSubset(nextSubTree);
				else if (arbitraryRootPartition.b.IsSubset(nextSubTree))
					arbitraryRootPartition.b.MoveSubset(nextSubTree);
				else
				{
					Console.WriteLine("Invalid input: Graph contains a cycle");
				}
			}
			arbitraryRootPartition.a.PrettyPrint(" ", false);
			arbitraryRootPartition.b.PrettyPrint(" ", false);
		}

		static List<Partition> TestCase1()
		{
			List<String> partitionStrings = new List<string> { "b/acde", "ba/cde", "bace/d", "bacd/e" };
			Console.WriteLine("Running Test Case 1: \n");
			foreach (var partitionString in partitionStrings)
			{
				Console.WriteLine(partitionString);
			}
			Console.WriteLine();
			return StringToPartition(partitionStrings);
		}
		static List<Partition> TestCase2()
		{
			List<String> partitionStrings = new List<string>
			{
				"ABD/CEFG",
				"BD/ACEFG",
				"D/ABCEFG",
				"G/ABCDEF",
				"E/ABCDFG",
				"EF/ABCDG"
			};
			Console.WriteLine("Running Test Case 2: \n");
			foreach (var partitionString in partitionStrings)
			{
				Console.WriteLine(partitionString);
			}
			Console.WriteLine();
			return StringToPartition(partitionStrings);
		}

		static List<Partition> StringToPartition(IEnumerable<string> partitionStrings)
		{
			List<Partition> partitions = new List<Partition>();
			foreach (var part in partitionStrings)
			{
				Set a = new Set();
				int pos = 0;
				while (pos < part.Length && part[pos] != '/')
				{
					a.SetLocalMember(part[pos].ToString());
					pos++;
				}
				//move past the '/'
				pos++;
				Set b = new Set();
				while (pos < part.Length)
				{
					b.SetLocalMember(part[pos].ToString());
					pos++;
				}
				Partition newPartition = new Partition(a, b);
				partitions.Add(newPartition);
			}
			return partitions;
		}
	}
}

