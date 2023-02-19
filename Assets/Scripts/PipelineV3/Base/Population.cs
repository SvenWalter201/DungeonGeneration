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
				if(summedRank < r)
					return current;
			}	
	
			return null;
		}

		public bool Insert(GenericLevel member, InsertionStrategy insertionStrategy)
		{
			//if there is space available in the population, insert the new member and return
			if(Count < genericParams.populationSize)
			{
				members.Add(member);
				return true;
			}

			//if no space is available use the specified insertion strategy to place the new member
			switch(insertionStrategy)
			{
				case InsertionStrategy.RandomReplacement:
					return RandomReplacement(member, false);

				case InsertionStrategy.RandomEliteReplacement:
					return RandomReplacement(member, true);
				
				case InsertionStrategy.RouletteReplacement:
					return RouletteReplacement(member, false);
				
				case InsertionStrategy.RouletteEliteReplacement:
					return RouletteReplacement(member, true);
			}

			return false;
		}

		public bool RandomReplacement(GenericLevel memberToInsert, bool elite)
		{
			var rand = UnityEngine.Random.Range(0, Count);
			var member = members[rand];

			var replacementConditionMet = isFpop ? (memberToInsert.fitness > member.fitness) : (memberToInsert.violatedConstraints < member.violatedConstraints);
			if(!elite || (elite && replacementConditionMet))
			{
				members[rand] = memberToInsert;
				return true;
			}

			return false;
		}

		public bool RouletteReplacement(GenericLevel memberToInsert, bool elite)
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
					{
						if(!elite || (elite && current.fitness <= memberToInsert.fitness))		
						{
							current = memberToInsert;
							return true;
						}			
							
						return false;
					}
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
					{
						if(!elite || (elite && current.violatedConstraints >= memberToInsert.violatedConstraints))
						{
							current = memberToInsert;
							return true;
						}					
					
						return false;
					}
				}	
			}

			return false;
		}	
	}
}