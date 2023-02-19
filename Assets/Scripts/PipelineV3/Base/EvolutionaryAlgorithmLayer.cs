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
			//this.sERef = sERef;
			this.genericParams = genericParams;

			//sERef.Initialize();
		}

		protected Population fPop, iPop;

		public abstract void InitializePopulation(GenericLevel previousLayerSolution);
			//Extract De’s from solution. Create new DE’s according to rules
			
		public abstract bool TerminationCondition();

		public abstract int CalculateFitness(GenericLevel gL);
		public abstract int CalculateConstraintViolations(GenericLevel gL);
		protected virtual void EndOfGenerationCallback(int currentGeneration){}
		protected virtual void ExecutionFinishedCallback(int currentGeneration){}

		public void MigrateToFPop(GenericLevel member)
		{
			CalculateFitness(member);
			if(fPop.Insert(member, genericParams.populationTransitionStrategy))
				fPop.Sort();

			iPop.members.Remove(member);
		}

		public void MigrateToIPop(GenericLevel member)
		{
			if(iPop.Insert(member, genericParams.populationTransitionStrategy))
				iPop.Sort();

			fPop.members.Remove(member);
		}

		public void CalculateConstraintViolations()
		{
			var membersToMigrate = new List<GenericLevel>();
			for (int i = fPop.Count - 1; i >= 0; i--)
			{
				var member = fPop.members[i];
				var violations = CalculateConstraintViolations(member);	
				if(violations > 0)
				{
					membersToMigrate.Add(member);
					fPop.members.RemoveAt(i);
				}
				else
					CalculateFitness(member);
			}

			fPop.Sort();

			//Check if any members of iPop have become valid and if so transition them to fPop
			for (int i = iPop.Count - 1; i >= 0; i--)		
			{
				var violations = CalculateConstraintViolations(iPop.members[i]);	
				if(violations == 0)
					MigrateToFPop(iPop.members[i]);
			}

			iPop.Sort();

			foreach (var member in membersToMigrate)
				MigrateToIPop(member);

		}

		public void InsertChildren(List<GenericLevel> children)
		{
			foreach (var child in children)
			{
				int violations = CalculateConstraintViolations(child);
				if(violations > 0)
				{
					iPop.Insert(child, genericParams.insertionStrategy);
					continue;
				}
				int fitness = CalculateFitness(child);
				fPop.Insert(child, genericParams.insertionStrategy);
			}
		}

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

		public GenericLevel Run(GenericLevel previousLayerSolution)
		{
			InitializePopulation(previousLayerSolution);
			CalculateConstraintViolations();

			int currentGeneration = 0;
			while(!TerminationCondition() && currentGeneration < genericParams.maxGenerations)
			{
				//select members from both populations and produce children
				var iPopSelected = iPop.Select();
				var fPopSelected = fPop.Select();
				var children = CreateChildren(iPopSelected);
				children.AddRange(CreateChildren(fPopSelected));

				foreach (var level in iPop.members)
					level.Mutate();
			
				foreach (var level in fPop.members)
					level.Mutate();

				foreach (var level in children)
					level.Mutate();

				CalculateConstraintViolations();

				InsertChildren(children);

				EndOfGenerationCallback(currentGeneration);
				currentGeneration++;
			}

			ExecutionFinishedCallback(currentGeneration);
			return null;
		}
	}
}


