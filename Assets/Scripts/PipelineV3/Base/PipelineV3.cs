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
		public abstract void Mutate(GenericLevel genericLevel);
	}

	public abstract class SpawnEnvironment
	{
		public abstract void Initialize();
		public abstract void Clear();
		public abstract void Spawn(string type, DesignElement dE);
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
		public abstract void Mutate();
		public abstract DesignElement Clone(GenericLevel newOwner);
	}
}