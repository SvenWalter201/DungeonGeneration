using System.Collections.Generic;

namespace PipelineV3
{
    public class Population
	{
		public int Count => members.Count;
		public bool IsFPop => isFpop;
		bool isFpop;
		GenericParameters genericParams;

		public Population(GenericParameters genericParams, int maximumSize, bool isFpop)
		{
			members = new List<GenericLevel>(maximumSize);
			this.isFpop = isFpop;
			this.genericParams = genericParams;
		}

		public List<GenericLevel> members;

		public void Sort()
		{
			if(isFpop)
				members.Sort((x,y) => -x.fitness.CompareTo(y.fitness));
			
			else
				members.Sort((x,y) => x.violatedConstraints.CompareTo(y.violatedConstraints));
		}

		public List<GenericLevel> Select()
		{
			int requiredMembers = genericParams.maxCrossovers * 2;
			if(Count < requiredMembers)
				requiredMembers = Count;
			
			var selectedMembers = new List<GenericLevel>(requiredMembers);
			var children = new List<GenericLevel>(requiredMembers / 2);
			
			for (int i = 0; i < requiredMembers; i++)
			{
				GenericLevel selectedLevel = null;
				switch(genericParams.selectionStrategy)
				{
					case SelectionStrategy.RouletteSelection:
						selectedLevel = RouletteSelect(); 
						break;
					
					case SelectionStrategy.RankSelection:
						selectedLevel = RankSelect();
						break;
				
				}
				selectedLevel.canBeSelected = false;
				selectedMembers.Add(selectedLevel);
			}

			return selectedMembers;
		}

		GenericLevel RouletteSelect()
		{
			float sum = 0;
			if(isFpop)
			{
				foreach (var current in members)
					sum += current.fitness;
				
				float r = UnityEngine.Random.Range(0, sum);
				for (int i = Count - 1; i >= 0; i--)
				{
					var current = members[i];
					sum -= current.fitness;
					if(sum < r)
						return current;
				}		
			}
			else
			{
				foreach (var current in members)
					sum += (1.0f / (float)current.violatedConstraints);

				float r = UnityEngine.Random.Range(0, sum);
				for (int i = Count - 1; i >= 0; i--)
				{
					var current = members[i];
					sum -= (1.0f / (float)current.violatedConstraints);
					if(sum < r)
						return current;
				}		
			}
			
			return null;
		}

		GenericLevel RankSelect()
		{
			int summedRank = 0;
			for (int i = 0; i < Count; i++)
				summedRank += (i+1);

			int r = UnityEngine.Random.Range(0, summedRank);

			for (int i = 0, n = Count; i < Count; i++, n--)
			{
				var current = members[i];
				summedRank -= n;
				if(summedRank <= r)
					return current;
			}	
	
			return null;
		}

		GenericLevel InverseRouletteSelect()
		{
			float sum = 0;
			if(isFpop)
			{
				foreach (var current in members)
					sum += (1.0f / (float)current.fitness);
				
				float r = UnityEngine.Random.Range(0, sum);
				for (int i = Count - 1; i >= 0; i--)
				{
					var current = members[i];
					sum -= (1.0f / (float)current.fitness);
					if(sum < r)
						return current;
				}		
			}
			else
			{
				foreach (var current in members)
					sum += current.violatedConstraints;

				float r = UnityEngine.Random.Range(0, sum);
				for (int i = Count - 1; i >= 0; i--)
				{
					var current = members[i];
					sum -= current.violatedConstraints;
					if(sum < r)
						return current;
				}		
			}
			
			return null;
		}

		GenericLevel InverseRankSelect()
		{
			int summedRank = 0;
			for (int i = 0; i < Count; i++)
				summedRank += (i+1);

			int r = UnityEngine.Random.Range(0, summedRank);

			for (int i = 0; i < Count; i++)
			{
				var current = members[i];
				summedRank -= i;
				if(summedRank <= r)
					return current;
			}	
	
			return null;
		}

		public int AddAndReduce(List<GenericLevel> newLevels)
		{
			foreach (var level in newLevels)
				level.recentlyAdded = true;
			
			members.AddRange(newLevels);
			Sort();
			while(members.Count > genericParams.populationSize)
			{
				GenericLevel selectedLevel = null;
				switch(genericParams.insertionStrategy)
				{
					case InsertionStrategy.RouletteReplacement:
						selectedLevel = InverseRouletteSelect(); 
						break;
					
					case InsertionStrategy.RankReplacement:
						selectedLevel = InverseRankSelect();
						break;
					case InsertionStrategy.AbsoluteFitnessReplacement:
						selectedLevel = members[members.Count - 1];
						break;
				}
				members.Remove(selectedLevel);
			}

			int newMembersCount = 0;
			foreach (var level in members)
			{
				if(level.recentlyAdded)
				{
					newMembersCount++;
					level.recentlyAdded = false;
				}
			}

			return newMembersCount;
		}
	}
}