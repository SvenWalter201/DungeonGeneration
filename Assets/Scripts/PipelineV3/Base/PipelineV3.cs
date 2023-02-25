using Unity.Mathematics;

namespace PipelineV3
{
	public class GenericParameters
	{
		public int populationSize, maxCrossovers, maxGenerations;
		public SelectionStrategy selectionStrategy;
		public InsertionStrategy insertionStrategy;
		public InsertionStrategy populationTransitionStrategy;
	}
	//BASE STUFF
	public abstract class Crossover
	{
		public abstract GenericLevel CrossoverFunc(GenericLevel lhs, GenericLevel rhs);
	}
	public abstract class Mutation
	{
		public int mutationsAttempted, mutationsAccepted;

		public void AddSuccessfulMutation()
		{
			mutationsAccepted++;
			mutationsAttempted++;		
		}

		protected abstract void MutateImpl(GenericLevel genericLevel);

		public int2 Mutate(GenericLevel genericLevel)
		{
			ResetMetrics();
			MutateImpl(genericLevel);
			return new int2(mutationsAccepted, mutationsAttempted);
		}

		public void ResetMetrics()
		{
			mutationsAccepted = 0;
			mutationsAttempted = 0;
		}

		public void EvaluateMutationResult(MutationResult result)
		{
			switch(result)
			{
				case MutationResult.None:
					break;
				case MutationResult.Accepted:
					mutationsAccepted++;
					mutationsAttempted++;
					break;
				case MutationResult.Rejected:
					mutationsAttempted++;
					break;
			}
		}
	}

	public abstract class SpawnEnvironment
	{
		public abstract void Initialize();
		public abstract void Clear();
		public abstract void Spawn(string type, DesignElement dE);
		public abstract void FinalizeEnvironment();
		public abstract SpawnEnvironment Clone();
			
	}

	public abstract class DesignElement
	{
		public DesignElement(GenericLevel levelReference)
		{
			this.levelReference = levelReference;
		}
		protected GenericLevel levelReference;	
		public abstract void Spawn(); //depending on the data structures used in Spawn environment, do some things, maybe instantiate DE	
		protected abstract bool Mutate();
		public abstract bool CheckValidity();
		public abstract DesignElement Clone(GenericLevel newOwner);
		public MutationResult Mutation()
		{
			var backup = Clone(levelReference);
			if(!Mutate()) //if none of the mutations of the DE occured, abort
				return MutationResult.None;

			//if a mutation occured, check if it is valid
			if(!CheckValidity()) //reject the mutation due to the level resulting violating fundamental constraints
			{
				levelReference.ReplaceDesignElement(this, backup);
				return MutationResult.Rejected;
			}
			return MutationResult.Accepted;
		}
	}

	public enum MutationResult
	{
		None,
		Rejected,
		Accepted
	}
}