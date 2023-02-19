using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

public static class GeneticRasterizer
{
    public const int populationSize = 20, maxGenerations = 20, crossesPerGeneration = 2;
    public static void Run()
    {
        List<TemporaryLevel> population = new List<TemporaryLevel>();
        for (int i = 0; i < populationSize; i++)
        {
            var tempLevel = new TemporaryLevel();
            tempLevel.Generate();
            tempLevel.CalculateFitness();
            population.Add(tempLevel);
        }

        int currentGeneration = 0;
        while(currentGeneration < maxGenerations)
        {
            ++currentGeneration;

            //retain elite
            population.Sort((x,y) => x.fitness.CompareTo(y.fitness));

            for (int i = 0; i < crossesPerGeneration; i++)
                population.RemoveAt(population.Count - 1);
            
            //select individuals for crossover based on weighted randomness 
            List<TemporaryLevel> populationCopy = new List<TemporaryLevel>(population.ToArray());
            TemporaryLevel[] crossOverLevels = new TemporaryLevel[crossesPerGeneration * 2];
            for (int i = 0; i < crossOverLevels.Length; i++)
            {
                float summedFitness = 0;

                foreach (var ent in populationCopy)
                    summedFitness += ent.fitness;

                float r = UnityEngine.Random.Range(0, summedFitness);
                float currentSum = 0;
                for (int j = 0; j < populationCopy.Count; j++)
                {
                    var currentLevel = populationCopy[j];
                    currentSum += currentLevel.fitness;
                    if(currentSum > r)
                    {
                        crossOverLevels[i] = currentLevel;
                        populationCopy.RemoveAt(j);
                        break;
                    }
                }
            }
            
            //create and mutate offspring
            for (int i = 0; i < crossOverLevels.Length; i+=2)
            {
                var levelA = crossOverLevels[i];
                var levelB = crossOverLevels[i+1];
                var child = TemporaryLevel.Cross(levelA, levelB);
                child.CalculateFitness();
                population.Add(child);
            }

        }
    }

    public const int maxPathLength = 5, maxRoomSize = 5;
    public const bool rectangularRoomsOnly = true, straightPathsOnly = true;
    [System.Serializable]
    public class TemporaryLevel
    {
        public int2 dimensions;             //the size of the level grid
        public List<TemporaryRoom> rooms;
        public int overlapErrors = 0, noPathErrors = 0;
        public float fitness = 0;

        public void Generate()
        {

        }


        public void CalculateFitness()
        {
            overlapErrors = noPathErrors = 0;
            bool[] occupied = new bool[dimensions.x * dimensions.y];
            for (int r = 0; r < rooms.Count; r++)
            {
                var currentRoom = rooms[r];
                for (int p = 0; p < currentRoom.positions.Length; p++)
                {
                    var currentPosition = currentRoom.positions[p];
                    var index = MathHelper.getIndex(currentPosition, dimensions.x);
                    if(occupied[index])
                        ++overlapErrors;
                    else
                        occupied[index] = true;
                }
            }

            //use A* or JPS to find out, if paths exist from the rooms to each other
            //when no such paths are found, increase noPathError
            int weightedErrorSum = noPathErrors * ((maxRoomSize * maxRoomSize) / 2 + 1) + overlapErrors;
            fitness = weightedErrorSum == 0 ? 1 : 1 / (float)weightedErrorSum; //might need tweaking
        }

        public static TemporaryLevel Cross(TemporaryLevel a, TemporaryLevel b)
        {
            return null;
        }
    }


    [System.Serializable]
    public class TemporaryRoom
    {
        public int2[] positions;
    }
}
