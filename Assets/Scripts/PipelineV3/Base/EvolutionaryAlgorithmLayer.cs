using System.Collections.Generic;
using Unity.Mathematics;

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

		public float2 SortIntoPools(bool creatorIsFPop, List<GenericLevel> members)
		{
			int swappedMembers = 0;
			int summedDelta = 0;
			int summedRelevantMembers = 0;
			foreach (var member in members)
			{
				int constraintViolations = CalculateConstraintViolations(member);
				if(constraintViolations > 0)
				{
					iPool.Add(member);
					if(creatorIsFPop)
						swappedMembers++;
					else if(!member.swappedPopulationDuringCrossover)
					{
						summedDelta += (constraintViolations - member.violatedConstraintsBeforeMutation);
						summedRelevantMembers++;
					}
					continue;
				}
				int fitness = CalculateFitness(member);
				fPool.Add(member);
				if(!creatorIsFPop)
					swappedMembers++;
				else if(!member.swappedPopulationDuringCrossover)
				{
					summedDelta += (fitness - member.fitnessBeforeMutation);
					summedRelevantMembers++;
				}
				
			}			
			return new float2(swappedMembers, summedDelta / (float)summedRelevantMembers);
		}

		public int PreMutationMeasures(List<GenericLevel> members, bool creatorIsFPop)
		{
			int swappedMembers = 0;
			foreach (var member in members)
			{
				int constraintViolations = CalculateConstraintViolations(member);	
				if(constraintViolations == 0)
				{
					CalculateFitness(member);
					if(!creatorIsFPop)
					{
						swappedMembers++;
						member.swappedPopulationDuringCrossover = true;
					}
				}			
				else if(constraintViolations != 0 && creatorIsFPop)
				{
					swappedMembers++;
					member.swappedPopulationDuringCrossover = true;
				}

				member.StoreCrossoverValues();
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

		protected string label;

		public GenericLevel Run(int seed, string label, GenericLevel previousLayerSolution)
		{
			this.label = label;
			UnityEngine.Random.InitState(seed);
			Initialize();


			int currentGeneration = 0;
			while(!TerminationCondition() && currentGeneration < genericParams.maxGenerations)
			{
				var generationInformation = new GenerationInformation();
				generationInformation.currentGeneration = currentGeneration;

				var fOffspring = CreateChildren( fPop.Select());

				var invalidOffspringBeforeMutations = PreMutationMeasures(fOffspring, true);

				var mutationResultSum = int2.zero;
				foreach (var level in fOffspring)
					mutationResultSum += level.Mutate();

				generationInformation.averageAcceptedMutationRateFOffspring = mutationResultSum.x / (float)mutationResultSum.y;

				var sortingResults = SortIntoPools(true, fOffspring);
				generationInformation.invalidationRate = fOffspring.Count > 0 ? sortingResults.x / (float)fOffspring.Count : 0;
				generationInformation.mutationInvalidationRate = invalidOffspringBeforeMutations > 0 ? sortingResults.x / (float)invalidOffspringBeforeMutations : 0;
				generationInformation.averageMutationFitnessDelta = sortingResults.y;
				
				var newIPopMembers = iPop.AddAndReduce(iPool);
				generationInformation.iPopIntegrationRate = iPool.Count > 0 ? newIPopMembers / (float)iPool.Count : 0;

				iPool.Clear();
				var iOffspring = CreateChildren( iPop.Select());

				var validOffspringBeforeMutations = PreMutationMeasures(iOffspring, false);

				mutationResultSum = int2.zero;
				foreach (var level in iOffspring)
					mutationResultSum += level.Mutate();

				generationInformation.averageAcceptedMutationRateIOffspring = mutationResultSum.x / (float)mutationResultSum.y;

				sortingResults = SortIntoPools(false, iOffspring);
				generationInformation.validationRate = iOffspring.Count > 0 ? sortingResults.x / (float)iOffspring.Count : 0;
				generationInformation.mutationValidationRate = validOffspringBeforeMutations > 0 ? sortingResults.x / (float)validOffspringBeforeMutations : 0;
				generationInformation.averageMutationConstraintViolationDelta = sortingResults.y;

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
		public float validationRate; //at what rate do variation operators validate existing invalid levels
		public float invalidationRate; //at what rate do variation operators invalidate existing valid levels
		public float fPopIntegrationRate;
		public float iPopIntegrationRate;
		public float averageAcceptedMutationRateFOffspring;
		public float averageAcceptedMutationRateIOffspring;
		public float mutationValidationRate;   //at what rate do mutations validate existing invalid levels
		public float mutationInvalidationRate; //at what rate do mutations invalid existing valid levels

		public float crossoverValidationRate; //at what rate do crossovers validate existing invalid levels
		public float crossoverInvalidationRate; //at what rate do crossovers invalidate existing valid levels
		public float averageMutationFitnessDelta;
		public float averageMutationConstraintViolationDelta;
		
	}
}


