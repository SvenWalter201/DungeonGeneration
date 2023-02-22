using System.Collections.Generic;

namespace PipelineV3
{
	public enum SelectionStrategy
	{
		//SingleTournamentSelection, 
		//DoubleTournamentSelection, 
		RouletteSelection, 
		RankSelection
	}

	public enum InsertionStrategy
	{
		RandomReplacement, //insert children at random, replacing anyone
		RouletteReplacement, //replace members with a probability inversly proportional to their fitness //average
		RankReplacement, //replace members with a probability inversly proportional to their rank //median
		AbsoluteFitnessReplacement, //replace the least fit members with the children
		//LocalEliteReplacement, //children replace their parents if they are more fit //!!!
		RandomEliteReplacement, //children are compared to random members in the population and replace them if they are more fit
		RouletteEliteReplacement, //like RouletteReplacement but only replace if the child is more fit
		RankEliteReplacement //like RankReplacement but only replace if the child is more fit
	}


    public abstract class EvolutionaryAlgorithmLayer
	{
		protected GenericParameters genericParams;

		public EvolutionaryAlgorithmLayer(GenericParameters genericParams)
		{
			this.genericParams = genericParams;
		}

		protected Population fPop, iPop;
		List<GenericLevel> iPool = new List<GenericLevel>();
		List<GenericLevel> fPool = new List<GenericLevel>();

			
		public abstract bool TerminationCondition();

		public abstract int CalculateFitness(GenericLevel gL);
		public abstract int CalculateConstraintViolations(GenericLevel gL);
		protected virtual void EndOfGenerationCallback(GenerationInformation currentGeneration){}
		protected virtual void ExecutionFinishedCallback(int currentGeneration){}

		public List<GenericLevel> CreateChildren(List<GenericLevel> selectedMembers)
		{
			var children = new List<GenericLevel>(selectedMembers.Count / 2);
			for (int i = 0; i < selectedMembers.Count - 1; i+=2)
			{
				var lhs = selectedMembers[i];
				var rhs = selectedMembers[i+1];
				var child = lhs.Crossover(rhs);
				children.Add(child);
			}

			foreach (var member in selectedMembers)
				member.canBeSelected = true;
			
			return children;
		}

		public int SortIntoPools(bool creatorIsFPop, List<GenericLevel> members)
		{
			int swappedMembers = 0;
			foreach (var member in members)
			{
				int violations = CalculateConstraintViolations(member);
				if(violations > 0)
				{
					iPool.Add(member);
					if(creatorIsFPop)
						swappedMembers++;
					continue;
				}
				int fitness = CalculateFitness(member);
				fPool.Add(member);
				if(!creatorIsFPop)
					swappedMembers++;
			}			
			return swappedMembers;
		}

		public abstract GenericLevel CreateMember();

		const int maxInitializationAttempts = 5000;
		public void Initialize()
		{
            iPop = new Population(genericParams, genericParams.populationSize, false);
            fPop = new Population(genericParams, genericParams.populationSize, true);

			int attempt = 0;
			while(attempt < maxInitializationAttempts && fPop.Count < genericParams.populationSize)
			{
				var member = CreateMember();
				var violations = CalculateConstraintViolations(member);
				if(violations > 0)
				{
					attempt++;
					continue;
				}
				var fitness = CalculateFitness(member);
				fPop.members.Add(member);
				attempt++;
			}
			attempt = 0;
			while(attempt < maxInitializationAttempts && iPop.Count < genericParams.populationSize)
			{
				var member = CreateMember();
				var violations = CalculateConstraintViolations(member);
				if(violations == 0)
				{
					attempt++;
					continue;
				}
				iPop.members.Add(member);
				attempt++;
			}

			fPop.Sort();
			iPop.Sort();
		}

		public GenericLevel Run(GenericLevel previousLayerSolution)
		{
			Initialize();

			int currentGeneration = 0;
			while(!TerminationCondition() && currentGeneration < genericParams.maxGenerations)
			{
				var generationInformation = new GenerationInformation();
				generationInformation.currentGeneration = currentGeneration;

				var fOffspring = CreateChildren( fPop.Select());
				foreach (var level in fOffspring)
					level.Mutate();
				var invalidOffspring = SortIntoPools(true, fOffspring);
				generationInformation.invalidationRate = fOffspring.Count > 0 ? invalidOffspring / (float)fOffspring.Count : 0;

				var newIPopMembers = iPop.AddAndReduce(iPool);
				generationInformation.iPopIntegrationRate = iPool.Count > 0 ? newIPopMembers / (float)iPool.Count : 0;

				iPool.Clear();
				var iOffspring = CreateChildren( iPop.Select());
				foreach (var level in iOffspring)
					level.Mutate();
				var validOffspring = SortIntoPools(false, iOffspring);
				generationInformation.validationRate = iOffspring.Count > 0 ? validOffspring / (float)iOffspring.Count : 0;

				var newFPopMembers = fPop.AddAndReduce(fPool);
				generationInformation.fPopIntegrationRate = fPool.Count > 0 ? newFPopMembers / (float)fPool.Count : 0;
				fPool.Clear();

				EndOfGenerationCallback(generationInformation);
				currentGeneration++;
			}

			ExecutionFinishedCallback(currentGeneration);
			return null;
		}
	}

	public struct GenerationInformation
	{
		public int currentGeneration;
		public float validationRate;
		public float invalidationRate;
		public float fPopIntegrationRate;
		public float iPopIntegrationRate;
		
	}
}


