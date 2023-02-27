using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

namespace PipelineV3
{
public class GenericLevel : System.IComparable<GenericLevel>
	{
    	public Dictionary<System.Type, List<DesignElement>> sortedList;
    	//public List<Constraint> constraints; //does this need to be here?

		public GenericLevel(SpawnEnvironment spawnEnvironment, Mutation mutationFunction, Crossover crossoverFunction)
		{
			sortedList = new Dictionary<System.Type, List<DesignElement>>();

			this.spawnEnvironment = spawnEnvironment;
			this.mutationFunction = mutationFunction;
			this.crossoverFunction = crossoverFunction;

			fitness = 0;
			violatedConstraints = 0;
			canBeSelected = true;
		}

		public SpawnEnvironment spawnEnvironment;
		Mutation mutationFunction;
		Crossover crossoverFunction;
		public float fitness = 0, violatedConstraints = 0; 
		public bool canBeSelected, recentlyAdded = false;

		//values for evaluating crossoverMetrics
		public float fitnessBeforeMutation = 0, violatedConstraintsBeforeMutation = 0;
		public bool swappedPopulationDuringCrossover = false;
		public void StoreCrossoverValues()
		{
			fitnessBeforeMutation = fitness;
			violatedConstraintsBeforeMutation = violatedConstraints;
		}

        public int TotalDesignElementCount 
        {
            get
            {
                int count = 0;
                foreach (var kVPair in sortedList)
                    count += kVPair.Value.Count;
                
                return count;
            }
        }

		public GenericLevel Crossover(GenericLevel other)
		{
			return crossoverFunction.CrossoverFunc(this, other);
		}

		public int2 Mutate()
		{
			return mutationFunction.Mutate(this);
		}

        public int GetCountOfDesignElementsOfType<D>() where D : DesignElement
        {
			if(sortedList.TryGetValue(typeof(D), out List<DesignElement> list))
			{
				if(list == null)
					return 0;
			
				return list.Count;
			}
			
			return 0;
        }

		public List<D> GetDesignElementsOfType<D>() where D : DesignElement
		{
			if(sortedList.TryGetValue(typeof(D), out List<DesignElement> list))
			{
				var castList = new List<D>(list.Count);
				foreach (var item in list)
				{
					var castItem = (D)item;
					//Debug.Log(castItem.ToString());
					castList.Add(castItem);
				}
				return castList;
			}

			return null;
		}

        public void SetDesignElements(Dictionary<System.Type, List<DesignElement>> designElements)
        {
            this.sortedList = designElements;
        }

        public void AddDesignElements(List<DesignElement> designElements)
        {
            foreach (var de in designElements)
                AddDesignElement(de);
            
        }

		public void ReplaceDesignElement(DesignElement original, DesignElement newElement)
		{
			var type = original.GetType();
            if(sortedList.ContainsKey(type))
            {
                if(!sortedList[type].Remove(original))
					Debug.LogWarning($"Tried to remove DesignElement of Type: {type.ToString()}, but level did not contain it");
				
				sortedList[type].Add(newElement);
            }
            else
            {
                Debug.LogWarning($"Tried to remove DesignElement of Type: {type.ToString()}, but level does not contain elements of that type");
            }
		}

		public bool AddDesignElement(DesignElement designElement)
		{
			if(!designElement.CheckValidity())
				return false;
				
            var type = designElement.GetType();
			if(sortedList.ContainsKey(type))
			{
				sortedList[type].Add(designElement);
				return true;
			}
			sortedList.Add(type, new List<DesignElement>{ designElement });
			return true;
		}

        public void RemoveDesignElement(DesignElement designElement)
        {
            var type = designElement.GetType();
            if(sortedList.ContainsKey(type))
            {
                if(!sortedList[type].Remove(designElement))
					Debug.LogWarning($"Tried to remove DesignElement of Type: {type.ToString()}, but level did not contain it");
            }
            else
            {
                Debug.LogWarning($"Tried to remove DesignElement of Type: {type.ToString()}, but level does not contain elements of that type");
            }
        }

		public bool TryGetDesignElementsOfType<D>(out List<DesignElement> l) where D : DesignElement
		{
			if(sortedList.ContainsKey(typeof(D)))
			{
				l = sortedList[typeof(D)];
				return true;
			}
			l = null;
			return false;
		} 

		public int CompareTo(GenericLevel other)
		{
			return -1 * fitness.CompareTo(other.fitness);
		}

		public GenericLevel Clone()
		{
			var gL = new GenericLevel(spawnEnvironment.Clone(), mutationFunction, crossoverFunction);

			gL.fitness = fitness;
			gL.violatedConstraints = violatedConstraints;

			var clonedSortedList = new Dictionary<System.Type, List<DesignElement>>();

			foreach(var key in sortedList.Keys)
			{
				var value = sortedList[key];
				var clonedValue = new List<DesignElement>(value.Count);
				foreach (var item in value)
					clonedValue.Add(item.Clone(gL));
				
				clonedSortedList.Add(key, clonedValue);
			}
			gL.sortedList = clonedSortedList;
			//gL.constraints = constraints;
			return gL;
		}
	}
}

